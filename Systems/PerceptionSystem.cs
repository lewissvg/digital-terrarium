using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class PerceptionSystem
{
    private const float CarnivorePlantThreshold = 0.95f;
    private const float AttackBuffer = 0.2f;

    public static void Tick(World world, List<Organism> organisms, SpatialIndex spatialIndex, SimulationConfig config)
    {
        int tileSize = world.TileSize;
        float halfTile = tileSize * 0.5f;

        foreach (var organism in organisms)
        {
            // Calculate hunger factor: 1.0 when full, up to HungerDilationMultiplier when starving
            float energyRatio = organism.Energy / organism.MaxEnergy;
            float hungerFactor = 1f + (1f - energyRatio) * (config.HungerDilationMultiplier - 1f);
            float effectiveRange = organism.Genes.SenseRange * hungerFactor;
            float effectiveRangeSquared = effectiveRange * effectiveRange;

            float perceptionCost = config.PerceptionCostCoefficient * MathF.PI * effectiveRange * effectiveRange;
            if (organism.State != AIState.Rest)
                organism.Energy -= perceptionCost;

            float bestFoodDistanceSquared = float.MaxValue;
            Vector2? bestFoodPosition = null;

            if (organism.Genes.DietType < CarnivorePlantThreshold)
            {
                int centerTileX = (int)(organism.Position.X / tileSize);
                int centerTileY = (int)(organism.Position.Y / tileSize);
                int radiusTiles = (int)MathF.Ceiling(effectiveRange / tileSize);

                for (int dy = -radiusTiles; dy <= radiusTiles; dy++)
                {
                    int tileY = centerTileY + dy;
                    if (tileY < 0 || tileY >= world.Height) continue;

                    for (int dx = -radiusTiles; dx <= radiusTiles; dx++)
                    {
                        int tileX = centerTileX + dx;
                        if (tileX < 0 || tileX >= world.Width) continue;
                        if (!world.HasFood(tileX, tileY)) continue;

                        var center = new Vector2(tileX * tileSize + halfTile, tileY * tileSize + halfTile);
                        float distanceSquared = Vector2.DistanceSquared(organism.Position, center);
                        if (distanceSquared <= effectiveRangeSquared && distanceSquared < bestFoodDistanceSquared)
                        {
                            bestFoodDistanceSquared = distanceSquared;
                            bestFoodPosition = center;
                        }
                    }
                }
            }

            // Prey detection uses base sense range (hunger doesn't help find predators)
            float baseRangeSquared = organism.Genes.SenseRange * organism.Genes.SenseRange;
            float bestPreyDistanceSquared = float.MaxValue;
            Organism bestPrey = null;

            foreach (var other in spatialIndex.Query(organism.Position, organism.Genes.SenseRange))
            {
                if (ReferenceEquals(other, organism)) continue;
                if (other.Energy <= 0f) continue;
                if (organism.Genes.DietType - other.Genes.DietType <= AttackBuffer) continue;

                float distanceSquared = Vector2.DistanceSquared(organism.Position, other.Position);
                if (distanceSquared <= baseRangeSquared && distanceSquared < bestPreyDistanceSquared)
                {
                    bestPreyDistanceSquared = distanceSquared;
                    bestPrey = other;
                }
            }

            if (bestPrey != null && bestPreyDistanceSquared <= bestFoodDistanceSquared)
            {
                organism.TargetPrey = bestPrey;
                organism.Target = bestPrey.Position;
            }
            else if (bestFoodPosition.HasValue)
            {
                organism.TargetPrey = null;
                organism.Target = bestFoodPosition;
            }
            else
            {
                organism.TargetPrey = null;
                organism.Target = null;
            }
        }
    }
}
