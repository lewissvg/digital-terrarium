using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class RenderSystem
{
    private readonly Texture2D _pixel;
    public Texture2D Pixel => _pixel;
    private static readonly Color BackgroundColor = new(15, 15, 20);
    private static readonly Color FoodColor = new(80, 180, 80);
    private static readonly Color OrganismColor = new(220, 220, 220);
    private static readonly Color RestColor = new(140, 110, 80);

    public RenderSystem(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, World world, List<Organism> organisms, Rectangle viewport)
    {
        spriteBatch.Draw(_pixel, viewport, BackgroundColor);

        int tileSize = world.TileSize;
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                if (!world.HasFood(x, y))
                {
                    continue;
                }

                var rect = new Rectangle(
                    viewport.X + x * tileSize,
                    viewport.Y + y * tileSize,
                    tileSize,
                    tileSize);
                spriteBatch.Draw(_pixel, rect, FoodColor);
            }
        }

        foreach (var organism in organisms)
        {
            Color baseColor = BodyColor(organism.Genes.DietType);
            Color drawColor = organism.State == AIState.Rest
                ? Color.Lerp(baseColor, RestColor, 0.3f)
                : baseColor;

            int bodyLength = Math.Max(2, (int)MathF.Round(organism.Genes.Speed));
            int bodyWidth = Math.Max(2, (int)MathF.Round(4f / Math.Max(0.5f, organism.Genes.Metabolism)));

            float rotation = organism.Velocity.LengthSquared() > 0.01f
                ? MathF.Atan2(organism.Velocity.Y, organism.Velocity.X)
                : 0f;

            var origin = new Vector2(0.5f, 0.5f);
            var screenPosition = new Vector2(viewport.X + organism.Position.X, viewport.Y + organism.Position.Y);
            var scale = new Vector2(bodyLength, bodyWidth);

            spriteBatch.Draw(
                texture: _pixel,
                position: screenPosition,
                sourceRectangle: null,
                color: drawColor,
                rotation: rotation,
                origin: origin,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0f);
        }
    }

    private static Color BodyColor(float dietType)
    {
        byte r = (byte)(dietType * 255f);
        byte g = (byte)((1f - dietType) * 255f);
        return new Color(r, g, (byte)0, (byte)255);
    }
}
