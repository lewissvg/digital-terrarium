namespace DigitalTerrarium.Core;

public static class BiomeNoise
{
    public static float[,] Generate(int worldWidth, int worldHeight, int noiseGridSize, Random rng)
    {
        var lowRes = new float[noiseGridSize + 1, noiseGridSize + 1];
        for (int y = 0; y <= noiseGridSize; y++)
            for (int x = 0; x <= noiseGridSize; x++)
                lowRes[x, y] = (float)rng.NextDouble();

        var output = new float[worldWidth, worldHeight];
        for (int wy = 0; wy < worldHeight; wy++)
        {
            float ny = (wy / (float)worldHeight) * noiseGridSize;
            int y0 = (int)ny;
            float fy = ny - y0;

            for (int wx = 0; wx < worldWidth; wx++)
            {
                float nx = (wx / (float)worldWidth) * noiseGridSize;
                int x0 = (int)nx;
                float fx = nx - x0;

                float v00 = lowRes[x0,     y0];
                float v10 = lowRes[x0 + 1, y0];
                float v01 = lowRes[x0,     y0 + 1];
                float v11 = lowRes[x0 + 1, y0 + 1];

                float top    = v00 + (v10 - v00) * fx;
                float bottom = v01 + (v11 - v01) * fx;
                output[wx, wy] = top + (bottom - top) * fy;
            }
        }

        return output;
    }
}
