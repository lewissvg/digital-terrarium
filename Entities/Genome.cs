namespace DigitalTerrarium.Entities;

public record Genome(
    float Speed,
    float Metabolism,
    float SenseRange,
    float DietType,
    float TerrainAffinity)
{
    public const float MinSpeed           = 0.1f;
    public const float MinMetabolism      = 0.1f;
    public const float MinSenseRange      = 1f;
    public const float MinDietType        = 0f;
    public const float MaxDietType        = 1f;
    public const float MinTerrainAffinity = 0f;
    public const float MaxTerrainAffinity = 1f;

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
            TerrainAffinity: DriftAdd(TerrainAffinity, MinTerrainAffinity, MaxTerrainAffinity));
    }
}
