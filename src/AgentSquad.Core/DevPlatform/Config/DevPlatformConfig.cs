namespace AgentSquad.Core.DevPlatform.Config;

/// <summary>
/// Platform provider type. Determines which implementation is registered for
/// IPullRequestService, IWorkItemService, etc.
/// </summary>
public enum DevPlatformType
{
    GitHub,
    AzureDevOps
}

/// <summary>
/// Authentication method for the dev platform.
/// </summary>
public enum DevPlatformAuthMethod
{
    /// <summary>Personal Access Token (both GitHub and ADO).</summary>
    Pat,

    /// <summary>Azure CLI bearer token (ADO only). Uses az account get-access-token.</summary>
    AzureCliBearer,

    /// <summary>Service Principal with client secret (ADO only, future).</summary>
    ServicePrincipal
}

/// <summary>
/// Configuration for the dev platform provider.
/// Extends the existing ProjectConfig with platform-aware settings.
/// </summary>
public class DevPlatformConfig
{
    /// <summary>Which platform to use. Default: GitHub (backward compatible).</summary>
    public DevPlatformType Platform { get; set; } = DevPlatformType.GitHub;

    /// <summary>Authentication method. Default: PAT.</summary>
    public DevPlatformAuthMethod AuthMethod { get; set; } = DevPlatformAuthMethod.Pat;

    /// <summary>
    /// Azure DevOps organization URL (e.g., "https://dev.azure.com/myorg").
    /// Only used when Platform = AzureDevOps.
    /// </summary>
    public string AdoOrganizationUrl { get; set; } = "";

    /// <summary>
    /// Azure DevOps project name.
    /// Only used when Platform = AzureDevOps.
    /// </summary>
    public string AdoProject { get; set; } = "";

    /// <summary>
    /// Azure DevOps repository name.
    /// Only used when Platform = AzureDevOps.
    /// </summary>
    public string AdoRepository { get; set; } = "";

    /// <summary>
    /// Azure AD tenant ID for bearer token auth (e.g., "72f988bf-86f1-41af-91ab-2d7cd011db47").
    /// Only used when AuthMethod = AzureCliBearer.
    /// </summary>
    public string AzureTenantId { get; set; } = "";

    /// <summary>
    /// Default work item type for ADO (e.g., "Task", "Bug").
    /// Only used when Platform = AzureDevOps.
    /// </summary>
    public string DefaultWorkItemType { get; set; } = "Task";

    /// <summary>
    /// Work item type for executive escalation items.
    /// GitHub: Issue with "executive-request" label. ADO: User Story.
    /// </summary>
    public string ExecutiveWorkItemType { get; set; } = "User Story";

    /// <summary>
    /// Configurable work item state mappings from AgentSquad internal states to platform states.
    /// Keys are AgentSquad states: "Open", "InProgress", "Blocked", "Resolved".
    /// Values are platform-specific states.
    /// When empty, defaults are used per platform.
    /// </summary>
    public Dictionary<string, string> StateMappings { get; set; } = new();
}
