namespace DigitalTerrarium.Entities;

public record Genome(float Speed, float Metabolism, float SenseRange)
{
    public const float MinSpeed = 0.1f;
    public const float MinMetabolism = 0.1f;
    public const float MinSenseRange = 1f;

    public Genome Mutate(float rate, Random rng)
    {
        float Drift(float value, float min)
        {
            float multiplier = 1f + ((float)rng.NextDouble() * 2f - 1f) * rate;
            return MathF.Max(min, value * multiplier);
        }

        return new Genome(
            Speed: Drift(Speed, MinSpeed),
            Metabolism: Drift(Metabolism, MinMetabolism),
            SenseRange: Drift(SenseRange, MinSenseRange));
    }
}
