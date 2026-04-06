namespace AgentSquad.Core.Configuration;

using AgentSquad.Core.AI;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Maps model tier names (premium, standard, budget, local) to configured Kernel instances.
/// When Copilot CLI is enabled, uses it as the default provider with API-key fallback.
/// </summary>
public class ModelRegistry
{
    private readonly Dictionary<string, ModelConfig> _modelConfigs;
    private readonly Dictionary<string, Kernel> _kernelCache = new();
    private readonly ILoggerFactory _loggerFactory;
    private readonly CopilotCliConfig _cliConfig;
    private readonly CopilotCliProcessManager? _processManager;
    private readonly HashSet<string> _cliFallbackTiers = new();

    public ModelRegistry(
        AgentSquadConfig config,
        ILoggerFactory loggerFactory,
        CopilotCliProcessManager? processManager = null)
    {
        _modelConfigs = config.Models;
        _loggerFactory = loggerFactory;
        _cliConfig = config.CopilotCli;
        _processManager = processManager;
    }

    /// <summary>Fired when a tier falls back from Copilot CLI to API-key provider.</summary>
    public event EventHandler<FallbackTriggeredEventArgs>? FallbackTriggered;

    /// <summary>Get or create a Kernel instance for the given model tier.</summary>
    public Kernel GetKernel(string modelTier)
    {
        if (_kernelCache.TryGetValue(modelTier, out var cached))
            return cached;

        Kernel? kernel = null;

        // Try Copilot CLI first if enabled and available (and not already fallen back for this tier)
        if (_cliConfig.Enabled && _processManager?.IsAvailable == true && !_cliFallbackTiers.Contains(modelTier))
        {
            kernel = BuildCopilotCliKernel(modelTier);
        }

        // Fall back to API-key provider
        kernel ??= BuildApiKeyKernel(modelTier);

        _kernelCache[modelTier] = kernel;
        return kernel;
    }

    /// <summary>Get the ModelConfig for a tier (for reading settings like temperature).</summary>
    public ModelConfig? GetModelConfig(string modelTier)
    {
        return _modelConfigs.TryGetValue(modelTier, out var config) ? config : null;
    }

    /// <summary>List available model tiers.</summary>
    public IReadOnlyList<string> GetAvailableTiers() => _modelConfigs.Keys.ToList().AsReadOnly();

    /// <summary>
    /// Mark a tier as needing API-key fallback. Called when Copilot CLI fails at runtime.
    /// Clears the kernel cache for that tier so the next call rebuilds with API keys.
    /// </summary>
    public void TriggerFallback(string modelTier, string reason)
    {
        _cliFallbackTiers.Add(modelTier);
        _kernelCache.Remove(modelTier);

        var logger = _loggerFactory.CreateLogger<ModelRegistry>();
        logger.LogWarning("Copilot CLI fallback triggered for tier '{Tier}': {Reason}", modelTier, reason);

        FallbackTriggered?.Invoke(this, new FallbackTriggeredEventArgs
        {
            ModelTier = modelTier,
            Reason = reason
        });
    }

    private Kernel BuildCopilotCliKernel(string modelTier)
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_loggerFactory);

        var cliService = new CopilotCliChatCompletionService(
            _processManager!,
            _cliConfig,
            _loggerFactory.CreateLogger<CopilotCliChatCompletionService>());

        builder.Services.AddKeyedSingleton<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>(
            _cliConfig.ModelName, cliService);

        var logger = _loggerFactory.CreateLogger<ModelRegistry>();
        logger.LogInformation("Tier '{Tier}' using Copilot CLI provider (model: {Model})",
            modelTier, _cliConfig.ModelName);

        return builder.Build();
    }

    private Kernel BuildApiKeyKernel(string modelTier)
    {
        if (!_modelConfigs.TryGetValue(modelTier, out var config))
            throw new ArgumentException(
                $"Unknown model tier: {modelTier}. Available: {string.Join(", ", _modelConfigs.Keys)}");

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_loggerFactory);

        switch (config.Provider.ToLowerInvariant())
        {
            case "openai":
                builder.AddOpenAIChatCompletion(config.Model, config.ApiKey);
                break;

            case "azure-openai":
            case "azureopenai":
                if (string.IsNullOrEmpty(config.Endpoint))
                    throw new ArgumentException("Azure OpenAI requires an Endpoint.");
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: config.Model,
                    endpoint: config.Endpoint,
                    apiKey: config.ApiKey);
                break;

            case "anthropic":
                builder.AddOpenAIChatCompletion(
                    modelId: config.Model,
                    apiKey: config.ApiKey,
                    endpoint: new Uri(config.Endpoint ?? "https://api.anthropic.com/v1"));
                break;

            case "ollama":
                builder.AddOpenAIChatCompletion(
                    modelId: config.Model,
                    apiKey: "ollama",
                    endpoint: new Uri(config.Endpoint ?? "http://localhost:11434/v1"));
                break;

            default:
                throw new ArgumentException($"Unknown provider: {config.Provider}");
        }

        return builder.Build();
    }
}

public class FallbackTriggeredEventArgs : EventArgs
{
    public required string ModelTier { get; init; }
    public required string Reason { get; init; }
}
