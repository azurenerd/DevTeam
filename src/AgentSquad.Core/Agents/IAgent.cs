namespace AgentSquad.Core.Agents;

public interface IAgent
{
    AgentIdentity Identity { get; }
    AgentStatus Status { get; }
    string? StatusReason { get; }

    Task InitializeAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
    Task HandleMessageAsync(AgentMessage message, CancellationToken ct = default);

    event EventHandler<AgentStatusChangedEventArgs>? StatusChanged;
    event EventHandler? ErrorsChanged;
    event EventHandler<AgentActivityEventArgs>? ActivityLogged;

    IReadOnlyList<AgentLogEntry> RecentErrors { get; }
    void ClearErrors();
}

public class AgentStatusChangedEventArgs : EventArgs
{
    public required AgentIdentity Agent { get; init; }
    public required AgentStatus OldStatus { get; init; }
    public required AgentStatus NewStatus { get; init; }
    public string? Reason { get; init; }
}

public class AgentActivityEventArgs : EventArgs
{
    public required string AgentId { get; init; }
    public required string EventType { get; init; }
    public required string Details { get; init; }
}
