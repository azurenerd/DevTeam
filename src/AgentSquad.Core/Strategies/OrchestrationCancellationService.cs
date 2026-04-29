using System.Collections.Concurrent;

namespace AgentSquad.Core.Strategies;

/// <summary>
/// Provides per-task cancellation support for strategy orchestration.
/// Dashboard can request cancellation via the REST API; the orchestrator
/// checks <see cref="IsCancellationRequested"/> during its run loop.
/// </summary>
public interface IOrchestrationCancellationService
{
    /// <summary>Register a CancellationTokenSource for a task so it can be cancelled externally.</summary>
    void Register(string runId, string taskId, CancellationTokenSource cts);

    /// <summary>Unregister when task completes normally.</summary>
    void Unregister(string runId, string taskId);

    /// <summary>Request cancellation of a running orchestration. Returns false if not found.</summary>
    bool RequestCancellation(string runId, string taskId);

    /// <summary>Check if cancellation was requested (without cancelling the token).</summary>
    bool IsCancellationRequested(string runId, string taskId);

    /// <summary>Get all currently registered (active) orchestration task IDs.</summary>
    IReadOnlyList<(string RunId, string TaskId)> GetActiveOrchestrations();
}

public sealed class OrchestrationCancellationService : IOrchestrationCancellationService
{
    private readonly ConcurrentDictionary<(string RunId, string TaskId), CancellationTokenSource> _sources = new();

    public void Register(string runId, string taskId, CancellationTokenSource cts)
        => _sources.TryAdd((runId, taskId), cts);

    public void Unregister(string runId, string taskId)
        => _sources.TryRemove((runId, taskId), out _);

    public bool RequestCancellation(string runId, string taskId)
    {
        if (!_sources.TryGetValue((runId, taskId), out var cts))
            return false;

        try
        {
            cts.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public bool IsCancellationRequested(string runId, string taskId)
    {
        if (!_sources.TryGetValue((runId, taskId), out var cts))
            return false;
        return cts.IsCancellationRequested;
    }

    public IReadOnlyList<(string RunId, string TaskId)> GetActiveOrchestrations()
        => _sources.Keys.ToList();
}
