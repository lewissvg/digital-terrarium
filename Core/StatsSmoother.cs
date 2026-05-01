namespace DigitalTerrarium.Core;

public class StatsSmoother
{
    private readonly Queue<StatsSnapshot> _history = new();
    private int _windowSize;

    public StatsSmoother(int windowSize) { _windowSize = Math.Max(1, windowSize); }

    public void Resize(int windowSize)
    {
        _windowSize = Math.Max(1, windowSize);
        while (_history.Count > _windowSize) _history.Dequeue();
    }

    public void Push(StatsSnapshot snap)
    {
        _history.Enqueue(snap);
        while (_history.Count > _windowSize) _history.Dequeue();
    }

    public StatsSnapshot Smoothed
    {
        get
        {
            int n = _history.Count;
            if (n == 0) return default;

            StatsSnapshot latest = default;
            float sumSpeed = 0, sumMet = 0, sumSense = 0, sumDiet = 0, sumAff = 0;
            int sumPopMud = 0, sumPopGrass = 0, sumPopSand = 0;

            foreach (var s in _history)
            {
                latest = s;
                sumSpeed += s.AvgSpeed;
                sumMet   += s.AvgMetabolism;
                sumSense += s.AvgSenseRange;
                sumDiet  += s.AvgDietType;
                sumAff   += s.AvgTerrainAffinity;
                sumPopMud   += s.PopMud;
                sumPopGrass += s.PopGrassland;
                sumPopSand  += s.PopSand;
            }

            return latest with
            {
                AvgSpeed           = sumSpeed / n,
                AvgMetabolism      = sumMet   / n,
                AvgSenseRange      = sumSense / n,
                AvgDietType        = sumDiet  / n,
                AvgTerrainAffinity = sumAff   / n,
                PopMud             = sumPopMud   / n,
                PopGrassland       = sumPopGrass / n,
                PopSand            = sumPopSand  / n,
            };
        }
    }
}
