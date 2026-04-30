using DigitalTerrarium.Core;
using Xunit;

namespace DigitalTerrarium.Tests.Core;

public class BiomeNoiseTests
{
    [Fact]
    public void Generate_ProducesValuesInUnitRange()
    {
        var noise = BiomeNoise.Generate(100, 100, noiseGridSize: 10, new Random(1));

        for (int y = 0; y < 100; y++)
            for (int x = 0; x < 100; x++)
                Assert.InRange(noise[x, y], 0f, 1f);
    }

    [Fact]
    public void Generate_IsDeterministicGivenSameSeed()
    {
        var a = BiomeNoise.Generate(50, 50, noiseGridSize: 10, new Random(42));
        var b = BiomeNoise.Generate(50, 50, noiseGridSize: 10, new Random(42));

        for (int y = 0; y < 50; y++)
            for (int x = 0; x < 50; x++)
                Assert.Equal(a[x, y], b[x, y], precision: 5);
    }

    [Fact]
    public void Generate_DifferentSeedsProduceDifferentMaps()
    {
        var a = BiomeNoise.Generate(50, 50, noiseGridSize: 10, new Random(1));
        var b = BiomeNoise.Generate(50, 50, noiseGridSize: 10, new Random(2));

        bool anyDifferent = false;
        for (int y = 0; y < 50 && !anyDifferent; y++)
            for (int x = 0; x < 50 && !anyDifferent; x++)
                if (Math.Abs(a[x, y] - b[x, y]) > 0.001f) anyDifferent = true;
        Assert.True(anyDifferent);
    }
}
