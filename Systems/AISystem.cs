using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class AISystem
{
    private const int WanderRefreshTicks = 30;
    private const float CriticalHungerFraction = 0.15f; // Below this, MUST seek food
    private const float WanderlustSpeedBonus = 0.75f; // Max bonus: wanderlust 1.0 = 75% speed increase
    private const float WanderlustRestPenalty = 0.5f; // Wanderlust reduces rest threshold by up to 50%

    public static void Tick(List<Organism> organisms, SimulationConfig config, Random rng)
    {
        foreach (var organism in organisms)
        {
            // FLEE BEHAVIOR: Highest priority - run from predators!
            if (organism.State == AIState.Flee && organism.Predator != null && organism.Predator.Energy > 0)
            {
                // Move directly away from predator
                Vector2 awayFromPredator = organism.Position - organism.Predator.Position;
                if (awayFromPredator.LengthSquared() > 0.0001f)
                {
                    awayFromPredator.Normalize();
                }
                else
                {
                    // If somehow at same position, pick random direction
                    float angle = (float)(rng.NextDouble() * Math.PI * 2);
                    awayFromPredator = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                }
                
                // Flee at increased speed (panic bonus)
                organism.Velocity = awayFromPredator * organism.Genes.Speed * 1.5f;
                continue; // Skip rest of AI logic while fleeing
            }

            // Wanderlust increases rest threshold: nomads resist resting
            float wanderlustPenalty = 1f - (organism.Genes.Wanderlust * WanderlustRestPenalty);
            float restThreshold = organism.MaxEnergy * SimulationConfig.RestThresholdFraction * wanderlustPenalty;
            float criticalHunger = organism.MaxEnergy * CriticalHungerFraction;
            bool isHungry = organism.Energy < restThreshold;
            bool isStarving = organism.Energy < criticalHunger;

            // Auto-exit Rest when energy reaches recovery cap OR stagnation penalty forces relocation
            if (organism.State == AIState.Rest)
            {
                float cap = organism.MaxEnergy * SimulationConfig.RestRecoveryCapFraction;
                bool energyRecovered = organism.Energy >= cap;
                bool forcedOutByStagnation = organism.MetabolismPenalty > 2f;
                if (energyRecovered || forcedOutByStagnation)
                {
                    organism.State = AIState.Wander;
                    // fall through to decision logic below
                }
                else
                {
                    // remain in Rest, skip remaining AI processing
                    continue;
                }
            }

            // HUNGER OVERRIDE: If starving or very hungry AND we can see food → pursue it!
            // This is the key fix: low energy + visible target = actively seek food, not rest
            if (isHungry && organism.Target.HasValue)
            {
                organism.State = AIState.Target;
                Vector2 direction = organism.Target.Value - organism.Position;
                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                }
                // Move faster when starving to reach food quicker
                float urgencyMult = isStarving ? 1.5f : 1.0f;
                organism.Velocity = direction * organism.Genes.Speed * urgencyMult;
                continue;
            }

            // Normal target pursuit (well-fed and see something interesting)
            if (organism.Target.HasValue)
            {
                organism.State = AIState.Target;
                Vector2 direction = organism.Target.Value - organism.Position;
                if (direction.LengthSquared() > 0.0001f)
                {
                    direction.Normalize();
                }
                organism.Velocity = direction * organism.Genes.Speed;
                continue;
            }

            // Low energy AND no target visible → Rest (wait for food to respawn nearby)
            // But wanderers resist this even when tired
            if (isHungry)
            {
                organism.State = AIState.Rest;
                organism.Velocity = Vector2.Zero;
                continue;
            }

            // Well-fed and no targets → Wander/explore
            // Wanderlust bonus: nomads explore faster
            organism.State = AIState.Wander;
            if (organism.WanderTicksRemaining <= 0)
            {
                float angle = (float)(rng.NextDouble() * Math.PI * 2);
                float wanderSpeedBonus = 1f + (organism.Genes.Wanderlust * WanderlustSpeedBonus);
                organism.Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) 
                    * (organism.Genes.Speed * 0.5f * wanderSpeedBonus);
                organism.WanderTicksRemaining = WanderRefreshTicks;
            }
            else
            {
                organism.WanderTicksRemaining--;
            }
        }
    }
}
