using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class FeedingSystemTests
{
    [Fact]
    public void Tick_EatsFoodOnSameTileAndIncreasesEnergy()
    {
        var world = new World();
        world.SetFood(10, 10, true);
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = 50;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(75, organism.Energy, precision: 3);
        Assert.False(world.HasFood(10, 10));
    }

    [Fact]
    public void Tick_DoesNotExceedMaxEnergy()
    {
        var world = new World();
        world.SetFood(10, 10, true);
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy - 5;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(organism.MaxEnergy, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_SandAlignedOrganismInSand_FullYield()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Sand);
        world.SetFood(tx, ty, true);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Energy = 50f;

        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        Assert.Equal(75f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_MudAlignedOrganismInSand_HalfYield()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Sand);
        world.SetFood(tx, ty, true);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, TerrainAffinity: 0f), 0);
        o.Energy = 50f;

        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        Assert.Equal(50f + 25f * 0.5f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_AnyOrganismInMud_FullYield_RegardlessOfAffinity()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Mud);
        world.SetFood(tx, ty, true);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Energy = 50f;

        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        Assert.Equal(75f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_DoesNothingOnEmptyTile()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        float before = organism.Energy;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(before, organism.Energy, precision: 3);
    }
}
