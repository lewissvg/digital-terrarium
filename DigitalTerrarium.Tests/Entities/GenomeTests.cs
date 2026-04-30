using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Tests.Entities;

public class GenomeTests
{
    [Fact]
    public void Mutate_AppliesDriftWithinRate()
    {
        var parent = new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 40f);
        var rng = new Random(42);

        Genome child = parent.Mutate(rate: 0.05f, rng);

        Assert.InRange(child.Speed, 4f * 0.95f, 4f * 1.05f);
        Assert.InRange(child.Metabolism, 1f * 0.95f, 1f * 1.05f);
        Assert.InRange(child.SenseRange, 40f * 0.95f, 40f * 1.05f);
    }

    [Fact]
    public void Mutate_ClampsBelowMinimums()
    {
        var tiny = new Genome(Speed: 0.05f, Metabolism: 0.05f, SenseRange: 0.5f);
        var rng = new Random(0);

        Genome child = tiny.Mutate(rate: 0.5f, rng);

        Assert.True(child.Speed >= 0.1f);
        Assert.True(child.Metabolism >= 0.1f);
        Assert.True(child.SenseRange >= 1f);
    }

    [Fact]
    public void Mutate_IsDeterministicGivenSameSeed()
    {
        var parent = new Genome(3f, 1f, 30f);

        Genome a = parent.Mutate(0.1f, new Random(123));
        Genome b = parent.Mutate(0.1f, new Random(123));

        Assert.Equal(a, b);
    }
}
