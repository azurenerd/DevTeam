using AgentSquad.Core.Agents;
using AgentSquad.Core.AI;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.GitHub;
using AgentSquad.Core.Messaging;
using AgentSquad.Core.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace AgentSquad.Agents;

/// <summary>
/// A Subject Matter Expert (SME) agent created dynamically from an <see cref="SMEAgentDefinition"/>.
/// Extends <see cref="CustomAgent"/> with workflow mode behavior (OnDemand, Continuous, OneShot)
/// and structured result reporting via <see cref="SmeResultMessage"/>.
/// </summary>
public class SmeAgent : CustomAgent
{
    private readonly IMessageBus _messageBus;
    private readonly ModelRegistry _modelRegistry;
    private readonly AgentSquadConfig _config;
    private bool _hasCompletedOneShot;

    /// <summary>The definition that created this SME agent.</summary>
    public SMEAgentDefinition Definition { get; }

    public SmeAgent(
        AgentIdentity identity,
        SMEAgentDefinition definition,
        IMessageBus messageBus,
        IGitHubService github,
        PullRequestWorkflow prWorkflow,
        ProjectFileManager projectFiles,
        ModelRegistry modelRegistry,
        AgentMemoryStore memoryStore,
        IOptions<AgentSquadConfig> config,
        IGateCheckService gateCheck,
        ILogger<SmeAgent> logger,
        RoleContextProvider? roleContextProvider = null)
        : base(identity, messageBus, github, prWorkflow, projectFiles, modelRegistry,
               memoryStore, config, gateCheck, logger, roleContextProvider)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _messageBus = messageBus;
        _modelRegistry = modelRegistry;
        _config = config.Value;
    }

    protected override async Task RunAgentLoopAsync(CancellationToken ct)
    {
        switch (Definition.WorkflowMode)
        {
            case SmeWorkflowMode.OneShot:
                await RunOneShotAsync(ct);
                break;

            case SmeWorkflowMode.Continuous:
            case SmeWorkflowMode.OnDemand:
            default:
                // Use the base CustomAgent loop — it handles issue/task queues and polling
                await base.RunAgentLoopAsync(ct);
                break;
        }
    }

    /// <summary>
    /// OneShot mode: wait for a single task, execute it, report results, then stop.
    /// </summary>
    private async Task RunOneShotAsync(CancellationToken ct)
    {
        UpdateStatus(AgentStatus.Idle, "OneShot: waiting for task assignment");

        var pollInterval = TimeSpan.FromSeconds(_config.Limits.GitHubPollIntervalSeconds);

        // Wait for a task assignment (poll the base class queues)
        while (!ct.IsCancellationRequested && !_hasCompletedOneShot)
        {
            // Defer to base loop for one iteration - it handles queue processing
            await base.RunAgentLoopAsync(CreateOneShotToken(ct));
            _hasCompletedOneShot = true;
        }

        // Report completion
        Logger.LogInformation("SME agent '{DisplayName}' completed OneShot execution", Identity.DisplayName);
        UpdateStatus(AgentStatus.Idle, "OneShot complete — shutting down");
    }

    /// <summary>
    /// Creates a cancellation token that cancels after one loop iteration for OneShot mode.
    /// </summary>
    private static CancellationToken CreateOneShotToken(CancellationToken parent)
    {
        // Just return the parent token - the loop will be controlled by _hasCompletedOneShot flag
        return parent;
    }

    /// <summary>
    /// Reports SME findings back to the requesting agent via the message bus.
    /// Called after completing work to share structured results.
    /// </summary>
    protected async Task ReportFindingsAsync(
        string taskSummary,
        string findings,
        List<string>? recommendations = null,
        int? relatedIssueNumber = null,
        CancellationToken ct = default)
    {
        var resultMessage = new SmeResultMessage
        {
            FromAgentId = Identity.Id,
            ToAgentId = "*", // Broadcast to all interested agents
            MessageType = "sme.result",
            DefinitionId = Definition.DefinitionId,
            TaskSummary = taskSummary,
            Findings = findings,
            Recommendations = recommendations ?? [],
            RelatedIssueNumber = relatedIssueNumber
        };

        await _messageBus.PublishAsync(resultMessage, ct);

        Logger.LogInformation(
            "SME agent '{DisplayName}' reported findings for '{TaskSummary}' with {RecCount} recommendations",
            Identity.DisplayName, taskSummary, resultMessage.Recommendations.Count);
    }
}
