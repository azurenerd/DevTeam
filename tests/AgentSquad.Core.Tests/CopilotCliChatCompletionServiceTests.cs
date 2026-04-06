using AgentSquad.Core.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AgentSquad.Core.Tests;

public class CopilotCliChatCompletionServiceTests
{
    [Fact]
    public void FormatChatHistory_SingleUserMessage_NoLabels()
    {
        var history = new ChatHistory();
        history.AddUserMessage("Write a hello world program.");

        var prompt = CopilotCliChatCompletionService.FormatChatHistoryAsPrompt(history);

        Assert.Equal("Write a hello world program.", prompt);
    }

    [Fact]
    public void FormatChatHistory_SystemPlusUserMessage_IncludesSystemContext()
    {
        var history = new ChatHistory();
        history.AddSystemMessage("You are a senior engineer.");
        history.AddUserMessage("Write a hello world program.");

        var prompt = CopilotCliChatCompletionService.FormatChatHistoryAsPrompt(history);

        Assert.Contains("[SYSTEM CONTEXT]", prompt);
        Assert.Contains("You are a senior engineer.", prompt);
        Assert.Contains("Write a hello world program.", prompt);
    }

    [Fact]
    public void FormatChatHistory_MultiTurnConversation_FormatsAsLabeled()
    {
        var history = new ChatHistory();
        history.AddSystemMessage("You are an architect.");
        history.AddUserMessage("Design a system for X.");
        history.AddAssistantMessage("Here is my design for X...");
        history.AddUserMessage("Now add caching.");

        var prompt = CopilotCliChatCompletionService.FormatChatHistoryAsPrompt(history);

        Assert.Contains("[SYSTEM CONTEXT]", prompt);
        Assert.Contains("You are an architect.", prompt);
        Assert.Contains("[CONVERSATION HISTORY]", prompt);
        Assert.Contains("[USER]: Design a system for X.", prompt);
        Assert.Contains("[ASSISTANT]: Here is my design for X...", prompt);
        Assert.Contains("[USER]: Now add caching.", prompt);
        Assert.Contains("[INSTRUCTION]:", prompt);
    }

    [Fact]
    public void FormatChatHistory_EmptyHistory_ReturnsEmpty()
    {
        var history = new ChatHistory();
        var prompt = CopilotCliChatCompletionService.FormatChatHistoryAsPrompt(history);
        Assert.Equal(string.Empty, prompt.Trim());
    }

    [Fact]
    public void FormatChatHistory_SystemOnly_ReturnsSystemContext()
    {
        var history = new ChatHistory();
        history.AddSystemMessage("You are helpful.");

        var prompt = CopilotCliChatCompletionService.FormatChatHistoryAsPrompt(history);

        Assert.Contains("[SYSTEM CONTEXT]", prompt);
        Assert.Contains("You are helpful.", prompt);
    }

    [Fact]
    public void FormatChatHistory_MultipleSystemMessages_CombinesThem()
    {
        var history = new ChatHistory();
        history.AddSystemMessage("You are a developer.");
        history.AddSystemMessage("Follow clean code principles.");
        history.AddUserMessage("Write a function.");

        var prompt = CopilotCliChatCompletionService.FormatChatHistoryAsPrompt(history);

        Assert.Contains("You are a developer.", prompt);
        Assert.Contains("Follow clean code principles.", prompt);
    }
}
