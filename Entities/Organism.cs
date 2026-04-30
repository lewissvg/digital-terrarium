using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Entities;

public class Organism
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Energy;
    public float MaxEnergy;
    public int Age;
    public int Generation;
    public Genome Genes = null!;
    public AIState State;
    public Vector2? Target;
    public Organism TargetPrey;
    public int WanderTicksRemaining;

    public static float ComputeMaxEnergy(Genome genes) =>
        100f + (genes.SenseRange * 0.5f) + (1f / genes.Metabolism * 20f);

    public static Organism NewBorn(Vector2 position, Genome genes, int generation)
    {
        float maxEnergy = ComputeMaxEnergy(genes);
        return new Organism
        {
            Position = position,
            Velocity = Vector2.Zero,
            Energy = maxEnergy * 0.5f,
            MaxEnergy = maxEnergy,
            Age = 0,
            Generation = generation,
            Genes = genes,
            State = AIState.Wander,
            Target = null,
            TargetPrey = null,
            WanderTicksRemaining = 0
        };
    }
}
