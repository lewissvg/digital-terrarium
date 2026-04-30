namespace DigitalTerrarium.Core;

public record SimulationConfig
{
    public int Seed { get; init; } = 0;
    public float InitialFoodDensity { get; init; } = 0.15f;
    public float FoodRegenRate { get; init; } = 2.0f;
    public float FoodEnergyValue { get; init; } = 25f;
    public int StartingPopulation { get; init; } = 50;
    public float MutationRate { get; init; } = 0.05f;
    public float ReproductionThreshold { get; init; } = 0.9f;
    public float EnergyDrainCoefficient { get; init; } = 0.05f;
    public float RestEnergyRecovery { get; init; } = 0.05f;

    public const int WorldTilesX = 200;
    public const int WorldTilesY = 200;
    public const int TileSize = 4;
    public const int TickRateHz = 30;
    public const float RestThresholdFraction = 0.20f;
    public const float RestRecoveryCapFraction = 0.50f;

    public static SimulationConfig Default => new();

    public static SimulationConfig WithWallClockSeed() =>
        Default with { Seed = (int)(DateTime.UtcNow.Ticks & int.MaxValue) };
}
