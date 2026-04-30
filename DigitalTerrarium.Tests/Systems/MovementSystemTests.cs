using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class MovementSystemTests
{
    [Fact]
    public void Tick_AppliesVelocityToPosition()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Velocity = new Vector2(2, -1);
        organism.State = AIState.Target;

        MovementSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(102f, organism.Position.X, precision: 3);
        Assert.Equal(99f, organism.Position.Y, precision: 3);
    }

    [Fact]
    public void Tick_ClampsAtWorldBounds()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(0, 0), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.Velocity = new Vector2(-5, -5);
        organism.State = AIState.Wander;

        MovementSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.True(organism.Position.X >= 0);
        Assert.True(organism.Position.Y >= 0);
        Assert.True(organism.Position.X < world.PixelWidth);
        Assert.True(organism.Position.Y < world.PixelHeight);
    }

    [Fact]
    public void Tick_DrainsEnergyWhileMoving()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(Speed: 4f, Metabolism: 1f, SenseRange: 30f, DietType: 0.5f, TerrainAffinity: 0.5f), 0);
        organism.Velocity = new Vector2(4, 0);
        organism.State = AIState.Target;
        float energyBefore = organism.Energy;

        MovementSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(energyBefore - 0.8f, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_RestingOrganismRecoversEnergyUpToCap()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.State = AIState.Rest;
        organism.Velocity = Vector2.Zero;
        organism.Energy = organism.MaxEnergy * 0.10f;
        float before = organism.Energy;

        MovementSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(before + 0.05f, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_RestingOrganismDoesNotRecoverPastCap()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.State = AIState.Rest;
        organism.Velocity = Vector2.Zero;
        organism.Energy = organism.MaxEnergy * 0.50f;
        float before = organism.Energy;

        MovementSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(before, organism.Energy, precision: 3);
    }

    [Fact]
    public void Tick_IncrementsAge()
    {
        var world = new World();
        Organism organism = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, 0.5f), 0);
        organism.State = AIState.Wander;

        MovementSystem.Tick(world, new List<Organism> { organism }, SimulationConfig.Default);

        Assert.Equal(1, organism.Age);
    }

    [Fact]
    public void Tick_MudAlignedOrganismInMud_MovesAtFullSpeed()
    {
        var world = new World();
        int tx = (int)(100f / world.TileSize);
        int ty = (int)(100f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Mud);

        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 0f), 0);
        o.Velocity = new Vector2(2, 0);
        o.State = AIState.Target;

        MovementSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        Assert.Equal(102f, o.Position.X, precision: 3);
    }

    [Fact]
    public void Tick_SandAlignedOrganismInMud_MovesAtHalfSpeed()
    {
        var world = new World();
        int tx = (int)(100f / world.TileSize);
        int ty = (int)(100f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Mud);

        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Velocity = new Vector2(2, 0);
        o.State = AIState.Target;

        MovementSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        // Sand-aligned in Mud: match=0, MovementBase(Mud)=0.5 → effective=0.5
        Assert.Equal(101f, o.Position.X, precision: 3);
    }

    [Fact]
    public void Tick_DrainAppliesOnIntent_NotEffectiveSpeed()
    {
        var world = new World();
        int tx = (int)(100f / world.TileSize);
        int ty = (int)(100f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Mud);

        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 1f), 0);
        o.Velocity = new Vector2(4, 0);
        o.State = AIState.Target;
        float energyBefore = o.Energy;

        MovementSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        Assert.Equal(energyBefore - 0.8f, o.Energy, precision: 3);
    }

    [Fact]
    public void Tick_OrganismInGrassland_NoSpeedPenalty_RegardlessOfAffinity()
    {
        var world = new World();
        int tx = (int)(100f / world.TileSize);
        int ty = (int)(100f / world.TileSize);
        world.Biomes.SetForTest(tx, ty, BiomeType.Grassland);

        var o = Organism.NewBorn(new Vector2(100, 100), new Genome(4, 1, 30, 0.5f, TerrainAffinity: 0f), 0);
        o.Velocity = new Vector2(2, 0);
        o.State = AIState.Target;

        MovementSystem.Tick(world, new List<Organism> { o }, SimulationConfig.Default);

        Assert.Equal(102f, o.Position.X, precision: 3);
    }
}
