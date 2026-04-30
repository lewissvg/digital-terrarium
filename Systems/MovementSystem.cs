using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Systems;

public static class MovementSystem
{
    private const float Mass = 1f;

    public static void Tick(World world, List<Organism> organisms, SimulationConfig config)
    {
        float maxX = world.PixelWidth - 0.001f;
        float maxY = world.PixelHeight - 0.001f;
        float restCapFraction = SimulationConfig.RestRecoveryCapFraction;

        foreach (var organism in organisms)
        {
            organism.Age++;

            if (organism.State == AIState.Rest)
            {
                float cap = organism.MaxEnergy * restCapFraction;
                if (organism.Energy < cap)
                {
                    organism.Energy = MathF.Min(cap, organism.Energy + config.RestEnergyRecovery);
                }

                continue;
            }

            organism.Position += organism.Velocity;

            if (organism.Position.X < 0f)
            {
                organism.Position.X = 0f;
            }
            else if (organism.Position.X > maxX)
            {
                organism.Position.X = maxX;
            }

            if (organism.Position.Y < 0f)
            {
                organism.Position.Y = 0f;
            }
            else if (organism.Position.Y > maxY)
            {
                organism.Position.Y = maxY;
            }

            float speed = organism.Velocity.Length();
            float drain = config.EnergyDrainCoefficient * speed * speed * organism.Genes.Metabolism * Mass;
            organism.Energy -= drain;
        }
    }
}
