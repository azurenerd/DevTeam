namespace AgentSquad.Core.Configuration;

/// <summary>
/// Service that evaluates human interaction gates at workflow touchpoints.
/// When a gate requires human approval, the service signals the need (via GitHub labels/comments)
/// and returns a result indicating the workflow should wait.
/// </summary>
public interface IGateCheckService
{
    /// <summary>
    /// Check whether a gate requires human approval and act accordingly.
    /// If the gate doesn't require human approval, returns Proceed immediately.
    /// If human approval is required, posts a notification and returns WaitingForHuman.
    /// </summary>
    /// <param name="gateId">The gate identifier (use <see cref="GateIds"/> constants).</param>
    /// <param name="context">Human-readable description of what's being gated (e.g., "PMSpec.md review").</param>
    /// <param name="resourceNumber">Optional PR or Issue number to label/comment on.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<GateResult> CheckGateAsync(string gateId, string context, int? resourceNumber = null, CancellationToken ct = default);

    /// <summary>
    /// Check if a gate has been approved by a human (looks for approval label/comment on the resource).
    /// </summary>
    Task<bool> IsGateApprovedAsync(string gateId, int resourceNumber, CancellationToken ct = default);

    /// <summary>
    /// Check if the master human interaction switch is enabled.
    /// When false, all gates auto-proceed.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Quick check if a specific gate requires human approval (no side effects).
    /// </summary>
    bool RequiresHuman(string gateId);
}

/// <summary>Result of a gate check.</summary>
public enum GateResult
{
    /// <summary>Gate does not require human approval or is already approved — proceed.</summary>
    Proceed,

    /// <summary>Gate requires human approval — workflow should pause until approved.</summary>
    WaitingForHuman,

    /// <summary>Gate timed out and fallback action was applied.</summary>
    TimedOutWithFallback,
}
