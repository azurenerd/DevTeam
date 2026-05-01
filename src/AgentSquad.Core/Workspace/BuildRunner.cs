using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Core.Workspace;

/// <summary>
/// Executes build commands in a local workspace and parses the output for errors.
/// Supports dotnet build, npm run build, and other configurable build tools.
/// </summary>
public class BuildRunner
{
    private readonly ILogger<BuildRunner> _logger;

    public BuildRunner(ILogger<BuildRunner> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Run the configured build command in the workspace directory.
    /// Auto-detects .sln file when using 'dotnet build' to avoid MSB1011 errors.
    /// </summary>
    public async Task<BuildResult> BuildAsync(
        string workspacePath,
        string buildCommand,
        int timeoutSeconds = 120,
        CancellationToken ct = default)
    {
        // Auto-resolve build target for bare 'dotnet build' to avoid MSB1011
        var resolvedCommand = ResolveBuildCommand(workspacePath, buildCommand);
        _logger.LogInformation("Running build in {Path}: {Command}", workspacePath, resolvedCommand);

        var result = await RunCommandAsync(workspacePath, resolvedCommand, timeoutSeconds, ct);

        var parsedErrors = ParseBuildErrors(result.StandardOutput + "\n" + result.StandardError);

        var buildResult = new BuildResult
        {
            Success = result.Success,
            Output = result.StandardOutput,
            Errors = result.StandardError,
            Duration = result.Duration,
            ParsedErrors = parsedErrors
        };

        if (buildResult.Success)
            _logger.LogInformation("Build succeeded in {Duration:F1}s", buildResult.Duration.TotalSeconds);
        else
            _logger.LogWarning("Build FAILED with {Count} errors in {Duration:F1}s",
                parsedErrors.Count, buildResult.Duration.TotalSeconds);

        return buildResult;
    }

    /// <summary>
    /// Parse build output for individual error messages.
    /// Supports dotnet/MSBuild, npm/Node, and generic error patterns.
    /// </summary>
    internal static IReadOnlyList<string> ParseBuildErrors(string output)
    {
        var errors = new List<string>();

        // dotnet/MSBuild errors: "File.cs(42,10): error CS1002: ; expected"
        var dotnetErrors = Regex.Matches(output,
            @"^.*?(?:error\s+CS\d+|error\s+MSB\d+|error\s+NU\d+):.*$",
            RegexOptions.Multiline);
        foreach (Match m in dotnetErrors)
            errors.Add(m.Value.Trim());

        // Generic "error:" pattern (catches most build tools)
        if (errors.Count == 0)
        {
            var genericErrors = Regex.Matches(output,
                @"^.*\berror\b.*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (Match m in genericErrors)
            {
                var line = m.Value.Trim();
                // Skip noise lines
                if (line.Contains("0 Error(s)", StringComparison.OrdinalIgnoreCase)) continue;
                if (line.Contains("error(s)", StringComparison.OrdinalIgnoreCase) && line.Contains("warning(s)", StringComparison.OrdinalIgnoreCase)) continue;
                errors.Add(line);
            }
        }

        return errors;
    }

    private async Task<ProcessResult> RunCommandAsync(
        string workDir, string command, int timeoutSeconds, CancellationToken ct)
    {
        // Split command into executable and arguments
        var (exe, args) = ParseCommand(command);

        var startInfo = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = args,
            WorkingDirectory = workDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var sw = Stopwatch.StartNew();
        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            sw.Stop();
            return new ProcessResult
            {
                ExitCode = -1,
                StandardOutput = await stdoutTask,
                StandardError = $"Build timed out after {timeoutSeconds}s",
                Duration = sw.Elapsed
            };
        }

        sw.Stop();

        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = await stdoutTask,
            StandardError = await stderrTask,
            Duration = sw.Elapsed
        };
    }

    internal static (string Exe, string Args) ParseCommand(string command)
    {
        command = command.Trim();

        // Handle "dotnet build --foo", "npm run build", etc.
        var parts = command.Split(' ', 2);
        var exe = parts[0];
        var args = parts.Length > 1 ? parts[1] : "";

        // On Windows, if the command is "npm" or "npx", use cmd /c
        if (OperatingSystem.IsWindows() &&
            exe is "npm" or "npx" or "yarn" or "pnpm")
        {
            args = $"/c {command}";
            exe = "cmd";
        }

        return (exe, args);
    }

    /// <summary>
    /// When the build command is bare 'dotnet build' (no project/sln target specified),
    /// auto-detect the .sln or .csproj to avoid MSB1011 "multiple project files" errors.
    /// Priority: single .sln > single .csproj > first .sln alphabetically.
    /// </summary>
    internal string ResolveBuildCommand(string workspacePath, string buildCommand)
    {
        // Only resolve for bare 'dotnet build' (no target already specified)
        var trimmed = buildCommand.Trim();
        if (!trimmed.Equals("dotnet build", StringComparison.OrdinalIgnoreCase))
            return buildCommand;

        var slnFiles = Directory.GetFiles(workspacePath, "*.sln");
        var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj");

        // No .NET project files at all — check for Node.js project
        if (slnFiles.Length == 0 && csprojFiles.Length == 0)
        {
            // Search subdirectories too
            var anySlns = Directory.GetFiles(workspacePath, "*.sln", SearchOption.AllDirectories);
            var anyCsprojs = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);

            if (anySlns.Length > 0)
            {
                var target = Path.GetRelativePath(workspacePath, anySlns[0]);
                _logger.LogInformation("Auto-resolved build target to {Target} (found in subdirectory)", target);
                return $"dotnet build {target}";
            }

            if (anyCsprojs.Length > 0)
            {
                var target = Path.GetRelativePath(workspacePath, anyCsprojs[0]);
                _logger.LogInformation("Auto-resolved build target to {Target} (found in subdirectory)", target);
                return $"dotnet build {target}";
            }

            // No .NET files anywhere — check if this is a Node.js project
            var packageJson = Path.Combine(workspacePath, "package.json");
            if (File.Exists(packageJson))
            {
                _logger.LogInformation("No .NET project found; detected package.json — switching to 'npm run build'");
                return "npm run build";
            }

            _logger.LogWarning("No .NET project or package.json found in {Path}; returning original command", workspacePath);
            return buildCommand;
        }

        // If exactly one target exists, dotnet build works fine as-is
        if (slnFiles.Length + csprojFiles.Length <= 1)
            return buildCommand;

        // Prefer .sln file (it includes all projects)
        if (slnFiles.Length >= 1)
        {
            var target = Path.GetFileName(slnFiles[0]);
            _logger.LogInformation("Auto-resolved build target to {Target} (found {SlnCount} .sln, {CsprojCount} .csproj)",
                target, slnFiles.Length, csprojFiles.Length);
            return $"dotnet build {target}";
        }

        // Fallback: use first .csproj
        if (csprojFiles.Length >= 1)
        {
            var target = Path.GetFileName(csprojFiles[0]);
            _logger.LogInformation("Auto-resolved build target to {Target} (no .sln, {Count} .csproj files)",
                target, csprojFiles.Length);
            return $"dotnet build {target}";
        }

        return buildCommand;
    }
}
