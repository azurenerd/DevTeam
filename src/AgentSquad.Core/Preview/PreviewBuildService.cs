using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.Workspace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentSquad.Core.Preview;

/// <summary>
/// Orchestrates cloning/updating a working branch, building, and running
/// the project locally for human preview. Manages process lifecycle and
/// streams output via events.
/// </summary>
public sealed class PreviewBuildService : IDisposable
{
    private readonly ILogger<PreviewBuildService> _logger;
    private readonly AgentSquadConfig _config;
    private readonly string _settingsPath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private Process? _runningProcess;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    // Regex to redact tokens from output (GitHub PAT, ADO PAT patterns)
    private static readonly Regex TokenRedactor = new(
        @"(https?://)([^@:]+)([:@])[^@/]+(@)",
        RegexOptions.Compiled);

    public PreviewState State { get; private set; } = PreviewState.Idle;
    public string? ErrorMessage { get; private set; }
    public string? AppUrl { get; private set; }
    public int ActualPort { get; private set; }
    public int? RunningProcessId => _runningProcess?.Id;

    /// <summary>Raised when new output lines are available (already token-redacted).</summary>
    public event Action<string>? OutputReceived;

    /// <summary>Raised when state changes.</summary>
    public event Action<PreviewState>? StateChanged;

    public PreviewBuildService(
        ILogger<PreviewBuildService> logger,
        IOptions<AgentSquadConfig> config,
        string? settingsPath = null)
    {
        _logger = logger;
        _config = config.Value;
        _settingsPath = settingsPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "preview-settings.json");
    }

    public async Task<PreviewSettings> LoadSettingsAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_settingsPath))
            return new PreviewSettings();

        var json = await File.ReadAllTextAsync(_settingsPath, ct);
        return JsonSerializer.Deserialize<PreviewSettings>(json, JsonOpts) ?? new PreviewSettings();
    }

    public async Task SaveSettingsAsync(PreviewSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var json = JsonSerializer.Serialize(settings, JsonOpts);
        var tmp = _settingsPath + ".tmp";
        await File.WriteAllTextAsync(tmp, json, ct);
        File.Move(tmp, _settingsPath, overwrite: true);
    }

    /// <summary>
    /// Clone (or update) the working branch, build, and run the project.
    /// Streams output via <see cref="OutputReceived"/>.
    /// </summary>
    public async Task StartAsync(PreviewSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (State == PreviewState.Running || State == PreviewState.Cloning || State == PreviewState.Building)
        {
            _logger.LogWarning("Preview is already in state {State}, ignoring start request", State);
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ClonePath))
            throw new InvalidOperationException("Clone path must be specified.");

        ErrorMessage = null;
        AppUrl = null;

        try
        {
            // === Stage 1: Clone or Update ===
            SetState(PreviewState.Cloning);
            await CloneOrUpdateAsync(settings, ct);

            // === Stage 2: Build ===
            SetState(PreviewState.Building);
            var buildCmd = ResolveCommand(settings.BuildCommandOverride, _config.Workspace.BuildCommand,
                DetectBuildCommand(settings.ClonePath));
            Emit($"▶ Building: {buildCmd}");
            var buildResult = await RunCommandAsync(buildCmd, settings.ClonePath, timeoutSeconds: 180, ct);
            if (buildResult != 0)
            {
                SetState(PreviewState.Failed);
                ErrorMessage = "Build failed. Check output above for errors.";
                Emit($"❌ Build failed (exit code {buildResult})");
                return;
            }
            Emit("✅ Build succeeded");

            // === Stage 3: Run ===
            SetState(PreviewState.Running);
            ActualPort = ResolvePort(settings.Port);
            var runCmd = ResolveRunCommand(settings, ActualPort);
            Emit($"▶ Starting app on port {ActualPort}: {runCmd}");
            await StartAppProcessAsync(runCmd, settings.ClonePath, ActualPort, ct);
        }
        catch (OperationCanceledException)
        {
            SetState(PreviewState.Stopped);
            Emit("⏹ Preview cancelled.");
        }
        catch (Exception ex)
        {
            SetState(PreviewState.Failed);
            ErrorMessage = ex.Message;
            Emit($"❌ Error: {ex.Message}");
            _logger.LogError(ex, "Preview build/run failed");
        }
    }

    /// <summary>Stop the running preview process.</summary>
    public void Stop()
    {
        if (_runningProcess is { HasExited: false } proc)
        {
            try
            {
                proc.Kill(entireProcessTree: true);
                Emit("⏹ Preview process stopped.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to kill preview process");
            }
        }

        _runningProcess = null;
        AppUrl = null;
        SetState(PreviewState.Stopped);
    }

    /// <summary>Get current status snapshot for the API.</summary>
    public async Task<PreviewStatus> GetStatusAsync(PreviewSettings? settings, CancellationToken ct = default)
    {
        string? branch = null, sha = null, message = null;
        DateTime? lastUpdated = null;
        var clonePath = settings?.ClonePath ?? "";

        if (!string.IsNullOrWhiteSpace(clonePath) && Directory.Exists(Path.Combine(clonePath, ".git")))
        {
            try
            {
                branch = await GetGitOutputAsync("rev-parse --abbrev-ref HEAD", clonePath, ct);
                sha = await GetGitOutputAsync("rev-parse --short HEAD", clonePath, ct);
                message = await GetGitOutputAsync("log -1 --format=%s", clonePath, ct);
                var dateStr = await GetGitOutputAsync("log -1 --format=%aI", clonePath, ct);
                if (DateTime.TryParse(dateStr, out var dt))
                    lastUpdated = dt.ToUniversalTime();
            }
            catch { /* git info is best-effort */ }
        }

        return new PreviewStatus
        {
            State = State,
            ErrorMessage = ErrorMessage,
            AppUrl = AppUrl,
            ProcessId = RunningProcessId,
            BranchName = branch,
            HeadCommitSha = sha,
            HeadCommitMessage = message,
            LastUpdatedUtc = lastUpdated,
            ActualPort = ActualPort
        };
    }

    #region Private Helpers

    private void SetState(PreviewState newState)
    {
        State = newState;
        StateChanged?.Invoke(newState);
    }

    private void Emit(string line)
    {
        var redacted = RedactTokens(line);
        OutputReceived?.Invoke(redacted);
    }

    private static string RedactTokens(string input)
    {
        // Redact PATs in URLs: https://token@github.com → https://***@github.com
        return TokenRedactor.Replace(input, "$1***$4");
    }

    private async Task CloneOrUpdateAsync(PreviewSettings settings, CancellationToken ct)
    {
        var clonePath = settings.ClonePath;
        var branch = _config.Project.WorkingBranch ?? _config.Project.DefaultBranch;
        var cloneUrl = _config.GetGitCloneUrl();

        if (Directory.Exists(Path.Combine(clonePath, ".git")))
        {
            Emit($"📂 Repository already exists at {clonePath}");
            Emit($"🔄 Pulling latest from {branch}...");

            await RunCommandAsync($"git -C \"{clonePath}\" fetch origin", clonePath, 60, ct);
            await RunCommandAsync($"git -C \"{clonePath}\" checkout {branch}", clonePath, 30, ct);
            var pullResult = await RunCommandAsync(
                $"git -C \"{clonePath}\" pull origin {branch}", clonePath, 120, ct);

            if (pullResult != 0)
            {
                Emit("⚠️ Pull had conflicts or errors. Attempting hard reset to remote...");
                await RunCommandAsync(
                    $"git -C \"{clonePath}\" reset --hard origin/{branch}", clonePath, 30, ct);
            }

            Emit("✅ Repository updated");
        }
        else
        {
            // Create parent directory if needed
            var parent = Path.GetDirectoryName(clonePath);
            if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
                Directory.CreateDirectory(parent);

            Emit($"📥 Cloning {branch} to {clonePath}...");
            var result = await RunCommandAsync(
                $"git clone --branch {branch} --single-branch \"{cloneUrl}\" \"{clonePath}\"",
                parent ?? Directory.GetCurrentDirectory(), 300, ct);

            if (result != 0)
                throw new InvalidOperationException("Git clone failed. Check output for details.");

            Emit("✅ Clone complete");
        }
    }

    private async Task<int> RunCommandAsync(string command, string workingDir, int timeoutSeconds, CancellationToken ct)
    {
        var isWindows = OperatingSystem.IsWindows();
        var psi = new ProcessStartInfo
        {
            FileName = isWindows ? "cmd.exe" : "/bin/sh",
            Arguments = isWindows ? $"/c {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = new Process { StartInfo = psi };
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) Emit(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Emit(e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            await proc.WaitForExitAsync(cts.Token);
            return proc.ExitCode;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            proc.Kill(entireProcessTree: true);
            Emit($"⏱ Command timed out after {timeoutSeconds}s");
            return -1;
        }
    }

    private async Task StartAppProcessAsync(string command, string workingDir, int port, CancellationToken ct)
    {
        var isWindows = OperatingSystem.IsWindows();
        var psi = new ProcessStartInfo
        {
            FileName = isWindows ? "cmd.exe" : "/bin/sh",
            Arguments = isWindows ? $"/c {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var proc = new Process { StartInfo = psi };
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) Emit(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Emit(e.Data); };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        _runningProcess = proc;

        // Wait for app to be ready (poll HTTP)
        AppUrl = $"http://localhost:{port}";
        Emit($"⏳ Waiting for app at {AppUrl}...");

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
        {
            if (proc.HasExited)
            {
                SetState(PreviewState.Failed);
                ErrorMessage = $"App process exited with code {proc.ExitCode}";
                Emit($"❌ App exited immediately (code {proc.ExitCode})");
                return;
            }

            try
            {
                var resp = await httpClient.GetAsync(AppUrl, ct);
                if ((int)resp.StatusCode < 500)
                {
                    Emit($"✅ App is running at {AppUrl}");
                    return;
                }
            }
            catch { /* not ready yet */ }

            await Task.Delay(1000, ct);
        }

        // Even if not responding to HTTP, the process is running
        if (!proc.HasExited)
        {
            Emit($"⚠️ App process is running but not responding to HTTP at {AppUrl}. It may use a different port or be a non-web project.");
        }
    }

    private string ResolveCommand(string? userOverride, string configDefault, string autoDetected)
    {
        if (!string.IsNullOrWhiteSpace(userOverride)) return userOverride;
        if (!string.IsNullOrWhiteSpace(configDefault)) return configDefault;
        return autoDetected;
    }

    private string ResolveRunCommand(PreviewSettings settings, int port)
    {
        if (!string.IsNullOrWhiteSpace(settings.RunCommandOverride))
            return settings.RunCommandOverride.Replace("{port}", port.ToString());

        if (!string.IsNullOrWhiteSpace(_config.Workspace.AppStartCommand))
            return _config.Workspace.AppStartCommand.Replace("{port}", port.ToString());

        return DetectRunCommand(settings.ClonePath, port);
    }

    private int ResolvePort(int preferred)
    {
        if (preferred > 0 && IsPortFree(preferred))
            return preferred;

        // Find a free port in the 5100-5199 range
        for (int p = 5100; p < 5200; p++)
        {
            if (IsPortFree(p)) return p;
        }

        // Fallback: OS-assigned
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static bool IsPortFree(int port)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch { return false; }
    }

    /// <summary>Auto-detect build command from project files.</summary>
    private static string DetectBuildCommand(string projectPath)
    {
        if (Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly).Length > 0)
            return "dotnet build";
        if (Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly).Length > 0)
            return "dotnet build";
        if (File.Exists(Path.Combine(projectPath, "package.json")))
            return "npm install && npm run build";
        if (File.Exists(Path.Combine(projectPath, "requirements.txt")))
            return "pip install -r requirements.txt";
        if (File.Exists(Path.Combine(projectPath, "Cargo.toml")))
            return "cargo build";
        if (File.Exists(Path.Combine(projectPath, "go.mod")))
            return "go build ./...";

        return "echo No build system detected";
    }

    /// <summary>Auto-detect run command from project files.</summary>
    private static string DetectRunCommand(string projectPath, int port)
    {
        // .NET projects
        var slnFiles = Directory.GetFiles(projectPath, "*.sln", SearchOption.TopDirectoryOnly);
        if (slnFiles.Length > 0)
        {
            // Find a web project (has launchSettings.json or is a Blazor/ASP.NET project)
            var webProjects = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories)
                .Where(f => File.ReadAllText(f).Contains("Microsoft.NET.Sdk.Web"))
                .ToList();

            if (webProjects.Count > 0)
            {
                var projDir = Path.GetDirectoryName(webProjects[0])!;
                var relPath = Path.GetRelativePath(projectPath, projDir);
                return $"dotnet run --project \"{relPath}\" --urls http://localhost:{port}";
            }

            // Console app
            return "dotnet run";
        }

        if (Directory.GetFiles(projectPath, "*.csproj", SearchOption.TopDirectoryOnly).Length > 0)
            return $"dotnet run --urls http://localhost:{port}";

        // Node.js projects
        if (File.Exists(Path.Combine(projectPath, "package.json")))
        {
            var pkgJson = File.ReadAllText(Path.Combine(projectPath, "package.json"));
            if (pkgJson.Contains("\"dev\""))
                return $"npx cross-env PORT={port} npm run dev";
            if (pkgJson.Contains("\"start\""))
                return $"npx cross-env PORT={port} npm start";
        }

        // Python
        if (File.Exists(Path.Combine(projectPath, "manage.py")))
            return $"python manage.py runserver 0.0.0.0:{port}";
        if (File.Exists(Path.Combine(projectPath, "app.py")) || File.Exists(Path.Combine(projectPath, "main.py")))
            return $"python -m uvicorn main:app --port {port}";

        return $"echo No run command detected for port {port}";
    }

    private static async Task<string?> GetGitOutputAsync(string args, string workingDir, CancellationToken ct)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        if (proc == null) return null;

        var output = await proc.StandardOutput.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        return output?.Trim();
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _lock.Dispose();
    }
}
