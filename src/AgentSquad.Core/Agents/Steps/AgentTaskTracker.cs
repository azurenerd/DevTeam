using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AgentSquad.Core.Agents.Steps;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IAgentTaskTracker"/>.
/// Stores steps per agent with bounded capacity and fires change notifications
/// for real-time dashboard updates via SignalR.
/// </summary>
public class AgentTaskTracker : IAgentTaskTracker
{
    private readonly ConcurrentDictionary<string, List<AgentTaskStep>> _agentSteps = new();
    private readonly ConcurrentDictionary<string, AgentTaskStep> _stepsById = new();
    private readonly ILogger<AgentTaskTracker> _logger;
    private int _stepCounter;

    private const int MaxStepsPerAgent = 100;

    public event Action<AgentTaskStep>? OnStepChanged;

    public AgentTaskTracker(ILogger<AgentTaskTracker> logger)
    {
        _logger = logger;
    }

    public string BeginStep(string agentId, string taskId, string stepName, string? description = null, string? modelTier = null)
    {
        ArgumentNullException.ThrowIfNull(agentId);
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(stepName);

        var list = _agentSteps.GetOrAdd(agentId, _ => new List<AgentTaskStep>());
        int index;
        lock (list)
        {
            index = list.Count;
        }

        var stepId = $"{agentId}-step-{Interlocked.Increment(ref _stepCounter)}";
        var step = new AgentTaskStep
        {
            Id = stepId,
            AgentId = agentId,
            TaskId = taskId,
            StepIndex = index,
            Name = stepName,
            Description = description,
            Status = AgentTaskStepStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            ModelUsed = modelTier
        };

        _stepsById[stepId] = step;

        lock (list)
        {
            list.Add(step);
            if (list.Count > MaxStepsPerAgent)
                list.RemoveRange(0, list.Count - MaxStepsPerAgent);
        }

        _logger.LogDebug("[{AgentId}] Step started: {StepName} ({StepId})", agentId, stepName, stepId);
        NotifyChanged(step);
        return stepId;
    }

    public void RecordSubStep(string stepId, string description, TimeSpan? duration = null, decimal cost = 0)
    {
        if (!_stepsById.TryGetValue(stepId, out var step)) return;

        var subStep = new AgentTaskSubStep
        {
            TurnIndex = step.SubSteps.Count,
            Description = description,
            Duration = duration,
            EstimatedCost = cost
        };

        lock (step.SubSteps)
        {
            step.SubSteps.Add(subStep);
        }

        step.EstimatedCost += cost;
        NotifyChanged(step);
    }

    public void CompleteStep(string stepId, AgentTaskStepStatus status = AgentTaskStepStatus.Completed)
    {
        if (!_stepsById.TryGetValue(stepId, out var step)) return;

        step.Status = status;
        step.CompletedAt = DateTime.UtcNow;
        _logger.LogDebug("[{AgentId}] Step completed: {StepName} ({Status})", step.AgentId, step.Name, status);
        NotifyChanged(step);
    }

    public void FailStep(string stepId, string reason)
    {
        if (!_stepsById.TryGetValue(stepId, out var step)) return;

        step.Status = AgentTaskStepStatus.Failed;
        step.CompletedAt = DateTime.UtcNow;
        step.Description = reason;
        _logger.LogWarning("[{AgentId}] Step failed: {StepName} — {Reason}", step.AgentId, step.Name, reason);
        NotifyChanged(step);
    }

    public void SetStepWaiting(string stepId)
    {
        if (!_stepsById.TryGetValue(stepId, out var step)) return;

        step.Status = AgentTaskStepStatus.WaitingOnHuman;
        _logger.LogDebug("[{AgentId}] Step waiting on human: {StepName}", step.AgentId, step.Name);
        NotifyChanged(step);
    }

    public void RecordLlmCall(string stepId, decimal cost = 0)
    {
        if (!_stepsById.TryGetValue(stepId, out var step)) return;

        Interlocked.Increment(ref step._llmCallCount);
        if (cost > 0)
            step.EstimatedCost += cost;

        NotifyChanged(step);
    }

    public IReadOnlyList<AgentTaskStep> GetSteps(string agentId)
    {
        if (!_agentSteps.TryGetValue(agentId, out var list))
            return Array.Empty<AgentTaskStep>();

        lock (list)
        {
            return list.ToList();
        }
    }

    public IReadOnlyList<AgentTaskStep> GetActiveSteps()
    {
        var active = new List<AgentTaskStep>();
        foreach (var kvp in _agentSteps)
        {
            lock (kvp.Value)
            {
                active.AddRange(kvp.Value.Where(s => s.Status == AgentTaskStepStatus.InProgress));
            }
        }
        return active;
    }

    public AgentTaskStep? GetCurrentStep(string agentId)
    {
        if (!_agentSteps.TryGetValue(agentId, out var list))
            return null;

        lock (list)
        {
            return list.LastOrDefault(s => s.Status == AgentTaskStepStatus.InProgress);
        }
    }

    public IReadOnlyList<AgentTaskStep> GetTaskSteps(string agentId, string taskId)
    {
        if (!_agentSteps.TryGetValue(agentId, out var list))
            return Array.Empty<AgentTaskStep>();

        lock (list)
        {
            return list.Where(s => s.TaskId == taskId).ToList();
        }
    }

    public IReadOnlyList<AgentTaskGroup> GetGroupedSteps(string agentId)
    {
        if (!_agentSteps.TryGetValue(agentId, out var list))
            return Array.Empty<AgentTaskGroup>();

        List<AgentTaskStep> snapshot;
        lock (list)
        {
            snapshot = list.ToList();
        }

        // Group by TaskId, preserving insertion order (first step in each group determines order)
        var groups = new List<AgentTaskGroup>();
        var seen = new Dictionary<string, List<AgentTaskStep>>();
        var order = new List<string>();

        foreach (var step in snapshot)
        {
            if (!seen.TryGetValue(step.TaskId, out var taskSteps))
            {
                taskSteps = new List<AgentTaskStep>();
                seen[step.TaskId] = taskSteps;
                order.Add(step.TaskId);
            }
            taskSteps.Add(step);
        }

        foreach (var taskId in order)
        {
            groups.Add(new AgentTaskGroup
            {
                TaskId = taskId,
                DisplayName = GetTaskDisplayName(taskId),
                Steps = seen[taskId]
            });
        }

        return groups;
    }

    /// <summary>Converts a TaskId to a human-friendly display name.</summary>
    internal static string GetTaskDisplayName(string taskId)
    {
        // Static well-known mappings
        if (WellKnownTaskNames.TryGetValue(taskId, out var name))
            return name;

        // Dynamic patterns
        if (taskId.StartsWith("te-pr-", StringComparison.OrdinalIgnoreCase))
            return $"PR #{taskId[6..]} Testing";

        if (taskId.StartsWith("issue-", StringComparison.OrdinalIgnoreCase))
            return $"Issue #{taskId[6..]}";

        if (taskId.StartsWith("research-", StringComparison.OrdinalIgnoreCase))
            return "Research";

        if (taskId.StartsWith("arch-", StringComparison.OrdinalIgnoreCase))
            return "Architecture Design";

        // Fallback: titlecase the taskId
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(taskId.Replace('-', ' ').Replace('_', ' '));
    }

    private static readonly Dictionary<string, string> WellKnownTaskNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pm-kickoff"] = "Project Kickoff",
        ["pm-spec"] = "PM Specification",
        ["pm-monitoring"] = "Team Monitoring",
        ["pe-planning"] = "Engineering Planning",
        ["pe-orchestration"] = "Engineer Orchestration",
        ["pe-review"] = "Code Review",
        ["te-loop"] = "Test Monitoring",
        ["te-review"] = "Test Review",
    };

    public (int completed, int total) GetProgress(string agentId)
    {
        if (!_agentSteps.TryGetValue(agentId, out var list))
            return (0, 0);

        lock (list)
        {
            var completed = list.Count(s => s.Status is AgentTaskStepStatus.Completed or AgentTaskStepStatus.Skipped);
            return (completed, list.Count);
        }
    }

    private void NotifyChanged(AgentTaskStep step)
    {
        try
        {
            OnStepChanged?.Invoke(step);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in step changed handler");
        }
    }
}
