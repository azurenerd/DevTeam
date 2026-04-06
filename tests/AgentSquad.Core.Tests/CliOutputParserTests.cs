using AgentSquad.Core.AI;

namespace AgentSquad.Core.Tests;

public class CliOutputParserTests
{
    [Fact]
    public void StripAnsiCodes_RemovesColorCodes()
    {
        var input = "\x1B[32mHello\x1B[0m \x1B[1;34mWorld\x1B[0m";
        var result = CliOutputParser.StripAnsiCodes(input);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void StripAnsiCodes_HandlesEmptyString()
    {
        Assert.Equal(string.Empty, CliOutputParser.StripAnsiCodes(""));
        Assert.Equal(string.Empty, CliOutputParser.StripAnsiCodes(null!));
    }

    [Fact]
    public void StripAnsiCodes_PreservesPlainText()
    {
        var input = "Just plain text with no escape codes";
        Assert.Equal(input, CliOutputParser.StripAnsiCodes(input));
    }

    [Fact]
    public void RemoveCliChrome_StripsBannerLines()
    {
        var input = """
            GitHub Copilot CLI v1.0.18
            Powered by Claude Opus 4.6
            Session ID: abc-123
            Model: claude-opus-4-6

            Here is the actual AI response.
            It has multiple lines.
            """;

        var result = CliOutputParser.RemoveCliChrome(input);

        Assert.Contains("Here is the actual AI response.", result);
        Assert.Contains("It has multiple lines.", result);
        Assert.DoesNotContain("GitHub Copilot", result);
        Assert.DoesNotContain("Powered by", result);
        Assert.DoesNotContain("Session ID:", result);
        Assert.DoesNotContain("Model:", result);
    }

    [Fact]
    public void RemoveCliChrome_StripsPromptMarkers()
    {
        var input = "> user input here\nThe response text\n> another prompt";
        var result = CliOutputParser.RemoveCliChrome(input);

        Assert.Contains("The response text", result);
        Assert.DoesNotContain("> user input here", result);
    }

    [Fact]
    public void RemoveCliChrome_StripsSeparatorLines()
    {
        var input = "Response start\n────────────────\nMore content\n===============\nEnd";
        var result = CliOutputParser.RemoveCliChrome(input);

        Assert.Contains("Response start", result);
        Assert.Contains("More content", result);
        Assert.Contains("End", result);
        Assert.DoesNotContain("────", result);
        Assert.DoesNotContain("====", result);
    }

    [Fact]
    public void ResolveCarriageReturns_KeepsLastOverwrite()
    {
        // Simulates a progress bar: "Loading...\rDone!     "
        var input = "Loading...\rDone!     ";
        var result = CliOutputParser.ResolveCarriageReturns(input);

        Assert.Contains("Done!", result);
        Assert.DoesNotContain("Loading", result);
    }

    [Fact]
    public void ResolveCarriageReturns_NoOpWhenNoCarriageReturns()
    {
        var input = "Normal line 1\nNormal line 2";
        Assert.Equal(input, CliOutputParser.ResolveCarriageReturns(input));
    }

    [Fact]
    public void CollapseBlankLines_CollapsesExcessiveBlanks()
    {
        var input = "Line 1\n\n\n\n\nLine 2";
        var result = CliOutputParser.CollapseBlankLines(input);

        // 2 blank lines preserved = "Line 1\n\n\nLine 2" (3 newlines)
        // But 4+ blank lines should not survive (which would be 5+ newlines)
        var newlineRuns = result.Split("Line 1")[1].Split("Line 2")[0];
        var blankLineCount = newlineRuns.Count(c => c == '\n') - 1; // subtract the line-ending newlines
        Assert.True(blankLineCount <= 2, $"Expected at most 2 blank lines, got {blankLineCount}");
        Assert.Contains("Line 1", result);
        Assert.Contains("Line 2", result);
    }

    [Fact]
    public void Parse_FullPipeline_CleanOutput()
    {
        var rawOutput = """
            GitHub Copilot CLI v1.0
            Powered by Claude
            ────────────────────
            > my prompt

            Here is the clean response.
            It includes code:
            ```csharp
            Console.WriteLine("Hello");
            ```
            Done.
            """;

        var result = CliOutputParser.Parse(rawOutput);

        Assert.Contains("Here is the clean response.", result);
        Assert.Contains("Console.WriteLine", result);
        Assert.Contains("Done.", result);
        Assert.DoesNotContain("GitHub Copilot", result);
        Assert.DoesNotContain("Powered by", result);
        Assert.DoesNotContain("> my prompt", result);
    }

    [Fact]
    public void Parse_EmptyInput_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, CliOutputParser.Parse(""));
        Assert.Equal(string.Empty, CliOutputParser.Parse(null!));
    }

    [Fact]
    public void Parse_AnsiCodesAndChrome_Combined()
    {
        // ANSI bold + Version chrome line + actual content
        var rawOutput = "\x1B[1mSome styled text\x1B[0m\nThe actual content here";

        var result = CliOutputParser.Parse(rawOutput);

        // Should contain the non-chrome content
        Assert.Contains("The actual content here", result);
        // ANSI escape bytes should be stripped
        Assert.False(result.Any(c => c == '\x1B'), "Result should not contain ESC characters");
    }

    [Fact]
    public void ParseJsonOutput_ExtractsAssistantMessage()
    {
        var jsonl = """
            {"type":"session.tools_updated","data":{"model":"claude-opus-4.6"},"id":"1","timestamp":"2026-01-01T00:00:00Z","ephemeral":true}
            {"type":"user.message","data":{"content":"Say hello"},"id":"2","timestamp":"2026-01-01T00:00:01Z"}
            {"type":"assistant.message_delta","data":{"messageId":"m1","deltaContent":"Hello!"},"id":"3","timestamp":"2026-01-01T00:00:02Z","ephemeral":true}
            {"type":"assistant.message","data":{"messageId":"m1","content":"Hello! How can I help you?","toolRequests":[],"outputTokens":10},"id":"4","timestamp":"2026-01-01T00:00:03Z"}
            {"type":"assistant.turn_end","data":{"turnId":"0"},"id":"5","timestamp":"2026-01-01T00:00:03Z"}
            {"type":"result","timestamp":"2026-01-01T00:00:04Z","sessionId":"abc-123","exitCode":0,"usage":{"premiumRequests":1,"totalApiDurationMs":2000,"sessionDurationMs":4000}}
            """;

        var content = CliOutputParser.ParseJsonOutput(jsonl);

        Assert.NotNull(content);
        Assert.Equal("Hello! How can I help you?", content);
    }

    [Fact]
    public void ParseJsonOutput_ReturnsNullForEmptyInput()
    {
        Assert.Null(CliOutputParser.ParseJsonOutput(""));
        Assert.Null(CliOutputParser.ParseJsonOutput(null!));
        Assert.Null(CliOutputParser.ParseJsonOutput("   "));
    }

    [Fact]
    public void ParseJsonOutput_HandlesNoAssistantMessage()
    {
        var jsonl = """
            {"type":"session.tools_updated","data":{},"id":"1","timestamp":"2026-01-01T00:00:00Z"}
            {"type":"result","timestamp":"2026-01-01T00:00:01Z","exitCode":1}
            """;

        Assert.Null(CliOutputParser.ParseJsonOutput(jsonl));
    }

    [Fact]
    public void ParseJsonOutput_SkipsMalformedLines()
    {
        var jsonl = """
            not-valid-json
            {"type":"assistant.message","data":{"content":"Valid response"},"id":"1","timestamp":"2026-01-01T00:00:00Z"}
            also { not } valid
            """;

        var content = CliOutputParser.ParseJsonOutput(jsonl);
        Assert.Equal("Valid response", content);
    }

    [Fact]
    public void ParseJsonUsage_ExtractsResultStats()
    {
        var jsonl = """
            {"type":"assistant.message","data":{"content":"Hello"},"id":"1","timestamp":"2026-01-01T00:00:00Z"}
            {"type":"result","timestamp":"2026-01-01T00:00:01Z","sessionId":"sess-456","exitCode":0,"usage":{"premiumRequests":3,"totalApiDurationMs":4550,"sessionDurationMs":10111}}
            """;

        var usage = CliOutputParser.ParseJsonUsage(jsonl);

        Assert.NotNull(usage);
        Assert.Equal("sess-456", usage!.SessionId);
        Assert.Equal(0, usage.ExitCode);
        Assert.Equal(3, usage.PremiumRequests);
        Assert.Equal(4550, usage.TotalApiDurationMs);
        Assert.Equal(10111, usage.SessionDurationMs);
    }

    [Fact]
    public void ParseJsonUsage_ReturnsNullWhenNoResultEvent()
    {
        var jsonl = """{"type":"assistant.message","data":{"content":"Hello"},"id":"1","timestamp":"2026-01-01T00:00:00Z"}""";
        Assert.Null(CliOutputParser.ParseJsonUsage(jsonl));
    }

    [Fact]
    public void ParseJsonOutput_TrimsWhitespace()
    {
        var jsonl = """{"type":"assistant.message","data":{"content":"\n\nHello world!\n"},"id":"1","timestamp":"2026-01-01T00:00:00Z"}""";
        var content = CliOutputParser.ParseJsonOutput(jsonl);
        Assert.Equal("Hello world!", content);
    }
}
