using AgentSquad.Core.Agents;
using AgentSquad.Core.Agents.Decisions;
using AgentSquad.Core.Agents.Steps;
using AgentSquad.Core.AI;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.DevPlatform.Capabilities;
using AgentSquad.Core.GitHub;
using AgentSquad.Core.Messaging;
using AgentSquad.Core.Persistence;
using AgentSquad.Core.Prompts;
using AgentSquad.Core.Workspace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AgentSquad.Agents;

/// <summary>
/// A specialist engineer agent created dynamically from an <see cref="SMEAgentDefinition"/>.
/// Unlike <see cref="SmeAgent"/> (which extends CustomAgent), this extends <see cref="EngineerAgentBase"/>
/// and has full engineering capabilities: rework loops, build/test verification, clarification handling,
/// and the complete PR lifecycle. The specialist persona is injected from the definition.
/// 
/// Registers as <see cref="AgentRole.SoftwareEngineer"/> so the leader SE sees it as a team member
/// and can assign work to it via skill-based matching on <see cref="AgentIdentity.Capabilities"/>.
/// </summary>
public class SpecialistEngineerAgent : EngineerAgentBase
{
    /// <summary>The SME definition that created this specialist.</summary>
    public SMEAgentDefinition Definition { get; }

    public SpecialistEngineerAgent(
        AgentIdentity identity,
        SMEAgentDefinition definition,
        IMessageBus messageBus,
        IssueWorkflow issueWorkflow,
        PullRequestWorkflow prWorkflow,
        ProjectFileManager projectFiles,
        ModelRegistry modelRegistry,
        AgentStateStore stateStore,
        AgentMemoryStore memoryStore,
        IOptions<AgentSquadConfig> config,
        IGateCheckService gateCheck,
        ILogger<SpecialistEngineerAgent> logger,
        IPromptTemplateService? promptService = null,
        RoleContextProvider? roleContextProvider = null,
        BuildRunner? buildRunner = null,
        TestRunner? testRunner = null,
        Core.Metrics.BuildTestMetrics? metrics = null,
        PlaywrightRunner? playwrightRunner = null,
        DecisionGateService? decisionGate = null,
        IAgentTaskTracker? taskTracker = null,
        IBranchService? branchService = null)
        : base(identity, messageBus, prWorkflow, issueWorkflow,
               projectFiles, modelRegistry, stateStore, config.Value, memoryStore, gateCheck, logger,
               promptService, roleContextProvider, buildRunner, testRunner, metrics, playwrightRunner, decisionGate, taskTracker,
               branchService: branchService)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    protected override string GetRoleDisplayName() => Definition.RoleName;

    protected override string GetImplementationSystemPrompt(string techStack)
    {
        // Try loading from specialist-engineer prompt template first
        if (PromptService is not null)
        {
            var rendered = PromptService.RenderAsync("specialist-engineer/implementation-system",
                new Dictionary<string, string>
                {
                    ["tech_stack"] = techStack,
                    ["role_name"] = Definition.RoleName,
                    ["specialist_persona"] = Definition.SystemPrompt,
                    ["capabilities"] = string.Join(", ", Definition.Capabilities)
                }).GetAwaiter().GetResult();
            if (rendered is not null) return rendered;
        }

        // Fallback: build prompt from definition
        var capabilities = Definition.Capabilities.Count > 0
            ? $"Your specialized capabilities: {string.Join(", ", Definition.Capabilities)}. "
            : "";

        return $"You are a {Definition.RoleName} — a specialist engineer on the development team. " +
            $"{Definition.SystemPrompt}\n\n" +
            $"The project uses {techStack} as its technology stack. " +
            $"{capabilities}" +
            "The PM Specification defines the business requirements, and the Architecture " +
            "document defines the technical design. The GitHub Issue contains the User Story " +
            "and acceptance criteria for this specific task. " +
            "Produce detailed, production-quality code that leverages your domain expertise. " +
            "Ensure the implementation fulfills the business goals from the PM spec.\n\n" +
            "DEPENDENCY RULE: Before using ANY external library, package, or framework, check the project's " +
            "dependency manifest (e.g., .csproj, package.json, requirements.txt, etc.). " +
            "If a dependency is not already listed, add it to the manifest and include that file in your output. " +
            "Never import/using/require a package without ensuring it is declared in the project.";
    }

    protected override string GetReworkSystemPrompt(string techStack)
    {
        if (PromptService is not null)
        {
            var rendered = PromptService.RenderAsync("specialist-engineer/rework-system",
                new Dictionary<string, string>
                {
                    ["tech_stack"] = techStack,
                    ["role_name"] = Definition.RoleName,
                    ["specialist_persona"] = Definition.SystemPrompt,
                    ["capabilities"] = string.Join(", ", Definition.Capabilities)
                }).GetAwaiter().GetResult();
            if (rendered is not null) return rendered;
        }

        return $"You are a {Definition.RoleName} addressing review feedback on your pull request. " +
            $"The project uses {techStack}. " +
            $"{Definition.SystemPrompt}\n\n" +
            "You have access to the full architecture, PM spec, and engineering plan. " +
            "Carefully read the feedback, understand what needs to be fixed, and produce " +
            "an updated implementation that addresses ALL the feedback points. " +
            "Apply your specialist expertise to ensure the fix is thorough and production-quality.";
    }
}
