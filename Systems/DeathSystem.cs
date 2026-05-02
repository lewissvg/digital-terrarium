using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Systems;

public static class DeathSystem
{
    /// <summary>
    /// Config for decomposition nutrients
    /// </summary>
    public static float NutrientSpreadRadius { get; set; } = 1.5f; // Tiles to spread nutrients
    public static float NeighborNutrientMultiplier { get; set; } = 0.5f; // How much neighbors get vs center

    public static void Tick(List<Organism> organisms, List<Corpse> corpses)
    {
        // Transition dead organisms into corpses
        foreach (var organism in organisms)
        {
            if (organism.Energy <= 0f)
            {
                // Calculate size based on organism genome
                float size = MathHelper.Lerp(0.5f, 1.5f, organism.Genes.BodyRadius / 10f);
                corpses.Add(Corpse.Create(organism.Position, organism.Genes, organism.Generation, size));
            }
        }

        organisms.RemoveAll(organism => organism.Energy <= 0f);
    }

    /// <summary>
    /// Decay corpses over time, removing fully decayed ones.
    /// Also releases nutrients into the biomass grid as corpses decompose.
    /// </summary>
    public static void DecayCorpses(World world, List<Corpse> corpses, float nutrientRate = 1f)
    {
        int tileSize = world.TileSize;
        int tileX = 0, tileY = 0;

        for (int i = corpses.Count - 1; i >= 0; i--)
        {
            var corpse = corpses[i];

            // Calculate tile position once
            tileX = (int)(corpse.Position.X / tileSize);
            tileY = (int)(corpse.Position.Y / tileSize);

            // Release nutrients to biomass grid
            float nutrientsToRelease = corpse.GetNutrientYieldPerTick() * nutrientRate;
            if (nutrientsToRelease > 0.001f)
            {
                // Center tile gets full nutrients
                world.AddBiomass(tileX, tileY, nutrientsToRelease);

                // Spread to neighboring tiles (nutrient gradient) with configurable radius
                float neighborNutrients = nutrientsToRelease * NeighborNutrientMultiplier;
                int radius = (int)NutrientSpreadRadius;

                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (dx == 0 && dy == 0) continue; // Skip center

                        // Distance-based falloff
                        float dist = MathF.Sqrt(dx * dx + dy * dy);
                        if (dist > NutrientSpreadRadius) continue;

                        float falloff = 1f - (dist / (NutrientSpreadRadius + 1f));
                        falloff = MathHelper.Lerp(0.3f, 1f, falloff);

                        world.AddBiomass(tileX + dx, tileY + dy, neighborNutrients * falloff);
                    }
                }
            }

            // Decay the corpse timer
            float newTimer = corpse.DecayTimer - 1f;
            float newRemaining = corpse.RemainingNutrients - nutrientsToRelease;
            newRemaining = Math.Max(0, newRemaining);

            if (newTimer <= 0f || newRemaining <= 0f)
            {
                corpses.RemoveAt(i);
            }
            else
            {
                // Update the corpse (replace in list)
                corpses[i] = corpse with { DecayTimer = newTimer, RemainingNutrients = newRemaining };
            }
        }
    }
}

internal static class MathHelper
{
    public static float Lerp(float a, float b, float t) =>
        a + (b - a) * t;
}