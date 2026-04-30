using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class DeathSystemTests
{
    [Fact]
    public void Tick_RemovesZeroOrNegativeEnergyOrganisms()
    {
        Organism alive = Organism.NewBorn(Vector2.Zero, new Genome(1, 1, 1, 0.5f), 0);
        alive.Energy = 10;

        Organism deadA = Organism.NewBorn(Vector2.Zero, new Genome(1, 1, 1, 0.5f), 0);
        deadA.Energy = 0;

        Organism deadB = Organism.NewBorn(Vector2.Zero, new Genome(1, 1, 1, 0.5f), 0);
        deadB.Energy = -3;

        var organisms = new List<Organism> { alive, deadA, deadB };

        DeathSystem.Tick(organisms);

        Assert.Single(organisms);
        Assert.Same(alive, organisms[0]);
    }
}
