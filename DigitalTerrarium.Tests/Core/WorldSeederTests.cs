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

        // Compute the biome-weighted expected density after biome scatter is applied
        int totalTiles = world.Width * world.Height;
        double expectedDensity = 0.0;
        for (int y = 0; y < world.Height; y++)
            for (int x = 0; x < world.Width; x++)
                expectedDensity += config.InitialFoodDensity * BiomeProperties.FoodRegenMultiplier(world.Biomes.At(x, y));
        expectedDensity /= totalTiles;

        double actualDensity = (double)world.CountFood() / totalTiles;
        Assert.InRange(actualDensity, expectedDensity * 0.90, expectedDensity * 1.10);
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
        var organisms = new List<Organism> { Organism.NewBorn(default, new Genome(1, 1, 1, 0.5f, 0.5f), 0) };
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
    public void Seed_GeneratesBiomeMap_NotAllGrassland()
    {
        var world = new World();
        var organisms = new List<Organism>();
        var config = SimulationConfig.Default with { Seed = 1 };

        WorldSeeder.Seed(world, organisms, config, new Random(config.Seed));

        var (m, g, s) = world.Biomes.CountTiles();
        Assert.True(m > 0);
        Assert.True(g > 0);
        Assert.True(s > 0);
    }

    [Fact]
    public void Seed_TerrainAffinity_SpansFullRange()
    {
        var world = new World();
        var organisms = new List<Organism>();
        var config = SimulationConfig.Default with { Seed = 1, StartingPopulation = 200 };
        WorldSeeder.Seed(world, organisms, config, new Random(config.Seed));

        foreach (var o in organisms)
            Assert.InRange(o.Genes.TerrainAffinity, 0f, 1f);

        float min = organisms.Min(o => o.Genes.TerrainAffinity);
        float max = organisms.Max(o => o.Genes.TerrainAffinity);
        Assert.True(min < 0.2f, "Should see organisms with low affinity");
        Assert.True(max > 0.8f, "Should see organisms with high affinity");
    }

    [Fact]
    public void Seed_FoodDensity_RespectsBiomeMultipliers()
    {
        var world = new World();
        var organisms = new List<Organism>();
        var config = SimulationConfig.Default with { Seed = 1, InitialFoodDensity = 0.15f };

        WorldSeeder.Seed(world, organisms, config, new Random(config.Seed));

        int mudTotal = 0, mudFood = 0;
        int sandTotal = 0, sandFood = 0;
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                switch (world.Biomes.At(x, y))
                {
                    case BiomeType.Mud:
                        mudTotal++;
                        if (world.HasFood(x, y)) mudFood++;
                        break;
                    case BiomeType.Sand:
                        sandTotal++;
                        if (world.HasFood(x, y)) sandFood++;
                        break;
                }
            }
        }

        if (mudTotal > 100 && sandTotal > 100)
        {
            double mudFraction = (double)mudFood / mudTotal;
            double sandFraction = (double)sandFood / sandTotal;
            Assert.True(mudFraction > sandFraction * 5,
                $"Expected Mud food density (×2.0) much higher than Sand (×0.2), got mud={mudFraction:F3} sand={sandFraction:F3}");
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
