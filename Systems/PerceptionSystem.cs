using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class PerceptionSystem
{
    public static void Tick(World world, List<Organism> organisms, SimulationConfig config)
    {
        int tileSize = world.TileSize;
        float halfTile = tileSize * 0.5f;

        foreach (var organism in organisms)
        {
            float perceptionCost = config.PerceptionCostCoefficient * MathF.PI * organism.Genes.SenseRange * organism.Genes.SenseRange;
            organism.Energy -= perceptionCost;

            int centerTileX = (int)(organism.Position.X / tileSize);
            int centerTileY = (int)(organism.Position.Y / tileSize);
            int radiusTiles = (int)MathF.Ceiling(organism.Genes.SenseRange / tileSize);
            float rangeSquared = organism.Genes.SenseRange * organism.Genes.SenseRange;

            float bestDistanceSquared = float.MaxValue;
            Vector2? best = null;

            for (int dy = -radiusTiles; dy <= radiusTiles; dy++)
            {
                int tileY = centerTileY + dy;
                if (tileY < 0 || tileY >= world.Height)
                {
                    continue;
                }

                for (int dx = -radiusTiles; dx <= radiusTiles; dx++)
                {
                    int tileX = centerTileX + dx;
                    if (tileX < 0 || tileX >= world.Width || !world.HasFood(tileX, tileY))
                    {
                        continue;
                    }

                    var center = new Vector2(tileX * tileSize + halfTile, tileY * tileSize + halfTile);
                    float distanceSquared = Vector2.DistanceSquared(organism.Position, center);
                    if (distanceSquared <= rangeSquared && distanceSquared < bestDistanceSquared)
                    {
                        bestDistanceSquared = distanceSquared;
                        best = center;
                    }
                }
            }

            organism.Target = best;
            organism.TargetPrey = null;
        }
    }
}
