using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class RenderSystem
{
    private readonly Texture2D _pixel;
    private readonly Texture2D _triangle;
    public Texture2D Pixel => _pixel;
    private static readonly Color FoodColor = new(80, 180, 80);
    private static readonly Color RestColor = new(140, 110, 80);

    public RenderSystem(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _triangle = TriangleTexture.Build(graphicsDevice, size: 16);
    }

    public void Draw(SpriteBatch spriteBatch, World world, List<Organism> organisms, Rectangle viewport)
    {
        int tileSize = world.TileSize;

        // Biome layer
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                var biome = world.Biomes.At(x, y);
                var rect = new Rectangle(
                    viewport.X + x * tileSize,
                    viewport.Y + y * tileSize,
                    tileSize, tileSize);
                spriteBatch.Draw(_pixel, rect, BiomeProperties.BackgroundColor(biome));
            }
        }

        // Food layer
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                if (!world.HasFood(x, y)) continue;

                var rect = new Rectangle(
                    viewport.X + x * tileSize,
                    viewport.Y + y * tileSize,
                    tileSize, tileSize);
                spriteBatch.Draw(_pixel, rect, FoodColor);
            }
        }

        // Organism layer (triangle sprites)
        foreach (var o in organisms)
        {
            Color baseColor = BodyColor(o.Genes.DietType);
            Color drawColor = o.State == AIState.Rest
                ? Color.Lerp(baseColor, RestColor, 0.3f)
                : baseColor;

            int bodyLength = Math.Max(3, (int)MathF.Round(o.Genes.Speed * 1.2f));
            int bodyWidth  = Math.Max(3, (int)MathF.Round(5f / Math.Max(0.5f, o.Genes.Metabolism)));

            float rotation = o.Velocity.LengthSquared() > 0.01f
                ? MathF.Atan2(o.Velocity.Y, o.Velocity.X)
                : 0f;

            var screenPos = new Vector2(viewport.X + o.Position.X, viewport.Y + o.Position.Y);
            var origin = new Vector2(_triangle.Width * 0.5f, _triangle.Height * 0.5f);
            var scale = new Vector2(
                bodyLength / (float)_triangle.Width,
                bodyWidth  / (float)_triangle.Height);

            spriteBatch.Draw(
                texture: _triangle,
                position: screenPos,
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
