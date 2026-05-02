using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public class RenderSystem
{
    private readonly GraphicsDevice _graphics;
    private readonly Texture2D _pixel;
    private readonly Texture2D _glow;
    public Texture2D Pixel => _pixel;

    // Cached render target for the entire biomass layer
    private RenderTarget2D? _biomassLayer;
    private Vector2 _lastCacheSize = Vector2.Zero;
    private bool _cacheInvalidated = true;
    
    // Performance: skip food rendering if FPS is low
    private int _frameCount;
    private double _fpsAccumulator;
    private double _currentFps = 60;
    private const double FpsUpdateInterval = 0.5;

    public RenderSystem(GraphicsDevice graphicsDevice)
    {
        _graphics = graphicsDevice;
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
    
    /// <summary>
    /// Call this when the world changes significantly to invalidate the biomass cache
    /// </summary>
    public void InvalidateBiomassCache()
    {
        _cacheInvalidated = true;
    }

    /// <summary>
    /// Update FPS tracking (call each frame)
    /// </summary>
    private void UpdateFps(GameTime gameTime)
    {
        _frameCount++;
        _fpsAccumulator += gameTime.ElapsedGameTime.TotalSeconds;
        if (_fpsAccumulator >= FpsUpdateInterval)
        {
            _currentFps = _frameCount / _fpsAccumulator;
            _frameCount = 0;
            _fpsAccumulator = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch, World world, List<Organism> organisms, List<Corpse> corpses, Rectangle viewport, GameTime gameTime)
    {
        UpdateFps(gameTime);
        
        int tileSize = world.TileSize;

        // --- BIOME LAYER (static, draw once per frame - can't easily cache due to batching) ---
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

        // --- BIOMASS LAYER (cached) ---
        // Only render detailed biomass when FPS is good
        if (_currentFps > 25)
        {
            DrawBiomassLayer(spriteBatch, world, viewport, tileSize);
        }

        // --- ORGANISM LAYER (main performance target) ---
        foreach (var o in organisms)
        {
            DrawOrganismOptimized(spriteBatch, o, viewport);
        }

        // --- CORPSE LAYER (simplified) ---
        foreach (var corpse in corpses)
        {
            DrawCorpseSimple(spriteBatch, corpse, viewport);
        }
    }
    
    /// <summary>
    /// Draw biomass as simplified layer - faster than rendering each dot
    /// </summary>
    private void DrawBiomassLayer(SpriteBatch spriteBatch, World world, Rectangle viewport, int tileSize)
    {
        // Draw biomass as colored rectangles (much faster than individual dots with glow)
        // Only draw tiles that have significant biomass
        for (int y = 0; y < world.Height; y++)
        {
            for (int x = 0; x < world.Width; x++)
            {
                float biomass = world.GetBiomass(x, y);
                if (biomass < 0.15f) continue; // Skip low biomass
                
                int cx = viewport.X + x * tileSize + tileSize / 2;
                int cy = viewport.Y + y * tileSize + tileSize / 2;
                
                // Size scales with biomass
                float dotSize = tileSize * MathHelper.Lerp(0.5f, 1.2f, biomass);
                byte alpha = (byte)MathHelper.Clamp(biomass * 180, 40, 200);
                byte green = (byte)MathHelper.Clamp(140 + biomass * 60, 100, 200);
                
                var rect = new Rectangle((int)(cx - dotSize / 2), (int)(cy - dotSize / 2), 
                    (int)dotSize, (int)dotSize);
                spriteBatch.Draw(_pixel, rect, new Color((byte)80, green, (byte)60, alpha));
            }
        }
    }

    private void DrawCorpseSimple(SpriteBatch spriteBatch, Corpse corpse, Rectangle viewport)
    {
        float progress = corpse.DecayProgress;
        var screenPos = new Vector2(viewport.X + corpse.Position.X, viewport.Y + corpse.Position.Y);
        
        // Simple fading circle
        float size = MathHelper.Lerp(8f, 2f, progress);
        float alpha = MathHelper.Lerp(0.7f, 0.1f, progress);
        byte a = (byte)(alpha * 200);
        
        Color color = new Color((byte)80, (byte)60, (byte)50, a);
        
        // Glow
        spriteBatch.Draw(_glow, screenPos, null, new Color((byte)color.R, (byte)color.G, (byte)color.B, (byte)(a / 2)),
            0f, new Vector2(8, 8), size / 16f, SpriteEffects.None, 0.02f);
        
        // Body
        spriteBatch.Draw(_pixel, screenPos, null, color, 0f, 
            new Vector2(0.5f, 0.5f), size, SpriteEffects.None, 0.03f);
    }

    private void DrawMovementTrailSimple(SpriteBatch spriteBatch, Organism o, Rectangle viewport, Color baseColor, float energyScale)
    {
        // Ultra-simple trail: just 2 fading dots
        int trailIndex = 0;
        foreach (var trailPos in o.TrailPositions)
        {
            trailIndex++;
            if (trailIndex > 2) break; // Only 2 trail segments max
            
            float t = trailIndex / 3f;
            float alpha = MathHelper.Lerp(0.3f, 0.1f, t);
            
            var screenTrailPos = new Vector2(viewport.X + trailPos.X, viewport.Y + trailPos.Y);
            
            // Simple fading dot
            float size = (o.Genes.BodyRadius * energyScale) * MathHelper.Lerp(0.5f, 0.3f, t);
            spriteBatch.Draw(_pixel, screenTrailPos, null, 
                new Color((byte)baseColor.R, (byte)baseColor.G, (byte)baseColor.B, (byte)(alpha * 150)),
                0f, new Vector2(0.5f, 0.5f), size, SpriteEffects.None, -0.05f);
        }
    }

    private void DrawOrganismOptimized(SpriteBatch spriteBatch, Organism o, Rectangle viewport)
    {
        var genes = o.Genes;
        
        // Get color from genome
        Color baseColor = GeneColor(genes);
        
        // Apply energy modifier
        float energyRatio = o.Energy / o.MaxEnergy;
        Color stateColor = ApplyStateEffects(o.State, energyRatio, baseColor);
        
        // Scale based on energy
        float energyScale = 0.5f + (energyRatio * 0.5f);
        
        // Screen position
        var screenPos = new Vector2(viewport.X + o.Position.X, viewport.Y + o.Position.Y);
        
        // Simple trail (2 dots max)
        if (OrganismShapes.EnableTrails)
        {
            DrawMovementTrailSimple(spriteBatch, o, viewport, stateColor, energyScale);
        }
        
        // Pulse glow (only when hunting)
        if (o.State == AIState.Target && o.Target.HasValue)
        {
            float glowPulse = 1f + (MathF.Sin(DateTime.UtcNow.Ticks * 0.015f) + 1f) * 0.1f;
            float glowSize = genes.BodyRadius * energyScale * 1.2f;
            spriteBatch.Draw(_glow, screenPos, null, stateColor * 0.2f * glowPulse,
                0f, new Vector2(8, 8), glowSize / 16f * glowPulse, 
                SpriteEffects.None, -0.1f);
        }
        
        // Draw organism using optimized method
        OrganismShapes.DrawOrganism(spriteBatch, screenPos, o.Velocity, genes, stateColor, energyScale, 0f);
        
        // Energy bar (only when low)
        if (energyRatio < 0.6f)
        {
            int bodyWidth = (int)(genes.BodyWidth * energyScale);
            float barWidth = bodyWidth * 1.2f;
            float barHeight = 3f;
            var barPos = screenPos + new Vector2(-barWidth * 0.5f, -bodyWidth - 4f);
            
            // Background
            spriteBatch.Draw(_pixel, new Rectangle((int)barPos.X, (int)barPos.Y, (int)barWidth, (int)barHeight),
                new Color((byte)15, (byte)15, (byte)22, (byte)200));
            
            // Fill
            float fillWidth = Math.Max(1, barWidth * energyRatio);
            Color energyColor = energyRatio > 0.4f ? new Color((byte)100, (byte)200, (byte)100) 
                : energyRatio > 0.2f ? new Color((byte)200, (byte)180, (byte)60) 
                : new Color((byte)200, (byte)80, (byte)60);
            spriteBatch.Draw(_pixel, new Rectangle((int)barPos.X + 1, (int)barPos.Y + 1, (int)fillWidth - 1, (int)barHeight - 2),
                energyColor);
        }
        
        // Fleeing indicator (subtle)
        if (o.State == AIState.Flee)
        {
            float pulse = (MathF.Sin(DateTime.UtcNow.Ticks * 0.03f) + 1f) * 0.25f;
            spriteBatch.Draw(_glow, screenPos, null, new Color((byte)255, (byte)100, (byte)100, (byte)(pulse * 255)),
                0f, new Vector2(8, 8), 0.5f, SpriteEffects.None, 0.1f);
        }
    }

    private Color GeneColor(Genome genes)
    {
        float diet = genes.DietType;
        byte r, g, b;

        if (diet < 0.5f)
        {
            float t = diet * 2f;
            r = (byte)(255 * t);
            g = 200;
            b = (byte)(50 * (1f - t));
        }
        else
        {
            float t = (diet - 0.5f) * 2f;
            r = 255;
            g = (byte)(200 * (1f - t));
            b = 0;
        }

        byte blue = (byte)(genes.Wanderlust * 80);
        int combinedB = Math.Min(255, b + blue);

        return new Color(r, g, (byte)combinedB, (byte)255);
    }

    private Color ApplyStateEffects(AIState state, float energyRatio, Color baseColor)
    {
        return state switch
        {
            AIState.Rest => Color.Lerp(baseColor, new Color((byte)80, (byte)70, (byte)60), 0.5f),
            AIState.Target when energyRatio < 0.3f => Color.Lerp(baseColor, Color.White, 0.3f),
            AIState.Wander => Color.Lerp(baseColor, new Color((byte)120, (byte)120, (byte)180), 0.15f),
            AIState.Target => Color.Lerp(baseColor, new Color((byte)255, (byte)200, (byte)100), 0.15f),
            AIState.Flee => Color.Lerp(baseColor, new Color((byte)255, (byte)120, (byte)120), 0.4f),
            _ => baseColor
        };
    }
}

internal static class MathHelper
{
    public const float TwoPi = MathF.PI * 2f;
    public const float Pi = MathF.PI;

    public static float Clamp(float value, float min, float max) =>
        value < min ? min : value > max ? max : value;

    public static float Lerp(float a, float b, float t) =>
        a + (b - a) * t;
}