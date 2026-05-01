using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class ReproductionSystemTests
{
    [Fact]
    public void Tick_SplitsAtThreshold()
    {
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.95f;
        var organisms = new List<Organism> { organism };

        ReproductionSystem.Tick(organisms, new World(), SimulationConfig.Default, new Random(1));

        Assert.Equal(2, organisms.Count);
        Assert.Equal(1, organisms[1].Generation);
    }

    [Fact]
    public void Tick_DoesNotSplitBelowThreshold()
    {
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.5f;
        var organisms = new List<Organism> { organism };

        ReproductionSystem.Tick(organisms, new World(), SimulationConfig.Default, new Random(1));

        Assert.Single(organisms);
    }

    [Fact]
    public void Tick_HalvesParentEnergyAndAssignsHalfToChild()
    {
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.95f;
        float originalEnergy = organism.Energy;
        var organisms = new List<Organism> { organism };

        ReproductionSystem.Tick(organisms, new World(), SimulationConfig.Default, new Random(1));

        Organism child = organisms[1];
        Assert.Equal(originalEnergy * 0.5f, organism.Energy, precision: 3);
        Assert.Equal(originalEnergy * 0.5f, child.Energy, precision: 3);
    }

    [Fact]
    public void Tick_AppliesMutationWithinRange()
    {
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.95f;
        var organisms = new List<Organism> { organism };
        SimulationConfig config = SimulationConfig.Default with { MutationRate = 0.05f };

        ReproductionSystem.Tick(organisms, new World(), config, new Random(7));

        Organism child = organisms[1];
        Assert.InRange(child.Genes.Speed, 4f * 0.95f, 4f * 1.05f);
        Assert.InRange(child.Genes.Metabolism, 1f * 0.95f, 1f * 1.05f);
        Assert.InRange(child.Genes.SenseRange, 30f * 0.95f, 30f * 1.05f);
    }

    [Fact]
    public void Tick_BlocksReproductionWhenAffinityMismatchesBiome()
    {
        var world = new World();
        int tx = (int)(100f / world.TileSize);
        int ty = (int)(100f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Sand);

        // Affinity 0 in Sand: match = 0
        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 0f), 0);
        o.Energy = o.MaxEnergy * 0.95f;
        var organisms = new List<Organism> { o };

        ReproductionSystem.Tick(organisms, world, SimulationConfig.Default, new Random(1));

        Assert.Single(organisms);
    }

    [Fact]
    public void Tick_AllowsReproductionWhenAffinityMatchesBiome()
    {
        var world = new World();
        int tx = (int)(100f / world.TileSize);
        int ty = (int)(100f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Mud);

        // Affinity 0 in Mud: match = 1
        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 0f), 0);
        o.Energy = o.MaxEnergy * 0.95f;
        var organisms = new List<Organism> { o };

        ReproductionSystem.Tick(organisms, world, SimulationConfig.Default, new Random(1));

        Assert.Equal(2, organisms.Count);
    }

    [Fact]
    public void Tick_RespectsConfigurableThreshold_Zero_AlwaysReproduces()
    {
        var world = new World();
        // default Grassland, affinity 1 → match = Max(0, 1-2*|1-0.5|) = 0
        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Energy = o.MaxEnergy * 0.95f;
        var organisms = new List<Organism> { o };
        var config = SimulationConfig.Default with { ReproductionMatchThreshold = 0f };

        ReproductionSystem.Tick(organisms, world, config, new Random(1));

        Assert.Equal(2, organisms.Count);
    }

    [Fact]
    public void Tick_RespectsConfigurableThreshold_One_RequiresPerfectMatch()
    {
        var world = new World();
        // default Grassland, affinity 0.5 → match = 1.0 exactly
        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 0.5f), 0);
        o.Energy = o.MaxEnergy * 0.95f;
        var organisms = new List<Organism> { o };
        var config = SimulationConfig.Default with { ReproductionMatchThreshold = 1f };

        ReproductionSystem.Tick(organisms, world, config, new Random(1));

        Assert.Equal(2, organisms.Count);
    }
}
