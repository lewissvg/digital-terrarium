using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Tests.Entities;

public class GenomeTests
{
    [Fact]
    public void Mutate_AppliesDriftWithinRate()
    {
        var parent = new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 40f, DietType: 0.3f, TerrainAffinity: 0.5f);
        var rng = new Random(42);

        Genome child = parent.Mutate(rate: 0.05f, rng);

        Assert.InRange(child.Speed, 4f * 0.95f, 4f * 1.05f);
        Assert.InRange(child.Metabolism, 1f * 0.95f, 1f * 1.05f);
        Assert.InRange(child.SenseRange, 40f * 0.95f, 40f * 1.05f);
    }

    [Fact]
    public void Mutate_ClampsBelowMinimums()
    {
        var tiny = new Genome(Speed: 0.05f, Metabolism: 0.05f, SenseRange: 0.5f, DietType: 0.5f, TerrainAffinity: 0.5f);
        var rng = new Random(0);

        Genome child = tiny.Mutate(rate: 0.5f, rng);

        Assert.True(child.Speed >= 0.1f);
        Assert.True(child.Metabolism >= 0.1f);
        Assert.True(child.SenseRange >= 1f);
    }

    [Fact]
    public void Mutate_IsDeterministicGivenSameSeed()
    {
        var parent = new Genome(3f, 1f, 30f, 0.4f, 0.5f);

        Genome a = parent.Mutate(0.1f, new Random(123));
        Genome b = parent.Mutate(0.1f, new Random(123));

        Assert.Equal(a, b);
    }

    [Fact]
    public void Mutate_DietType_IsAdditive_AndClamped()
    {
        var parent = new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 30f, DietType: 0.5f, TerrainAffinity: 0.5f);
        var rng = new Random(42);

        var child = parent.Mutate(rate: 0.05f, rng);

        Assert.InRange(child.DietType, 0.45f, 0.55f);
    }

    [Fact]
    public void Mutate_DietType_AtZero_CanProduceNonZeroChild()
    {
        var parent = new Genome(4f, 1f, 30f, DietType: 0f, TerrainAffinity: 0.5f);
        bool sawNonZero = false;
        var rng = new Random(1);
        for (int i = 0; i < 50; i++)
        {
            var child = parent.Mutate(0.05f, rng);
            if (child.DietType > 0f)
            {
                sawNonZero = true;
                break;
            }
        }

        Assert.True(sawNonZero, "Additive mutation must let DietType drift up from 0");
    }

    [Fact]
    public void Mutate_DietType_ClampsToOne()
    {
        var parent = new Genome(4f, 1f, 30f, DietType: 1f, TerrainAffinity: 0.5f);
        var rng = new Random(0);

        for (int i = 0; i < 20; i++)
        {
            var child = parent.Mutate(0.5f, rng);
            Assert.True(child.DietType <= 1f);
            Assert.True(child.DietType >= 0f);
        }
    }

    [Fact]
    public void Mutate_OtherGenes_StillMultiplicative()
    {
        var parent = new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 40f, DietType: 0.3f, TerrainAffinity: 0.5f);
        var rng = new Random(42);

        var child = parent.Mutate(rate: 0.05f, rng);

        Assert.InRange(child.Speed, 4f * 0.95f, 4f * 1.05f);
        Assert.InRange(child.Metabolism, 1f * 0.95f, 1f * 1.05f);
        Assert.InRange(child.SenseRange, 40f * 0.95f, 40f * 1.05f);
    }

    [Fact]
    public void Mutate_TerrainAffinity_IsAdditive_AndClamped()
    {
        var parent = new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 30f, DietType: 0.3f, TerrainAffinity: 0.5f);
        var rng = new Random(42);

        var child = parent.Mutate(rate: 0.05f, rng);

        Assert.InRange(child.TerrainAffinity, 0.45f, 0.55f);
    }

    [Fact]
    public void Mutate_TerrainAffinity_AtZero_CanProduceNonZeroChild()
    {
        var parent = new Genome(4f, 1f, 30f, 0.3f, TerrainAffinity: 0f);
        bool sawNonZero = false;
        var rng = new Random(1);
        for (int i = 0; i < 50; i++)
        {
            var child = parent.Mutate(0.05f, rng);
            if (child.TerrainAffinity > 0f) { sawNonZero = true; break; }
        }
        Assert.True(sawNonZero, "Additive mutation must let TerrainAffinity drift up from 0");
    }

    [Fact]
    public void Mutate_TerrainAffinity_ClampsToOne()
    {
        var parent = new Genome(4f, 1f, 30f, 0.3f, TerrainAffinity: 1f);
        var rng = new Random(0);
        for (int i = 0; i < 20; i++)
        {
            var child = parent.Mutate(0.5f, rng);
            Assert.True(child.TerrainAffinity <= 1f);
            Assert.True(child.TerrainAffinity >= 0f);
        }
    }
}
