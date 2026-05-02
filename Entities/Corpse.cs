using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Entities;

public readonly struct Corpse
{
    public Vector2 Position { get; init; }
    public float DecayTimer { get; init; }      // Current remaining decay time (ticks)
    public float MaxDecayTime { get; init; }     // Total decay duration
    public Genome Genes { get; init; }           // For coloring
    public int Generation { get; init; }         // For visual indication
    public float Size { get; init; }             // Size based on organism (affects nutrient yield)

    public float DecayProgress => 1f - (DecayTimer / MaxDecayTime); // 0 = fresh, 1 = decayed

    /// Duration in ticks for a corpse to fully decay
    public const float DecayDuration = 180f; // ~3 seconds at 60fps, configurable

    /// Base nutrient yield from a corpse (at full size)
    public const float BaseNutrientYield = 2.0f; // How much biomass this corpse will release
    public static float NutrientYieldMultiplier { get; set; } = 1.0f; // Config multiplier

    /// How much is still remaining to be released (accumulates as corpse decays)
    public float RemainingNutrients { get; init; }

    public static Corpse Create(Vector2 position, Genome genes, int generation, float size = 1f)
    {
        return new Corpse
        {
            Position = position,
            DecayTimer = DecayDuration,
            MaxDecayTime = DecayDuration,
            Genes = genes,
            Generation = generation,
            Size = size,
            RemainingNutrients = BaseNutrientYield * size * NutrientYieldMultiplier
        };
    }

    /// <summary>
    /// Calculate nutrient spike amount for this decay tick
    /// </summary>
    public float GetNutrientYieldPerTick()
    {
        // Release nutrients gradually throughout decay
        // Earlier in decay = more nutrients (freshest)
        float decayStage = 1f - (DecayTimer / MaxDecayTime); // 0 = fresh, 1 = nearly gone
        float baseYield = RemainingNutrients / MaxDecayTime;
        
        // Add some variation - early decay releases more
        float stageMultiplier = MathHelper.Lerp(1.5f, 0.5f, decayStage);
        
        return baseYield * stageMultiplier;
    }
}

internal static class MathHelper
{
    public static float Lerp(float a, float b, float t) =>
        a + (b - a) * t;
}