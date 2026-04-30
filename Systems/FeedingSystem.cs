using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Systems;

public static class FeedingSystem
{
    public static void Tick(World world, List<Organism> organisms, SimulationConfig config)
    {
        int tileSize = world.TileSize;
        foreach (var o in organisms)
        {
            int tx = (int)(o.Position.X / tileSize);
            int ty = (int)(o.Position.Y / tileSize);
            if (!world.HasFood(tx, ty)) continue;

            var biome = world.Biomes.At(tx, ty);
            float match = BiomeProperties.Match(o.Genes.TerrainAffinity, biome);
            float baseYield = BiomeProperties.FoodYieldBase(biome);
            float effectiveYield = baseYield + (1f - baseYield) * match;

            world.SetFood(tx, ty, false);
            o.Energy = MathF.Min(o.MaxEnergy, o.Energy + config.FoodEnergyValue * effectiveYield);
        }
    }
}
