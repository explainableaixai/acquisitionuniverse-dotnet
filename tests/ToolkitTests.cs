using AlphaQuantum.AcquisitionUniverse;
using Xunit;

public class ToolkitTests
{
    [Fact]
    public void FifteenSignals() => Assert.Equal(15, Signals.All.Count);

    [Fact]
    public void GroupOwnedZeroesOutreach()
    {
        Assert.Equal(100, Scoring.Composite(new Company("a", 100, 100, 100)));
        Assert.Equal(70, Scoring.Composite(new Company("a", 100, 100, 0, true)));
    }

    [Fact]
    public void RankCoverageBrief()
    {
        var r = Scoring.Rank(new[] { new Company("a", 50), new Company("b", 90, 50) });
        Assert.Equal("b", r[0].Company.Id);
        var cov = Signals.Coverage(new Dictionary<string, string?> { [Signals.All[0]] = "x", [Signals.All[1]] = null });
        Assert.Equal((1, 2), cov);
        var brief = new ThesisBrief { Vertical = "Water", Regions = new[] { "US" } };
        Assert.Single(brief.Validate());
        Assert.Contains("free-pilot.php", brief.ToText());
    }
}
