using AgentSquad.Core.DevPlatform.Models;

namespace AgentSquad.Core.DevPlatform.Capabilities;

/// <summary>
/// PR code review operations.
/// Maps to GitHub Reviews/Comments API or Azure DevOps PR Threads API.
/// </summary>
public interface IReviewService
{
    Task AddCommentAsync(int prId, string comment, CancellationToken ct = default);
    Task<IReadOnlyList<PlatformComment>> GetCommentsAsync(int prId, CancellationToken ct = default);
    Task AddReviewAsync(int prId, string body, string eventType, CancellationToken ct = default);

    /// <summary>
    /// Create a review with inline comments on specific lines.
    /// GitHub: PR review with comments. ADO: PR threads with positions.
    /// </summary>
    Task CreateReviewWithInlineCommentsAsync(
        int prId, string body, string eventType,
        IReadOnlyList<PlatformInlineComment> comments,
        string? commitId = null, CancellationToken ct = default);

    /// <summary>Get existing review threads for context-aware re-reviews.</summary>
    Task<IReadOnlyList<PlatformReviewThread>> GetThreadsAsync(int prId, CancellationToken ct = default);

    /// <summary>
    /// Reply to a review thread and optionally resolve it.
    /// threadId is platform-specific: GitHub GraphQL node_id, ADO thread int ID.
    /// </summary>
    Task ResolveThreadAsync(
        int prId, string threadId, string replyBody,
        CancellationToken ct = default);
}
