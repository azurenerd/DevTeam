using AgentSquad.Core.Strategies;

namespace AgentSquad.Dashboard.Services;

/// <summary>
/// Dashboard-facing read model for strategy candidate state. Two impls:
/// - <see cref="InProcessStrategiesDataService"/> when the dashboard is hosted in the Runner.
/// - <see cref="HttpStrategiesDataService"/> when standalone — calls the Runner REST API.
/// </summary>
public interface IStrategiesDataService
{
    Task<IReadOnlyList<TaskSnapshot>> GetActiveTasksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskSnapshot>> GetRecentTasksAsync(int limit = 50, CancellationToken ct = default);
    Task<EnabledStrategiesInfo> GetEnabledAsync(CancellationToken ct = default);
    Task<bool> CancelOrchestrationAsync(string runId, string taskId, CancellationToken ct = default);
}

public sealed record EnabledStrategiesInfo(bool MasterEnabled, IReadOnlyList<string> EnabledStrategies);
