using AgentSquad.Core.Configuration;
using AgentSquad.Core.DevPlatform.Capabilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;

namespace AgentSquad.Core.DevPlatform.Providers.GitHub;

/// <summary>
/// Adapts Octokit repository management to <see cref="IRepositoryManagementService"/>.
/// </summary>
public sealed class GitHubRepositoryManagementAdapter : IRepositoryManagementService
{
    private readonly IGitHubClient _client;
    private readonly ILogger<GitHubRepositoryManagementAdapter> _logger;

    public GitHubRepositoryManagementAdapter(IOptions<AgentSquadConfig> config, ILogger<GitHubRepositoryManagementAdapter> logger)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

        var token = config.Value.Project?.GitHubToken ?? "";
        _client = new GitHubClient(new ProductHeaderValue("AgentSquad"))
        {
            Credentials = new Credentials(token)
        };
    }

    public async Task<RepositoryCreationResult> CreateRepositoryAsync(string name, bool isPrivate = true, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(name);

        try
        {
            var newRepo = new NewRepository(name) { Private = isPrivate };
            var repo = await _client.Repository.Create(newRepo);

            _logger.LogInformation("Created GitHub repository {RepoName} (private={IsPrivate})", name, isPrivate);
            return new RepositoryCreationResult(true, repo.HtmlUrl, null);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Failed to create GitHub repository {RepoName}", name);
            return new RepositoryCreationResult(false, null, ex.Message);
        }
    }
}
