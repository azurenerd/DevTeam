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
    /// </summary>
    public async Task<BuildResult> BuildAsync(
        string workspacePath,
        string buildCommand,
        int timeoutSeconds = 120,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Running build in {Path}: {Command}", workspacePath, buildCommand);

        var result = await RunCommandAsync(workspacePath, buildCommand, timeoutSeconds, ct);

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
}
