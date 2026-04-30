using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class PerceptionSystemTests
{
    [Fact]
    public void Tick_FindsNearestFoodInRange()
    {
        var world = new World();
        world.SetFood(10, 10, true);
        world.SetFood(50, 50, true);

        Organism organism = Organism.NewBorn(new Vector2(40, 40), new Genome(1, 1, 30, 0.5f), 0);
        var organisms = new List<Organism> { organism };

        PerceptionSystem.Tick(world, organisms);

        Assert.NotNull(organism.TargetFood);
        Assert.Equal(10 * 4 + 2, organism.TargetFood!.Value.X, precision: 1);
        Assert.Equal(10 * 4 + 2, organism.TargetFood!.Value.Y, precision: 1);
    }

    [Fact]
    public void Tick_ReturnsNullWhenNoFoodInRange()
    {
        var world = new World();
        world.SetFood(50, 50, true);

        Organism organism = Organism.NewBorn(new Vector2(40, 40), new Genome(1, 1, 5, 0.5f), 0);
        var organisms = new List<Organism> { organism };

        PerceptionSystem.Tick(world, organisms);

        Assert.Null(organism.TargetFood);
    }

    [Fact]
    public void Tick_RespectsSenseRangeRadius()
    {
        var world = new World();
        world.SetFood(15, 10, true);
        Organism organism = Organism.NewBorn(new Vector2(42, 42), new Genome(1, 1, 25, 0.5f), 0);
        var organisms = new List<Organism> { organism };

        PerceptionSystem.Tick(world, organisms);

        Assert.NotNull(organism.TargetFood);
    }
}
