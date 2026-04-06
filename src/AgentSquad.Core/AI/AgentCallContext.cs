namespace AgentSquad.Core.AI;

/// <summary>
/// Ambient context that flows the current agent ID through async call chains.
/// Set at the agent loop entry point; read by CopilotCliChatCompletionService
/// to attribute AI usage to the correct agent without changing every call site.
/// </summary>
public static class AgentCallContext
{
    private static readonly AsyncLocal<string?> _currentAgentId = new();
    private static readonly AsyncLocal<string?> _currentModel = new();

    /// <summary>The agent ID currently executing in this async context.</summary>
    public static string? CurrentAgentId
    {
        get => _currentAgentId.Value;
        set => _currentAgentId.Value = value;
    }

    /// <summary>The model name the current agent is using.</summary>
    public static string? CurrentModel
    {
        get => _currentModel.Value;
        set => _currentModel.Value = value;
    }
}
