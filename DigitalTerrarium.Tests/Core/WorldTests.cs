using DigitalTerrarium.Core;

namespace DigitalTerrarium.Tests.Core;

public class WorldTests
{
    [Fact]
    public void NewWorld_HasCorrectDimensions()
    {
        var world = new World();

        Assert.Equal(SimulationConfig.WorldTilesX, world.Width);
        Assert.Equal(SimulationConfig.WorldTilesY, world.Height);
        Assert.Equal(SimulationConfig.WorldTilesX * SimulationConfig.TileSize, world.PixelWidth);
        Assert.Equal(SimulationConfig.WorldTilesY * SimulationConfig.TileSize, world.PixelHeight);
    }

    [Fact]
    public void NewWorld_HasNoFood()
    {
        var world = new World();

        Assert.Equal(0, world.CountFood());
        Assert.Equal(world.Width * world.Height, world.CountEmptyTiles());
    }

    [Fact]
    public void TileAt_ReturnsNullOutsideBounds()
    {
        var world = new World();

        Assert.False(world.HasFoodAtPixel(-1, -1));
        Assert.False(world.HasFoodAtPixel(world.PixelWidth + 1, 0));
    }

    [Fact]
    public void SetFood_UpdatesCounts()
    {
        var world = new World();

        world.SetFood(10, 20, true);
        world.SetFood(11, 20, true);

        Assert.True(world.HasFood(10, 20));
        Assert.Equal(2, world.CountFood());
        Assert.Equal(world.Width * world.Height - 2, world.CountEmptyTiles());
    }
}
