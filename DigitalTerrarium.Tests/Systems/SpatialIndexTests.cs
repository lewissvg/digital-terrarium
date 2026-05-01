using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;
using Xunit;

namespace DigitalTerrarium.Tests.Systems;

public class SpatialIndexTests
{
    private static Organism Make(float x, float y) =>
        Organism.NewBorn(new Vector2(x, y), new Genome(2, 1, 30, 0.5f, 0.5f), 0);

    [Fact]
    public void Build_PutsOrganismInCorrectCell()
    {
        var index = new SpatialIndex(worldPixelWidth: 1200, worldPixelHeight: 1200, cellPixels: 80);
        var o = Make(100, 100); // → cell (1, 1)
        index.Build(new List<Organism> { o });

        var found = index.Query(new Vector2(100, 100), 1f).ToList();
        Assert.Contains(o, found);
    }

    [Fact]
    public void Build_ClampsOutOfBoundsToBoundaryCell()
    {
        var index = new SpatialIndex(1200, 1200, 80);
        var a = Make(-5, 1500); // off both ends
        index.Build(new List<Organism> { a });

        var found = index.Query(new Vector2(0, 1199), 1f).ToList();
        Assert.Contains(a, found);
    }

    [Fact]
    public void Query_ReturnsOrganismsInRadiusNeighbourhood()
    {
        var index = new SpatialIndex(1200, 1200, 80);
        var near    = Make(100, 100);   // cell (1, 1)
        var sameCell = Make(140, 110);  // cell (1, 1)
        var adjCell = Make(190, 100);   // cell (2, 1)
        var farCell = Make(500, 500);   // cell (6, 6)
        index.Build(new List<Organism> { near, sameCell, adjCell, farCell });

        var results = index.Query(new Vector2(100, 100), radius: 50f).ToList();
        Assert.Contains(near, results);
        Assert.Contains(sameCell, results);
        Assert.Contains(adjCell, results);
        Assert.DoesNotContain(farCell, results);
    }

    [Fact]
    public void Query_RadiusLargerThanCell_ScansMoreCells()
    {
        var index = new SpatialIndex(1200, 1200, 80);
        var near = Make(100, 100); // cell (1, 1)
        var two  = Make(260, 100); // cell (3, 1)  — 2 cells away
        index.Build(new List<Organism> { near, two });

        var results = index.Query(new Vector2(100, 100), radius: 160f).ToList();
        Assert.Contains(near, results);
        Assert.Contains(two, results);
    }

    [Fact]
    public void Build_RebuildsClearsPreviousState()
    {
        var index = new SpatialIndex(1200, 1200, 80);
        var first  = Make(100, 100);
        var second = Make(500, 500);

        index.Build(new List<Organism> { first });
        index.Build(new List<Organism> { second });

        var fromFirstPos = index.Query(new Vector2(100, 100), 1f).ToList();
        Assert.DoesNotContain(first, fromFirstPos);

        var fromSecondPos = index.Query(new Vector2(500, 500), 1f).ToList();
        Assert.Contains(second, fromSecondPos);
    }

    [Fact]
    public void Query_DeterministicOrder()
    {
        var index = new SpatialIndex(1200, 1200, 80);
        var a = Make(100, 100);
        var b = Make(120, 110);
        var c = Make(140, 105);
        index.Build(new List<Organism> { a, b, c });

        var first  = index.Query(new Vector2(100, 100), 50f).ToList();
        var second = index.Query(new Vector2(100, 100), 50f).ToList();
        Assert.Equal(first, second);
    }
}
