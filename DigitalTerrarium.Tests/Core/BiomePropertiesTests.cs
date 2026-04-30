using DigitalTerrarium.Core;
using Xunit;

namespace DigitalTerrarium.Tests.Core;

public class BiomePropertiesTests
{
    [Fact]
    public void Optimum_ReturnsExpectedValuesForEachBiome()
    {
        Assert.Equal(0f,   BiomeProperties.Optimum(BiomeType.Mud));
        Assert.Equal(0.5f, BiomeProperties.Optimum(BiomeType.Grassland));
        Assert.Equal(1f,   BiomeProperties.Optimum(BiomeType.Sand));
    }

    [Fact]
    public void FoodRegenMultiplier_BoostsMudAndPenalisesSand()
    {
        Assert.Equal(2.0f, BiomeProperties.FoodRegenMultiplier(BiomeType.Mud));
        Assert.Equal(1.0f, BiomeProperties.FoodRegenMultiplier(BiomeType.Grassland));
        Assert.Equal(0.2f, BiomeProperties.FoodRegenMultiplier(BiomeType.Sand));
    }

    [Fact]
    public void MovementBase_OnlyMudIsSlow()
    {
        Assert.Equal(0.5f, BiomeProperties.MovementBase(BiomeType.Mud));
        Assert.Equal(1.0f, BiomeProperties.MovementBase(BiomeType.Grassland));
        Assert.Equal(1.0f, BiomeProperties.MovementBase(BiomeType.Sand));
    }

    [Fact]
    public void FoodYieldBase_OnlySandIsLow()
    {
        Assert.Equal(1.0f, BiomeProperties.FoodYieldBase(BiomeType.Mud));
        Assert.Equal(1.0f, BiomeProperties.FoodYieldBase(BiomeType.Grassland));
        Assert.Equal(0.5f, BiomeProperties.FoodYieldBase(BiomeType.Sand));
    }

    [Theory]
    [InlineData(0f,   BiomeType.Mud,       1f)]
    [InlineData(0.5f, BiomeType.Mud,       0f)]
    [InlineData(1f,   BiomeType.Mud,       0f)]
    [InlineData(0f,   BiomeType.Grassland, 0f)]
    [InlineData(0.5f, BiomeType.Grassland, 1f)]
    [InlineData(1f,   BiomeType.Grassland, 0f)]
    [InlineData(0f,   BiomeType.Sand,      0f)]
    [InlineData(0.5f, BiomeType.Sand,      0f)]
    [InlineData(1f,   BiomeType.Sand,      1f)]
    public void Match_AtBoundaries(float affinity, BiomeType biome, float expected)
    {
        Assert.Equal(expected, BiomeProperties.Match(affinity, biome), precision: 3);
    }
}
