using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

/// <summary>
/// Handles threat detection and flee behavior.
/// Runs after PerceptionSystem (knows who's targeting whom)
/// and before AISystem (decides flee behavior).
/// </summary>
public static class ThreatResponseSystem
{
    // Prey will flee when a predator is within this fraction of their sense range
    private const float FleeTriggerDistanceFraction = 0.5f;
    
    // Minimum distance a predator must be to trigger fleeing (prevents immediate flee at spawn)
    private const float MinFleeDistance = 20f;
    
    public static void Tick(List<Organism> organisms, SpatialIndex spatialIndex, SimulationConfig config)
    {
        // First pass: clear predator targeting (will be set if still being hunted)
        foreach (var o in organisms)
        {
            o.Predator = null;
        }
        
        // Second pass: predators mark their prey
        foreach (var hunter in organisms)
        {
            if (hunter.TargetPrey != null && hunter.TargetPrey.Energy > 0)
            {
                hunter.TargetPrey.Predator = hunter;
            }
        }
        
        // Third pass: prey decide to flee
        foreach (var o in organisms)
        {
            UpdateFlee(o, spatialIndex, config);
        }
    }

    private static void UpdateFlee(Organism prey, SpatialIndex spatialIndex, SimulationConfig config)
    {
        // If already fleeing, handle recovery
        if (prey.State == AIState.Flee)
        {
            prey.FleeTicksRemaining--;
            
            // Recover after fleeing long enough
            if (prey.FleeTicksRemaining <= 0)
            {
                prey.State = AIState.Wander;
                prey.Predator = null;
                prey.FleeOrigin = null;
            }
            return;
        }
        
        // Check if we're being hunted by someone we can see
        if (prey.Predator == null) return;
        
        var predator = prey.Predator;
        if (predator.Energy <= 0) return; // Predator died
        
        float distanceToPredator = Vector2.Distance(prey.Position, predator.Position);
        
        // Only flee if predator is close enough AND far enough (not spawn protection)
        float fleeTriggerDistance = prey.Genes.SenseRange * FleeTriggerDistanceFraction;
        
        if (distanceToPredator > fleeTriggerDistance || distanceToPredator < MinFleeDistance)
            return;
        
        // Initiate flee!
        prey.State = AIState.Flee;
        prey.Target = null; // Clear any food/target
        prey.TargetPrey = null;
        
        // Calculate flee duration based on distance and speed
        // Further = longer flee, faster = shorter flee
        int baseDuration = Organism.BaseFleeDuration;
        int distanceBonus = (int)(distanceToPredator / 5f);
        int speedPenalty = (int)(prey.Genes.Speed * 2f);
        prey.FleeTicksRemaining = Math.Max(15, baseDuration + distanceBonus - speedPenalty);
        
        prey.FleeOrigin = prey.Position;
    }
}