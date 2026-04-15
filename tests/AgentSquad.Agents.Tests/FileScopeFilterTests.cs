using AgentSquad.Agents;

namespace AgentSquad.Agents.Tests;

public class FileScopeFilterTests
{
    [Fact]
    public void ExtractAllowedFiles_MarkdownFormat_ParsesCreateAndModify()
    {
        var description = """
            ## File Plan
            - ➕ **Create:** `Components/TimelineSection.razor`
            - ✏️ **Modify:** `Pages/Index.razor`
            - 📎 **Reference (do not recreate):** `wwwroot/css/site.css`
            """;

        var allowed = EngineerAgentBase.ExtractAllowedFilesFromDescription(description);

        Assert.Contains("Components/TimelineSection.razor", allowed);
        Assert.Contains("Pages/Index.razor", allowed);
        // Reference files should NOT be in allowed (they shouldn't be modified)
        Assert.DoesNotContain("wwwroot/css/site.css", allowed);
    }

    [Fact]
    public void ExtractAllowedFiles_RawFormat_ParsesCreateAndModify()
    {
        var description = "CREATE:src/Components/Header.razor\nMODIFY:src/Pages/Home.razor\nUSE:wwwroot/css/app.css";

        var allowed = EngineerAgentBase.ExtractAllowedFilesFromDescription(description);

        Assert.Contains("src/Components/Header.razor", allowed);
        Assert.Contains("src/Pages/Home.razor", allowed);
        Assert.DoesNotContain("wwwroot/css/app.css", allowed);
    }

    [Fact]
    public void ExtractAllowedFiles_EmptyDescription_ReturnsEmpty()
    {
        Assert.Empty(EngineerAgentBase.ExtractAllowedFilesFromDescription(null));
        Assert.Empty(EngineerAgentBase.ExtractAllowedFilesFromDescription(""));
        Assert.Empty(EngineerAgentBase.ExtractAllowedFilesFromDescription("No file plan here"));
    }

    [Fact]
    public void ExtractAllowedFiles_CaseInsensitive()
    {
        var description = "create:src/Foo.cs\nmodify:src/Bar.cs";
        var allowed = EngineerAgentBase.ExtractAllowedFilesFromDescription(description);

        Assert.Contains("src/Foo.cs", allowed);
        Assert.Contains("src/Bar.cs", allowed);
    }

    [Fact]
    public void ExtractAllowedFiles_NormalizesBackslashes()
    {
        var description = "- ➕ **Create:** `src\\Components\\Header.razor`";
        var allowed = EngineerAgentBase.ExtractAllowedFilesFromDescription(description);

        // Should be normalized to forward slashes
        Assert.Contains("src/Components/Header.razor", allowed);
    }
}
