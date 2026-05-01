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
        // Use Wanderlust=0 to test base speed without bonus
        Organism organism = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f, Wanderlust: 0), 0);
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

    [Fact]
    public void Tick_WanderlustIncreasesWanderSpeed()
    {
        // Organism with max wanderlust should move faster when wandering
        Organism wanderer = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f, Wanderlust: 1f), 0);
        wanderer.Energy = wanderer.MaxEnergy * 0.5f;
        wanderer.Target = null;
        wanderer.WanderTicksRemaining = 0;

        // Organism with no wanderlust should move at base speed
        Organism settler = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f, Wanderlust: 0f), 0);
        settler.Energy = settler.MaxEnergy * 0.5f;
        settler.Target = null;
        settler.WanderTicksRemaining = 0;

        var organisms = new List<Organism> { wanderer, settler };
        AISystem.Tick(organisms, Config, new Random(1));

        // Wanderer should have higher velocity due to wanderlust bonus
        float wandererSpeed = wanderer.Velocity.Length();
        float settlerSpeed = settler.Velocity.Length();
        Assert.True(wandererSpeed > settlerSpeed);
    }

    [Fact]
    public void Tick_WanderlustIncreasesRestThreshold()
    {
        // High wanderlust organism should resist resting
        Organism wanderer = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f, Wanderlust: 1f), 0);
        wanderer.Energy = wanderer.MaxEnergy * 0.15f; // Below normal 20% rest threshold
        wanderer.Target = null;

        // Lower wanderlust organism should rest at same energy
        Organism settler = Organism.NewBorn(new Vector2(10, 10), new Genome(4, 1, 30, 0.5f, 0.5f, Wanderlust: 0f), 0);
        settler.Energy = settler.MaxEnergy * 0.15f;
        settler.Target = null;

        var organisms = new List<Organism> { wanderer, settler };
        AISystem.Tick(organisms, Config, new Random(1));

        // With wanderlust=1, rest threshold is reduced by 50%, so 15% energy is above threshold
        // With wanderlust=0, rest threshold is normal, so 15% energy is below threshold (rest)
        Assert.NotEqual(AIState.Rest, wanderer.State);
        Assert.Equal(AIState.Rest, settler.State);
    }
}
