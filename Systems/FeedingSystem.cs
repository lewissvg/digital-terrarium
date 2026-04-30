using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Systems;

public static class FeedingSystem
{
    public static void Tick(World world, List<Organism> organisms, SimulationConfig config)
    {
        int tileSize = world.TileSize;
        foreach (var organism in organisms)
        {
            int tileX = (int)(organism.Position.X / tileSize);
            int tileY = (int)(organism.Position.Y / tileSize);
            if (!world.HasFood(tileX, tileY))
            {
                continue;
            }

            world.SetFood(tileX, tileY, false);
            organism.Energy = MathF.Min(organism.MaxEnergy, organism.Energy + config.FoodEnergyValue);
        }
    }
}
