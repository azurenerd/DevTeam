using AgentSquad.Core.Configuration;
using AgentSquad.Core.DevPlatform.Capabilities;
using AgentSquad.Core.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace AgentSquad.Core.Tests;

public class ProjectFileManagerPathTests
{
    private readonly Mock<IRepositoryContentService> _repoContent = new();
    private readonly ProjectFileManager _manager;

    public ProjectFileManagerPathTests()
    {
        _manager = new ProjectFileManager(
            _repoContent.Object,
            NullLogger<ProjectFileManager>.Instance,
            "main");
    }

    [Fact]
    public async Task GetPMSpecAsync_WithArtifactBasePath_ReadsScopedPath()
    {
        _manager.ArtifactBasePath = "AgentDocs/101";
        _repoContent
            .Setup(r => r.GetFileContentAsync("AgentDocs/101/PMSpec.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# PM Spec from scoped path");

        var result = await _manager.GetPMSpecAsync();

        Assert.Equal("# PM Spec from scoped path", result);
    }

    [Fact]
    public async Task GetPMSpecAsync_WithArtifactBasePath_DoesNotFallBackToRoot()
    {
        _manager.ArtifactBasePath = "AgentDocs/101";
        _repoContent
            .Setup(r => r.GetFileContentAsync("AgentDocs/101/PMSpec.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        // Root file exists but should NOT be read when scoped
        _repoContent
            .Setup(r => r.GetFileContentAsync("PMSpec.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# Legacy PM Spec");

        var result = await _manager.GetPMSpecAsync();

        // Should get placeholder, not the root file content
        Assert.Contains("No PM specification", result);
        _repoContent.Verify(
            r => r.GetFileContentAsync("PMSpec.md", "main", It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetPMSpecAsync_EmptyBasePath_ReadsFromRoot()
    {
        _manager.ArtifactBasePath = "";
        _repoContent
            .Setup(r => r.GetFileContentAsync("PMSpec.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# Root PM Spec");

        var result = await _manager.GetPMSpecAsync();

        Assert.Equal("# Root PM Spec", result);
    }

    [Fact]
    public async Task UpdatePMSpecAsync_WithArtifactBasePath_WritesToScopedPath()
    {
        _manager.ArtifactBasePath = "AgentDocs/42";

        await _manager.UpdatePMSpecAsync("# New spec");

        _repoContent.Verify(r => r.CreateOrUpdateFileAsync(
            "AgentDocs/42/PMSpec.md",
            "# New spec",
            It.IsAny<string>(),
            "main",
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetArchitectureDocAsync_WithArtifactBasePath_ReadsScopedPath()
    {
        _manager.ArtifactBasePath = "AgentDocs/101";
        _repoContent
            .Setup(r => r.GetFileContentAsync("AgentDocs/101/Architecture.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# Arch doc");

        var result = await _manager.GetArchitectureDocAsync();

        Assert.Equal("# Arch doc", result);
    }

    [Fact]
    public async Task GetResearchDocAsync_WithArtifactBasePath_DoesNotFallBackToRoot()
    {
        _manager.ArtifactBasePath = "AgentDocs/999";
        _repoContent
            .Setup(r => r.GetFileContentAsync("AgentDocs/999/Research.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _repoContent
            .Setup(r => r.GetFileContentAsync("Research.md", "main", It.IsAny<CancellationToken>()))
            .ReturnsAsync("# Root Research");

        var result = await _manager.GetResearchDocAsync();

        // Should get placeholder, not the root file content
        Assert.Contains("No research", result);
    }
}

public class ProjectWorkflowProfileTests
{
    [Fact]
    public void ArtifactBasePath_WithDocsFolderAndScope_ReturnsScopedPath()
    {
        var profile = new ProjectWorkflowProfile(
            singlePrMode: true,
            docsFolderPath: "AgentDocs",
            runScope: "101");

        Assert.Equal("AgentDocs/101", profile.ArtifactBasePath);
    }

    [Fact]
    public void ArtifactBasePath_EmptyDocsFolder_ReturnsEmpty()
    {
        var profile = new ProjectWorkflowProfile(
            singlePrMode: true,
            docsFolderPath: "",
            runScope: "101");

        Assert.Equal("", profile.ArtifactBasePath);
    }

    [Fact]
    public void ArtifactBasePath_DefaultConstructor_GeneratesGuidScope()
    {
        var profile = new ProjectWorkflowProfile();

        Assert.StartsWith("AgentDocs/", profile.ArtifactBasePath);
        Assert.Equal("AgentDocs/".Length + 8, profile.ArtifactBasePath.Length);
    }

    [Fact]
    public void GetArtifactPath_ReturnsScopedDocPath()
    {
        IWorkflowProfile profile = new ProjectWorkflowProfile(
            singlePrMode: true,
            docsFolderPath: "AgentDocs",
            runScope: "42");

        Assert.Equal("AgentDocs/42/PMSpec.md", profile.GetArtifactPath("PMSpec.md"));
    }
}
