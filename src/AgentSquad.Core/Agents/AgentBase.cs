using Microsoft.Extensions.Logging;

namespace AgentSquad.Core.Agents;

public abstract class AgentBase : IAgent, IDisposable
{
    private readonly object _statusLock = new();
    private readonly object _errorLock = new();
    private AgentStatus _status = AgentStatus.Requested;
    private string? _statusReason;
    private bool _disposed;
    private readonly List<AgentLogEntry> _recentErrors = new();

    protected AgentBase(AgentIdentity identity, ILogger<AgentBase> logger)
    {
        Identity = identity ?? throw new ArgumentNullException(nameof(identity));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LifetimeCts = new CancellationTokenSource();
    }

    public AgentIdentity Identity { get; }

    public AgentStatus Status
    {
        get { lock (_statusLock) { return _status; } }
    }

    public string? StatusReason
    {
        get { lock (_statusLock) { return _statusReason; } }
    }

    /// <summary>Gets recent error/warning log entries for this agent.</summary>
    public IReadOnlyList<AgentLogEntry> RecentErrors
    {
        get { lock (_errorLock) { return _recentErrors.ToList(); } }
    }

    /// <summary>Clears all tracked errors/warnings.</summary>
    public void ClearErrors()
    {
        lock (_errorLock) { _recentErrors.Clear(); }
        ErrorsChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<AgentStatusChangedEventArgs>? StatusChanged;
    public event EventHandler? ErrorsChanged;

    protected ILogger<AgentBase> Logger { get; }
    protected CancellationTokenSource LifetimeCts { get; }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        UpdateStatus(AgentStatus.Initializing, "Agent initialization started");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LifetimeCts.Token);
        await OnInitializeAsync(linked.Token);
        UpdateStatus(AgentStatus.Online, "Agent initialized successfully");
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        UpdateStatus(AgentStatus.Working, "Agent starting main loop");
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LifetimeCts.Token);
        await OnStartAsync(linked.Token);
        await RunAgentLoopAsync(linked.Token);
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        Logger.LogInformation("Agent {AgentId} stopping", Identity.Id);
        await LifetimeCts.CancelAsync();
        await OnStopAsync(ct);
        UpdateStatus(AgentStatus.Offline, "Agent stopped gracefully");
    }

    public async Task HandleMessageAsync(AgentMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, LifetimeCts.Token);
        Logger.LogDebug("Agent {AgentId} received message {MessageType} from {FromAgent}",
            Identity.Id, message.MessageType, message.FromAgentId);
        await OnMessageReceivedAsync(message, linked.Token);
    }

    protected void UpdateStatus(AgentStatus newStatus, string? reason = null)
    {
        AgentStatus oldStatus;
        lock (_statusLock)
        {
            oldStatus = _status;
            _status = newStatus;
            _statusReason = reason;
        }

        Logger.LogInformation("Agent {AgentId} status changed: {OldStatus} -> {NewStatus} ({Reason})",
            Identity.Id, oldStatus, newStatus, reason ?? "no reason");

        StatusChanged?.Invoke(this, new AgentStatusChangedEventArgs
        {
            Agent = Identity,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = reason
        });
    }

    /// <summary>Record an error or warning that will be visible in the dashboard.</summary>
    protected void RecordError(string message, LogLevel level = LogLevel.Error, Exception? exception = null)
    {
        var entry = new AgentLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            ExceptionDetails = exception?.ToString()
        };

        lock (_errorLock)
        {
            _recentErrors.Add(entry);
            // Keep last 50 entries max
            if (_recentErrors.Count > 50)
                _recentErrors.RemoveAt(0);
        }

        ErrorsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual Task OnInitializeAsync(CancellationToken ct) => Task.CompletedTask;
    protected virtual Task OnStartAsync(CancellationToken ct) => Task.CompletedTask;
    protected virtual Task OnStopAsync(CancellationToken ct) => Task.CompletedTask;
    protected virtual Task OnMessageReceivedAsync(AgentMessage message, CancellationToken ct) => Task.CompletedTask;

    protected abstract Task RunAgentLoopAsync(CancellationToken ct);

    public void Dispose()
    {
        if (!_disposed)
        {
            LifetimeCts.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>A log entry recorded by an agent for dashboard display.</summary>
public record AgentLogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public string Message { get; init; } = "";
    public string? ExceptionDetails { get; init; }
}
