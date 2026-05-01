using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class MovementSystem
{
    private const float Mass = 1f;

    public static void Tick(World world, List<Organism> organisms, SimulationConfig config)
    {
        float maxX = world.PixelWidth - 0.001f;
        float maxY = world.PixelHeight - 0.001f;
        float restCap = SimulationConfig.RestRecoveryCapFraction;

        foreach (var o in organisms)
        {
            o.Age++;

            if (o.State == AIState.Rest)
            {
                float cap = o.MaxEnergy * restCap;
                if (o.Energy < cap)
                    o.Energy = MathF.Min(cap, o.Energy + config.RestEnergyRecovery);
                continue;
            }

            // Drain: based on INTENDED velocity (raw)
            float intendedSpeed = o.Velocity.Length();
            float drain = config.EnergyDrainCoefficient * intendedSpeed * intendedSpeed * o.Genes.Metabolism * Mass;
            o.Energy -= drain;

            // Effective displacement: scaled by biome × affinity match
            var biome = world.Biomes.AtPixel(o.Position.X, o.Position.Y, world.TileSize);
            float match = BiomeProperties.Match(o.Genes.TerrainAffinity, biome);
            float baseMove = BiomeProperties.MovementBase(biome);
            float effectiveMult = baseMove + (1f - baseMove) * match;

            o.Position += o.Velocity * effectiveMult;

            bool clamped = false;
            if (o.Position.X < 0)         { o.Position.X = 0;    o.Velocity.X = 0; clamped = true; }
            else if (o.Position.X > maxX) { o.Position.X = maxX; o.Velocity.X = 0; clamped = true; }
            if (o.Position.Y < 0)         { o.Position.Y = 0;    o.Velocity.Y = 0; clamped = true; }
            else if (o.Position.Y > maxY) { o.Position.Y = maxY; o.Velocity.Y = 0; clamped = true; }
            if (clamped) o.Target = null;
        }
    }
}
