using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class FeedingSystem
{
    public static void Tick(World world, List<Organism> organisms, SimulationConfig config, int currentTick, Random rng)
    {
        int tileSize = world.TileSize;
        foreach (var o in organisms)
        {
            int tx = (int)(o.Position.X / tileSize);
            int ty = (int)(o.Position.Y / tileSize);
            
            // Check if tile has enough biomass to eat
            if (!world.HasFood(tx, ty)) continue;

            var biome = world.Biomes.At(tx, ty);
            float match = BiomeProperties.Match(o.Genes.TerrainAffinity, biome);
            float baseYield = BiomeProperties.FoodYieldBase(biome);
            float effectiveYield = baseYield + (1f - baseYield) * match;

            // Get current biomass level for yield calculation
            float biomassLevel = world.GetBiomass(tx, ty);
            float biomassMultiplier = MathHelper.Lerp(0.5f, 1f, biomassLevel); // 50-100% based on density

            // Consume biomass and give energy
            world.ConsumeBiomass(tx, ty, config.BiomassConsumptionRate);
            o.Energy = MathF.Min(o.MaxEnergy, o.Energy + config.FoodEnergyValue * effectiveYield * biomassMultiplier);

            // Record consumption for biome recovery timing
            world.Biomes.RecordConsumption(tx, ty, currentTick);

            // Biome degradation: consuming food on Grassland has a chance to degrade it to Mud
            if (biome == BiomeType.Grassland && rng.NextDouble() < config.GrasslandDegradationChance)
            {
                world.Biomes.SetTile(tx, ty, BiomeType.Mud);
            }
        }
    }
}