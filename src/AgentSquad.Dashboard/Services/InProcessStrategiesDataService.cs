using AgentSquad.Core.Configuration;
using AgentSquad.Core.Strategies;
using Microsoft.Extensions.Options;

namespace AgentSquad.Dashboard.Services;

public sealed class InProcessStrategiesDataService : IStrategiesDataService
{
    private readonly CandidateStateStore _store;
    private readonly IOptionsMonitor<StrategyFrameworkConfig> _cfg;
    private readonly IOrchestrationCancellationService? _cancellation;

    public InProcessStrategiesDataService(
        CandidateStateStore store,
        IOptionsMonitor<StrategyFrameworkConfig> cfg,
        IOrchestrationCancellationService? cancellation = null)
    {
        _store = store;
        _cfg = cfg;
        _cancellation = cancellation;
    }

    public Task<IReadOnlyList<TaskSnapshot>> GetActiveTasksAsync(CancellationToken ct = default)
        => Task.FromResult(_store.GetActiveTasks());

    public Task<IReadOnlyList<TaskSnapshot>> GetRecentTasksAsync(int limit = 50, CancellationToken ct = default)
        => Task.FromResult(_store.GetRecentTasks(limit));

    public Task<EnabledStrategiesInfo> GetEnabledAsync(CancellationToken ct = default)
    {
        var c = _cfg.CurrentValue;
        return Task.FromResult(new EnabledStrategiesInfo(c.Enabled, c.EnabledStrategies.ToList()));
    }

    public Task<bool> CancelOrchestrationAsync(string runId, string taskId, CancellationToken ct = default)
        => Task.FromResult(_cancellation?.RequestCancellation(runId, taskId) ?? false);
}
