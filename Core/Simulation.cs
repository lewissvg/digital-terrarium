using DigitalTerrarium.Entities;
using DigitalTerrarium.Systems;

namespace DigitalTerrarium.Core;

public class Simulation
{
    public SimulationConfig Config { get; private set; }
    public World World { get; private set; }
    public List<Organism> Organisms { get; }
    public int TickCount { get; private set; }
    public StatsSnapshot LatestStats { get; private set; }
    public StatsSnapshot LatestSmoothedStats => _smoother.Smoothed;

    private Random _rng;
    private SpatialIndex _spatialIndex = null!;
    private StatsSmoother _smoother = null!;

    public Simulation(SimulationConfig config)
    {
        Config = config;
        World = new World(config);
        Organisms = new List<Organism>();
        _rng = new Random(config.Seed);
        _spatialIndex = new SpatialIndex(World.PixelWidth, World.PixelHeight, config.SpatialCellPixels);
        WorldSeeder.Seed(World, Organisms, config, _rng);
        LatestStats = Stats.Compute(Organisms, World);
        _smoother = new StatsSmoother(config.StatsSmoothingTicks);
        _smoother.Push(LatestStats);
    }

    public void Tick()
    {
        _spatialIndex.Build(Organisms);
        World.RegenerateFood(Config.FoodRegenRate, _rng);
        PerceptionSystem.Tick(World, Organisms, _spatialIndex, Config);
        ThreatResponseSystem.Tick(Organisms, _spatialIndex, Config);
        AISystem.Tick(Organisms, Config, _rng);
        MovementSystem.Tick(World, Organisms, Config);
        FeedingSystem.Tick(World, Organisms, Config);
        CombatSystem.Tick(Organisms);
        ReproductionSystem.Tick(Organisms, World, Config, _rng);
        DeathSystem.Tick(Organisms);
        LatestStats = Stats.Compute(Organisms, World);
        _smoother.Push(LatestStats);
        TickCount++;
    }

    public void ApplyConfigAndReset(SimulationConfig newConfig)
    {
        Config = newConfig;
        _rng = new Random(newConfig.Seed);
        World = new World(newConfig);
        _spatialIndex = new SpatialIndex(World.PixelWidth, World.PixelHeight, newConfig.SpatialCellPixels);
        Organisms.Clear();
        WorldSeeder.Seed(World, Organisms, newConfig, _rng);
        TickCount = 0;
        LatestStats = Stats.Compute(Organisms, World);
        _smoother = new StatsSmoother(newConfig.StatsSmoothingTicks);
        _smoother.Push(LatestStats);
    }
}
