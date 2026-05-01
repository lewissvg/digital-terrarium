using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class PerceptionSystemTests
{
    private static SpatialIndex BuildIndex(World world, List<Organism> organisms)
    {
        var index = new SpatialIndex(world.PixelWidth, world.PixelHeight, 80);
        index.Build(organisms);
        return index;
    }

    [Fact]
    public void Tick_FindsNearestFoodInRange()
    {
        var world = new World();
        world.SetFood(10, 10, true);
        world.SetFood(50, 50, true);

        Organism organism = Organism.NewBorn(new Vector2(40, 40), new Genome(1, 1, 30, 0.5f, 0.5f), 0);
        var organisms = new List<Organism> { organism };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.NotNull(organism.Target);
        Assert.Equal(10 * 4 + 2, organism.Target!.Value.X, precision: 1);
        Assert.Equal(10 * 4 + 2, organism.Target!.Value.Y, precision: 1);
    }

    [Fact]
    public void Tick_ReturnsNullWhenNoFoodInRange()
    {
        var world = new World();
        world.SetFood(50, 50, true);

        Organism organism = Organism.NewBorn(new Vector2(40, 40), new Genome(1, 1, 5, 0.5f, 0.5f), 0);
        var organisms = new List<Organism> { organism };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.Null(organism.Target);
    }

    [Fact]
    public void Tick_RespectsSenseRangeRadius()
    {
        var world = new World();
        world.SetFood(15, 10, true);
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 25, 0.5f, 0.5f), 0);
        var organisms = new List<Organism> { organism };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.NotNull(organism.Target);
    }

    [Fact]
    public void Tick_AppliesPerceptionEnergyCost()
    {
        var world = new World();
        var organism = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = 100f;
        var organisms = new List<Organism> { organism };

        var config = SimulationConfig.Default with { PerceptionCostCoefficient = 0.001f };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, config);

        Assert.Equal(100f - 2.827f, organism.Energy, precision: 2);
    }

    [Fact]
    public void Tick_NoPerceptionCostWhenResting()
    {
        var world = new World();
        var organism = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = 50f;
        organism.State = AIState.Rest;
        var organisms = new List<Organism> { organism };
        var index = BuildIndex(world, organisms);

        var config = SimulationConfig.Default with { PerceptionCostCoefficient = 0.001f };

        PerceptionSystem.Tick(world, organisms, index, config);

        Assert.Equal(50f, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_OmnivoreTargetsPreyWhenInRange()
    {
        var world = new World();
        var hunter = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 50, DietType: 0.8f, TerrainAffinity: 0.5f), 0);
        var prey = Organism.NewBorn(new Vector2(50, 40), new Genome(2, 1, 10, DietType: 0.1f, TerrainAffinity: 0.5f), 0);
        var organisms = new List<Organism> { hunter, prey };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.Equal(prey, hunter.TargetPrey);
        Assert.NotNull(hunter.Target);
        Assert.Equal(prey.Position, hunter.Target!.Value);
    }

    [Fact]
    public void Tick_AttackBuffer_PreventsCloseDietPredation()
    {
        var world = new World();
        var a = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 50, DietType: 0.6f, TerrainAffinity: 0.5f), 0);
        var b = Organism.NewBorn(new Vector2(50, 40), new Genome(4, 1, 50, DietType: 0.5f, TerrainAffinity: 0.5f), 0);
        var organisms = new List<Organism> { a, b };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.Null(a.TargetPrey);
        Assert.Null(b.TargetPrey);
    }

    [Fact]
    public void Tick_PureCarnivoreIgnoresFoodTiles()
    {
        var world = new World();
        world.SetFood(10, 10, true);
        var carnivore = Organism.NewBorn(new Vector2(45, 45), new Genome(4, 1, 30, DietType: 0.97f, TerrainAffinity: 0.5f), 0);
        var organisms = new List<Organism> { carnivore };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.Null(carnivore.Target);
        Assert.Null(carnivore.TargetPrey);
    }

    [Fact]
    public void Tick_OmnivorePicksNearerOfPreyOrFood()
    {
        var world = new World();
        world.SetFood(10, 10, true);
        var hunter = Organism.NewBorn(new Vector2(45, 45), new Genome(4, 1, 60, DietType: 0.7f, TerrainAffinity: 0.5f), 0);
        var farPrey = Organism.NewBorn(new Vector2(80, 80), new Genome(2, 1, 5, DietType: 0.1f, TerrainAffinity: 0.5f), 0);
        var organisms = new List<Organism> { hunter, farPrey };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.Null(hunter.TargetPrey);
        Assert.NotNull(hunter.Target);
    }

    [Fact]
    public void Tick_PureHerbivoreIgnoresPrey()
    {
        var world = new World();
        var herb = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 50, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        var prey = Organism.NewBorn(new Vector2(50, 40), new Genome(2, 1, 10, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        var organisms = new List<Organism> { herb, prey };
        var index = BuildIndex(world, organisms);

        PerceptionSystem.Tick(world, organisms, index, SimulationConfig.Default);

        Assert.Null(herb.TargetPrey);
    }
}
