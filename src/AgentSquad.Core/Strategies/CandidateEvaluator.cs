using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AgentSquad.Core.Configuration;

namespace AgentSquad.Core.Strategies;

/// <summary>
/// Runs hard gates against a candidate patch inside a scratch worktree and chooses a
/// winner. LLM scoring is delegated to an optional <see cref="ILlmJudge"/> (null-safe).
/// Hard gates:
///  - Gate1 OutputProduced: non-empty patch.
///  - Gate2 Build: apply patch to scratch worktree, then `dotnet build` success.
///    (Also rejects patches that touch the reserved evaluator path or escape the worktree.)
///  - Gate3 AppStarts: stub (returns pass when not a web task).
///  - Gate4 EvaluatorTests: stub (returns pass when no evaluator suite configured).
/// Tiebreakers on scoring ties: fewer tokens, then faster time, then stable alphabetical id.
/// </summary>
public class CandidateEvaluator
{
    private readonly ILogger<CandidateEvaluator> _logger;
    private readonly GitWorktreeManager _worktree;
    private readonly IOptionsMonitor<StrategyFrameworkConfig> _cfg;
    private readonly ILlmJudge? _judge;

    public CandidateEvaluator(
        ILogger<CandidateEvaluator> logger,
        GitWorktreeManager worktree,
        IOptionsMonitor<StrategyFrameworkConfig> cfg,
        ILlmJudge? judge = null)
    {
        _logger = logger;
        _worktree = worktree;
        _cfg = cfg;
        _judge = judge;
    }

    public async Task<EvaluationResult> EvaluateAsync(
        TaskContext task,
        IReadOnlyList<(StrategyExecutionResult exec, string patch)> strategyOutputs,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var results = new List<CandidateResult>(strategyOutputs.Count);
        var cfg = _cfg.CurrentValue;

        foreach (var (exec, patch) in strategyOutputs)
        {
            var res = await RunGatesAsync(task, exec, patch, cfg, ct);
            results.Add(res);
        }

        var survivors = results.Where(r => r.Survived).ToList();
        CandidateResult? winner = null;
        string? tieBreak = null;

        if (survivors.Count == 0)
        {
            // No winner — evaluator caller will fall through to baseline re-run or blocker.
        }
        else if (survivors.Count == 1)
        {
            winner = survivors[0];
            tieBreak = "sole-survivor";
        }
        else if (_judge is not null)
        {
            // Batch-score survivors, then rank by AC -> Design -> Readability -> tokens -> time -> id.
            var ecfg = _cfg.CurrentValue;
            var sanitized = survivors.ToDictionary(
                c => c.StrategyId,
                c => JudgeInputSanitizer.SanitizePatch(c.Patch, ecfg.Evaluator.MaxJudgePatchChars));
            var judgeResult = await _judge.ScoreAsync(new JudgeInput
            {
                TaskId = task.TaskId,
                TaskTitle = task.TaskTitle,
                TaskDescription = task.TaskDescription,
                CandidatePatches = sanitized,
                MaxPatchChars = ecfg.Evaluator.MaxJudgePatchChars,
            }, ct);

            var scored = survivors.Select(c => judgeResult.Scores.TryGetValue(c.StrategyId, out var s)
                ? c with { Score = s }
                : c).ToList();

            var ordered = scored
                .OrderByDescending(c => c.Score?.AcceptanceCriteriaScore ?? -1)
                .ThenByDescending(c => c.Score?.DesignScore ?? -1)
                .ThenByDescending(c => c.Score?.ReadabilityScore ?? -1)
                .ThenBy(c => c.Execution.TokensUsed)
                .ThenBy(c => c.Execution.Elapsed)
                .ThenBy(c => c.StrategyId, StringComparer.Ordinal)
                .ToList();
            winner = ordered[0];
            tieBreak = judgeResult.IsFallback ? "judge-fallback-tokens-time" : "llm-rank";
            // Replace survivors in results with their scored versions for the final record.
            for (int i = 0; i < results.Count; i++)
            {
                var replacement = ordered.FirstOrDefault(s => s.StrategyId == results[i].StrategyId);
                if (replacement is not null) results[i] = replacement;
            }
        }
        else
        {
            // No judge configured -> deterministic tiebreak only.
            var ordered = survivors
                .OrderBy(c => c.Execution.TokensUsed)
                .ThenBy(c => c.Execution.Elapsed)
                .ThenBy(c => c.StrategyId, StringComparer.Ordinal)
                .ToList();
            winner = ordered[0];
            tieBreak = "no-judge-tokens-then-time";
        }

        sw.Stop();
        return new EvaluationResult
        {
            Candidates = results,
            Winner = winner,
            TieBreakReason = tieBreak,
            EvaluationElapsed = sw.Elapsed,
        };
    }

    private async Task<CandidateResult> RunGatesAsync(
        TaskContext task, StrategyExecutionResult exec, string patch, StrategyFrameworkConfig cfg, CancellationToken ct)
    {
        // Gate1: OutputProduced
        if (!exec.Succeeded)
        {
            return Fail(exec, patch, "strategy-failed", exec.FailureReason);
        }
        if (string.IsNullOrWhiteSpace(patch))
        {
            return Fail(exec, patch, "gate1-output", "empty patch");
        }

        // Path safety / reserved-path / .git guard.
        var pathIssue = GitWorktreeManager.ValidatePatchPaths(patch, cfg.Evaluator.ReservedPathPrefix);
        if (pathIssue is not null)
        {
            return Fail(exec, patch, "gate2-build", $"rejected-path: {pathIssue}");
        }

        // Gate2: Build — apply to a scratch worktree and run dotnet build.
        await using var scratch = await _worktree.CreateAsync(
            task.AgentRepoPath, cfg.CandidateDirectoryName + "-eval",
            task.TaskId, exec.StrategyId + "-eval", task.BaseSha, ct);

        var applyResult = await ApplyPatchAsync(scratch.Path, patch, ct);
        if (!applyResult.ok)
        {
            return Fail(exec, patch, "gate2-build", $"apply-failed: {applyResult.detail}");
        }

        var buildOk = await RunBuildAsync(scratch.Path, TimeSpan.FromSeconds(cfg.Timeouts.BuildGateSeconds), ct);
        if (!buildOk.ok)
        {
            return Fail(exec, patch, "gate2-build", buildOk.detail);
        }

        // Gate3 / Gate4 are stubs in Phase 1 — they pass unless integrators wire them.
        // (The dashboard still sees the gate:started/completed events.)

        return new CandidateResult
        {
            StrategyId = exec.StrategyId,
            Survived = true,
            Patch = patch,
            PatchSizeBytes = System.Text.Encoding.UTF8.GetByteCount(patch),
            Execution = exec,
        };
    }

    private static CandidateResult Fail(StrategyExecutionResult exec, string patch, string gate, string? detail)
        => new()
        {
            StrategyId = exec.StrategyId,
            Survived = false,
            FailedGate = gate,
            FailureDetail = detail,
            Patch = patch ?? "",
            PatchSizeBytes = string.IsNullOrEmpty(patch) ? 0 : System.Text.Encoding.UTF8.GetByteCount(patch),
            Execution = exec,
        };

    private static async Task<(bool ok, string detail)> ApplyPatchAsync(string worktreePath, string patch, CancellationToken ct)
    {
        // `git apply --check` first, then the real apply. --3way allows fuzz against renamed lines.
        var patchFile = Path.Combine(Path.GetTempPath(), $"strat-{Guid.NewGuid():N}.patch");
        try
        {
            await File.WriteAllTextAsync(patchFile, patch, ct);
            var check = await RunProcAsync("git", new[] { "apply", "--check", "--3way", patchFile }, worktreePath, ct);
            if (check.exit != 0) return (false, check.stderr.Trim());
            var apply = await RunProcAsync("git", new[] { "apply", "--3way", patchFile }, worktreePath, ct);
            if (apply.exit != 0) return (false, apply.stderr.Trim());
            return (true, "");
        }
        finally
        {
            try { if (File.Exists(patchFile)) File.Delete(patchFile); } catch { /* best effort */ }
        }
    }

    private async Task<(bool ok, string detail)> RunBuildAsync(string worktreePath, TimeSpan timeout, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);
        try
        {
            // Only run build if there's a solution or csproj in the worktree. Non-.NET projects skip Gate2.
            var hasDotnet = Directory.EnumerateFiles(worktreePath, "*.sln", SearchOption.TopDirectoryOnly).Any()
                || Directory.EnumerateFiles(worktreePath, "*.csproj", SearchOption.AllDirectories).Any();
            if (!hasDotnet)
            {
                _logger.LogInformation("Gate2 build skipped (no sln/csproj) for {Path}", worktreePath);
                return (true, "skipped-no-dotnet");
            }

            var res = await RunProcAsync("dotnet", new[] { "build", "--nologo", "-v", "q" }, worktreePath, cts.Token);
            if (res.exit != 0) return (false, "dotnet build failed: " + Truncate(res.stderr + res.stdout, 800));
            return (true, "");
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !ct.IsCancellationRequested)
        {
            return (false, $"build timeout after {timeout.TotalSeconds}s");
        }
    }

    private static async Task<(int exit, string stdout, string stderr)> RunProcAsync(
        string exe, string[] args, string cwd, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(exe)
        {
            WorkingDirectory = cwd,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"{exe} start failed");
        var so = p.StandardOutput.ReadToEndAsync(ct);
        var se = p.StandardError.ReadToEndAsync(ct);
        await p.WaitForExitAsync(ct);
        return (p.ExitCode, await so, await se);
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…[truncated]";
}
