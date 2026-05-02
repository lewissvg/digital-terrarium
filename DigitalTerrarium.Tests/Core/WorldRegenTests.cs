using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Tests.Core;

public class WorldRegenTests
{
    [Fact]
    public void RegenerateFood_BiomassAccumulatesOverTime()
    {
        var world = new World();
        var rng = new Random(1);
        
        // Record initial total biomass
        float initialTotal = 0f;
        for (int y = 0; y < world.Height; y++)
            for (int x = 0; x < world.Width; x++)
                initialTotal += world.GetBiomass(x, y);
        
        // Run regeneration
        for (int tick = 0; tick < 100; tick++)
        {
            world.RegenerateFood(regenRate: 0.005f, rng);
        }
        
        // Count total biomass after regeneration
        float finalTotal = 0f;
        int edibleTiles = 0;
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                float b = world.GetBiomass(x, y);
                finalTotal += b;
                if (b >= BiomeProperties.BiomassEatThreshold)
                    edibleTiles++;
            }
        }
        
        // Biomass should have increased
        Assert.True(finalTotal > initialTotal, "Biomass should accumulate over time");
        
        // With this regen rate, should have some edible tiles
        Assert.True(edibleTiles > 0, "Should have some edible tiles after regeneration");
    }

    [Fact]
    public void RegenerateFood_MudTilesAccumulateFasterThanSand()
    {
        var world = new World();
        // Set up 10x10 mud tiles on left, sand tiles on right
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                world.Biomes.SetForTest(x, y, BiomeType.Mud);
        for (int y = 0; y < 10; y++)
            for (int x = 10; x < 20; x++)
                world.Biomes.SetForTest(x, y, BiomeType.Sand);

        var rng = new Random(1);
        for (int t = 0; t < 100; t++)
            world.RegenerateFood(regenRate: 0.01f, rng);

        float mudTotal = 0, sandTotal = 0;
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                mudTotal += world.GetBiomass(x, y);
        for (int y = 0; y < 10; y++)
            for (int x = 10; x < 20; x++)
                sandTotal += world.GetBiomass(x, y);

        Assert.True(mudTotal > sandTotal,
            $"Expected Mud biomass > Sand biomass, got Mud={mudTotal:F2} Sand={sandTotal:F2}");
    }

    [Fact]
    public void RegenerateFood_NoOpWhenWorldFull()
    {
        var world = new World();
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                world.SetBiomass(x, y, 1.0f); // Full biomass everywhere
            }
        }

        float before = 0f;
        float after = 0f;
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                before += world.GetBiomass(x, y);
            }
        }
        
        world.RegenerateFood(regenRate: 0.01f, new Random(1));
        
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                after += world.GetBiomass(x, y);
            }
        }

        Assert.Equal(before, after); // No change when full
    }

    [Fact]
    public void RegenerateFood_ConsumedTilesCanRecover()
    {
        var world = new World();
        world.SetBiomass(10, 10, 1.0f); // Full biomass
        
        // Simulate consumption (eating reduces biomass)
        world.ConsumeBiomass(10, 10, 0.5f);
        Assert.True(world.GetBiomass(10, 10) < 1.0f);
        
        // After regeneration, biomass should increase
        var rng = new Random(1);
        for (int t = 0; t < 50; t++)
            world.RegenerateFood(regenRate: 0.01f, rng);
            
        Assert.True(world.GetBiomass(10, 10) > 0.5f, "Biomass should recover after being consumed");
    }
}