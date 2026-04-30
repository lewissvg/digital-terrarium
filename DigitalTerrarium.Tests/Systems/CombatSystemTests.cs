using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class CombatSystemTests
{
    [Fact]
    public void Tick_PredatorWithinRange_KillsPreyAndAbsorbsEnergy()
    {
        var hunter = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, DietType: 1.0f, TerrainAffinity: 0.5f), 0);
        var prey = Organism.NewBorn(new Vector2(42, 40), new Genome(2, 1, 10, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        prey.Energy = 50f;
        hunter.Energy = 50f;
        hunter.TargetPrey = prey;
        hunter.Target = prey.Position;

        CombatSystem.Tick(new List<Organism> { hunter, prey });

        Assert.Equal(MathF.Min(hunter.MaxEnergy, 100f), hunter.Energy, precision: 3);
        Assert.Equal(0f, prey.Energy);
        Assert.Null(hunter.TargetPrey);
        Assert.Null(hunter.Target);
    }

    [Fact]
    public void Tick_PredatorOutsideRange_DoesNotKill()
    {
        var hunter = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, DietType: 1.0f, TerrainAffinity: 0.5f), 0);
        var prey = Organism.NewBorn(new Vector2(80, 40), new Genome(2, 1, 10, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        prey.Energy = 50f;
        hunter.TargetPrey = prey;
        hunter.Target = prey.Position;
        float hunterEnergyBefore = hunter.Energy;

        CombatSystem.Tick(new List<Organism> { hunter, prey });

        Assert.Equal(50f, prey.Energy);
        Assert.Equal(hunterEnergyBefore, hunter.Energy);
        Assert.Equal(prey, hunter.TargetPrey);
    }

    [Fact]
    public void Tick_OmnivoreYieldIsScaledByDietType()
    {
        var hunter = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, DietType: 0.6f, TerrainAffinity: 0.5f), 0);
        var prey = Organism.NewBorn(new Vector2(42, 40), new Genome(2, 1, 10, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        prey.Energy = 100f;
        hunter.Energy = 50f;
        hunter.TargetPrey = prey;

        CombatSystem.Tick(new List<Organism> { hunter, prey });

        float expected = MathF.Min(hunter.MaxEnergy, 50f + 60f);
        Assert.Equal(expected, hunter.Energy, precision: 3);
        Assert.Equal(0f, prey.Energy);
    }

    [Fact]
    public void Tick_TwoPredatorsSamePrey_FirstWins()
    {
        var p1 = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, DietType: 0.9f, TerrainAffinity: 0.5f), 0);
        var p2 = Organism.NewBorn(new Vector2(41, 41), new Genome(4, 1, 30, DietType: 0.9f, TerrainAffinity: 0.5f), 0);
        var prey = Organism.NewBorn(new Vector2(42, 40), new Genome(2, 1, 10, DietType: 0.0f, TerrainAffinity: 0.5f), 0);
        prey.Energy = 50f;
        p1.TargetPrey = prey;
        p1.Target = prey.Position;
        p2.TargetPrey = prey;
        p2.Target = prey.Position;
        p1.Energy = 50f;
        float p2EnergyBefore = p2.Energy;

        CombatSystem.Tick(new List<Organism> { p1, p2, prey });

        Assert.Equal(0f, prey.Energy);
        Assert.True(p1.Energy > 50f);
        Assert.Equal(p2EnergyBefore, p2.Energy);
        Assert.Null(p2.TargetPrey);
    }

    [Fact]
    public void Tick_NoTarget_NoOp()
    {
        var organism = Organism.NewBorn(new Vector2(40, 40), new Genome(4, 1, 30, DietType: 1.0f, TerrainAffinity: 0.5f), 0);
        organism.Energy = 50f;
        float before = organism.Energy;

        CombatSystem.Tick(new List<Organism> { organism });

        Assert.Equal(before, organism.Energy);
    }
}
