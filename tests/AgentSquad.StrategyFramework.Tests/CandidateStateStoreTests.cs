using AgentSquad.Core.Strategies;
using AgentSquad.Core.Strategies.Contracts;
using Xunit;

namespace AgentSquad.StrategyFramework.Tests;

public class CandidateStateStoreTests
{
    [Fact]
    public void Started_event_creates_task_and_running_candidate()
    {
        var store = new CandidateStateStore();
        var at = DateTimeOffset.UtcNow;

        store.RecordStarted(new CandidateStartedEvent("run1", "task1", "baseline", at));

        var active = store.GetActiveTasks();
        Assert.Single(active);
        var t = active[0];
        Assert.Equal("run1", t.RunId);
        Assert.Equal("task1", t.TaskId);
        Assert.True(t.Candidates.ContainsKey("baseline"));
        Assert.Equal(CandidateState.Running, t.Candidates["baseline"].State);
        Assert.Equal(at, t.Candidates["baseline"].StartedAt);
    }

    [Fact]
    public void Second_strategy_started_merges_into_existing_task()
    {
        var store = new CandidateStateStore();
        var at = DateTimeOffset.UtcNow;

        store.RecordStarted(new CandidateStartedEvent("run1", "task1", "baseline", at));
        store.RecordStarted(new CandidateStartedEvent("run1", "task1", "mcp-enhanced", at.AddMilliseconds(10)));

        var active = store.GetActiveTasks();
        Assert.Single(active);
        Assert.Equal(2, active[0].Candidates.Count);
        Assert.Contains("baseline", active[0].Candidates.Keys);
        Assert.Contains("mcp-enhanced", active[0].Candidates.Keys);
    }

    [Fact]
    public void Completed_event_updates_candidate_state()
    {
        var store = new CandidateStateStore();
        var at = DateTimeOffset.UtcNow;
        store.RecordStarted(new CandidateStartedEvent("r", "t", "baseline", at));

        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "baseline", true, null, 1.5, 42));

        var c = store.GetActiveTasks()[0].Candidates["baseline"];
        Assert.Equal(CandidateState.Completed, c.State);
        Assert.True(c.Succeeded);
        Assert.Equal(1.5, c.ElapsedSec);
        Assert.Equal(42L, c.TokensUsed);
    }

    [Fact]
    public void Scored_event_updates_scores_and_state()
    {
        var store = new CandidateStateStore();
        store.RecordStarted(new CandidateStartedEvent("r", "t", "baseline", DateTimeOffset.UtcNow));
        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "baseline", true, null, 1.0, 10));

        store.RecordScored(new CandidateScoredEvent("r", "t", "baseline", 8, 7, 9));

        var c = store.GetActiveTasks()[0].Candidates["baseline"];
        Assert.Equal(CandidateState.Scored, c.State);
        Assert.Equal(8, c.AcScore);
        Assert.Equal(7, c.DesignScore);
        Assert.Equal(9, c.ReadabilityScore);
    }

    [Fact]
    public void Winner_event_moves_task_from_active_to_recent()
    {
        var store = new CandidateStateStore();
        store.RecordStarted(new CandidateStartedEvent("r", "t", "baseline", DateTimeOffset.UtcNow));
        store.RecordStarted(new CandidateStartedEvent("r", "t", "mcp-enhanced", DateTimeOffset.UtcNow));
        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "baseline", true, null, 1.0, 10));
        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "mcp-enhanced", true, null, 1.1, 12));
        store.RecordScored(new CandidateScoredEvent("r", "t", "baseline", 8, 7, 9));
        store.RecordScored(new CandidateScoredEvent("r", "t", "mcp-enhanced", 9, 8, 9));

        store.RecordWinner(new WinnerSelectedEvent("r", "t", "mcp-enhanced", "higher-total-score", 0.4));

        Assert.Empty(store.GetActiveTasks());
        var recent = store.GetRecentTasks();
        Assert.Single(recent);
        Assert.Equal("mcp-enhanced", recent[0].WinnerStrategyId);
        Assert.Equal(CandidateState.Winner, recent[0].Candidates["mcp-enhanced"].State);
        Assert.Equal("higher-total-score", recent[0].TieBreakReason);
    }

    [Fact]
    public void OnChange_fires_for_each_mutation()
    {
        var store = new CandidateStateStore();
        var count = 0;
        store.OnChange += _ => Interlocked.Increment(ref count);

        store.RecordStarted(new CandidateStartedEvent("r", "t", "baseline", DateTimeOffset.UtcNow));
        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "baseline", true, null, 1.0, 10));
        store.RecordScored(new CandidateScoredEvent("r", "t", "baseline", 8, 7, 9));
        store.RecordWinner(new WinnerSelectedEvent("r", "t", "baseline", "only-survivor", 0.1));

        Assert.Equal(4, count);
    }

    [Fact]
    public void Recent_ring_respects_capacity()
    {
        var store = new CandidateStateStore(recentCapacity: 3);

        for (var i = 0; i < 5; i++)
        {
            var taskId = $"t{i}";
            store.RecordStarted(new CandidateStartedEvent("r", taskId, "baseline", DateTimeOffset.UtcNow));
            store.RecordWinner(new WinnerSelectedEvent("r", taskId, "baseline", "solo", 0.1));
        }

        var recent = store.GetRecentTasks();
        Assert.Equal(3, recent.Count);
        Assert.Equal("t4", recent[0].TaskId);
        Assert.Equal("t3", recent[1].TaskId);
        Assert.Equal("t2", recent[2].TaskId);
    }

    [Fact]
    public void ArchiveTaskIfActive_moves_task_without_winner()
    {
        var store = new CandidateStateStore();
        store.RecordStarted(new CandidateStartedEvent("r", "t", "baseline", DateTimeOffset.UtcNow));
        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "baseline", false, "gate-failed", 1.0, 5));

        store.ArchiveTaskIfActive("r", "t", "all-candidates-failed");

        Assert.Empty(store.GetActiveTasks());
        var recent = store.GetRecentTasks();
        Assert.Single(recent);
        Assert.Null(recent[0].WinnerStrategyId);
        Assert.Equal("all-candidates-failed", recent[0].TieBreakReason);
    }

    [Fact]
    public void Late_events_for_unknown_task_are_silently_ignored()
    {
        var store = new CandidateStateStore();

        store.RecordCompleted(new CandidateCompletedEvent("r", "t", "baseline", true, null, 1.0, 10));
        store.RecordScored(new CandidateScoredEvent("r", "t", "baseline", 8, 7, 9));
        store.RecordWinner(new WinnerSelectedEvent("r", "t", "baseline", "orphan", 0.0));

        Assert.Empty(store.GetActiveTasks());
        Assert.Empty(store.GetRecentTasks());
    }
}
