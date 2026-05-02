using DigitalTerrarium.Core;

namespace DigitalTerrarium.Systems;

/// <summary>
/// Handles dynamic biome changes: recovery of degraded tiles back to Grassland
/// </summary>
public static class BiomeSystem
{
    /// <summary>
    /// Process biome recovery: Mud tiles have a chance to recover to Grassland over time
    /// </summary>
    public static void Tick(World world, SimulationConfig config, int currentTick, Random rng)
    {
        int width = world.Biomes.Width;
        int height = world.Biomes.Height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var biome = world.Biomes.At(x, y);
                
                // Only process Mud tiles for recovery
                if (biome != BiomeType.Mud) continue;

                // Check if enough time has passed since last consumption
                if (!world.Biomes.CanRecover(x, y, currentTick, config.MudRecoveryCooldown)) continue;

                // Random chance to recover
                if (rng.NextDouble() < config.MudRecoveryRate)
                {
                    world.Biomes.SetTile(x, y, BiomeType.Grassland);
                }
            }
        }
    }
}