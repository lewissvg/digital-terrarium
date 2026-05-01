using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public static class OrganismShapes
{
    private static GraphicsDevice? _gd;
    private static Texture2D _triangle = null!;
    private static Texture2D _diamond = null!;
    private static Texture2D _circle = null!;
    private static Texture2D _arrow = null!;

    public static Texture2D Triangle => _triangle ?? throw new InvalidOperationException("Call Initialize first");
    public static Texture2D Diamond => _diamond ?? throw new InvalidOperationException("Call Initialize first");
    public static Texture2D Circle => _circle ?? throw new InvalidOperationException("Call Initialize first");
    public static Texture2D Arrow => _arrow ?? throw new InvalidOperationException("Call Initialize first");

    public static void Initialize(GraphicsDevice gd)
    {
        _gd = gd;
        _triangle = BuildTriangle(32);
        _diamond = BuildDiamond(32);
        _circle = BuildCircle(32);
        _arrow = BuildArrow(32);
    }

    private static Texture2D BuildTriangle(int size)
    {
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float halfWidth = ((size - 1) * 0.5f) * (1f - x / (float)(size - 1));
                float dy = MathF.Abs(y - (size - 1) * 0.5f);
                data[y * size + x] = dy <= halfWidth ? Color.White : Color.Transparent;
            }
        }
        return CreateTexture(size, data);
    }

    private static Texture2D BuildDiamond(int size)
    {
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cx = size * 0.5f;
                float cy = size * 0.5f;
                float dx = MathF.Abs(x - cx) / cx;
                float dy = MathF.Abs(y - cy) / cy;
                data[y * size + x] = (dx + dy) <= 1f ? Color.White : Color.Transparent;
            }
        }
        return CreateTexture(size, data);
    }

    private static Texture2D BuildCircle(int size)
    {
        var data = new Color[size * size];
        float radius = size * 0.45f;
        float cx = size * 0.5f;
        float cy = size * 0.5f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist <= radius)
                {
                    float alpha = Math.Clamp((radius - dist) / 3f, 0f, 1f);
                    data[y * size + x] = new Color((byte)(255), (byte)(255), (byte)(255), (byte)(alpha * 255));
                }
                else
                {
                    data[y * size + x] = Color.Transparent;
                }
            }
        }
        return CreateTexture(size, data);
    }

    private static Texture2D BuildArrow(int size)
    {
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float lineEnd = size * 0.6f;
                bool inLine = y >= size * 0.4f && y <= size * 0.6f && x <= lineEnd;
                
                float tipY = size * 0.5f;
                float halfWidth = (size - 1 - lineEnd) * (1f - (x - lineEnd) / (float)(size - 1 - lineEnd));
                bool inTip = x >= lineEnd && MathF.Abs(y - tipY) <= halfWidth;
                
                data[y * size + x] = (inLine || inTip) ? Color.White : Color.Transparent;
            }
        }
        return CreateTexture(size, data);
    }

    private static Texture2D CreateTexture(int size, Color[] data)
    {
        var tex = new Texture2D(_gd!, size, size);
        tex.SetData(data);
        return tex;
    }

    public static Texture2D GetForOrganism(float speed, float wanderlust, float senseRange)
    {
        // Shape based on speed + wanderlust combination
        // Fast wanderers = arrow (migratory)
        // Slow wanderers = diamond (exploratory)
        // Fast settlers = triangle (burst speed)
        // Slow settlers = circle (stable)
        
        bool isFast = speed > 5f;
        bool isNomad = wanderlust > 0.5f;
        
        if (isFast && isNomad) return Arrow;
        if (!isFast && isNomad) return Diamond;
        if (isFast && !isNomad) return Triangle;
        return Circle;
    }
}