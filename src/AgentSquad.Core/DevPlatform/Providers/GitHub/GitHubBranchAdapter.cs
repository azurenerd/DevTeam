using AgentSquad.Core.DevPlatform.Capabilities;
using AgentSquad.Core.GitHub;

namespace AgentSquad.Core.DevPlatform.Providers.GitHub;

/// <summary>
/// Adapts <see cref="IGitHubService"/> branch operations to <see cref="IBranchService"/>.
/// </summary>
public sealed class GitHubBranchAdapter : IBranchService
{
    private readonly IGitHubService _github;

    public GitHubBranchAdapter(IGitHubService github)
    {
        ArgumentNullException.ThrowIfNull(github);
        _github = github;
    }

    public Task CreateAsync(string branchName, string? fromBranch = null, CancellationToken ct = default)
        => _github.CreateBranchAsync(branchName, fromBranch ?? "main", ct);

    public Task<bool> ExistsAsync(string branchName, CancellationToken ct = default)
        => _github.BranchExistsAsync(branchName, ct);

    public Task DeleteAsync(string branchName, CancellationToken ct = default)
        => _github.DeleteBranchAsync(branchName, ct);

    public Task<IReadOnlyList<string>> ListAsync(string? prefix = null, CancellationToken ct = default)
        => _github.ListBranchesAsync(prefix, ct);

    public Task CleanToBaselineAsync(
        IReadOnlyList<string> preserveFiles, string commitMessage,
        string? branch = null, CancellationToken ct = default)
        => _github.CleanRepoToBaselineAsync(preserveFiles, commitMessage, branch ?? "main", ct);
}
