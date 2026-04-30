using DigitalTerrarium.Core;

namespace DigitalTerrarium.Tests.Core;

public class DeterminismTests
{
    [Fact]
    public void TwoSimulationsWithSameSeed_ProduceIdenticalStateAfter1000Ticks()
    {
        SimulationConfig config = SimulationConfig.Default with { Seed = 12345 };

        var simA = new Simulation(config);
        var simB = new Simulation(config);

        for (int i = 0; i < 1000; i++)
        {
            simA.Tick();
            simB.Tick();
        }

        Assert.Equal(simA.Organisms.Count, simB.Organisms.Count);
        Assert.Equal(simA.World.CountFood(), simB.World.CountFood());

        for (int i = 0; i < simA.Organisms.Count; i++)
        {
            var a = simA.Organisms[i];
            var b = simB.Organisms[i];
            Assert.Equal(a.Position.X, b.Position.X, precision: 5);
            Assert.Equal(a.Position.Y, b.Position.Y, precision: 5);
            Assert.Equal(a.Energy, b.Energy, precision: 5);
            Assert.Equal(a.Age, b.Age);
            Assert.Equal(a.Generation, b.Generation);
            Assert.Equal(a.Genes, b.Genes);
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentState()
    {
        var simA = new Simulation(SimulationConfig.Default with { Seed = 1 });
        var simB = new Simulation(SimulationConfig.Default with { Seed = 2 });

        bool anyDifferent = false;
        for (int i = 0; i < simA.Organisms.Count && i < simB.Organisms.Count; i++)
        {
            if (simA.Organisms[i].Position != simB.Organisms[i].Position)
            {
                anyDifferent = true;
                break;
            }
        }

        Assert.True(anyDifferent);
    }
}
