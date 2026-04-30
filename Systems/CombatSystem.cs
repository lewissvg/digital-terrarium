using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class CombatSystem
{
    private const float AttackRange = 4f;
    private const float AttackBuffer = 0.2f;

    public static void Tick(List<Organism> organisms)
    {
        foreach (var organism in organisms)
        {
            var prey = organism.TargetPrey;
            if (prey == null) continue;

            if (prey.Energy <= 0f)
            {
                organism.TargetPrey = null;
                organism.Target = null;
                continue;
            }

            float distance = Vector2.Distance(organism.Position, prey.Position);
            if (distance > AttackRange) continue;
            if ((organism.Genes.DietType - prey.Genes.DietType) <= AttackBuffer) continue;

            float transferred = prey.Energy * organism.Genes.DietType;
            organism.Energy = MathF.Min(organism.MaxEnergy, organism.Energy + transferred);
            prey.Energy = 0f;

            organism.TargetPrey = null;
            organism.Target = null;
        }
    }
}
