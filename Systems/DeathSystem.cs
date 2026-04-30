using DigitalTerrarium.Entities;

namespace DigitalTerrarium.Systems;

public static class DeathSystem
{
    public static void Tick(List<Organism> organisms)
    {
        organisms.RemoveAll(organism => organism.Energy <= 0f);
    }
}
