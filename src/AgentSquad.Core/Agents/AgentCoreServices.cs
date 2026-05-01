using AgentSquad.Core.AI;
using AgentSquad.Core.Agents.Reasoning;
using AgentSquad.Core.Agents.Steps;
using AgentSquad.Core.Configuration;
using AgentSquad.Core.Messaging;
using AgentSquad.Core.Persistence;
using AgentSquad.Core.Prompts;
using Microsoft.Extensions.Options;

namespace AgentSquad.Core.Agents;

/// <summary>
/// Core services that every agent needs. Registered as a singleton and resolved
/// from DI by <c>ActivatorUtilities.CreateInstance</c> in AgentFactory.
/// </summary>
public class AgentCoreServices
{
    public AgentCoreServices(
        IMessageBus messageBus,
        ModelRegistry modelRegistry,
        IChatCompletionRunner chatRunner,
        ProjectFileManager projectFiles,
        AgentMemoryStore memoryStore,
        IGateCheckService gateCheck,
        IOptions<AgentSquadConfig> config,
        IPromptTemplateService? promptService = null,
        RoleContextProvider? roleContextProvider = null,
        SelfAssessmentService? selfAssessment = null,
        IAgentReasoningLog? reasoningLog = null,
        IAgentTaskTracker? taskTracker = null,
        AgentStateStore? stateStore = null)
    {
        MessageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        ModelRegistry = modelRegistry ?? throw new ArgumentNullException(nameof(modelRegistry));
        ChatRunner = chatRunner ?? throw new ArgumentNullException(nameof(chatRunner));
        ProjectFiles = projectFiles ?? throw new ArgumentNullException(nameof(projectFiles));
        MemoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
        GateCheck = gateCheck ?? throw new ArgumentNullException(nameof(gateCheck));
        ConfigOptions = config ?? throw new ArgumentNullException(nameof(config));
        PromptService = promptService;
        RoleContextProvider = roleContextProvider;
        SelfAssessment = selfAssessment;
        ReasoningLog = reasoningLog;
        TaskTracker = taskTracker;
        StateStore = stateStore;
    }

    // Required services — every agent needs these
    public IMessageBus MessageBus { get; }
    public ModelRegistry ModelRegistry { get; }
    public IChatCompletionRunner ChatRunner { get; }
    public ProjectFileManager ProjectFiles { get; }
    public AgentMemoryStore MemoryStore { get; }
    public IGateCheckService GateCheck { get; }
    public IOptions<AgentSquadConfig> ConfigOptions { get; }

    /// <summary>Convenience accessor — equivalent to <c>ConfigOptions.Value</c>.</summary>
    public AgentSquadConfig Config => ConfigOptions.Value;

    // Optional services — not all agents use these
    public IPromptTemplateService? PromptService { get; }
    public RoleContextProvider? RoleContextProvider { get; }
    public SelfAssessmentService? SelfAssessment { get; }
    public IAgentReasoningLog? ReasoningLog { get; }
    public IAgentTaskTracker? TaskTracker { get; }
    public AgentStateStore? StateStore { get; }
}
