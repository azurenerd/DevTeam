namespace AgentSquad.Core.Strategies.Contracts;

/// <summary>
/// Frozen SignalR event contract for candidate lifecycle. Dashboard (Phase 4) builds
/// against these payload shapes; orchestrator emits them. Event names are stable.
/// </summary>
public static class StrategyEvents
{
    public const string CandidateStarted     = "candidate:started";
    public const string CandidateCompleted   = "candidate:completed";
    public const string CandidateEvaluated   = "candidate:evaluated";
    public const string CandidateScored      = "candidate:scored";
    public const string WinnerSelected       = "winner:selected";
    public const string GateStarted          = "gate:started";
    public const string GateCompleted        = "gate:completed";
    public const string CandidateDetail      = "candidate:detail";
}

public record CandidateStartedEvent(string RunId, string TaskId, string StrategyId, DateTimeOffset At);
public record CandidateCompletedEvent(string RunId, string TaskId, string StrategyId, bool Succeeded, string? FailureReason, double ElapsedSec, long? TokensUsed);
public record GateEvent(string RunId, string TaskId, string StrategyId, string GateId, bool? Passed, string? Detail);

/// <summary>
/// Emitted after build-gate evaluation for every candidate (whether it survived or not).
/// Carries the screenshot and gate result. Distinct from <see cref="CandidateScoredEvent"/>
/// which requires real LLM judge scores.
/// </summary>
public record CandidateEvaluatedEvent(
    string RunId, string TaskId, string StrategyId,
    bool Survived, string? FailedGate, string? FailureDetail,
    string? ScreenshotBase64,
    string? JudgeSkippedReason);

public record CandidateScoredEvent(string RunId, string TaskId, string StrategyId, int AcScore, int DesignScore, int ReadabilityScore, string? ScreenshotBase64 = null);
public record WinnerSelectedEvent(string RunId, string TaskId, string StrategyId, string TieBreakReason, double EvaluationElapsedSec);

/// <summary>
/// Emitted after evaluation with the full execution summary for a candidate.
/// Carries file changes parsed from the patch, diagnostic logs, metrics, and judge reasoning.
/// Separate from <see cref="CandidateEvaluatedEvent"/> to keep the lightweight event small
/// and avoid breaking existing SignalR subscribers.
/// </summary>
public record CandidateDetailEvent(
    string RunId,
    string TaskId,
    string StrategyId,
    CandidateExecutionSummary Summary);
