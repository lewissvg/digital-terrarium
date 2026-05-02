namespace DigitalTerrarium.Core;

public record SimulationConfig
{
    public int Seed { get; init; } = 0;
    public float InitialFoodDensity { get; init; } = 0.15f;
    public float FoodRegenRate { get; init; } = 0.003f;     // Biomass accumulation rate per tick
    public float FoodEnergyValue { get; init; } = 25f;
    public float BiomassEatThreshold { get; init; } = 0.3f;  // Minimum biomass needed to eat
    public float BiomassConsumptionRate { get; init; } = 0.4f; // How much biomass eating removes
    public int StartingPopulation { get; init; } = 50;
    public float MutationRate { get; init; } = 0.05f;
    public float InitialMaxDietType { get; init; } = 0.4f;
    public float PerceptionCostCoefficient { get; init; } = 0.0001f;
    public float ReproductionThreshold { get; init; } = 0.9f;
    public float EnergyDrainCoefficient { get; init; } = 0.05f;
    public float RestEnergyRecovery { get; init; } = 0.05f;
    public int RestStagnationThreshold { get; init; } = 100;
    public float RestStagnationPenaltyRate { get; init; } = 0.02f;
    public float HungerDilationMultiplier { get; init; } = 3.0f;
    public float InitialWanderlustMin { get; init; } = 0.0f;
    public float InitialWanderlustMax { get; init; } = 1.0f;

    public int BiomeNoiseScale { get; init; } = 20;
    public float MudSandBalance { get; init; } = 0.5f;
    public int SpatialCellPixels { get; init; } = 80;
    public float ReproductionMatchThreshold { get; init; } = 0.5f;
    public float CarnivoreTax { get; init; } = 0.5f;
    public int StatsSmoothingTicks { get; init; } = 30;
    public int WindowWidth { get; init; } = 1424;
    public int WindowHeight { get; init; } = 1200;
    public int ViewportSize { get; init; } = 1200;

    public int WorldTilesX { get; init; } = 250;
    public int WorldTilesY { get; init; } = 250;

    // Decomposition (corpse nutrients)
    public float DecompositionRate { get; init; } = 1.0f;    // Multiplier for nutrient release (0-2)
    public float CorpseNutrientYield { get; init; } = 2.5f;  // Base nutrients per corpse
    public float CorpseSpreadRadius { get; init; } = 1.5f;   // How far nutrients spread to neighbors

    // Biome degradation & recovery
    public float GrasslandDegradationChance { get; init; } = 0.02f;  // 2% chance grassland degrades to mud when eaten
    public float MudRecoveryRate { get; init; } = 0.0005f;            // 0.05% chance per tick for mud to recover
    public int MudRecoveryCooldown { get; init; } = 600;              // Ticks of no consumption before recovery can start

    public const int TileSize = 4;
    public const int TickRateHz = 30;
    public const float RestThresholdFraction = 0.20f;
    public const float RestRecoveryCapFraction = 0.50f;

    public static SimulationConfig Default => new();

    public static SimulationConfig WithWallClockSeed() =>
        Default with { Seed = (int)(DateTime.UtcNow.Ticks & int.MaxValue) };
}
