using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class RenderSystem
{
    private readonly Texture2D _pixel;
    private readonly Texture2D _glow;
    public Texture2D Pixel => _pixel;

    public RenderSystem(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Create glow texture (soft circle)
        _glow = new Texture2D(graphicsDevice, 16, 16);
        var glowData = new Color[16 * 16];
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float cx = 7.5f, cy = 7.5f;
                float dist = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist < 8)
                {
                    byte alpha = (byte)Math.Max(0, (int)(150 - dist * 20));
                    glowData[y * 16 + x] = new Color((byte)255, (byte)255, (byte)255, alpha);
                }
                else
                {
                    glowData[y * 16 + x] = Color.Transparent;
                }
            }
        }
        _glow.SetData(glowData);

        OrganismShapes.Initialize(graphicsDevice);
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

        // Food layer with subtle glow
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                if (!world.HasFood(x, y)) continue;

                int cx = viewport.X + x * tileSize;
                int cy = viewport.Y + y * tileSize;
                var foodPos = new Vector2(cx, cy);

                // Soft glow behind food
                spriteBatch.Draw(_glow, foodPos, null, new Color(100, 220, 100, 60), 0f,
                    new Vector2(8, 8), 0.8f, SpriteEffects.None, 0.05f);

                // Food dot
                int foodSize = tileSize + 2;
                var rect = new Rectangle(cx - 1, cy - 1, foodSize, foodSize);
                spriteBatch.Draw(_pixel, rect, new Color(100, 220, 100, 200));
            }
        }

        // Organism layer
        foreach (var o in organisms)
        {
            DrawOrganism(spriteBatch, o, viewport);
        }
    }

    private void DrawOrganism(SpriteBatch spriteBatch, Organism o, Rectangle viewport)
    {
        var genes = o.Genes;

        // Get base color from genes
        Color baseColor = GeneColor(genes);

        // Apply state modifiers
        float energyRatio = o.Energy / o.MaxEnergy;
        Color stateColor = ApplyStateEffects(o.State, energyRatio, baseColor);

        // Calculate size based on genes (MUCH larger)
        int bodyLength = Math.Max(10, (int)MathF.Round(genes.Speed * 3f));
        int bodyWidth = Math.Max(8, (int)MathF.Round(8f / Math.Max(0.3f, genes.Metabolism)));

        // Scale based on energy (starving = smaller)
        float energyScale = 0.5f + (energyRatio * 0.5f);
        bodyLength = (int)(bodyLength * energyScale);
        bodyWidth = (int)(bodyWidth * energyScale);

        // Rotation based on velocity
        float rotation = o.Velocity.LengthSquared() > 0.01f
            ? MathF.Atan2(o.Velocity.Y, o.Velocity.X)
            : 0f;

        // Get shape based on genes
        var shape = OrganismShapes.GetForOrganism(genes.Speed, genes.Wanderlust, genes.SenseRange);

        // Screen position
        var screenPos = new Vector2(viewport.X + o.Position.X, viewport.Y + o.Position.Y);
        var origin = new Vector2(shape.Width * 0.5f, shape.Height * 0.5f);
        var scale = new Vector2(
            bodyLength / (float)shape.Width,
            bodyWidth / (float)shape.Height);

        // Draw glow/shadow effect first (behind organism)
        float glowPulse = 1f;
        if (o.State == AIState.Target && o.Target.HasValue)
        {
            glowPulse = 1f + (MathF.Sin(DateTime.UtcNow.Ticks * 0.015f) + 1f) * 0.15f;
        }
        spriteBatch.Draw(_glow, screenPos, null, stateColor * 0.25f * glowPulse, rotation,
            new Vector2(8, 8), bodyLength / 8f, SpriteEffects.None, -0.1f);

        // Draw main body
        spriteBatch.Draw(
            texture: shape,
            position: screenPos,
            sourceRectangle: null,
            color: stateColor,
            rotation: rotation,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0f);



        // Draw energy bar (always visible when below 80%)
        if (energyRatio < 0.8f)
        {
            float barWidth = bodyLength * 1.2f;
            float barHeight = 4f;
            var barPos = screenPos + new Vector2(-barWidth * 0.5f, -bodyWidth - 5f);

            // Background with subtle transparency
            spriteBatch.Draw(_pixel, new Rectangle((int)barPos.X, (int)barPos.Y, (int)barWidth, (int)barHeight),
                new Color(15, 15, 22, 220));

            // Energy fill level (inset by 1px for subtle border effect)
            float energyWidth = Math.Max(2, barWidth * energyRatio - 2);
            Color energyColor;
            if (energyRatio > 0.6f)
                energyColor = new Color(100, 230, 100);
            else if (energyRatio > 0.4f)
                energyColor = new Color(230, 220, 80);
            else if (energyRatio > 0.2f)
                energyColor = new Color(230, 130, 60);
            else
                energyColor = new Color(230, 80, 80);

            spriteBatch.Draw(_pixel, new Rectangle((int)barPos.X + 1, (int)barPos.Y + 1, (int)energyWidth, (int)barHeight - 2),
                energyColor);
        }

        // Generation indicator: ring for older generations (subtle)
        if (o.Generation > 2)
        {
            float alpha = Math.Min(0.35f, (o.Generation - 2) * 0.08f);
            var genColor = new Color(1f, 1f, 1f, alpha);
            spriteBatch.Draw(_pixel, screenPos, null, genColor, rotation,
                new Vector2(0.5f, 0.5f), bodyLength * 1.4f, SpriteEffects.None, -0.01f);
        }

        // Starving indicator: red tint pulse when critical
        if (energyRatio < 0.2f)
        {
            float pulse = (MathF.Sin(DateTime.UtcNow.Ticks * 0.02f) + 1f) * 0.5f;
            var starveColor = new Color(1f, 0f, 0f, pulse * 0.25f);
            spriteBatch.Draw(_pixel, screenPos, null, starveColor, rotation,
                new Vector2(0.5f, 0.5f), bodyLength * 1.8f, SpriteEffects.None, 0.05f);
        }

        // Fleeing indicator: rapid pulsing aura
        if (o.State == AIState.Flee)
        {
            float pulse = (MathF.Sin(DateTime.UtcNow.Ticks * 0.03f) + 1f) * 0.5f;
            var fleeColor = new Color(1f, 0.3f, 0.3f, pulse * 0.3f);
            spriteBatch.Draw(_glow, screenPos, null, fleeColor, rotation,
                new Vector2(8, 8), bodyLength * 0.12f, SpriteEffects.None, 0.06f);
        }
    }

    private Color GeneColor(Genome genes)
    {
        // Base color from diet: Green (herbivore) -> Yellow (omnivore) -> Red (carnivore)
        float diet = genes.DietType;
        byte r, g, b;

        if (diet < 0.5f)
        {
            // Green to Yellow
            float t = diet * 2f;
            r = (byte)(255 * t);
            g = 200;
            b = (byte)(50 * (1f - t));
        }
        else
        {
            // Yellow to Red
            float t = (diet - 0.5f) * 2f;
            r = 255;
            g = (byte)(200 * (1f - t));
            b = 0;
        }

        // Wanderlust adds blue tint for nomads
        byte blue = (byte)(genes.Wanderlust * 100);

        // Combine and clamp
        int combinedB = Math.Min(255, b + blue);

        return new Color(r, g, (byte)combinedB, (byte)255);
    }

    private Color ApplyStateEffects(AIState state, float energyRatio, Color baseColor)
    {
        return state switch
        {
            AIState.Rest => Color.Lerp(baseColor, new Color(80, 70, 60), 0.5f), // Dark brown when resting
            AIState.Target when energyRatio < 0.3f => Color.Lerp(baseColor, Color.White, 0.4f), // White tint when starving
            AIState.Wander => Color.Lerp(baseColor, new Color(120, 120, 180), 0.2f), // Slight blue tint when exploring
            AIState.Target => Color.Lerp(baseColor, new Color(255, 200, 100), 0.2f), // Golden tint when hunting
            AIState.Flee => Color.Lerp(baseColor, new Color(255, 100, 100), 0.5f), // Red/pink tint when fleeing (panic)
            _ => baseColor
        };
    }
}