namespace DigitalTerrarium.Core;

public class World
{
    public int Width { get; } = SimulationConfig.WorldTilesX;
    public int Height { get; } = SimulationConfig.WorldTilesY;
    public int TileSize { get; } = SimulationConfig.TileSize;
    public int PixelWidth => Width * TileSize;
    public int PixelHeight => Height * TileSize;

    private readonly bool[,] _food;
    private int _foodCount;

    public BiomeMap Biomes { get; }

    public World()
    {
        _food = new bool[Width, Height];
        _foodCount = 0;
        Biomes = new BiomeMap(Width, Height);
    }

    public bool HasFood(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return false;
        }

        return _food[tileX, tileY];
    }

    public bool HasFoodAtPixel(float px, float py)
    {
        int tileX = (int)(px / TileSize);
        int tileY = (int)(py / TileSize);
        return HasFood(tileX, tileY);
    }

    public void SetFood(int tileX, int tileY, bool present)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return;
        }

        if (_food[tileX, tileY] == present)
        {
            return;
        }

        _food[tileX, tileY] = present;
        _foodCount += present ? 1 : -1;
    }

    public int CountFood() => _foodCount;

    public int CountEmptyTiles() => (Width * Height) - _foodCount;

    public IEnumerable<(int x, int y)> EmptyTiles()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (!_food[x, y])
                {
                    yield return (x, y);
                }
            }
        }
    }

    public void Clear()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                _food[x, y] = false;
            }
        }

        _foodCount = 0;
    }

    public void RegenerateFood(float regenRate, Random rng)
    {
        int empty = CountEmptyTiles();
        if (empty == 0) return;

        double basePerTileChance = regenRate / empty;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (_food[x, y]) continue;
                float biomeMult = BiomeProperties.FoodRegenMultiplier(Biomes.At(x, y));
                if (rng.NextDouble() < basePerTileChance * biomeMult)
                {
                    _food[x, y] = true;
                    _foodCount++;
                }
            }
        }
    }
}
