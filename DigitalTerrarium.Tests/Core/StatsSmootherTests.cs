using DigitalTerrarium.Core;

namespace DigitalTerrarium.Tests.Core;

public class StatsSmootherTests
{
    private static StatsSnapshot Make(float diet, int popMud = 0, int popG = 0, int popS = 0, int pop = 1) =>
        new StatsSnapshot(
            Population: pop, OldestAge: 0, MaxGeneration: 0,
            AvgSpeed: 0, AvgMetabolism: 0, AvgSenseRange: 0,
            AvgDietType: diet, AvgTerrainAffinity: 0,
            PopMud: popMud, PopGrassland: popG, PopSand: popS);

    [Fact]
    public void Smoothed_OnEmpty_ReturnsDefault()
    {
        var s = new StatsSmoother(30);
        Assert.Equal(default, s.Smoothed);
    }

    [Fact]
    public void Push_BelowWindow_ReturnsRollingMean()
    {
        var s = new StatsSmoother(30);
        s.Push(Make(diet: 0.0f));
        s.Push(Make(diet: 0.5f));
        s.Push(Make(diet: 1.0f));

        Assert.Equal(0.5f, s.Smoothed.AvgDietType, precision: 3);
    }

    [Fact]
    public void Push_OverWindow_DiscardsOldest()
    {
        var s = new StatsSmoother(3);
        s.Push(Make(diet: 0.0f));
        s.Push(Make(diet: 0.0f));
        s.Push(Make(diet: 0.0f));
        s.Push(Make(diet: 1.0f)); // oldest 0.0 falls out
        s.Push(Make(diet: 1.0f));
        s.Push(Make(diet: 1.0f));

        Assert.Equal(1.0f, s.Smoothed.AvgDietType, precision: 3);
    }

    [Fact]
    public void Smoothed_PreservesPopulationFromLatest()
    {
        var s = new StatsSmoother(5);
        s.Push(Make(diet: 0f, pop: 100));
        s.Push(Make(diet: 0f, pop: 200));
        s.Push(Make(diet: 0f, pop: 50));

        Assert.Equal(50, s.Smoothed.Population);
    }

    [Fact]
    public void Push_AveragesPopByBiome()
    {
        var s = new StatsSmoother(2);
        s.Push(Make(diet: 0f, popMud: 10, popG: 20, popS: 30));
        s.Push(Make(diet: 0f, popMud: 20, popG: 30, popS: 40));

        Assert.Equal(15, s.Smoothed.PopMud);
        Assert.Equal(25, s.Smoothed.PopGrassland);
        Assert.Equal(35, s.Smoothed.PopSand);
    }
}
