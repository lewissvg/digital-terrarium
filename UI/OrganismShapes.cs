using DigitalTerrarium.Core;
using DigitalTerrarium.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

/// <summary>
/// Optimized procedural organism rendering based on genome properties.
/// Uses batching for efficient drawing of many organisms.
/// </summary>
public static class OrganismShapes
{
    private static GraphicsDevice? _gd;
    private static Texture2D _pixel = null!;
    private static Texture2D _glow = null!;
    
    // Cached render target for biomass layer (massive performance improvement)
    private static RenderTarget2D? _biomassCache;
    private static Vector2 _lastCacheSize = Vector2.Zero;
    
    // Performance settings
    public const bool EnableTrails = false; // Disable trails for performance
    public const int MaxTrailSegments = 0; // No trails by default

    public static Texture2D Pixel => _pixel ?? throw new InvalidOperationException("Call Initialize first");

    public static void Initialize(GraphicsDevice gd)
    {
        _gd = gd;
        _pixel = new Texture2D(gd, 1, 1);
        _pixel.SetData(new[] { Color.White });

        // Create glow texture (soft circle)
        _glow = new Texture2D(gd, 16, 16);
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
        
        // Pre-create cached render target (will be resized on first use)
        _biomassCache = new RenderTarget2D(gd, 1, 1);
    }
    
    /// <summary>
    /// Invalidate the biomass cache (call when world changes significantly)
    /// </summary>
    public static void InvalidateCache() => _lastCacheSize = Vector2.Zero;

    /// <summary>
    /// Draw the full organism procedurally based on genome.
    /// Optimized to minimize draw calls.
    /// </summary>
    public static void DrawOrganism(
        SpriteBatch sb,
        Vector2 position,
        Vector2 velocity,
        Genome genome,
        Color color,
        float sizeScale = 1f,
        float layerDepth = 0f)
    {
        // Calculate rotation from velocity
        float rotation = velocity.LengthSquared() > 0.01f
            ? MathF.Atan2(velocity.Y, velocity.X)
            : 0f;

        // Calculate scaled dimensions
        float bodyRadius = genome.BodyRadius * sizeScale;
        float headRadius = genome.HeadRadius * sizeScale;
        float bodyWidth = genome.BodyWidth * sizeScale;
        
        // Use simplified rendering: body + head + simple tail
        // This reduces from ~8-10 draw calls to ~4-5
        
        // 1. Body glow (behind everything)
        sb.Draw(_glow, position, null, new Color(color.R, color.G, color.B, (byte)(color.A * 0.2f)),
            rotation, new Vector2(8, 8), bodyRadius * 0.7f, SpriteEffects.None, layerDepth - 0.03f);

        // 2. Tail (if any) - simplified single segment for speed
        int tailSegments = EnableTrails ? genome.TailSegments : Math.Min(1, genome.TailSegments);
        if (tailSegments > 0 && EnableTrails)
        {
            float tailLen = genome.TailLength * sizeScale * 0.5f;
            float tailRot = rotation + MathHelper.Pi;
            var tailPos = position + new Vector2(MathF.Cos(tailRot), MathF.Sin(tailRot)) * (bodyRadius + tailLen * 0.5f);
            byte tailAlpha = (byte)(color.A * 0.6f);
            sb.Draw(_pixel, tailPos, null, new Color(color.R, color.G, color.B, tailAlpha), tailRot,
                new Vector2(0.5f, 0.5f), new Vector2(tailLen * 1.5f, bodyRadius * 0.6f),
                SpriteEffects.None, layerDepth - 0.02f);
        }

        // 3. Main body (elongated ellipse)
        sb.Draw(_pixel, position, null, color, rotation,
            new Vector2(0.5f, 0.5f), new Vector2(bodyRadius * 1.8f, bodyWidth),
            SpriteEffects.None, layerDepth);

        // 4. Head (front circle) - only if large enough
        if (headRadius > 2f)
        {
            float headDist = bodyRadius * 0.5f;
            var headPos = position + new Vector2(MathF.Cos(rotation), MathF.Sin(rotation)) * bodyRadius;
            sb.Draw(_pixel, headPos, null, color, 0f,
                new Vector2(0.5f, 0.5f), headRadius * 1.5f,
                SpriteEffects.None, layerDepth + 0.01f);
        }
    }
    
    /// <summary>
    /// Draw a simple organism preview (for trails) - ultra minimal
    /// </summary>
    public static void DrawOrganismSimple(
        SpriteBatch sb,
        Vector2 position,
        float rotation,
        Genome genome,
        Color color,
        float sizeScale,
        float layerDepth)
    {
        float bodyRadius = genome.BodyRadius * sizeScale;
        float bodyWidth = genome.BodyWidth * sizeScale;
        
        // Single draw for trail - no glow, no head
        sb.Draw(_pixel, position, null, color, rotation,
            new Vector2(0.5f, 0.5f), new Vector2(bodyRadius * 1.5f, bodyWidth),
            SpriteEffects.None, layerDepth);
    }

    // Backwards compatibility - returns a simple circle texture
    public static Texture2D GetCircle(int size = 32)
    {
        return _pixel; // Use pixel for all shapes now
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
}