using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public class SpatialIndex
{
    private readonly List<Organism>[,] _cells;
    public int Width { get; }
    public int Height { get; }
    public int CellPixels { get; }

    public SpatialIndex(int worldPixelWidth, int worldPixelHeight, int cellPixels)
    {
        if (cellPixels <= 0) throw new ArgumentOutOfRangeException(nameof(cellPixels));
        CellPixels = cellPixels;
        Width  = (worldPixelWidth  + cellPixels - 1) / cellPixels;
        Height = (worldPixelHeight + cellPixels - 1) / cellPixels;
        _cells = new List<Organism>[Width, Height];
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _cells[x, y] = new List<Organism>();
    }

    public void Build(IReadOnlyList<Organism> organisms)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _cells[x, y].Clear();

        foreach (var o in organisms)
        {
            int cx = Math.Clamp((int)(o.Position.X / CellPixels), 0, Width  - 1);
            int cy = Math.Clamp((int)(o.Position.Y / CellPixels), 0, Height - 1);
            _cells[cx, cy].Add(o);
        }
    }

    public IEnumerable<Organism> Query(Vector2 position, float radius)
    {
        int cx = Math.Clamp((int)(position.X / CellPixels), 0, Width  - 1);
        int cy = Math.Clamp((int)(position.Y / CellPixels), 0, Height - 1);
        int reach = Math.Max(1, (int)MathF.Ceiling(radius / CellPixels));

        int x0 = Math.Max(0, cx - reach);
        int x1 = Math.Min(Width  - 1, cx + reach);
        int y0 = Math.Max(0, cy - reach);
        int y1 = Math.Min(Height - 1, cy + reach);

        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                foreach (var o in _cells[x, y])
                    yield return o;
    }
}
