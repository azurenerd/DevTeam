using System.Collections.Concurrent;

namespace AgentSquad.Core.AI;

/// <summary>
/// Tracks which agents currently have active LLM calls in progress.
/// Used by the dashboard to overlay "Working (AI)" status on agents
/// that are waiting for a Copilot CLI response.
/// </summary>
public sealed class ActiveLlmCallTracker
{
    private readonly ConcurrentDictionary<string, LlmCallInfo> _activeCalls = new();

    public void NotifyCallStarted(string agentId, string modelName)
    {
        _activeCalls[agentId] = new LlmCallInfo(modelName, DateTime.UtcNow);
    }

    public void NotifyCallCompleted(string agentId)
    {
        _activeCalls.TryRemove(agentId, out _);
    }

    /// <summary>
    /// Returns the active LLM call info for the given agent, or null if no call is in progress.
    /// </summary>
    public LlmCallInfo? GetActiveCall(string agentId)
    {
        return _activeCalls.TryGetValue(agentId, out var info) ? info : null;
    }

    /// <summary>Returns all agents with active LLM calls.</summary>
    public IReadOnlyDictionary<string, LlmCallInfo> GetAllActiveCalls() => _activeCalls;

    public sealed record LlmCallInfo(string ModelName, DateTime StartedAt);
}
