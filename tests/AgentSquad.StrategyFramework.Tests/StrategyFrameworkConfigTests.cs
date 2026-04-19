using AgentSquad.Core.Configuration;

namespace AgentSquad.StrategyFramework.Tests;

public class StrategyFrameworkConfigTests
{
    [Fact]
    public void Defaults_match_user_locked_decisions()
    {
        var cfg = new StrategyFrameworkConfig();

        // Phase 1: framework defaults to OFF until baseline-contract (p1-baseline-contract)
        // replaces the marker-file stub with real single-shot SE generation. This prevents
        // the live SE pipeline from accidentally shipping no-op marker PRs when a runner
        // omits the StrategyFramework section from appsettings.
        Assert.False(cfg.Enabled);
        Assert.Equal("always", cfg.SamplingPolicy);
        Assert.Equal("full-review", cfg.PostWinnerFlow);
    }

    [Fact]
    public void Default_enabled_strategies_exclude_agentic_until_sandbox_hardened()
    {
        var cfg = new StrategyFrameworkConfig();

        Assert.Contains("baseline", cfg.EnabledStrategies);
        Assert.Contains("mcp-enhanced", cfg.EnabledStrategies);
        Assert.DoesNotContain("agentic-delegation", cfg.EnabledStrategies);
    }

    [Fact]
    public void Global_cap_is_below_sum_of_pools_to_prevent_overload()
    {
        var cfg = new StrategyFrameworkConfig();
        var sumOfPools = cfg.Concurrency.SingleShotSlots
                         + cfg.Concurrency.CandidateSlots
                         + cfg.Concurrency.AgenticSlots;

        Assert.True(cfg.Concurrency.GlobalMaxConcurrentProcesses <= sumOfPools,
            "GlobalMaxConcurrentProcesses must cap total concurrent copilot processes below raw pool sum.");
    }

    [Fact]
    public void Reserved_evaluator_path_is_under_tests_dir()
    {
        var cfg = new StrategyFrameworkConfig();
        Assert.StartsWith("tests/", cfg.Evaluator.ReservedPathPrefix);
    }
}
