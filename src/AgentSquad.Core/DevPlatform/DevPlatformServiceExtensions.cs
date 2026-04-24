using AgentSquad.Core.DevPlatform.Capabilities;
using AgentSquad.Core.DevPlatform.Config;
using AgentSquad.Core.DevPlatform.Providers.GitHub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AgentSquad.Core.DevPlatform;

/// <summary>
/// DI registration for the dev platform abstraction layer.
/// Registers capability interfaces based on the configured platform.
/// </summary>
public static class DevPlatformServiceExtensions
{
    /// <summary>
    /// Register all platform capability interfaces.
    /// For GitHub (default): wraps the existing IGitHubService via adapters.
    /// For AzureDevOps: registers ADO REST API implementations (future).
    /// </summary>
    public static IServiceCollection AddDevPlatform(this IServiceCollection services)
    {
        // Register adapters based on configured platform.
        // We use factory registrations so the platform can be determined at runtime from config.
        services.AddSingleton<IPullRequestService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubPullRequestAdapter>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps pull request support is not yet implemented. " +
                    "Set Platform to 'GitHub' in configuration."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        services.AddSingleton<IWorkItemService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubWorkItemAdapter>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps work item support is not yet implemented."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        services.AddSingleton<IRepositoryContentService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubRepositoryContentAdapter>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps repository content support is not yet implemented."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        services.AddSingleton<IBranchService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubBranchAdapter>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps branch support is not yet implemented."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        services.AddSingleton<IReviewService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubReviewAdapter>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps review support is not yet implemented."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        services.AddSingleton<IPlatformInfoService>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubPlatformInfoAdapter>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps platform info support is not yet implemented."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        services.AddSingleton<IPlatformHostContext>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<DevPlatformConfig>>().Value;
            return config.Platform switch
            {
                DevPlatformType.GitHub => ActivatorUtilities.CreateInstance<GitHubHostContext>(sp),
                DevPlatformType.AzureDevOps => throw new NotSupportedException(
                    "Azure DevOps host context is not yet implemented."),
                _ => throw new ArgumentOutOfRangeException(nameof(config.Platform))
            };
        });

        return services;
    }
}
