using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Tests.Core;

public class WorldRegenTests
{
    [Fact]
    public void RegenerateFood_AddsApproximatelyExpectedAmountOverManyTicks()
    {
        var world = new World();
        var rng = new Random(1);

        for (int tick = 0; tick < 1000; tick++)
        {
            world.RegenerateFood(regenRate: 2.0f, rng);
        }

        Assert.InRange(world.CountFood(), 1600, 2400);
    }

    [Fact]
    public void RegenerateFood_MudTilesRegenerateFasterThanSand()
    {
        var world = new World();
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                world.Biomes.SetForTest(x, y, BiomeType.Mud);
        for (int y = 0; y < 10; y++)
            for (int x = 10; x < 20; x++)
                world.Biomes.SetForTest(x, y, BiomeType.Sand);

        var rng = new Random(1);
        for (int t = 0; t < 200; t++)
            world.RegenerateFood(regenRate: 50f, rng);

        int mudFood = 0, sandFood = 0;
        for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
                if (world.HasFood(x, y)) mudFood++;
        for (int y = 0; y < 10; y++)
            for (int x = 10; x < 20; x++)
                if (world.HasFood(x, y)) sandFood++;

        Assert.True(mudFood > sandFood,
            $"Expected Mud > Sand food count, got Mud={mudFood}, Sand={sandFood}");
    }

    [Fact]
    public void RegenerateFood_NoOpWhenWorldFull()
    {
        var world = new World();
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                world.SetFood(x, y, true);
            }
        }

        int before = world.CountFood();
        world.RegenerateFood(regenRate: 5f, new Random(1));

        Assert.Equal(before, world.CountFood());
    }
}
