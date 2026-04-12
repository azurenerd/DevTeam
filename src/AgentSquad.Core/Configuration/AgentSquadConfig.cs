using AgentSquad.Core.Workspace;

namespace AgentSquad.Core.Configuration;

public class AgentSquadConfig
{
    public ProjectConfig Project { get; set; } = new();
    public Dictionary<string, ModelConfig> Models { get; set; } = new();
    public AgentConfigs Agents { get; set; } = new();
    public LimitsConfig Limits { get; set; } = new();
    public DashboardConfig Dashboard { get; set; } = new();
    public CopilotCliConfig CopilotCli { get; set; } = new();
    public WorkspaceConfig Workspace { get; set; } = new();
    public HumanInteractionConfig HumanInteraction { get; set; } = new();
}

public class ProjectConfig
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string GitHubRepo { get; set; } = "";
    public string GitHubToken { get; set; } = "";
    public string DefaultBranch { get; set; } = "main";

    /// <summary>
    /// The primary tech stack for the project. Agents use this in all prompts
    /// to ensure generated code, architecture, and plans target the correct language/framework.
    /// Examples: "C# .NET 8 with Blazor Server", "TypeScript with Next.js and React", "Python with FastAPI"
    /// </summary>
    public string TechStack { get; set; } = "C# .NET 8 with Blazor Server";

    /// <summary>
    /// GitHub username of the Executive stakeholder (human) for escalation.
    /// The PM agent creates executive-request Issues assigned to this user when
    /// it needs human clarification on requirements.
    /// </summary>
    public string ExecutiveGitHubUsername { get; set; } = "azurenerd";

    /// <summary>
    /// The SHA of the baseline commit that represents the "clean" repo state.
    /// Used by the dashboard cleanup to atomically reset the repo via Git Trees API.
    /// When set, cleanup resolves this commit's tree to determine which files to preserve.
    /// When empty, cleanup falls back to the preserve-files list approach.
    /// </summary>
    public string BaselineCommitSha { get; set; } = "";

    /// <summary>
    /// Custom prompt that guides the Researcher agent on what to investigate.
    /// When empty, a comprehensive default prompt is generated from the project description.
    /// Use this to steer research toward specific areas, technologies, or concerns.
    /// </summary>
    public string ResearchPrompt { get; set; } = "";
}

public class ModelConfig
{
    public string Provider { get; set; } = "";
    public string Model { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string? Endpoint { get; set; }
    public int MaxTokensPerRequest { get; set; } = 4096;
    public double Temperature { get; set; } = 0.3;
}

public class AgentConfigs
{
    public AgentConfig ProgramManager { get; set; } = new() { ModelTier = "premium", Enabled = true };
    public AgentConfig Researcher { get; set; } = new() { ModelTier = "standard", Enabled = true };
    public AgentConfig Architect { get; set; } = new() { ModelTier = "premium", Enabled = true };
    public AgentConfig PrincipalEngineer { get; set; } = new() { ModelTier = "premium", Enabled = true };
    public AgentConfig TestEngineer { get; set; } = new() { ModelTier = "standard", Enabled = true };
    public AgentConfig SeniorEngineerTemplate { get; set; } = new() { ModelTier = "standard" };
    public AgentConfig JuniorEngineerTemplate { get; set; } = new() { ModelTier = "local" };
}

public class AgentConfig
{
    public string ModelTier { get; set; } = "standard";
    public bool Enabled { get; set; } = true;
    public int? MaxDailyTokens { get; set; }
}

public class LimitsConfig
{
    /// <summary>
    /// Per-role engineer pool configuration. Controls how many additional engineers
    /// of each type can be spawned. The original PE is always created as a core agent;
    /// the pool config only governs ADDITIONAL engineers.
    /// Default: 2 PEs, 0 SEs, 0 JEs — only additional Principal Engineers are spawned.
    /// </summary>
    public EngineerPoolConfig EngineerPool { get; set; } = new();

    /// <summary>
    /// Legacy property. Now computed from EngineerPool totals.
    /// Setting this directly is ignored if EngineerPool is explicitly configured.
    /// </summary>
    public int MaxAdditionalEngineers
    {
        get => EngineerPool.PrincipalEngineerPool + EngineerPool.SeniorEngineerPool + EngineerPool.JuniorEngineerPool;
        set { } // no-op for backward compat deserialization
    }

    public int MaxDailyTokenBudget { get; set; } = 1_000_000;
    public int GitHubPollIntervalSeconds { get; set; } = 30;
    public int AgentTimeoutMinutes { get; set; } = 60;
    public int MaxConcurrentAgents { get; set; } = 10;

    /// <summary>
    /// Maximum number of clarification round-trips between an engineer and the PM
    /// on a single Issue before the engineer proceeds with best understanding.
    /// </summary>
    public int MaxClarificationRoundTrips { get; set; } = 5;

    /// <summary>
    /// Maximum number of rework cycles (review → change → re-review) per PR before
    /// the reviewer force-approves to prevent infinite loops.
    /// This is the default fallback; prefer the phase-specific limits below.
    /// </summary>
    public int MaxReworkCycles { get; set; } = 3;

    /// <summary>
    /// Maximum Architect ↔ Engineer rework cycles per PR.
    /// Architect reviews first (Phase 1); after this limit, force-approve and proceed to TE testing.
    /// Falls back to MaxReworkCycles if not explicitly set.
    /// </summary>
    public int MaxArchitectReworkCycles { get; set; } = 3;

    /// <summary>
    /// Maximum PM ↔ Engineer rework cycles per PR.
    /// PM reviews last (Phase 3, after TE adds tests); after this limit, force-approve and merge.
    /// Falls back to MaxReworkCycles if not explicitly set.
    /// </summary>
    public int MaxPmReworkCycles { get; set; } = 3;

    /// <summary>
    /// Maximum rework cycles for Test Engineer source-bug feedback, tracked independently
    /// from peer review rework so TE feedback isn't blocked by exhausted peer review cycles.
    /// </summary>
    public int MaxTestReworkCycles { get; set; } = 2;

    /// <summary>
    /// Maximum times the Test Engineer will request source bug fixes from an engineer
    /// for a single PR before giving up and removing the failing tests.
    /// </summary>
    public int MaxSourceBugRounds { get; set; } = 2;

    /// <summary>
    /// If the Principal Engineer estimates all remaining tasks can be completed within
    /// this many minutes, it won't request additional engineers.
    /// </summary>
    public int SelfCompletionThresholdMinutes { get; set; } = 10;

    /// <summary>
    /// Minimum number of parallelizable tasks required before the Principal Engineer
    /// requests a new engineer from the PM.
    /// </summary>
    public int MinParallelizableTasksForNewEngineer { get; set; } = 3;
}

/// <summary>
/// Controls the pool of additional engineers that can be spawned to parallelize work.
/// The original PrincipalEngineer is always created as a core agent (rank 0).
/// Additional engineers are spawned on demand by the PE when parallelizable tasks exist.
/// </summary>
public class EngineerPoolConfig
{
    /// <summary>
    /// Maximum additional Principal Engineers (premium tier, full review + implementation capability).
    /// These act as worker PEs — the original PE (rank 0) remains the leader.
    /// Default: 2. Set to 0 to disable additional PE spawning.
    /// </summary>
    public int PrincipalEngineerPool { get; set; } = 2;

    /// <summary>
    /// Maximum additional Senior Engineers (standard tier, self-review capability).
    /// Default: 0. Set > 0 to allow SE spawning alongside or instead of PEs.
    /// </summary>
    public int SeniorEngineerPool { get; set; } = 0;

    /// <summary>
    /// Maximum additional Junior Engineers (budget/local tier, basic implementation).
    /// Default: 0. Set > 0 to allow JE spawning alongside or instead of PEs.
    /// </summary>
    public int JuniorEngineerPool { get; set; } = 0;
}

public class DashboardConfig
{
    public int Port { get; set; } = 5050;
    public bool EnableSignalR { get; set; } = true;
}

/// <summary>
/// Configuration for the Copilot CLI AI provider.
/// When enabled, agents use the copilot CLI as the default AI backend instead of API keys.
/// </summary>
public class CopilotCliConfig
{
    /// <summary>Whether to use Copilot CLI as the default AI provider.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Path to the copilot executable. Defaults to "copilot" (assumes it's on PATH).</summary>
    public string ExecutablePath { get; set; } = "copilot";

    /// <summary>Maximum number of concurrent copilot processes.</summary>
    public int MaxConcurrentRequests { get; set; } = 4;

    /// <summary>Timeout in seconds for a single AI request.</summary>
    public int RequestTimeoutSeconds { get; set; } = 600;

    /// <summary>Alias for RequestTimeoutSeconds (backwards compatibility with appsettings).</summary>
    public int ProcessTimeoutSeconds
    {
        get => RequestTimeoutSeconds;
        set => RequestTimeoutSeconds = value;
    }

    /// <summary>Model to request from Copilot CLI (e.g., "claude-opus-4.6").</summary>
    public string ModelName { get; set; } = "claude-opus-4.6";

    /// <summary>Alias for ModelName (backwards compatibility with appsettings).</summary>
    public string DefaultModel
    {
        get => ModelName;
        set => ModelName = value;
    }

    /// <summary>
    /// Automatically approve all interactive prompts (y/n, selections, etc.).
    /// When true, the interactive watchdog auto-responds to unexpected prompts.
    /// </summary>
    public bool AutoApprovePrompts { get; set; } = true;

    /// <summary>Reasoning effort level: "low", "medium", "high", "xhigh", or null for default.</summary>
    public string? ReasoningEffort { get; set; }

    /// <summary>Use --silent flag to suppress stats and chrome in output.</summary>
    public bool SilentMode { get; set; } = true;

    /// <summary>Use --output-format json for structured JSONL output.</summary>
    public bool JsonOutput { get; set; } = false;

    /// <summary>
    /// When true, injects a brevity constraint into every prompt so AI responses
    /// return in ~10 seconds instead of minutes. Uses a faster model (claude-haiku-4.5)
    /// and limits responses to 500 words. Useful for testing E2E flow quickly.
    /// Set to false for production-quality output.
    /// </summary>
    public bool FastMode { get; set; } = false;

    /// <summary>Model to use when FastMode is enabled. Defaults to claude-haiku-4.5.</summary>
    public string FastModeModel { get; set; } = "claude-haiku-4.5";

    /// <summary>
    /// Maximum number of automatic retries for transient errors (auth failures, timeouts).
    /// Retries use exponential backoff (5s, 15s, 30s). Set to 0 to disable retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// When true, multi-turn agents (Researcher, Architect, PM) collapse their
    /// chain-of-thought into a single comprehensive prompt instead of multiple
    /// conversational turns. Faster and cheaper but potentially less thorough.
    /// Independent of FastMode — can use premium models with single-pass for
    /// speed without sacrificing model quality.
    /// </summary>
    public bool SinglePassMode { get; set; } = false;

    /// <summary>Tools to exclude from the CLI's available tools (e.g., "shell", "write").</summary>
    public List<string> ExcludedTools { get; set; } = new();

    /// <summary>Working directory for copilot processes. Null uses the current directory.</summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>Additional arguments to pass to the copilot CLI.</summary>
    public string? AdditionalArgs { get; set; }
}

/// <summary>
/// Configuration for human-agent hybrid interaction touchpoints.
/// Each workflow gate can be set to require human approval or run fully autonomously.
/// When Enabled is false (default), all gates auto-proceed regardless of individual settings.
/// </summary>
public class HumanInteractionConfig
{
    /// <summary>
    /// Master switch for human interaction gates. When false, all gates auto-proceed
    /// and agents operate fully autonomously. Set to true to activate individual gate settings.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Per-gate configuration. Keys are gate IDs (e.g., "ProjectKickoff", "PMSpecification").
    /// Gates not explicitly configured default to RequiresHuman=false.
    /// </summary>
    public Dictionary<string, GateConfig> Gates { get; set; } = new()
    {
        [GateIds.ProjectKickoff] = new(),
        [GateIds.AgentTeamComposition] = new(),
        [GateIds.ResearchFindings] = new(),
        [GateIds.ResearchCompleteness] = new(),
        [GateIds.PMSpecification] = new(),
        [GateIds.ArchitectureDesign] = new(),
        [GateIds.EngineeringPlan] = new(),
        [GateIds.TaskAssignment] = new(),
        [GateIds.PRCodeComplete] = new(),
        [GateIds.PRReviewApproval] = new(),
        [GateIds.ReworkExhaustion] = new(),
        [GateIds.SourceBugEscalation] = new(),
        [GateIds.TestResults] = new(),
        [GateIds.TestScreenshots] = new(),
        [GateIds.FinalPRApproval] = new(),
        [GateIds.FinalReview] = new(),
        [GateIds.DeploymentDecision] = new(),
    };

    /// <summary>Check if a specific gate requires human approval (respects Enabled master switch).</summary>
    public bool RequiresHuman(string gateId)
    {
        if (!Enabled) return false;
        return Gates.TryGetValue(gateId, out var gate) && gate.RequiresHuman;
    }

    /// <summary>Apply a preset: sets all gates to the specified RequiresHuman value.</summary>
    public void ApplyPreset(HumanInteractionPreset preset)
    {
        switch (preset)
        {
            case HumanInteractionPreset.FullAuto:
                Enabled = false;
                foreach (var gate in Gates.Values) gate.RequiresHuman = false;
                break;
            case HumanInteractionPreset.Supervised:
                Enabled = true;
                foreach (var kvp in Gates)
                {
                    kvp.Value.RequiresHuman = kvp.Key is GateIds.PMSpecification
                        or GateIds.ArchitectureDesign or GateIds.EngineeringPlan
                        or GateIds.ReworkExhaustion or GateIds.FinalPRApproval
                        or GateIds.FinalReview or GateIds.DeploymentDecision;
                }
                break;
            case HumanInteractionPreset.FullControl:
                Enabled = true;
                foreach (var gate in Gates.Values) gate.RequiresHuman = true;
                break;
        }
    }
}

/// <summary>Configuration for an individual workflow gate.</summary>
public class GateConfig
{
    /// <summary>When true, the workflow pauses at this gate until a human approves.</summary>
    public bool RequiresHuman { get; set; } = false;

    /// <summary>
    /// Minutes to wait for human response before applying FallbackAction.
    /// 0 means wait indefinitely (no timeout).
    /// </summary>
    public int TimeoutMinutes { get; set; } = 0;

    /// <summary>Action when timeout expires: "auto-approve", "block", or "escalate".</summary>
    public string FallbackAction { get; set; } = "auto-approve";
}

/// <summary>Preset autonomy profiles for quick configuration.</summary>
public enum HumanInteractionPreset
{
    /// <summary>All gates auto-proceed. No human involvement.</summary>
    FullAuto,
    /// <summary>Critical gates require human approval; routine gates auto-proceed.</summary>
    Supervised,
    /// <summary>All gates require human approval.</summary>
    FullControl,
}

/// <summary>
/// Well-known gate IDs corresponding to the 17 workflow integration points
/// defined in the VisionDoc. Use these constants instead of magic strings.
/// </summary>
public static class GateIds
{
    // Phase: Initialization
    public const string ProjectKickoff = "ProjectKickoff";
    public const string AgentTeamComposition = "AgentTeamComposition";

    // Phase: Research
    public const string ResearchFindings = "ResearchFindings";
    public const string ResearchCompleteness = "ResearchCompleteness";

    // Phase: Architecture
    public const string PMSpecification = "PMSpecification";
    public const string ArchitectureDesign = "ArchitectureDesign";

    // Phase: Engineering Planning
    public const string EngineeringPlan = "EngineeringPlan";
    public const string TaskAssignment = "TaskAssignment";

    // Phase: Parallel Development
    public const string PRCodeComplete = "PRCodeComplete";
    public const string PRReviewApproval = "PRReviewApproval";
    public const string ReworkExhaustion = "ReworkExhaustion";
    public const string SourceBugEscalation = "SourceBugEscalation";

    // Phase: Testing
    public const string TestResults = "TestResults";
    public const string TestScreenshots = "TestScreenshots";
    public const string FinalPRApproval = "FinalPRApproval";

    // Phase: Review & Completion
    public const string FinalReview = "FinalReview";
    public const string DeploymentDecision = "DeploymentDecision";

    /// <summary>All gate IDs with display names, grouped by workflow phase.</summary>
    public static readonly IReadOnlyList<(string Phase, string Id, string Name, string Description)> AllGates = new[]
    {
        ("Initialization", ProjectKickoff, "Project Kickoff", "Review project description, goals, and constraints before agents begin"),
        ("Initialization", AgentTeamComposition, "Agent Team", "Review which agents to spawn and model tier assignments"),
        ("Research", ResearchFindings, "Research Findings", "Review Research.md — competitive analysis, technology landscape"),
        ("Research", ResearchCompleteness, "Research Complete", "Confirm all research threads are complete before moving on"),
        ("Architecture", PMSpecification, "PM Specification", "Review PMSpec.md — business requirements, user stories, acceptance criteria"),
        ("Architecture", ArchitectureDesign, "Architecture Design", "Review Architecture.md — system design, component diagrams, tech choices"),
        ("Engineering", EngineeringPlan, "Engineering Plan", "Review EngineeringPlan.md — task breakdown, assignments, dependencies"),
        ("Engineering", TaskAssignment, "Task Assignment", "Review PR creation and engineer assignment for each task"),
        ("Development", PRCodeComplete, "PR Code Complete", "Review individual PR when ready — code, tests, documentation"),
        ("Development", PRReviewApproval, "PR Review Result", "Review peer review result (approve/request changes)"),
        ("Development", ReworkExhaustion, "Rework Exhaustion", "Agent exhausted max rework cycles — human decides next step"),
        ("Development", SourceBugEscalation, "Source Bug Escalation", "TE found source bugs, escalating to engineer"),
        ("Testing", TestResults, "Test Results", "Review all test tier results — unit, integration, UI"),
        ("Testing", TestScreenshots, "Test Screenshots", "Review visual test artifacts for UI verification"),
        ("Testing", FinalPRApproval, "Final PR Approval", "Human is last reviewer before merge — after ALL agent reviews + tests"),
        ("Completion", FinalReview, "Final Review", "All PRs merged, all tests passing — confirm ready for completion"),
        ("Completion", DeploymentDecision, "Deployment", "Ship/no-ship decision"),
    };
}
