using AgentSquad.Core.Agents;
using AgentSquad.Core.Configuration;
using AgentSquad.Orchestrator;
using Microsoft.Extensions.Options;

namespace AgentSquad.Runner;

public class AgentSquadWorker : BackgroundService
{
    private readonly AgentSpawnManager _spawnManager;
    private readonly AgentRegistry _registry;
    private readonly WorkflowStateMachine _workflow;
    private readonly ILogger<AgentSquadWorker> _logger;
    private readonly AgentSquadConfig _config;
    private readonly List<Task> _agentTasks = new();

    public AgentSquadWorker(
        AgentSpawnManager spawnManager,
        AgentRegistry registry,
        WorkflowStateMachine workflow,
        ILogger<AgentSquadWorker> logger,
        IOptions<AgentSquadConfig> config)
    {
        _spawnManager = spawnManager ?? throw new ArgumentNullException(nameof(spawnManager));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("AgentSquad starting...");
        _logger.LogInformation("Project: {Name}", _config.Project.Name);

        Console.WriteLine(@"
   ___                    __  ____                    __
  / _ |___ ____ ___  ____/ / / __/__ ___ _____ ___ __/ /
 / __ / _ `/ -_) _ \/ __/ / _\ \/ _ `/ // / _ `/ _  / 
/_/ |_\_, /\__/_//_/\__/_/ /___/\_, /\_,_/\_,_/\_,_/  
     /___/                        /_/                   
");
        Console.WriteLine($"  Starting AgentSquad for project: {_config.Project.Name}");
        Console.WriteLine($"  GitHub: {_config.Project.GitHubRepo}");
        Console.WriteLine($"  Max additional engineers: {_config.Limits.MaxAdditionalEngineers}");
        Console.WriteLine();

        // Spawn all core agents
        var roles = new[]
        {
            AgentRole.ProgramManager,
            AgentRole.Researcher,
            AgentRole.Architect,
            AgentRole.PrincipalEngineer,
            AgentRole.TestEngineer
        };

        foreach (var role in roles)
        {
            var identity = await _spawnManager.SpawnAgentAsync(role, ct);
            if (identity == null)
            {
                _logger.LogCritical("Failed to spawn {Role} agent", role);
                if (role == AgentRole.ProgramManager) return;
                continue;
            }
            _logger.LogInformation("{Role} agent spawned: {Name}", role, identity.DisplayName);
        }

        _logger.LogInformation("All core agents spawned. Starting agent loops...");

        // Start all agent loops as background tasks
        foreach (var agent in _registry.GetAllAgents())
        {
            var agentTask = Task.Run(async () =>
            {
                try
                {
                    await agent.StartAsync(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    // Graceful shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Agent {AgentId} ({Role}) loop crashed",
                        agent.Identity.Id, agent.Identity.Role);
                }
            }, ct);
            _agentTasks.Add(agentTask);
        }

        _logger.LogInformation("All agent loops started. PM agent will orchestrate the workflow.");

        // Keep alive until cancellation — agents run as background tasks
        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("AgentSquad shutting down, waiting for agent loops...");
            await Task.WhenAll(_agentTasks).WaitAsync(TimeSpan.FromSeconds(10));
        }
    }
}
