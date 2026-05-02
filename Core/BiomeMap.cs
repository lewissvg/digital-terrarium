using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Core;

public class BiomeMap
{
    private readonly BiomeType[,] _biomes;
    private readonly int[,] _lastConsumptionTick; // Track when tile was last consumed
    public int Width  { get; }
    public int Height { get; }

    public BiomeMap(int width, int height)
    {
        Width  = width;
        Height = height;
        _biomes = new BiomeType[width, height];
        _lastConsumptionTick = new int[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _biomes[x, y] = BiomeType.Grassland;
    }

    public BiomeType At(int tileX, int tileY)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
            return BiomeType.Grassland;
        return _biomes[tileX, tileY];
    }

    public BiomeType AtPixel(float px, float py, int tileSize)
        => At((int)(px / tileSize), (int)(py / tileSize));

    public void Generate(int noiseGridSize, float mudSandBalance, Random rng)
    {
        const float bandSpread = 1f / 3f;
        float shift = mudSandBalance - 0.5f;
        float t1 = MathHelper.Clamp(0.5f - bandSpread / 2f - shift, 0.05f, 0.95f);
        float t2 = MathHelper.Clamp(0.5f + bandSpread / 2f - shift, 0.05f, 0.95f);
        if (t2 < t1) (t1, t2) = (t2, t1);

        var noise = BiomeNoise.Generate(Width, Height, noiseGridSize, rng);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float v = noise[x, y];
                _biomes[x, y] = v < t1 ? BiomeType.Mud
                              : v < t2 ? BiomeType.Grassland
                              : BiomeType.Sand;
            }
        }
    }

    public (int mud, int grassland, int sand) CountTiles()
    {
        int m = 0, g = 0, s = 0;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                switch (_biomes[x, y])
                {
                    case BiomeType.Mud:       m++; break;
                    case BiomeType.Grassland: g++; break;
                    case BiomeType.Sand:      s++; break;
                }
        return (m, g, s);
    }

    public void SetForTest(int tileX, int tileY, BiomeType biome)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height) return;
        _biomes[tileX, tileY] = biome;
    }

    public void SetTile(int tileX, int tileY, BiomeType biome)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height) return;
        _biomes[tileX, tileY] = biome;
    }

    /// <summary>
    /// Records that food was consumed at this tile, used for recovery timing
    /// </summary>
    public void RecordConsumption(int tileX, int tileY, int currentTick)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height) return;
        _lastConsumptionTick[tileX, tileY] = currentTick;
    }

    /// <summary>
    /// Returns true if enough ticks have passed since last consumption
    /// </summary>
    public bool CanRecover(int tileX, int tileY, int currentTick, int cooldownTicks)
    {
        if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height) return false;
        return (currentTick - _lastConsumptionTick[tileX, tileY]) >= cooldownTicks;
    }
}
