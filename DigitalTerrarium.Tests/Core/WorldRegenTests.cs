using DigitalTerrarium.Core;

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
