using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Tests.Core;

public class WorldSeederTests
{
    [Fact]
    public void Seed_ProducesApproximatelyExpectedFoodDensity()
    {
        var world = new World();
        var organisms = new List<Organism>();
        SimulationConfig config = SimulationConfig.Default with { Seed = 1, InitialFoodDensity = 0.20f };
        var rng = new Random(config.Seed);

        WorldSeeder.Seed(world, organisms, config, rng);

        int totalTiles = world.Width * world.Height;
        double actualDensity = (double)world.CountFood() / totalTiles;
        Assert.InRange(actualDensity, 0.18, 0.22);
    }

    [Fact]
    public void Seed_SpawnsRequestedPopulation()
    {
        var world = new World();
        var organisms = new List<Organism>();
        SimulationConfig config = SimulationConfig.Default with { Seed = 1, StartingPopulation = 75 };
        var rng = new Random(config.Seed);

        WorldSeeder.Seed(world, organisms, config, rng);

        Assert.Equal(75, organisms.Count);
    }

    [Fact]
    public void Seed_IsDeterministicGivenSameSeed()
    {
        static World MakeAndSeed(int seed)
        {
            var world = new World();
            var organisms = new List<Organism>();
            SimulationConfig config = SimulationConfig.Default with { Seed = seed };
            WorldSeeder.Seed(world, organisms, config, new Random(seed));
            return world;
        }

        World a = MakeAndSeed(7);
        World b = MakeAndSeed(7);

        Assert.Equal(a.CountFood(), b.CountFood());
        for (int i = 0; i < 50; i++)
        {
            Assert.Equal(a.HasFood(i, i), b.HasFood(i, i));
        }
    }

    [Fact]
    public void Seed_ResetsExistingState()
    {
        var world = new World();
        var organisms = new List<Organism> { Organism.NewBorn(default, new Genome(1, 1, 1, 0.5f), 0) };
        world.SetFood(0, 0, true);

        SimulationConfig config = SimulationConfig.Default with { Seed = 1, StartingPopulation = 10 };
        WorldSeeder.Seed(world, organisms, config, new Random(config.Seed));

        Assert.Equal(10, organisms.Count);
    }

    [Fact]
    public void Seed_DietType_RespectsInitialMaxDietType()
    {
        var world = new World();
        var organisms = new List<Organism>();
        var config = SimulationConfig.Default with { Seed = 1, StartingPopulation = 200, InitialMaxDietType = 0.4f };
        var rng = new Random(config.Seed);

        WorldSeeder.Seed(world, organisms, config, rng);

        foreach (var organism in organisms)
        {
            Assert.InRange(organism.Genes.DietType, 0f, 0.4f);
        }
    }

    [Fact]
    public void Seed_DietType_AtZero_AllPureHerbivores()
    {
        var world = new World();
        var organisms = new List<Organism>();
        var config = SimulationConfig.Default with { Seed = 1, StartingPopulation = 50, InitialMaxDietType = 0f };

        WorldSeeder.Seed(world, organisms, config, new Random(config.Seed));

        Assert.All(organisms, organism => Assert.Equal(0f, organism.Genes.DietType));
    }
}
