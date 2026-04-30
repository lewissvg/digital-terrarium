namespace DigitalTerrarium.Core;

public class TimeController
{
    public static readonly int[] SpeedSteps = { 0, 1, 2, 8, 32 };

    public int SpeedIndex { get; private set; } = 1;

    public int CurrentMultiplier => SpeedSteps[SpeedIndex];

    private double _accumulatorSeconds;

    public const double FrameBudgetSeconds = 0.012;

    public void StepUp()
    {
        if (SpeedIndex < SpeedSteps.Length - 1)
        {
            SpeedIndex++;
        }
    }

    public void StepDown()
    {
        if (SpeedIndex > 0)
        {
            SpeedIndex--;
        }
    }

    public void TogglePause()
    {
        SpeedIndex = SpeedIndex == 0 ? 1 : 0;
    }

    public void RunPendingTicks(double realDeltaSeconds, Action tickAction)
    {
        if (CurrentMultiplier == 0)
        {
            _accumulatorSeconds = 0;
            return;
        }

        double secondsPerTick = 1.0 / (SimulationConfig.TickRateHz * CurrentMultiplier);
        _accumulatorSeconds += realDeltaSeconds;

        DateTime frameStart = DateTime.UtcNow;
        while (_accumulatorSeconds >= secondsPerTick)
        {
            tickAction();
            _accumulatorSeconds -= secondsPerTick;

            if ((DateTime.UtcNow - frameStart).TotalSeconds >= FrameBudgetSeconds)
            {
                _accumulatorSeconds = 0;
                return;
            }
        }
    }
}
