namespace AgentSquad.Core.Configuration;

/// <summary>
/// Defines an MCP server that agents can use for tool calls.
/// Contains launch configuration and capability metadata.
/// </summary>
public record McpServerDefinition
{
    /// <summary>Unique name for this server (e.g., "github", "playwright", "fetch")</summary>
    public required string Name { get; init; }

    /// <summary>Human-readable description of what this server provides</summary>
    public string Description { get; init; } = "";

    /// <summary>Command to launch the server (e.g., "npx", "uvx", "docker")</summary>
    public string Command { get; init; } = "";

    /// <summary>Arguments for the launch command (e.g., ["-y", "@modelcontextprotocol/server-github"])</summary>
    public List<string> Args { get; init; } = [];

    /// <summary>Environment variables for the server process</summary>
    public Dictionary<string, string> Env { get; init; } = new();

    /// <summary>Transport type for MCP communication</summary>
    public McpTransportType Transport { get; init; } = McpTransportType.Stdio;

    /// <summary>URL for HTTP/SSE transport servers</summary>
    public string? Url { get; init; }

    /// <summary>Runtime prerequisites (e.g., ["node", "python", "docker"])</summary>
    public List<string> RequiredRuntimes { get; init; } = [];

    /// <summary>Capability keywords this server provides (e.g., ["github-issues", "github-prs"])</summary>
    public List<string> ProvidedCapabilities { get; init; } = [];
}

public enum McpTransportType
{
    Stdio,
    Http,
    Sse
}
