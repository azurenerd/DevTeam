namespace AgentSquad.Core.Strategies;

/// <summary>
/// LLM judge interface used by <see cref="CandidateEvaluator"/> to score surviving candidates.
/// Implementations MUST:
///  - return a deterministic error (not throw) on malformed LLM output
///  - truncate / sanitize patch content before submission (prompt-injection hardening)
///  - keep tokens under the configured per-call cap
/// </summary>
public interface ILlmJudge
{
    Task<JudgeResult> ScoreAsync(JudgeInput input, CancellationToken ct);
}

public record JudgeInput
{
    public required string TaskId { get; init; }
    public required string TaskTitle { get; init; }
    public required string TaskDescription { get; init; }
    /// <summary>Candidate id → patch text (already truncated).</summary>
    public required IReadOnlyDictionary<string, string> CandidatePatches { get; init; }
    public int MaxPatchChars { get; init; } = 40_000;
}

public record JudgeResult
{
    /// <summary>Per-candidate scores. Empty when the judge failed.</summary>
    public required IReadOnlyDictionary<string, CandidateScore> Scores { get; init; }
    public string? Error { get; init; }
    public long TokensUsed { get; init; }
    public bool IsFallback => Scores.Count == 0;
}

/// <summary>
/// Default no-op judge. Returns empty scores (not an error) — triggers the evaluator's
/// fall-through-to-tiebreakers path. Used when no real judge is wired.
/// </summary>
public sealed class NullLlmJudge : ILlmJudge
{
    public Task<JudgeResult> ScoreAsync(JudgeInput input, CancellationToken ct) =>
        Task.FromResult(new JudgeResult { Scores = new Dictionary<string, CandidateScore>() });
}

/// <summary>
/// Utility helpers for judge input hardening: truncate per-candidate patch
/// content and strip control characters that would otherwise leak into the prompt.
/// </summary>
public static class JudgeInputSanitizer
{
    public static string SanitizePatch(string patch, int maxChars)
    {
        if (string.IsNullOrEmpty(patch)) return "";
        var filtered = new System.Text.StringBuilder(Math.Min(patch.Length, maxChars));
        foreach (var c in patch)
        {
            if (filtered.Length >= maxChars) break;
            // Keep printable + newline/tab, drop other control chars (defense in depth).
            if (c is '\n' or '\r' or '\t' || !char.IsControl(c))
                filtered.Append(c);
        }
        if (patch.Length > maxChars)
            filtered.Append("\n[... truncated ").Append(patch.Length - maxChars).Append(" bytes ...]\n");
        return filtered.ToString();
    }
}
