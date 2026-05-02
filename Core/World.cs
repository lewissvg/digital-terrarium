using DigitalTerrarium.Core;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Core;

public class World
{
    public int Width { get; }
    public int Height { get; }
    public int TileSize { get; }
    public int PixelWidth => Width * TileSize;
    public int PixelHeight => Height * TileSize;

    private readonly float[,] _biomass;  // 0.0 to 1.0 density of food on each tile
    private int _tilesWithFood;          // Count of tiles above eat threshold

    public BiomeMap Biomes { get; }

    public World(SimulationConfig config)
    {
        Width    = config.WorldTilesX;
        Height   = config.WorldTilesY;
        TileSize = SimulationConfig.TileSize;
        _biomass = new float[Width, Height];
        _tilesWithFood = 0;
        Biomes = new BiomeMap(Width, Height);
    }

    // Convenience for tests that don't care about config
    public World() : this(SimulationConfig.Default) { }

    /// <summary>
    /// Returns true if tile has enough biomass to be eaten
    /// </summary>
    public bool HasFood(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return false;
        }

        return _biomass[tileX, tileY] >= BiomeProperties.BiomassEatThreshold;
    }

    public bool HasFoodAtPixel(float px, float py)
    {
        int tileX = (int)(px / TileSize);
        int tileY = (int)(py / TileSize);
        return HasFood(tileX, tileY);
    }

    /// <summary>
    /// Get raw biomass level (0.0 to 1.0)
    /// </summary>
    public float GetBiomass(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return 0f;
        }
        return _biomass[tileX, tileY];
    }

    /// <summary>
    /// Set exact biomass level (clamped to 0-1)
    /// </summary>
    public void SetBiomass(int tileX, int tileY, float value)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return;
        }

        float oldBiomass = _biomass[tileX, tileY];
        _biomass[tileX, tileY] = MathHelper.Clamp(value, 0f, 1f);

        // Update tilesWithFood count
        bool wasEdible = oldBiomass >= BiomeProperties.BiomassEatThreshold;
        bool isEdible = _biomass[tileX, tileY] >= BiomeProperties.BiomassEatThreshold;
        if (wasEdible != isEdible)
        {
            _tilesWithFood += isEdible ? 1 : -1;
        }
    }

    /// <summary>
    /// Backwards-compatible method: set food presence as bool (sets biomass to full or zero)
    /// </summary>
    public void SetFood(int tileX, int tileY, bool present)
    {
        SetBiomass(tileX, tileY, present ? 0.8f : 0f);
    }

    /// <summary>
    /// Add to biomass level (for regeneration)
    /// </summary>
    public void AddBiomass(int tileX, int tileY, float amount)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return;
        }

        float oldBiomass = _biomass[tileX, tileY];
        float newBiomass = MathHelper.Clamp(oldBiomass + amount, 0f, 1f);
        _biomass[tileX, tileY] = newBiomass;

        // Update tilesWithFood count
        bool wasEdible = oldBiomass >= BiomeProperties.BiomassEatThreshold;
        bool isEdible = newBiomass >= BiomeProperties.BiomassEatThreshold;
        if (wasEdible != isEdible)
        {
            _tilesWithFood += isEdible ? 1 : -1;
        }
    }

    /// <summary>
    /// Subtract from biomass (for consumption)
    /// </summary>
    public void ConsumeBiomass(int tileX, int tileY, float amount)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
        {
            return;
        }

        float oldBiomass = _biomass[tileX, tileY];
        float newBiomass = MathHelper.Clamp(oldBiomass - amount, 0f, 1f);
        _biomass[tileX, tileY] = newBiomass;

        // Update tilesWithFood count
        bool wasEdible = oldBiomass >= BiomeProperties.BiomassEatThreshold;
        bool isEdible = newBiomass >= BiomeProperties.BiomassEatThreshold;
        if (wasEdible != isEdible)
        {
            _tilesWithFood += isEdible ? 1 : -1;
        }
    }

    public int CountFood() => _tilesWithFood;

    public int CountEmptyTiles() => (Width * Height) - _tilesWithFood;

    public IEnumerable<(int x, int y)> EmptyTiles()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (_biomass[x, y] < BiomeProperties.BiomassEatThreshold)
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
                _biomass[x, y] = 0f;
            }
        }

        _tilesWithFood = 0;
    }

    /// <summary>
    /// Regenerate biomass on empty tiles (slow accumulation)
    /// </summary>
    public void RegenerateFood(float regenRate, Random rng)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                // Skip tiles already at max
                if (_biomass[x, y] >= 1f) continue;

                float biomeMult = BiomeProperties.FoodRegenMultiplier(Biomes.At(x, y));
                if (rng.NextDouble() < regenRate * biomeMult)
                {
                    AddBiomass(x, y, 0.1f); // Small incremental growth
                }
            }
        }
    }

    /// <summary>
    /// Seed initial food density as scattered biomass
    /// </summary>
    public void SeedFoodDensity(float density, Random rng)
    {
        int targetTiles = (int)(Width * Height * density);
        var tiles = new List<(int x, int y)>();
        
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                tiles.Add((x, y));

        // Shuffle tiles
        for (int i = tiles.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (tiles[i], tiles[j]) = (tiles[j], tiles[i]);
        }

        // Set biomass on first targetTiles with varying amounts
        for (int i = 0; i < Math.Min(targetTiles, tiles.Count); i++)
        {
            var (x, y) = tiles[i];
            float amount = 0.5f + (float)rng.NextDouble() * 0.5f; // 0.5 to 1.0
            _biomass[x, y] = amount;
            if (amount >= BiomeProperties.BiomassEatThreshold)
            {
                _tilesWithFood++;
            }
        }
    }
}