using Microsoft.Xna.Framework;

namespace DigitalTerrarium.Core;

public static class BiomeProperties
{
    public static float Optimum(BiomeType b) => b switch
    {
        BiomeType.Mud       => 0f,
        BiomeType.Grassland => 0.5f,
        BiomeType.Sand      => 1f,
        _ => 0.5f
    };

    public static float FoodRegenMultiplier(BiomeType b) => b switch
    {
        BiomeType.Mud       => 2.0f,
        BiomeType.Grassland => 1.0f,
        BiomeType.Sand      => 0.2f,
        _ => 1.0f
    };

    public static float MovementBase(BiomeType b) => b switch
    {
        BiomeType.Mud       => 0.5f,
        BiomeType.Grassland => 1.0f,
        BiomeType.Sand      => 1.0f,
        _ => 1.0f
    };

    public static float FoodYieldBase(BiomeType b) => b switch
    {
        BiomeType.Mud       => 1.0f,
        BiomeType.Grassland => 1.0f,
        BiomeType.Sand      => 0.5f,
        _ => 1.0f
    };

    public static Color BackgroundColor(BiomeType b) => b switch
    {
        BiomeType.Mud       => new Color(40, 25, 15),
        BiomeType.Grassland => new Color(20, 35, 20),
        BiomeType.Sand      => new Color(50, 45, 25),
        _ => new Color(15, 15, 20)
    };

    public static float Match(float affinity, BiomeType b)
        => MathF.Max(0f, 1f - 2f * MathF.Abs(affinity - Optimum(b)));
}
