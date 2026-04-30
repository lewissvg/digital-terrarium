using DigitalTerrarium.Core;
using Xunit;

namespace DigitalTerrarium.Tests.Core;

public class BiomeMapTests
{
    [Fact]
    public void NewMap_DefaultsToGrasslandEverywhere()
    {
        var map = new BiomeMap(50, 50);

        for (int y = 0; y < 50; y++)
            for (int x = 0; x < 50; x++)
                Assert.Equal(BiomeType.Grassland, map.At(x, y));
    }

    [Fact]
    public void At_OutOfBounds_ReturnsGrassland()
    {
        var map = new BiomeMap(50, 50);

        Assert.Equal(BiomeType.Grassland, map.At(-1, 5));
        Assert.Equal(BiomeType.Grassland, map.At(5, -1));
        Assert.Equal(BiomeType.Grassland, map.At(50, 5));
        Assert.Equal(BiomeType.Grassland, map.At(5, 50));
    }

    [Fact]
    public void Generate_DefaultBalance_ProducesAllThreeBiomes()
    {
        var map = new BiomeMap(300, 300);
        map.Generate(noiseGridSize: 20, mudSandBalance: 0.5f, new Random(1));

        var (m, g, s) = map.CountTiles();
        Assert.True(m > 0, "Expected some Mud tiles");
        Assert.True(g > 0, "Expected some Grassland tiles");
        Assert.True(s > 0, "Expected some Sand tiles");
        Assert.Equal(300 * 300, m + g + s);
    }

    [Fact]
    public void Generate_LowBalance_IsMudHeavy()
    {
        var map = new BiomeMap(300, 300);
        map.Generate(noiseGridSize: 20, mudSandBalance: 0.2f, new Random(1));

        var (m, _, s) = map.CountTiles();
        Assert.True(m > s, $"Expected Mud-heavy world, got Mud={m} Sand={s}");
    }

    [Fact]
    public void Generate_HighBalance_IsSandHeavy()
    {
        var map = new BiomeMap(300, 300);
        map.Generate(noiseGridSize: 20, mudSandBalance: 0.8f, new Random(1));

        var (m, _, s) = map.CountTiles();
        Assert.True(s > m, $"Expected Sand-heavy world, got Mud={m} Sand={s}");
    }

    [Fact]
    public void Generate_IsDeterministic()
    {
        var a = new BiomeMap(100, 100);
        a.Generate(20, 0.5f, new Random(42));

        var b = new BiomeMap(100, 100);
        b.Generate(20, 0.5f, new Random(42));

        for (int y = 0; y < 100; y++)
            for (int x = 0; x < 100; x++)
                Assert.Equal(a.At(x, y), b.At(x, y));
    }

    [Fact]
    public void AtPixel_DividesByTileSize()
    {
        var map = new BiomeMap(50, 50);
        Assert.Equal(BiomeType.Grassland, map.AtPixel(0f, 0f, 4));
        Assert.Equal(BiomeType.Grassland, map.AtPixel(199.99f, 199.99f, 4));
    }
}
