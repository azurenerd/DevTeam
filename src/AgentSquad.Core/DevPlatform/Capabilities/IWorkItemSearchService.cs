namespace AgentSquad.Core.DevPlatform.Capabilities;

public record WorkItemSearchResult(
    int Id,
    string Title,
    string State,
    string WorkItemType,  // "Issue", "Epic", "Feature", "User Story", "Task", "Bug"
    string Url
);

public interface IWorkItemSearchService
{
    /// <summary>
    /// Search for work items by URL, numeric ID, or text query.
    /// </summary>
    Task<IReadOnlyList<WorkItemSearchResult>> SearchAsync(string query, int maxResults = 10, CancellationToken ct = default);
}
