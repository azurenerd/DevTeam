using AgentSquad.Core.DevPlatform.Capabilities;
using AgentSquad.Core.DevPlatform.Models;
using AgentSquad.Core.GitHub;

namespace AgentSquad.Core.DevPlatform.Providers.GitHub;

/// <summary>
/// Adapts <see cref="IGitHubService"/> file operations to <see cref="IRepositoryContentService"/>.
/// </summary>
public sealed class GitHubRepositoryContentAdapter : IRepositoryContentService
{
    private readonly IGitHubService _github;

    public GitHubRepositoryContentAdapter(IGitHubService github)
    {
        ArgumentNullException.ThrowIfNull(github);
        _github = github;
    }

    public Task<string?> GetFileContentAsync(string path, string? branch = null, CancellationToken ct = default)
        => _github.GetFileContentAsync(path, branch, ct);

    public Task<byte[]?> GetFileBytesAsync(string path, string? branch = null, CancellationToken ct = default)
        => _github.GetFileBytesAsync(path, branch, ct);

    public Task CreateOrUpdateFileAsync(string path, string content, string commitMessage, string? branch = null, CancellationToken ct = default)
        => _github.CreateOrUpdateFileAsync(path, content, commitMessage, branch, ct);

    public Task DeleteFileAsync(string path, string commitMessage, string? branch = null, CancellationToken ct = default)
        => _github.DeleteFileAsync(path, commitMessage, branch, ct);

    public async Task BatchCommitFilesAsync(
        IReadOnlyList<PlatformFileCommit> files, string commitMessage,
        string branch, CancellationToken ct = default)
    {
        var tuples = files.Select(f => (f.Path, f.Content)).ToList();
        await _github.BatchCommitFilesAsync(tuples, commitMessage, branch, ct);
    }

    public Task<string?> CommitBinaryFileAsync(
        string path, byte[] content, string commitMessage,
        string branch, CancellationToken ct = default)
        => _github.CommitBinaryFileAsync(path, content, commitMessage, branch, ct);

    public async Task<IReadOnlyList<string>> GetRepositoryTreeAsync(string? branch = null, CancellationToken ct = default)
        => await _github.GetRepositoryTreeAsync(branch ?? "main", ct);

    public Task<IReadOnlyList<string>> GetRepositoryTreeForCommitAsync(string commitSha, CancellationToken ct = default)
        => _github.GetRepositoryTreeForCommitAsync(commitSha, ct);
}
