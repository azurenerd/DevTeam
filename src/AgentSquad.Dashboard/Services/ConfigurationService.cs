using System.Text.Json;
using System.Text.Json.Nodes;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.GitHub;
using AgentSquad.Core.GitHub.Models;
using Microsoft.Extensions.Options;

namespace AgentSquad.Dashboard.Services;

/// <summary>
/// Service for reading/writing appsettings.json and performing GitHub repo cleanup operations.
/// </summary>
public sealed class ConfigurationService
{
    private readonly IOptions<AgentSquadConfig> _config;
    private readonly IGitHubService _github;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _appSettingsPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null // preserve PascalCase
    };

    public ConfigurationService(
        IOptions<AgentSquadConfig> config,
        IGitHubService github,
        ILogger<ConfigurationService> logger,
        IWebHostEnvironment env)
    {
        _config = config;
        _github = github;
        _logger = logger;
        _appSettingsPath = Path.Combine(env.ContentRootPath, "appsettings.json");
    }

    /// <summary>Returns the current in-memory config snapshot.</summary>
    public AgentSquadConfig GetCurrentConfig() => _config.Value;

    /// <summary>
    /// Validates a GitHub PAT token against a specified repo.
    /// Returns a result with repo info on success, or error message on failure.
    /// </summary>
    public async Task<PatValidationResult> ValidatePatAsync(string token, string repoFullName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new PatValidationResult { Success = false, Error = "Token is empty." };

        if (string.IsNullOrWhiteSpace(repoFullName) || !repoFullName.Contains('/'))
            return new PatValidationResult { Success = false, Error = "Repo must be in 'owner/repo' format." };

        var parts = repoFullName.Split('/', 2);
        try
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("AgentSquad-Validate"))
            {
                Credentials = new Octokit.Credentials(token)
            };

            var repo = await client.Repository.Get(parts[0], parts[1]);
            var user = await client.User.Current();

            // Check key permissions by testing a read-only endpoint
            var scopes = new List<string>();
            try
            {
                await client.Issue.GetAllForRepository(parts[0], parts[1],
                    new Octokit.RepositoryIssueRequest { State = Octokit.ItemStateFilter.Open });
                scopes.Add("issues:read");
            }
            catch { /* no access */ }

            return new PatValidationResult
            {
                Success = true,
                RepoName = repo.FullName,
                RepoDescription = repo.Description ?? "(no description)",
                IsPrivate = repo.Private,
                DefaultBranch = repo.DefaultBranch,
                AuthenticatedUser = user.Login,
                Permissions = scopes
            };
        }
        catch (Octokit.NotFoundException)
        {
            return new PatValidationResult { Success = false, Error = $"Repository '{repoFullName}' not found. Check the repo name and that your PAT has access." };
        }
        catch (Octokit.AuthorizationException)
        {
            return new PatValidationResult { Success = false, Error = "Authorization failed. The PAT token is invalid or expired." };
        }
        catch (Exception ex)
        {
            return new PatValidationResult { Success = false, Error = $"Validation failed: {ex.Message}" };
        }
    }

    /// <summary>Returns the GitHub repo name from config.</summary>
    public string GetRepoName() => _config.Value.Project.GitHubRepo;

    /// <summary>Reads the raw JSON from appsettings.json.</summary>
    public async Task<JsonObject?> ReadAppSettingsAsync()
    {
        if (!File.Exists(_appSettingsPath))
        {
            _logger.LogWarning("appsettings.json not found at {Path}", _appSettingsPath);
            return null;
        }

        var json = await File.ReadAllTextAsync(_appSettingsPath);
        return JsonNode.Parse(json)?.AsObject();
    }

    /// <summary>
    /// Merges updated AgentSquad config values into appsettings.json, preserving
    /// non-AgentSquad sections (e.g., Logging).
    /// </summary>
    public async Task SaveConfigAsync(AgentSquadConfig updatedConfig)
    {
        var root = await ReadAppSettingsAsync() ?? new JsonObject();

        // Serialize the updated config section
        var configJson = JsonSerializer.SerializeToNode(updatedConfig, JsonOptions);
        root["AgentSquad"] = configJson;

        var output = root.ToJsonString(JsonOptions);
        await File.WriteAllTextAsync(_appSettingsPath, output);

        _logger.LogInformation("Configuration saved to {Path}", _appSettingsPath);
    }

    /// <summary>
    /// Scans the GitHub repo and returns a summary of what would be cleaned up.
    /// </summary>
    public async Task<CleanupSummary> ScanRepoForCleanupAsync(CancellationToken ct = default)
    {
        var config = _config.Value.Project;
        var summary = new CleanupSummary { RepoFullName = config.GitHubRepo };

        try
        {
            // Get all issues (open + closed)
            var allIssues = await _github.GetAllIssuesAsync(ct);
            summary.OpenIssues = allIssues.Count(i => i.State == "open");
            summary.ClosedIssues = allIssues.Count(i => i.State != "open");

            // Get all PRs
            var allPrs = await _github.GetAllPullRequestsAsync(ct);
            summary.OpenPrs = allPrs.Count(p => p.State == "open" && !p.IsMerged);
            summary.MergedPrs = allPrs.Count(p => p.IsMerged);
            summary.ClosedPrs = allPrs.Count(p => p.State != "open" && !p.IsMerged);

            // Get repo file tree
            var files = await _github.GetRepositoryTreeAsync(config.DefaultBranch, ct);
            summary.FileCount = files.Count;
            summary.Files = files.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan repo for cleanup");
            summary.Error = ex.Message;
        }

        return summary;
    }

    /// <summary>
    /// Executes the destructive cleanup operation on the GitHub repo.
    /// </summary>
    public async Task<CleanupResult> ExecuteCleanupAsync(
        string? caveats, CancellationToken ct = default)
    {
        var result = new CleanupResult();
        var config = _config.Value.Project;

        try
        {
            // Parse caveats to find files to preserve
            var preserveFiles = ParseCaveats(caveats);

            // 1. Delete ALL issues (open + closed) — uses GraphQL deleteIssue, falls back to close
            _logger.LogWarning("CLEANUP: Deleting all issues in {Repo}", config.GitHubRepo);
            var allIssues = await _github.GetAllIssuesAsync(ct);
            foreach (var issue in allIssues)
            {
                try
                {
                    var deleted = await _github.DeleteIssueAsync(issue.Number, ct);
                    if (deleted)
                        result.IssuesDeleted++;
                    else
                        result.IssuesClosed++; // fallback to close
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete issue #{Number}", issue.Number);
                    result.Errors.Add($"Failed to delete issue #{issue.Number}: {ex.Message}");
                }
            }

            // 2. Close all open PRs
            _logger.LogWarning("CLEANUP: Closing all open PRs in {Repo}", config.GitHubRepo);
            var openPrs = await _github.GetOpenPullRequestsAsync(ct);
            foreach (var pr in openPrs)
            {
                try
                {
                    await _github.ClosePullRequestAsync(pr.Number, ct);
                    result.PrsClosed++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to close PR #{Number}", pr.Number);
                    result.Errors.Add($"Failed to close PR #{pr.Number}: {ex.Message}");
                }
            }

            // 3. Delete agent branches (not main/master)
            _logger.LogWarning("CLEANUP: Deleting agent branches in {Repo}", config.GitHubRepo);
            // We can't list branches via our service, but we can delete known ones from closed PRs
            foreach (var pr in openPrs)
            {
                if (!string.IsNullOrEmpty(pr.HeadBranch) &&
                    pr.HeadBranch != config.DefaultBranch &&
                    pr.HeadBranch != "main" &&
                    pr.HeadBranch != "master")
                {
                    try
                    {
                        await _github.DeleteBranchAsync(pr.HeadBranch, ct);
                        result.BranchesDeleted++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete branch {Branch}", pr.HeadBranch);
                    }
                }
            }

            // 4. Delete all files from default branch (except preserved ones)
            _logger.LogWarning("CLEANUP: Deleting files from {Branch} in {Repo}",
                config.DefaultBranch, config.GitHubRepo);
            var files = await _github.GetRepositoryTreeAsync(config.DefaultBranch, ct);
            var filesToDelete = files
                .Where(f => !IsFilePreserved(f, preserveFiles))
                .ToList();

            foreach (var file in filesToDelete)
            {
                try
                {
                    await _github.DeleteFileAsync(file, $"Cleanup: remove {file}", config.DefaultBranch, ct);
                    result.FilesDeleted++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {File}", file);
                    result.Errors.Add($"Failed to delete {file}: {ex.Message}");
                }
            }

            result.FilesPreserved = files.Count - filesToDelete.Count;
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cleanup failed");
            result.Errors.Add($"Cleanup failed: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Parses caveat text to extract file preservation patterns.
    /// Looks for patterns like: "Leave X", "Keep X", "Preserve X", "Don't delete X"
    /// </summary>
    private static HashSet<string> ParseCaveats(string? caveats)
    {
        var preserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(caveats)) return preserved;

        // Extract quoted strings and file-like tokens
        var words = caveats.Split(new[] { ' ', ',', ';', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var word in words)
        {
            // Match file-like patterns (has extension or path separator)
            var clean = word.Trim('\"', '\'', '`', '(', ')');
            if (clean.Contains('.') || clean.Contains('/') || clean.Contains('\\'))
            {
                preserved.Add(clean);
            }
        }

        return preserved;
    }

    /// <summary>Checks if a file matches any preservation pattern.</summary>
    private static bool IsFilePreserved(string filePath, HashSet<string> preservePatterns)
    {
        if (preservePatterns.Count == 0) return false;

        foreach (var pattern in preservePatterns)
        {
            // Exact match
            if (filePath.Equals(pattern, StringComparison.OrdinalIgnoreCase)) return true;

            // Filename match (e.g., "Design.html" matches "src/Design.html")
            var fileName = Path.GetFileName(filePath);
            if (fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase)) return true;

            // Contains match for partial paths
            if (filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase)) return true;

            // Wildcard extension match (e.g., "*.html")
            if (pattern.StartsWith("*.") &&
                filePath.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

/// <summary>Summary of what would be affected by a repo cleanup.</summary>
public sealed class CleanupSummary
{
    public string RepoFullName { get; set; } = "";
    public int OpenIssues { get; set; }
    public int ClosedIssues { get; set; }
    public int OpenPrs { get; set; }
    public int MergedPrs { get; set; }
    public int ClosedPrs { get; set; }
    public int FileCount { get; set; }
    public List<string> Files { get; set; } = new();
    public string? Error { get; set; }

    public int TotalIssues => OpenIssues + ClosedIssues;
    public int TotalPrs => OpenPrs + MergedPrs + ClosedPrs;
}

/// <summary>Result of a repo cleanup operation.</summary>
public sealed class CleanupResult
{
    public bool Success { get; set; }
    public int IssuesDeleted { get; set; }
    public int IssuesClosed { get; set; }
    public int PrsClosed { get; set; }
    public int FilesDeleted { get; set; }
    public int FilesPreserved { get; set; }
    public int BranchesDeleted { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>Result of PAT token validation.</summary>
public sealed class PatValidationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? RepoName { get; set; }
    public string? RepoDescription { get; set; }
    public bool IsPrivate { get; set; }
    public string? DefaultBranch { get; set; }
    public string? AuthenticatedUser { get; set; }
    public List<string> Permissions { get; set; } = new();
}
