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
        // Set biomass to 0.5 so after consuming 0.4, it drops to 0.1 (below 0.3 threshold)
        world.SetBiomass(10, 10, 0.5f);
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = 50;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default, 0, new Random(42));

        // biomass multiplier for 0.5 = Lerp(0.5, 1, 0.5) = 0.75, so 25 * 0.75 = 18.75 energy gain
        Assert.Equal(68.75f, organism.Energy, precision: 3);
        // After consuming 0.4 from 0.5, biomass is 0.1 which is below eat threshold
        Assert.False(world.HasFood(10, 10));
    }

    [Fact]
    public void Tick_DoesNotExceedMaxEnergy()
    {
        var world = new World();
        world.SetBiomass(10, 10, 1.0f);
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy - 5;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default, 0, new Random(42));

        Assert.Equal(organism.MaxEnergy, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_SandAlignedOrganismInSand_FullYield()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Sand);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Energy = 50f;

        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default, 0, new Random(42));

        Assert.Equal(75f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_MudAlignedOrganismInSand_HalfYield()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Sand);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, TerrainAffinity: 0f), 0);
        o.Energy = 50f;

        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default, 0, new Random(42));

        // Half base yield from sand + terrain mismatch
        Assert.Equal(50f + 25f * 0.5f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_AnyOrganismInMud_FullYield_RegardlessOfAffinity()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Mud);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Energy = 50f;

        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default, 0, new Random(42));

        Assert.Equal(75f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_DoesNothingOnEmptyTile()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        float before = organism.Energy;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default, 0, new Random(42));

        Assert.Equal(before, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_OnGrassland_RecordsConsumption()
    {
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Grassland);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        
        FeedingSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default, 100, new Random(42));

        Assert.True(world.Biomes.CanRecover(tx, ty, 101, 0)); // cooldown=0 should work
    }

    [Fact]
    public void Tick_GrasslandDegradation_100PercentChance_DegradesToMud()
    {
        var config = SimulationConfig.Default with { GrasslandDegradationChance = 1.0f };
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Grassland);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        
        FeedingSystem.Tick(world, new List<Organism> { o }, config, 0, new Random(42));

        Assert.Equal(BiomeType.Mud, world.Biomes.At(tx, ty));
    }

    [Fact]
    public void Tick_GrasslandDegradation_ZeroChance_StaysGrassland()
    {
        var config = SimulationConfig.Default with { GrasslandDegradationChance = 0.0f };
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Grassland);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        
        // Run many times with 0% degradation
        for (int i = 0; i < 100; i++)
        {
            world.SetBiomass(tx, ty, 1.0f); // Reset biomass
            FeedingSystem.Tick(world, new List<Organism> { o }, config, i, new Random(42));
        }

        Assert.Equal(BiomeType.Grassland, world.Biomes.At(tx, ty));
    }

    [Fact]
    public void Tick_Sand_DoesNotDegradeToMud()
    {
        var config = SimulationConfig.Default with { GrasslandDegradationChance = 1.0f };
        var world = new World();
        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Sand);
        world.SetBiomass(tx, ty, 1.0f);

        var o = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        
        FeedingSystem.Tick(world, new List<Organism> { o }, config, 0, new Random(42));

        // Sand should NOT degrade (only Grassland degrades)
        Assert.Equal(BiomeType.Sand, world.Biomes.At(tx, ty));
    }

    [Fact]
    public void Tick_LowBiomass_HasReducedYield()
    {
        var world = new World();
        world.SetBiomass(10, 10, 0.3f); // Just above eat threshold
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = 50;

        FeedingSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default, 0, new Random(42));

        // biomass multiplier for 0.3 = Lerp(0.5, 1, 0.3) = 0.5 + 0.15 = 0.65
        // 25 * 0.65 = 16.25, so 50 + 16.25 = 66.25
        Assert.Equal(66.25f, organism.Energy, precision: 2);
    }
}