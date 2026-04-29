using AgentSquad.Core.Configuration;

namespace AgentSquad.Core.Tests;

public class WorkingBranchTests
{
    #region RunBranchProvider

    [Fact]
    public void EffectiveBranch_WhenNoRunSet_ReturnsDefaultBranch()
    {
        var provider = new RunBranchProvider("main");
        Assert.Equal("main", provider.EffectiveBranch);
    }

    [Fact]
    public void EffectiveBranch_AfterSetForRun_ReturnsWorkingBranch()
    {
        var provider = new RunBranchProvider("main");
        provider.SetForRun("feature/agent-work");
        Assert.Equal("feature/agent-work", provider.EffectiveBranch);
    }

    [Fact]
    public void EffectiveBranch_AfterSetForRunWithNull_ReturnsDefaultBranch()
    {
        var provider = new RunBranchProvider("main");
        provider.SetForRun(null);
        Assert.Equal("main", provider.EffectiveBranch);
    }

    [Fact]
    public void EffectiveBranch_AfterReset_ReturnsDefaultBranch()
    {
        var provider = new RunBranchProvider("main");
        provider.SetForRun("feature/agent-work");
        Assert.Equal("feature/agent-work", provider.EffectiveBranch);

        provider.Reset();
        Assert.Equal("main", provider.EffectiveBranch);
    }

    [Fact]
    public void EffectiveBranch_MultipleRunCycles_NoCrossRunLeakage()
    {
        var provider = new RunBranchProvider("main");

        // Run 1 with working branch
        provider.SetForRun("release/v1");
        Assert.Equal("release/v1", provider.EffectiveBranch);
        provider.Reset();

        // Run 2 without working branch
        provider.SetForRun(null);
        Assert.Equal("main", provider.EffectiveBranch);
        provider.Reset();

        // Run 3 with different working branch
        provider.SetForRun("dev");
        Assert.Equal("dev", provider.EffectiveBranch);
        provider.Reset();

        Assert.Equal("main", provider.EffectiveBranch);
    }

    [Fact]
    public void SetForRun_OverwritesPreviousRunBranch()
    {
        var provider = new RunBranchProvider("main");
        provider.SetForRun("branch-a");
        provider.SetForRun("branch-b");
        Assert.Equal("branch-b", provider.EffectiveBranch);
    }

    [Fact]
    public void EffectiveBranch_UsesCustomDefaultBranch()
    {
        var provider = new RunBranchProvider("develop");
        Assert.Equal("develop", provider.EffectiveBranch);

        provider.SetForRun("feature/x");
        Assert.Equal("feature/x", provider.EffectiveBranch);

        provider.Reset();
        Assert.Equal("develop", provider.EffectiveBranch);
    }

    #endregion

    #region IRunBranchProvider Interface

    [Fact]
    public void InterfaceContract_EffectiveBranch_MatchesConcrete()
    {
        IRunBranchProvider provider = new RunBranchProvider("main");
        Assert.Equal("main", provider.EffectiveBranch);
    }

    [Fact]
    public void InterfaceContract_ReflectsSetForRun()
    {
        var concrete = new RunBranchProvider("main");
        IRunBranchProvider iface = concrete;

        concrete.SetForRun("working");
        Assert.Equal("working", iface.EffectiveBranch);
    }

    #endregion

    #region DevelopSettings WorkingBranch

    [Fact]
    public void DevelopSettings_WorkingBranch_DefaultsToNull()
    {
        var settings = new DevelopSettings();
        Assert.Null(settings.WorkingBranch);
    }

    [Fact]
    public void DevelopSettings_WorkingBranch_RoundTrips()
    {
        var settings = new DevelopSettings { WorkingBranch = "release/v2" };
        Assert.Equal("release/v2", settings.WorkingBranch);
    }

    #endregion
}
