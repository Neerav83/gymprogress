using GymProgress.Domain;

namespace GymProgress.Domain.Tests;

public class StrengthMetricsTests
{
    [Fact]
    public void Volume_multiplies_weight_and_reps()
    {
        Assert.Equal(1800m, StrengthMetrics.Volume(45, 40));
    }

    [Fact]
    public void EstimatedOneRepMax_uses_epley_formula()
    {
        Assert.Equal(25m, StrengthMetrics.EstimatedOneRepMax(25, 1));
        Assert.Equal(33.33m, StrengthMetrics.EstimatedOneRepMax(25, 10));
    }

    [Fact]
    public void EstimatedOneRepMax_returns_zero_for_invalid_input()
    {
        Assert.Equal(0m, StrengthMetrics.EstimatedOneRepMax(0, 10));
        Assert.Equal(0m, StrengthMetrics.EstimatedOneRepMax(25, 0));
    }
}
