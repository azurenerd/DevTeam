using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Core.Configuration;

namespace AgentSquad.Core.Mcp;

/// <summary>
/// Resolves how to spawn the workspace-reader MCP server. Returns a command +
/// fixed-args pair that <see cref="McpConfigWriter.BuildConfig"/> can serialize.
/// Implementations MUST throw when resolution fails — callers (strategies) treat
/// a throw as a hard failure and do NOT silently fall back to a no-tools baseline
/// run (that would poison experiment records).
/// </summary>
public interface IMcpServerLocator
{
    /// <summary>
    /// Produce a <c>(command, fixedArgs)</c> pair for launching the server. The
    /// caller appends <c>--root &lt;worktree&gt;</c> to <paramref name="fixedArgs"/>
    /// when materializing the config.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the server binary (and its .NET runtimeconfig sidecar) cannot
    /// be located on disk.
    /// </exception>
    McpServerLaunchSpec Resolve();
}

/// <summary>Command and fixed args for spawning an MCP server subprocess.</summary>
public sealed record McpServerLaunchSpec(string Command, IReadOnlyList<string> FixedArgs, string ResolvedPath);

/// <summary>
/// Default locator. Resolution order:
/// <list type="number">
///   <item><description>Explicit config override (<see cref="StrategyFrameworkConfig.McpServerDllPath"/>).</description></item>
///   <item><description>
///     Probe paths relative to <see cref="AppContext.BaseDirectory"/>:
///     <c>./AgentSquad.McpServer.dll</c> (same output folder — production shape), then
///     sibling <c>bin/&lt;Configuration&gt;/net8.0/AgentSquad.McpServer.dll</c> (dev-tree shape).
///   </description></item>
/// </list>
/// The resolved DLL must have its <c>.runtimeconfig.json</c> sidecar present — a DLL
/// alone is not enough to invoke via <c>dotnet &lt;dll&gt;</c>.
/// </summary>
public sealed class DefaultMcpServerLocator : IMcpServerLocator
{
    private const string McpServerDllName = "AgentSquad.McpServer.dll";

    private readonly IOptionsMonitor<StrategyFrameworkConfig> _cfg;
    private readonly ILogger<DefaultMcpServerLocator> _logger;

    public DefaultMcpServerLocator(IOptionsMonitor<StrategyFrameworkConfig> cfg, ILogger<DefaultMcpServerLocator> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    public McpServerLaunchSpec Resolve()
    {
        var probed = new List<string>();

        // (1) Explicit config override.
        var configPath = _cfg.CurrentValue.McpServerDllPath;
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            var full = Path.GetFullPath(configPath);
            probed.Add(full);
            if (IsUsable(full)) return Build(full);
        }

        // (2) Production shape: DLL copied alongside the host exe.
        var baseDir = AppContext.BaseDirectory;
        var beside = Path.Combine(baseDir, McpServerDllName);
        probed.Add(beside);
        if (IsUsable(beside)) return Build(beside);

        // (3) Dev-tree shape: walk up to the repo root and look under src/AgentSquad.McpServer/bin/<cfg>/net8.0.
        // Matching configuration (Debug/Release) is picked by reading the containing build folder name.
        try
        {
            var config = GuessConfiguration(baseDir); // "Debug" or "Release"
            var repoRoot = FindRepoRoot(baseDir);
            if (repoRoot is not null)
            {
                var devPath = Path.Combine(
                    repoRoot, "src", "AgentSquad.McpServer", "bin", config, "net8.0", McpServerDllName);
                probed.Add(devPath);
                if (IsUsable(devPath)) return Build(devPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "DefaultMcpServerLocator: dev-tree probe failed (non-fatal)");
        }

        throw new InvalidOperationException(
            $"IMcpServerLocator could not find '{McpServerDllName}'. Set " +
            $"AgentSquad.StrategyFramework.McpServerDllPath to an absolute path. " +
            $"Probed: {string.Join(" ; ", probed)}");
    }

    private static bool IsUsable(string dllPath)
    {
        if (!File.Exists(dllPath)) return false;
        // Framework-dependent .NET apps require a runtimeconfig.json sidecar to run via `dotnet <dll>`.
        var runtimeCfg = Path.ChangeExtension(dllPath, ".runtimeconfig.json");
        return File.Exists(runtimeCfg);
    }

    private static McpServerLaunchSpec Build(string dllPath)
    {
        // The caller (McpConfigWriter) appends --root <worktree>. We only fix args
        // that are constant across invocations.
        return new McpServerLaunchSpec(
            Command: "dotnet",
            FixedArgs: new[] { dllPath },
            ResolvedPath: dllPath);
    }

    private static string GuessConfiguration(string baseDir)
    {
        // baseDir typically ends in `.../bin/Debug/net8.0` or `.../bin/Release/net8.0`.
        var parts = baseDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        for (int i = parts.Length - 1; i >= 1; i--)
        {
            if (string.Equals(parts[i - 1], "bin", StringComparison.OrdinalIgnoreCase))
                return parts[i];
        }
        return "Debug";
    }

    private static string? FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(start);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AgentSquad.sln"))) return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }
}
