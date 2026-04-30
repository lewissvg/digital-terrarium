using DigitalTerrarium.Core;

namespace DigitalTerrarium.Tests.Core;

public class SimulationTests
{
    [Fact]
    public void NewSimulation_HasSeededWorldAndPopulation()
    {
        var simulation = new Simulation(SimulationConfig.Default with { Seed = 1, StartingPopulation = 25 });

        Assert.Equal(25, simulation.Organisms.Count);
        Assert.True(simulation.World.CountFood() > 0);
        Assert.Equal(0, simulation.TickCount);
    }

    [Fact]
    public void Tick_IncrementsTickCount()
    {
        var simulation = new Simulation(SimulationConfig.Default with { Seed = 1 });

        simulation.Tick();
        simulation.Tick();
        simulation.Tick();

        Assert.Equal(3, simulation.TickCount);
    }

    [Fact]
    public void Tick_KeepsPopulationStableOver100Ticks()
    {
        var simulation = new Simulation(SimulationConfig.Default with { Seed = 1, StartingPopulation = 100 });

        for (int i = 0; i < 100; i++)
        {
            simulation.Tick();
        }

        Assert.InRange(simulation.Organisms.Count, 1, 10000);
    }

    [Fact]
    public void ApplyConfigAndReset_ReplacesConfigAndReseeds()
    {
        var simulation = new Simulation(SimulationConfig.Default with { Seed = 1, StartingPopulation = 25 });
        simulation.Tick();
        simulation.Tick();

        SimulationConfig newConfig = SimulationConfig.Default with { Seed = 2, StartingPopulation = 50 };
        simulation.ApplyConfigAndReset(newConfig);

        Assert.Equal(0, simulation.TickCount);
        Assert.Equal(50, simulation.Organisms.Count);
        Assert.Equal(newConfig, simulation.Config);
    }
}
