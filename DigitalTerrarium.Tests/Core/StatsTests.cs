using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Core;

public class StatsTests
{
    [Fact]
    public void Compute_OnEmptyPopulation_ReturnsZeros()
    {
        StatsSnapshot snapshot = Stats.Compute(new List<Organism>(), new World());

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
        Organism a = Organism.NewBorn(Vector2.Zero, new Genome(2, 1, 10, 0.5f, 0.5f), 0);
        Organism b = Organism.NewBorn(Vector2.Zero, new Genome(4, 2, 30, 0.5f, 0.5f), 5);
        a.Age = 100;
        b.Age = 50;

        StatsSnapshot snapshot = Stats.Compute(new List<Organism> { a, b }, new World());

        Assert.Equal(2, snapshot.Population);
        Assert.Equal(100, snapshot.OldestAge);
        Assert.Equal(5, snapshot.MaxGeneration);
        Assert.Equal(3f, snapshot.AvgSpeed, precision: 3);
        Assert.Equal(1.5f, snapshot.AvgMetabolism, precision: 3);
        Assert.Equal(20f, snapshot.AvgSenseRange, precision: 3);
    }

    [Fact]
    public void Compute_AveragesDietType()
    {
        var a = Organism.NewBorn(Vector2.Zero, new Genome(2, 1, 10, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        var b = Organism.NewBorn(Vector2.Zero, new Genome(4, 2, 30, DietType: 1.0f, TerrainAffinity: 0.5f), 0);

        var snapshot = Stats.Compute(new List<Organism> { a, b }, new World());

        Assert.Equal(0.5f, snapshot.AvgDietType, precision: 3);
    }

    [Fact]
    public void Compute_OnEmptyPopulation_AvgDietTypeIsZero()
    {
        var snapshot = Stats.Compute(new List<Organism>(), new World());

        Assert.Equal(0f, snapshot.AvgDietType);
    }

    [Fact]
    public void Compute_AveragesTerrainAffinity()
    {
        var world = new World();
        var a = Organism.NewBorn(Vector2.Zero, new Genome(2, 1, 10, 0.5f, TerrainAffinity: 0f), 0);
        var b = Organism.NewBorn(Vector2.Zero, new Genome(4, 2, 30, 0.5f, TerrainAffinity: 1f), 0);

        var snap = Stats.Compute(new List<Organism> { a, b }, world);

        Assert.Equal(0.5f, snap.AvgTerrainAffinity, precision: 3);
    }

    [Fact]
    public void Compute_TalliesPopulationByBiome()
    {
        var world = new World();
        world.Biomes.SetForTest(0, 0, BiomeType.Mud);
        world.Biomes.SetForTest(50, 50, BiomeType.Sand);

        var inMud = Organism.NewBorn(new Vector2(2, 2), new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        var inGrass = Organism.NewBorn(new Vector2(100, 100), new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        var inSand = Organism.NewBorn(new Vector2(202, 202), new Genome(1, 1, 1, 0.5f, 0.5f), 0);

        var snap = Stats.Compute(new List<Organism> { inMud, inGrass, inSand }, world);

        Assert.Equal(1, snap.PopMud);
        Assert.Equal(1, snap.PopGrassland);
        Assert.Equal(1, snap.PopSand);
    }

    [Fact]
    public void Compute_OnEmptyPopulation_PopulationsAreZero()
    {
        var world = new World();
        var snap = Stats.Compute(new List<Organism>(), world);

        Assert.Equal(0, snap.PopMud);
        Assert.Equal(0, snap.PopGrassland);
        Assert.Equal(0, snap.PopSand);
        Assert.Equal(0f, snap.AvgTerrainAffinity);
    }
}
