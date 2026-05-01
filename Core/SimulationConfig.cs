namespace DigitalTerrarium.Core;

public record SimulationConfig
{
    public int Seed { get; init; } = 0;
    public float InitialFoodDensity { get; init; } = 0.15f;
    public float FoodRegenRate { get; init; } = 2.0f;
    public float FoodEnergyValue { get; init; } = 25f;
    public int StartingPopulation { get; init; } = 50;
    public float MutationRate { get; init; } = 0.05f;
    public float InitialMaxDietType { get; init; } = 0.4f;
    public float PerceptionCostCoefficient { get; init; } = 0.0001f;
    public float ReproductionThreshold { get; init; } = 0.9f;
    public float EnergyDrainCoefficient { get; init; } = 0.05f;
    public float RestEnergyRecovery { get; init; } = 0.05f;

    public int BiomeNoiseScale { get; init; } = 20;
    public float MudSandBalance { get; init; } = 0.5f;
    public int SpatialCellPixels { get; init; } = 80;
    public float ReproductionMatchThreshold { get; init; } = 0.5f;
    public float CarnivoreTax { get; init; } = 0.5f;
    public int StatsSmoothingTicks { get; init; } = 30;

    public int WorldTilesX { get; init; } = 300;
    public int WorldTilesY { get; init; } = 300;
    public const int TileSize = 4;
    public const int TickRateHz = 30;
    public const float RestThresholdFraction = 0.20f;
    public const float RestRecoveryCapFraction = 0.50f;

    public static SimulationConfig Default => new();

    public static SimulationConfig WithWallClockSeed() =>
        Default with { Seed = (int)(DateTime.UtcNow.Ticks & int.MaxValue) };
}
