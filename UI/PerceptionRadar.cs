using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class PerceptionRadar
{
    private readonly Texture2D _pixel;
    private const int Segments = 32;
    private static readonly Color RadarColor = new((byte)255, (byte)255, (byte)255, (byte)64);

    public bool Visible;

    public PerceptionRadar(Texture2D pixel)
    {
        _pixel = pixel;
    }

    public void Draw(SpriteBatch spriteBatch, List<Organism> organisms, Rectangle viewport)
    {
        if (!Visible) return;

        foreach (var organism in organisms)
        {
            var center = new Vector2(viewport.X + organism.Position.X, viewport.Y + organism.Position.Y);
            DrawCircle(spriteBatch, center, organism.Genes.SenseRange, RadarColor);
        }
    }

    private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        for (int i = 0; i < Segments; i++)
        {
            float a1 = (float)(i * Math.PI * 2 / Segments);
            float a2 = (float)((i + 1) * Math.PI * 2 / Segments);
            var p1 = center + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;
            var p2 = center + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * radius;
            DrawLine(spriteBatch, p1, p2, color);
        }
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 a, Vector2 b, Color color)
    {
        float length = Vector2.Distance(a, b);
        float angle = MathF.Atan2(b.Y - a.Y, b.X - a.X);
        spriteBatch.Draw(
            texture: _pixel,
            position: a,
            sourceRectangle: null,
            color: color,
            rotation: angle,
            origin: Vector2.Zero,
            scale: new Vector2(length, 1f),
            effects: SpriteEffects.None,
            layerDepth: 0f);
    }
}
