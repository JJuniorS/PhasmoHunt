using PhasmoHunt.Models;
using PhasmoHunt.Services;

namespace PhasmoHunt.Tests;

public class GhostSpeedScalingTests
{
    private static GhostInfo Spirit() => new()
    {
        Id = "spirit",
        Name = "Spirit",
        BaseSpeedMps = 1.7,
        MinSpeedMps = 1.7,
        MaxSpeedMps = 1.7,
        Evidences = []
    };

    [Fact]
    public void MatchesSpeed_At120Percent_UsesScaledRange()
    {
        var ghost = Spirit();
        Assert.True(ghost.MatchesSpeed(2.04, SpeedCalculatorService.SpeedMatchToleranceMps, speedFactor: 1.2));
        Assert.False(ghost.MatchesSpeed(1.7, SpeedCalculatorService.SpeedMatchToleranceMps, speedFactor: 1.2));
    }

    [Fact]
    public void FormatSpeedRange_At120Percent_ShowsScaled()
    {
        var text = Spirit().FormatSpeedRange(1.2);
        Assert.Contains("2.04", text);
    }

    [Fact]
    public void Calculate_WithSpeedFactor_MatchesScaledCatalog()
    {
        var interval = (1.0 / 2.04) - SpeedCalculatorService.FootstepTimingOffsetSeconds;
        var stamps = new List<TimeSpan>();
        var t = TimeSpan.Zero;
        for (var i = 0; i < 6; i++)
        {
            stamps.Add(t);
            t += TimeSpan.FromSeconds(interval);
        }

        var calc = new SpeedCalculatorService();
        var result = calc.Calculate(stamps, [Spirit()], speedFactor: 1.2);
        Assert.Contains(result.CompatibleGhosts, g => g.Id == "spirit");

        var at100 = calc.Calculate(stamps, [Spirit()], speedFactor: 1.0);
        Assert.DoesNotContain(at100.CompatibleGhosts, g => g.Id == "spirit");
    }
}
