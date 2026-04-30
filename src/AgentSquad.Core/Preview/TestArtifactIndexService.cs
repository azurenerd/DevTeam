using AgentSquad.Core.Configuration;
using AgentSquad.Core.Workspace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentSquad.Core.Preview;

/// <summary>
/// Artifact metadata record for the Test Artifacts dashboard tab.
/// </summary>
public record TestArtifactEntry
{
    public required string Id { get; init; }
    public required string FileName { get; init; }
    public required string FullPath { get; init; }
    public required TestArtifactType Type { get; init; }
    public required string AgentName { get; init; }
    public string? PrNumber { get; init; }
    public DateTime CapturedAtUtc { get; init; }
    public long FileSizeBytes { get; init; }

    /// <summary>Relative URL path for serving this artifact via API.</summary>
    public string ApiPath => $"/api/preview/artifacts/{Id}";
}

public enum TestArtifactType
{
    Screenshot,
    Video,
    Trace
}

/// <summary>
/// Scans agent workspace test-results/ directories and indexes all Playwright artifacts
/// (screenshots, videos, traces) with metadata for the dashboard to display.
/// </summary>
public sealed class TestArtifactIndexService
{
    private readonly ILogger<TestArtifactIndexService> _logger;
    private readonly AgentSquadConfig _config;
    private List<TestArtifactEntry> _cache = [];
    private DateTime _lastScanUtc = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

    public TestArtifactIndexService(
        ILogger<TestArtifactIndexService> logger,
        IOptions<AgentSquadConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    /// <summary>
    /// Returns all indexed test artifacts, refreshing the cache if stale.
    /// </summary>
    public IReadOnlyList<TestArtifactEntry> GetArtifacts(bool forceRefresh = false)
    {
        if (!forceRefresh && DateTime.UtcNow - _lastScanUtc < _cacheDuration)
            return _cache;

        _cache = ScanAllWorkspaces();
        _lastScanUtc = DateTime.UtcNow;
        return _cache;
    }

    /// <summary>
    /// Find a specific artifact by ID.
    /// </summary>
    public TestArtifactEntry? GetArtifactById(string id)
    {
        var artifacts = GetArtifacts();
        return artifacts.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Get artifacts filtered by PR number.
    /// </summary>
    public IReadOnlyList<TestArtifactEntry> GetArtifactsByPR(string prNumber)
    {
        return GetArtifacts().Where(a => a.PrNumber == prNumber).ToList();
    }

    /// <summary>
    /// Get artifacts filtered by agent name.
    /// </summary>
    public IReadOnlyList<TestArtifactEntry> GetArtifactsByAgent(string agentName)
    {
        return GetArtifacts()
            .Where(a => a.AgentName.Contains(agentName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private List<TestArtifactEntry> ScanAllWorkspaces()
    {
        var results = new List<TestArtifactEntry>();
        var rootPath = _config.Workspace.RootPath;

        if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
        {
            _logger.LogDebug("Workspace root {Path} does not exist, no artifacts to index", rootPath);
            return results;
        }

        // Scan all agent subdirectories
        foreach (var agentDir in Directory.GetDirectories(rootPath))
        {
            var agentName = Path.GetFileName(agentDir);

            // Look for test-results in any repo subdirectory
            var testResultsDirs = Directory.GetDirectories(agentDir, _config.Workspace.TestResultsDir,
                SearchOption.AllDirectories);

            foreach (var testResultsDir in testResultsDirs)
            {
                // Try to determine PR number from directory structure
                // Pattern: .agents/{AgentName}/{Repo}/test-results/ or
                //          .agents/{AgentName}/{Repo}/pr-{N}/test-results/
                var prNumber = ExtractPrNumber(testResultsDir, agentDir);

                ScanDirectory(testResultsDir, agentName, prNumber, results);
            }
        }

        // Sort by capture time, newest first
        results.Sort((a, b) => b.CapturedAtUtc.CompareTo(a.CapturedAtUtc));

        _logger.LogDebug("Indexed {Count} test artifacts across all workspaces", results.Count);
        return results;
    }

    private void ScanDirectory(string dir, string agentName, string? prNumber, List<TestArtifactEntry> results)
    {
        if (!Directory.Exists(dir)) return;

        // Screenshots
        var screenshotsDir = Path.Combine(dir, "screenshots");
        if (Directory.Exists(screenshotsDir))
        {
            foreach (var file in Directory.GetFiles(screenshotsDir, "*.png", SearchOption.AllDirectories))
                results.Add(CreateEntry(file, TestArtifactType.Screenshot, agentName, prNumber));
        }

        // Also look for screenshots directly in test-results (some configs put them here)
        foreach (var file in Directory.GetFiles(dir, "*.png", SearchOption.TopDirectoryOnly))
            results.Add(CreateEntry(file, TestArtifactType.Screenshot, agentName, prNumber));

        // Videos
        var videosDir = Path.Combine(dir, "videos");
        if (Directory.Exists(videosDir))
        {
            foreach (var file in Directory.GetFiles(videosDir, "*.webm", SearchOption.AllDirectories))
                results.Add(CreateEntry(file, TestArtifactType.Video, agentName, prNumber));
        }

        // Traces
        var tracesDir = Path.Combine(dir, "traces");
        if (Directory.Exists(tracesDir))
        {
            foreach (var file in Directory.GetFiles(tracesDir, "*.zip", SearchOption.AllDirectories))
                results.Add(CreateEntry(file, TestArtifactType.Trace, agentName, prNumber));
        }
    }

    private static TestArtifactEntry CreateEntry(string fullPath, TestArtifactType type, string agentName, string? prNumber)
    {
        var fi = new FileInfo(fullPath);
        // Create a stable ID from relative path hash
        var id = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(fullPath)))[..16].ToLowerInvariant();

        return new TestArtifactEntry
        {
            Id = id,
            FileName = fi.Name,
            FullPath = fullPath,
            Type = type,
            AgentName = agentName,
            PrNumber = prNumber,
            CapturedAtUtc = fi.LastWriteTimeUtc,
            FileSizeBytes = fi.Exists ? fi.Length : 0
        };
    }

    private static string? ExtractPrNumber(string testResultsDir, string agentDir)
    {
        // Look for "pr-{N}" or "PR-{N}" in the path between agent dir and test-results
        var relativePath = Path.GetRelativePath(agentDir, testResultsDir);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        foreach (var segment in segments)
        {
            if (segment.StartsWith("pr-", StringComparison.OrdinalIgnoreCase) &&
                segment.Length > 3 &&
                int.TryParse(segment[3..], out _))
            {
                return segment[3..];
            }
        }

        return null;
    }
}
