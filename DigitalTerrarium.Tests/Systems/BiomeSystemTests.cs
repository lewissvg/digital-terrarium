using DigitalTerrarium.Core;
using DigitalTerrarium.Systems;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Tests.Systems;

public class BiomeSystemTests
{
    [Fact]
    public void Tick_MudTile_DoesNotRecover_IfRecentlyConsumed()
    {
        var config = SimulationConfig.Default with { MudRecoveryCooldown = 100 };
        var world = new World();
        int tx = 10, ty = 10;
        
        // Set tile to Mud
        world.Biomes.SetTile(tx, ty, BiomeType.Mud);
        
        // Record consumption recently (within cooldown)
        world.Biomes.RecordConsumption(tx, ty, 50);
        
        // Run BiomeSystem at tick 51 (only 1 tick since consumption, cooldown is 100)
        BiomeSystem.Tick(world, config, 51, new Random(42));
        
        // Tile should still be Mud (not recovered yet)
        Assert.Equal(BiomeType.Mud, world.Biomes.At(tx, ty));
    }

    [Fact]
    public void Tick_MudTile_CanRecover_AfterCooldown()
    {
        var config = SimulationConfig.Default with { MudRecoveryCooldown = 10, MudRecoveryRate = 1.0f };
        var world = new World();
        int tx = 10, ty = 10;
        
        // Set tile to Mud
        world.Biomes.SetTile(tx, ty, BiomeType.Mud);
        
        // Record consumption in the past (beyond cooldown)
        world.Biomes.RecordConsumption(tx, ty, 0);
        
        // Run BiomeSystem at tick 20 (20 ticks since consumption, cooldown is 10)
        BiomeSystem.Tick(world, config, 20, new Random(42));
        
        // Tile should now be Grassland (100% recovery rate)
        Assert.Equal(BiomeType.Grassland, world.Biomes.At(tx, ty));
    }

    [Fact]
    public void Tick_MudTile_LowRecoveryRate_MayNotRecover()
    {
        var config = SimulationConfig.Default with { MudRecoveryCooldown = 0, MudRecoveryRate = 0.0f };
        var world = new World();
        int tx = 10, ty = 10;
        
        // Set tile to Mud
        world.Biomes.SetTile(tx, ty, BiomeType.Mud);
        
        // Run BiomeSystem many times
        var rng = new Random(42); // 0% recovery rate
        for (int i = 0; i < 100; i++)
        {
            BiomeSystem.Tick(world, config, i, rng);
        }
        
        // Tile should still be Mud (0% recovery rate)
        Assert.Equal(BiomeType.Mud, world.Biomes.At(tx, ty));
    }

    [Fact]
    public void Tick_NonMudTile_NotAffected()
    {
        var config = SimulationConfig.Default with { MudRecoveryRate = 1.0f };
        var world = new World();
        int tx = 10, ty = 10;
        
        // Set tiles to different types
        world.Biomes.SetTile(tx, ty, BiomeType.Grassland);
        
        var rng = new Random(42);
        BiomeSystem.Tick(world, config, 100, rng);
        
        // Grassland should remain Grassland
        Assert.Equal(BiomeType.Grassland, world.Biomes.At(tx, ty));
    }
}