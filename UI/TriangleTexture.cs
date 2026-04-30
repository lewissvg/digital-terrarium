using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalTerrarium.UI;

public static class TriangleTexture
{
    public static Texture2D Build(GraphicsDevice gd, int size = 16)
    {
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // White right-pointing triangle: apex at right edge, base at left edge.
                // At column x, the vertical half-width shrinks linearly from (size-1)/2 at x=0 to 0 at x=size-1.
                float halfWidth = ((size - 1) * 0.5f) * (1f - x / (float)(size - 1));
                float dy = MathF.Abs(y - (size - 1) * 0.5f);
                data[y * size + x] = dy <= halfWidth ? Color.White : Color.Transparent;
            }
        }
        var tex = new Texture2D(gd, size, size);
        tex.SetData(data);
        return tex;
    }
}
