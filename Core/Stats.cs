using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Core;

public readonly record struct StatsSnapshot(
    int Population,
    int OldestAge,
    int MaxGeneration,
    float AvgSpeed,
    float AvgMetabolism,
    float AvgSenseRange);

public static class Stats
{
    public static StatsSnapshot Compute(List<Organism> organisms)
    {
        if (organisms.Count == 0)
        {
            return new StatsSnapshot(0, 0, 0, 0f, 0f, 0f);
        }

        int oldest = 0;
        int maxGeneration = 0;
        float sumSpeed = 0f;
        float sumMetabolism = 0f;
        float sumSenseRange = 0f;

        foreach (var organism in organisms)
        {
            if (organism.Age > oldest)
            {
                oldest = organism.Age;
            }

            if (organism.Generation > maxGeneration)
            {
                maxGeneration = organism.Generation;
            }

            sumSpeed += organism.Genes.Speed;
            sumMetabolism += organism.Genes.Metabolism;
            sumSenseRange += organism.Genes.SenseRange;
        }

        int count = organisms.Count;
        return new StatsSnapshot(
            Population: count,
            OldestAge: oldest,
            MaxGeneration: maxGeneration,
            AvgSpeed: sumSpeed / count,
            AvgMetabolism: sumMetabolism / count,
            AvgSenseRange: sumSenseRange / count);
    }
}
