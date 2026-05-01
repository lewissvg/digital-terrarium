using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;

namespace DigitalTerrarium.Core;

public class Simulation
{
    public SimulationConfig Config { get; private set; }
    public World World { get; }
    public List<Organism> Organisms { get; }
    public int TickCount { get; private set; }
    public StatsSnapshot LatestStats { get; private set; }

    private Random _rng;
    private SpatialIndex _spatialIndex = null!;

    public Simulation(SimulationConfig config)
    {
        Config = config;
        World = new World(config);
        Organisms = new List<Organism>();
        _rng = new Random(config.Seed);
        _spatialIndex = new SpatialIndex(World.PixelWidth, World.PixelHeight, config.SpatialCellPixels);
        WorldSeeder.Seed(World, Organisms, config, _rng);
        LatestStats = Stats.Compute(Organisms, World);
    }

    public void Tick()
    {
        _spatialIndex.Build(Organisms);
        World.RegenerateFood(Config.FoodRegenRate, _rng);
        PerceptionSystem.Tick(World, Organisms, _spatialIndex, Config);
        AISystem.Tick(Organisms, Config, _rng);
        MovementSystem.Tick(World, Organisms, Config);
        FeedingSystem.Tick(World, Organisms, Config);
        CombatSystem.Tick(Organisms);
        ReproductionSystem.Tick(Organisms, World, Config, _rng);
        DeathSystem.Tick(Organisms);
        LatestStats = Stats.Compute(Organisms, World);
        TickCount++;
    }

    public void ApplyConfigAndReset(SimulationConfig newConfig)
    {
        Config = newConfig;
        _rng = new Random(newConfig.Seed);
        _spatialIndex = new SpatialIndex(World.PixelWidth, World.PixelHeight, newConfig.SpatialCellPixels);
        WorldSeeder.Seed(World, Organisms, newConfig, _rng);
        TickCount = 0;
        LatestStats = Stats.Compute(Organisms, World);
    }
}
