using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Core;

public readonly record struct StatsSnapshot(
    int Population,
    int OldestAge,
    int MaxGeneration,
    float AvgSpeed,
    float AvgMetabolism,
    float AvgSenseRange,
    float AvgDietType,
    float AvgTerrainAffinity,
    int PopMud,
    int PopGrassland,
    int PopSand);

public static class Stats
{
    public static StatsSnapshot Compute(List<Organism> organisms, World world)
    {
        if (organisms.Count == 0)
            return new StatsSnapshot(0, 0, 0, 0f, 0f, 0f, 0f, 0f, 0, 0, 0);

        int oldest = 0, maxGen = 0;
        float sumSpeed = 0f, sumMet = 0f, sumSense = 0f, sumDiet = 0f, sumAff = 0f;
        int popMud = 0, popGrass = 0, popSand = 0;

        foreach (var o in organisms)
        {
            if (o.Age > oldest) oldest = o.Age;
            if (o.Generation > maxGen) maxGen = o.Generation;
            sumSpeed += o.Genes.Speed;
            sumMet   += o.Genes.Metabolism;
            sumSense += o.Genes.SenseRange;
            sumDiet  += o.Genes.DietType;
            sumAff   += o.Genes.TerrainAffinity;

            switch (world.Biomes.AtPixel(o.Position.X, o.Position.Y, world.TileSize))
            {
                case BiomeType.Mud:       popMud++; break;
                case BiomeType.Grassland: popGrass++; break;
                case BiomeType.Sand:      popSand++; break;
            }
        }

        int n = organisms.Count;
        return new StatsSnapshot(
            Population:         n,
            OldestAge:          oldest,
            MaxGeneration:      maxGen,
            AvgSpeed:           sumSpeed / n,
            AvgMetabolism:      sumMet / n,
            AvgSenseRange:      sumSense / n,
            AvgDietType:        sumDiet / n,
            AvgTerrainAffinity: sumAff / n,
            PopMud:             popMud,
            PopGrassland:       popGrass,
            PopSand:            popSand);
    }
}
