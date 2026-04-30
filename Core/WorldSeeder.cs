using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Core;

public static class WorldSeeder
{
    public static void Seed(World world, List<Organism> organisms, SimulationConfig config, Random rng)
    {
        world.Biomes.Generate(config.BiomeNoiseScale, config.MudSandBalance, rng);
        world.Clear();
        organisms.Clear();

        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                float biomeMult = BiomeProperties.FoodRegenMultiplier(world.Biomes.At(x, y));
                if (rng.NextDouble() < config.InitialFoodDensity * biomeMult)
                    world.SetFood(x, y, true);
            }
        }

        for (int i = 0; i < config.StartingPopulation; i++)
        {
            float speed = Lerp(2f, 8f, (float)rng.NextDouble());
            float metabolism = Lerp(0.5f, 1.5f, (float)rng.NextDouble());
            float senseRange = Lerp(10f, 80f, (float)rng.NextDouble());
            float dietType = (float)rng.NextDouble() * config.InitialMaxDietType;
            float affinity = (float)rng.NextDouble();
            var genes = new Genome(speed, metabolism, senseRange, dietType, affinity);

            float px = (float)rng.NextDouble() * world.PixelWidth;
            float py = (float)rng.NextDouble() * world.PixelHeight;

            organisms.Add(Organism.NewBorn(new Vector2(px, py), genes, generation: 0));
        }
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
