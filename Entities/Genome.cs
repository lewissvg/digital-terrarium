namespace DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

public record Genome(
    float Speed,
    float Metabolism,
    float SenseRange,
    float DietType,
    float TerrainAffinity,
    float Wanderlust = 0.5f) // Default 0.5 = neutral, no bias
{
    public const float MinSpeed           = 0.1f;
    public const float MinMetabolism      = 0.1f;
    public const float MinSenseRange      = 1f;
    public const float MinDietType        = 0f;
    public const float MaxDietType        = 1f;
    public const float MinTerrainAffinity = 0f;
    public const float MaxTerrainAffinity = 1f;
    public const float MinWanderlust      = 0f;
    public const float MaxWanderlust      = 1f;

    private static float Clamp(float v, float min, float max) => v < min ? min : v > max ? max : v;

    /// <summary>
    /// Core body radius based on metabolism (higher metabolism = smaller, faster body)
    /// </summary>
    public float BodyRadius => Clamp(4f / Metabolism, 3f, 10f);

    /// <summary>
    /// Head/antenna size based on sense range (better sensors = bigger head)
    /// </summary>
    public float HeadRadius => Clamp(SenseRange / 15f, 2f, 6f);

    /// <summary>
    /// Number of tail segments based on speed (faster = more segments)
    /// </summary>
    public int TailSegments => (int)Clamp(Speed * 1.5f, 1f, 6f);

    /// <summary>
    /// Length of tail segments
    /// </summary>
    public float TailLength => Clamp(Speed * 2f, 4f, 20f);

    /// <summary>
    /// Antenna/appendage length based on sense range
    /// </summary>
    public float AntennaLength => Clamp(SenseRange / 5f, 2f, 15f);

    /// <summary>
    /// Overall organism width (used for rendering scale)
    /// </summary>
    public float BodyWidth => BodyRadius * 1.5f;

    public Genome Mutate(float rate, Random rng)
    {
        float DriftMul(float value, float min)
        {
            float multiplier = 1f + ((float)rng.NextDouble() * 2f - 1f) * rate;
            return MathF.Max(min, value * multiplier);
        }

        float DriftAdd(float value, float min, float max)
        {
            float delta = ((float)rng.NextDouble() * 2f - 1f) * rate;
            float v = value + delta;
            if (v < min) v = min;
            else if (v > max) v = max;
            return v;
        }

        return new Genome(
            Speed:           DriftMul(Speed, MinSpeed),
            Metabolism:      DriftMul(Metabolism, MinMetabolism),
            SenseRange:      DriftMul(SenseRange, MinSenseRange),
            DietType:        DriftAdd(DietType, MinDietType, MaxDietType),
            TerrainAffinity: DriftAdd(TerrainAffinity, MinTerrainAffinity, MaxTerrainAffinity),
            Wanderlust:      DriftAdd(Wanderlust, MinWanderlust, MaxWanderlust));
    }
}