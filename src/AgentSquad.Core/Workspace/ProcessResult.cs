namespace AgentSquad.Core.Workspace;

/// <summary>
/// Result of running an external process (git, build, test).
/// </summary>
public record ProcessResult
{
    public required int ExitCode { get; init; }
    public required string StandardOutput { get; init; }
    public required string StandardError { get; init; }
    public required TimeSpan Duration { get; init; }
    public bool Success => ExitCode == 0;
}

/// <summary>
/// Result of a build operation with parsed error details.
/// </summary>
public record BuildResult
{
    public required bool Success { get; init; }
    public required string Output { get; init; }
    public required string Errors { get; init; }
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Individual build errors extracted from compiler output.
    /// Each entry is a single error message (e.g., "error CS1002: ; expected at File.cs:42").
    /// </summary>
    public IReadOnlyList<string> ParsedErrors { get; init; } = [];
}

/// <summary>
/// Result of a test execution with parsed pass/fail/skip counts.
/// </summary>
public record TestResult
{
    public required bool Success { get; init; }
    public required string Output { get; init; }
    public required int Passed { get; init; }
    public required int Failed { get; init; }
    public required int Skipped { get; init; }
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Details of individual test failures for AI feedback.
    /// Each entry describes one failing test with the error message.
    /// </summary>
    public IReadOnlyList<string> FailureDetails { get; init; } = [];

    public int Total => Passed + Failed + Skipped;
}
