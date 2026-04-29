namespace AgentSquad.Core.Configuration;

/// <summary>
/// Provides the effective branch for the current run. All components that need to know
/// which branch to target (PRs, file commits, workspace checkouts) read from this provider
/// instead of hardcoding "main" or reading config.Project.DefaultBranch.
/// </summary>
public interface IRunBranchProvider
{
    /// <summary>
    /// The branch all agent work should target. Returns the working branch if one is set
    /// for the current run, otherwise returns the repository's default branch (e.g., "main").
    /// </summary>
    string EffectiveBranch { get; }
}

/// <summary>
/// Singleton implementation of <see cref="IRunBranchProvider"/>. RunCoordinator calls
/// <see cref="SetForRun"/> on every run start/recover and <see cref="Reset"/> on
/// complete/fail/cancel to prevent branch leakage across runs.
/// </summary>
public class RunBranchProvider : IRunBranchProvider
{
    private readonly string _defaultBranch;
    private volatile string? _runBranch;

    public RunBranchProvider(string defaultBranch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultBranch);
        _defaultBranch = defaultBranch;
    }

    /// <inheritdoc />
    public string EffectiveBranch => _runBranch ?? _defaultBranch;

    /// <summary>
    /// Set the target branch for the current run. Pass null to use the default branch.
    /// Called by RunCoordinator on start and recovery.
    /// </summary>
    public void SetForRun(string? targetBranch)
    {
        _runBranch = string.IsNullOrWhiteSpace(targetBranch) ? null : targetBranch;
    }

    /// <summary>
    /// Clear the run-specific branch override, reverting to the default branch.
    /// Called by RunCoordinator on complete, fail, or cancel.
    /// </summary>
    public void Reset()
    {
        _runBranch = null;
    }
}
