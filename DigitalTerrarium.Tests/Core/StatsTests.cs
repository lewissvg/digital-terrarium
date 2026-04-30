using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Core;

public class StatsTests
{
    [Fact]
    public void Compute_OnEmptyPopulation_ReturnsZeros()
    {
        StatsSnapshot snapshot = Stats.Compute(new List<Organism>());

        Assert.Equal(0, snapshot.Population);
        Assert.Equal(0, snapshot.OldestAge);
        Assert.Equal(0, snapshot.MaxGeneration);
        Assert.Equal(0f, snapshot.AvgSpeed);
        Assert.Equal(0f, snapshot.AvgMetabolism);
        Assert.Equal(0f, snapshot.AvgSenseRange);
    }

    [Fact]
    public void Compute_AveragesGenes()
    {
        Organism a = Organism.NewBorn(Vector2.Zero, new Genome(2, 1, 10, 0.5f), 0);
        Organism b = Organism.NewBorn(Vector2.Zero, new Genome(4, 2, 30, 0.5f), 5);
        a.Age = 100;
        b.Age = 50;

        StatsSnapshot snapshot = Stats.Compute(new List<Organism> { a, b });

        Assert.Equal(2, snapshot.Population);
        Assert.Equal(100, snapshot.OldestAge);
        Assert.Equal(5, snapshot.MaxGeneration);
        Assert.Equal(3f, snapshot.AvgSpeed, precision: 3);
        Assert.Equal(1.5f, snapshot.AvgMetabolism, precision: 3);
        Assert.Equal(20f, snapshot.AvgSenseRange, precision: 3);
    }
}
