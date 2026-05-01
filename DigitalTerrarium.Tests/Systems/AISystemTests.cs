using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class AISystemTests
{
    private SimulationConfig Config => SimulationConfig.Default;

    [Fact]
    public void Tick_TargetsFoodWhenAvailableAndAboveRestThreshold()
    {
        Organism organism = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.5f;
        organism.Target = new Vector2(20, 10);

        AISystem.Tick(new List<Organism> { organism }, Config, new Random(1));

        Assert.Equal(AIState.Target, organism.State);
        Assert.True(organism.Velocity.X > 0);
        Assert.Equal(0f, organism.Velocity.Y, precision: 3);
    }

    [Fact]
    public void Tick_RestsWhenLowEnergyAndNoFood()
    {
        Organism organism = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.10f;
        organism.Target = null;

        AISystem.Tick(new List<Organism> { organism }, Config, new Random(1));

        Assert.Equal(AIState.Rest, organism.State);
        Assert.Equal(Vector2.Zero, organism.Velocity);
    }

    [Fact]
    public void Tick_WandersWhenNoTargetAndAdequateEnergy()
    {
        Organism organism = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.5f;
        organism.Target = null;

        AISystem.Tick(new List<Organism> { organism }, Config, new Random(1));

        Assert.Equal(AIState.Wander, organism.State);
        Assert.Equal(0.5f * 4f, organism.Velocity.Length(), precision: 3);
    }

    [Fact]
    public void Tick_RefreshesWanderHeadingWhenCounterExpires()
    {
        Organism organism = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Energy = organism.MaxEnergy * 0.5f;
        organism.Target = null;
        organism.WanderTicksRemaining = 0;

        AISystem.Tick(new List<Organism> { organism }, Config, new Random(1));

        Assert.True(organism.WanderTicksRemaining > 0);
    }

    [Fact]
    public void Tick_RestExitsToWanderWhenEnergyAtCap()
    {
        Organism o = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        o.State = AIState.Rest;
        o.Energy = o.MaxEnergy * SimulationConfig.RestRecoveryCapFraction; // exactly at cap
        o.Target = null;

        AISystem.Tick(new List<Organism> { o }, Config, new Random(1));

        Assert.NotEqual(AIState.Rest, o.State);
    }

    [Fact]
    public void Tick_RestRemainsBelowCap()
    {
        Organism o = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        o.State = AIState.Rest;
        o.Energy = o.MaxEnergy * 0.30f; // above 20% threshold, below 50% cap
        o.Target = null;

        AISystem.Tick(new List<Organism> { o }, Config, new Random(1));

        Assert.Equal(AIState.Rest, o.State);
    }
}
