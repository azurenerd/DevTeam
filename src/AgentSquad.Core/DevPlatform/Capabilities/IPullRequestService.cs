using AgentSquad.Core.DevPlatform.Models;

namespace AgentSquad.Core.DevPlatform.Capabilities;

/// <summary>
/// Pull request operations. Maps to GitHub Pull Requests or Azure DevOps Pull Requests.
/// </summary>
public interface IPullRequestService
{
    Task<PlatformPullRequest> CreateAsync(
        string title, string body, string headBranch, string baseBranch,
        IReadOnlyList<string> labels, CancellationToken ct = default);

    Task<PlatformPullRequest?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PlatformPullRequest>> ListOpenAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PlatformPullRequest>> ListAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PlatformPullRequest>> ListMergedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PlatformPullRequest>> ListForAgentAsync(string agentName, CancellationToken ct = default);

    Task UpdateAsync(
        int id, string? title = null, string? body = null,
        IReadOnlyList<string>? labels = null, CancellationToken ct = default);

    Task MergeAsync(int id, string? commitMessage = null, CancellationToken ct = default);
    Task CloseAsync(int id, CancellationToken ct = default);

    // PR file inspection
    Task<IReadOnlyList<string>> GetChangedFilesAsync(int prId, CancellationToken ct = default);
    Task<IReadOnlyList<PlatformFileDiff>> GetFileDiffsAsync(int prId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetCommitMessagesAsync(int prId, CancellationToken ct = default);
    Task<IReadOnlyList<PlatformCommitInfo>> GetCommitsWithDatesAsync(int prId, CancellationToken ct = default);

    // PR branch sync
    Task<bool> IsBehindBaseAsync(int prId, CancellationToken ct = default);
    Task<bool> UpdateBranchAsync(int prId, CancellationToken ct = default);
    Task<bool> RebaseBranchAsync(int prId, CancellationToken ct = default);
}
