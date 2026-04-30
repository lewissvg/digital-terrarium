namespace DigitalTerrarium.Entities;

public record Genome(float Speed, float Metabolism, float SenseRange, float DietType)
{
    public const float MinSpeed = 0.1f;
    public const float MinMetabolism = 0.1f;
    public const float MinSenseRange = 1f;
    public const float MinDietType = 0f;
    public const float MaxDietType = 1f;

    public Genome Mutate(float rate, Random rng)
    {
        float DriftMul(float value, float min)
        {
            float multiplier = 1f + ((float)rng.NextDouble() * 2f - 1f) * rate;
            return MathF.Max(min, value * multiplier);
        }

        float DriftAdd(float value)
        {
            float delta = ((float)rng.NextDouble() * 2f - 1f) * rate;
            float drifted = value + delta;
            if (drifted < MinDietType) drifted = MinDietType;
            else if (drifted > MaxDietType) drifted = MaxDietType;
            return drifted;
        }

        return new Genome(
            Speed: DriftMul(Speed, MinSpeed),
            Metabolism: DriftMul(Metabolism, MinMetabolism),
            SenseRange: DriftMul(SenseRange, MinSenseRange),
            DietType: DriftAdd(DietType));
    }
}
