using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class AISystem
{
    private const int WanderRefreshTicks = 30;

    public static void Tick(List<Organism> organisms, SimulationConfig config, Random rng)
    {
        foreach (var organism in organisms)
        {
            // Auto-exit Rest when energy reaches recovery cap
            if (organism.State == AIState.Rest)
            {
                float cap = organism.MaxEnergy * SimulationConfig.RestRecoveryCapFraction;
                if (organism.Energy >= cap)
                {
                    organism.State = AIState.Wander;
                    // fall through to wander/target logic below
                }
                else
                {
                    // remain in Rest, skip remaining AI processing
                    continue;
                }
            }

            float restThreshold = organism.MaxEnergy * SimulationConfig.RestThresholdFraction;

            if (organism.Target.HasValue && organism.Energy >= restThreshold)
            {
                organism.State = AIState.Target;
                Vector2 direction = organism.Target.Value - organism.Position;
                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                }
                organism.Velocity = direction * organism.Genes.Speed;
            }
            else if (organism.Energy < restThreshold && organism.Target == null)
            {
                organism.State = AIState.Rest;
                organism.Velocity = Vector2.Zero;
            }
            else
            {
                organism.State = AIState.Wander;
                if (organism.WanderTicksRemaining <= 0)
                {
                    float angle = (float)(rng.NextDouble() * Math.PI * 2);
                    organism.Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (organism.Genes.Speed * 0.5f);
                    organism.WanderTicksRemaining = WanderRefreshTicks;
                }
                else
                {
                    organism.WanderTicksRemaining--;
                }
            }
        }
    }
}
