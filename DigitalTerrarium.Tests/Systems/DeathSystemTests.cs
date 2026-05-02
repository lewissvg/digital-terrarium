using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class DeathSystemTests
{
    [Fact]
    public void Tick_RemovesZeroOrNegativeEnergyOrganisms()
    {
        Organism alive = Organism.NewBorn(Vector2.Zero, new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        alive.Energy = 10;

        Organism deadA = Organism.NewBorn(Vector2.Zero, new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        deadA.Energy = 0;

        Organism deadB = Organism.NewBorn(Vector2.Zero, new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        deadB.Energy = -3;

        var organisms = new List<Organism> { alive, deadA, deadB };
        var corpses = new List<Corpse>();

        DeathSystem.Tick(organisms, corpses);

        Assert.Single(organisms);
        Assert.Same(alive, organisms[0]);
    }

    [Fact]
    public void Tick_CreatesCorpseFromDeadOrganism()
    {
        var position = new Vector2(100, 200);
        Organism dead = Organism.NewBorn(position, new Genome(1, 1, 1, 0.5f, 0.5f), 2);
        dead.Energy = -5;

        var organisms = new List<Organism> { dead };
        var corpses = new List<Corpse>();

        DeathSystem.Tick(organisms, corpses);

        Assert.Empty(organisms);
        Assert.Single(corpses);
        Assert.Equal(position, corpses[0].Position);
        Assert.Equal(2, corpses[0].Generation);
    }

    [Fact]
    public void DecayCorpses_RemovesFullyDecayedCorpses()
    {
        var world = new World();
        var corpse = Corpse.Create(new Vector2(50, 50), new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        var corpses = new List<Corpse> { corpse };

        // Decay the corpse fully (needs Corpse.DecayDuration ticks)
        for (int i = 0; i < Corpse.DecayDuration + 1; i++)
        {
            DeathSystem.DecayCorpses(world, corpses);
        }

        Assert.Empty(corpses);
    }

    [Fact]
    public void DecayCorpses_PreservesPartiallyDecayedCorpses()
    {
        var world = new World();
        var corpse = Corpse.Create(new Vector2(50, 50), new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        var corpses = new List<Corpse> { corpse };

        // Tick a few times but not enough to fully decay
        for (int i = 0; i < 10; i++)
        {
            DeathSystem.DecayCorpses(world, corpses);
        }

        Assert.Single(corpses);
        Assert.True(corpses[0].DecayTimer < Corpse.DecayDuration);
    }

    [Fact]
    public void DecayCorpses_AddsNutrientsToWorld()
    {
        var world = new World();
        // Set tile position to ensure it's within world bounds
        var corpse = Corpse.Create(new Vector2(42, 42), new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        var corpses = new List<Corpse> { corpse };

        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);
        float initialBiomass = world.GetBiomass(tx, ty);

        // Run decomposition
        for (int i = 0; i < 10; i++)
        {
            DeathSystem.DecayCorpses(world, corpses);
        }

        // Biomass should have increased
        Assert.True(world.GetBiomass(tx, ty) > initialBiomass);
    }

    [Fact]
    public void DecayCorpses_SpreadsNutrientsToNeighbors()
    {
        var world = new World();
        // Place corpse at center
        var corpse = Corpse.Create(new Vector2(42, 42), new Genome(1, 1, 1, 0.5f, 0.5f), 0);
        var corpses = new List<Corpse> { corpse };

        int tx = (int)(42f / world.TileSize);
        int ty = (int)(42f / world.TileSize);

        // Run decomposition
        for (int i = 0; i < 50; i++)
        {
            DeathSystem.DecayCorpses(world, corpses);
        }

        // Neighbors should have received some biomass
        float centerBiomass = world.GetBiomass(tx, ty);
        float neighborBiomass = world.GetBiomass(tx + 1, ty);
        
        Assert.True(centerBiomass > 0, "Center tile should have biomass");
        Assert.True(neighborBiomass > 0, "Neighbor tile should have received nutrients");
    }
}