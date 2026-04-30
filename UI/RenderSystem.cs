using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class RenderSystem
{
    private readonly Texture2D _pixel;
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

            var rect = new Rectangle(
                viewport.X + (int)organism.Position.X - 1,
                viewport.Y + (int)organism.Position.Y - 1,
                3,
                3);
            spriteBatch.Draw(_pixel, rect, drawColor);
        }
    }

    private static Color BodyColor(float dietType)
    {
        byte r = (byte)(dietType * 255f);
        byte g = (byte)((1f - dietType) * 255f);
        return new Color(r, g, (byte)0, (byte)255);
    }
}
