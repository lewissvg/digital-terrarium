using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Entities;

public class OrganismTests
{
    [Fact]
    public void MaxEnergy_FollowsFormula()
    {
        var genes = new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 40f);
        Organism organism = Organism.NewBorn(Vector2.Zero, genes, generation: 0);

        Assert.Equal(140f, organism.MaxEnergy, precision: 3);
    }

    [Fact]
    public void NewBorn_StartsAtHalfMaxEnergy()
    {
        var genes = new Genome(4f, 1f, 40f);
        Organism organism = Organism.NewBorn(Vector2.Zero, genes, generation: 0);

        Assert.Equal(organism.MaxEnergy * 0.5f, organism.Energy, precision: 3);
        Assert.Equal(0, organism.Age);
        Assert.Equal(0, organism.Generation);
        Assert.Equal(AIState.Wander, organism.State);
    }
}
