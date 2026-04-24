using AgentSquad.Core.Configuration;
using AgentSquad.Core.DevPlatform.Capabilities;
using Microsoft.Extensions.Options;

namespace AgentSquad.Core.DevPlatform.Providers.GitHub;

/// <summary>
/// GitHub URL construction patterns. Eliminates hardcoded github.com references
/// from agent code by centralizing URL patterns here.
/// </summary>
public sealed class GitHubHostContext : IPlatformHostContext
{
    private readonly string _repo;
    private readonly string _defaultBranch;

    public GitHubHostContext(IOptions<AgentSquadConfig> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _repo = config.Value.Project.GitHubRepo;
        _defaultBranch = config.Value.Project.DefaultBranch;
    }

    public string GetCloneUrl(string token)
        => $"https://x-access-token:{token}@github.com/{_repo}.git";

    public string GetPullRequestWebUrl(int prId)
        => $"https://github.com/{_repo}/pull/{prId}";

    public string GetWorkItemWebUrl(int workItemId)
        => $"https://github.com/{_repo}/issues/{workItemId}";

    public string GetRawFileUrl(string path, string branch)
        => $"https://raw.githubusercontent.com/{_repo}/{branch}/{path}";

    public string DefaultBranch => _defaultBranch;
}
