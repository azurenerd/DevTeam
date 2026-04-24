using AgentSquad.Core.DevPlatform.Capabilities;
using AgentSquad.Core.DevPlatform.Models;
using AgentSquad.Core.GitHub;

namespace AgentSquad.Core.DevPlatform.Providers.GitHub;

/// <summary>
/// Provides platform metadata and rate limiting for GitHub.
/// </summary>
public sealed class GitHubPlatformInfoAdapter : IPlatformInfoService
{
    private readonly IGitHubService _github;

    public GitHubPlatformInfoAdapter(IGitHubService github)
    {
        ArgumentNullException.ThrowIfNull(github);
        _github = github;
    }

    public string PlatformName => "GitHub";
    public string RepositoryDisplayName => _github.RepositoryFullName;
    public PlatformCapabilities Capabilities => PlatformCapabilities.GitHub;

    public async Task<PlatformRateLimitInfo> GetRateLimitAsync(CancellationToken ct = default)
    {
        var info = await _github.GetRateLimitAsync(ct);
        return GitHubModelMapper.ToPlatform(info);
    }
}
