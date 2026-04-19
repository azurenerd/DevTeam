using AgentSquad.Core.Configuration;
using AgentSquad.Core.Strategies;
using Microsoft.Extensions.Options;

namespace AgentSquad.Dashboard.Services;

public sealed class InProcessStrategiesDataService : IStrategiesDataService
{
    private readonly CandidateStateStore _store;
    private readonly IOptionsMonitor<StrategyFrameworkConfig> _cfg;

    public InProcessStrategiesDataService(
        CandidateStateStore store,
        IOptionsMonitor<StrategyFrameworkConfig> cfg)
    {
        _store = store;
        _cfg = cfg;
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
}
