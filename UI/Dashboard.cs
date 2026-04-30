using DigitalTerrarium.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class Dashboard
{
    private readonly SpriteFont _font;

    public Dashboard(SpriteFont font)
    {
        _font = font;
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle area, Simulation simulation, TimeController time)
    {
        StatsSnapshot stats = simulation.LatestStats;
        string speedLabel = time.CurrentMultiplier == 0 ? "PAUSED" : $"{time.CurrentMultiplier}x";

        string text =
            $"TICK:        {simulation.TickCount,8}\n" +
            $"SPEED:       {speedLabel}\n" +
            $"SEED:        {simulation.Config.Seed}\n" +
            "\n" +
            $"POPULATION:  {stats.Population}\n" +
            $"GENERATIONS: {stats.MaxGeneration}\n" +
            $"OLDEST:      {stats.OldestAge}\n" +
            "\n" +
            "GENE AVERAGES\n" +
            $"  Speed:      {stats.AvgSpeed:F2}\n" +
            $"  Metabolism: {stats.AvgMetabolism:F2}\n" +
            $"  Sense:      {stats.AvgSenseRange:F1}\n" +
            $"  Diet:       {stats.AvgDietType:F2}";

        spriteBatch.DrawString(_font, text, new Vector2(area.X + 8, area.Y + 8), Color.White);
    }
}
