using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Systems;

public static class ReproductionSystem
{
    public static void Tick(List<Organism> organisms, SimulationConfig config, Random rng)
    {
        int originalCount = organisms.Count;
        for (int i = 0; i < originalCount; i++)
        {
            var parent = organisms[i];
            float threshold = parent.MaxEnergy * config.ReproductionThreshold;
            if (parent.Energy < threshold)
            {
                continue;
            }

            float halfEnergy = parent.Energy * 0.5f;
            parent.Energy = halfEnergy;

            Genome childGenes = parent.Genes.Mutate(config.MutationRate, rng);
            float angle = (float)(rng.NextDouble() * Math.PI * 2);
            float offset = SimulationConfig.TileSize;
            var childPosition = parent.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * offset;

            var child = Organism.NewBorn(childPosition, childGenes, parent.Generation + 1);
            child.Energy = halfEnergy;
            organisms.Add(child);
        }
    }
}
