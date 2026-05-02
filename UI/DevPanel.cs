using DigitalTerrarium.Core;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;

namespace DigitalTerrarium.UI;

public class DevPanel
{
    public Widget Root { get; }

    public bool IsVisible
    {
        get => Root.Visible;
        set => Root.Visible = value;
    }

    private SimulationConfig _pending;
    private readonly Action<SimulationConfig> _onApplyAndReset;
    private readonly Action _onReseed;

    private TextBox _seedBox = null!;
    private readonly Dictionary<string, Label> _labels = new();

    public DevPanel(SimulationConfig initial, Action<SimulationConfig> onApplyAndReset, Action onReseed)
    {
        _pending = initial;
        _onApplyAndReset = onApplyAndReset;
        _onReseed = onReseed;
        Root = BuildUi();
        Root.Visible = false;
    }

    private Widget BuildUi()
    {
        var panel = new VerticalStackPanel
        {
            Spacing = 8,
            Padding = new Myra.Graphics2D.Thickness(8),
            Background = new SolidBrush(new Microsoft.Xna.Framework.Color(20, 20, 30, 230))
        };

        // Header
        panel.Widgets.Add(new Label { Text = "DEV PANEL  (F3)", TextColor = Microsoft.Xna.Framework.Color.White });

        // Seed row
        var seedRow = new HorizontalStackPanel { Spacing = 4 };
        seedRow.Widgets.Add(new Label { Text = "Seed:", Width = 50, TextColor = Microsoft.Xna.Framework.Color.White });
        _seedBox = new TextBox { Text = _pending.Seed.ToString(), Width = 100 };
        _seedBox.TextChanged += (_, _) =>
        {
            if (int.TryParse(_seedBox.Text, out int value))
            {
                _pending = _pending with { Seed = value };
            }
        };
        seedRow.Widgets.Add(_seedBox);
        panel.Widgets.Add(seedRow);

        // Two-column layout using horizontal stack
        var columnsContainer = new HorizontalStackPanel { Spacing = 16 };

        // Left column
        var leftColumn = new VerticalStackPanel { Spacing = 4, Width = 260 };

        // Population & Food section
        leftColumn.Widgets.Add(new Label { Text = "--- Population & Food ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(leftColumn, "InitialFoodDensity", 0f, 1f,
            () => _pending.InitialFoodDensity, value => _pending = _pending with { InitialFoodDensity = value });
        AddSlider(leftColumn, "FoodRegenRate", 0f, 0.02f,
            () => _pending.FoodRegenRate, value => _pending = _pending with { FoodRegenRate = value });
        AddSlider(leftColumn, "FoodEnergyValue", 1f, 100f,
            () => _pending.FoodEnergyValue, value => _pending = _pending with { FoodEnergyValue = value });
        AddSlider(leftColumn, "BiomassEatThreshold", 0.1f, 0.9f,
            () => _pending.BiomassEatThreshold, v => _pending = _pending with { BiomassEatThreshold = v });
        AddSlider(leftColumn, "BiomassConsumptionRate", 0.1f, 1f,
            () => _pending.BiomassConsumptionRate, v => _pending = _pending with { BiomassConsumptionRate = v });
        AddSlider(leftColumn, "StartingPopulation", 10f, 500f,
            () => _pending.StartingPopulation, value => _pending = _pending with { StartingPopulation = (int)value });

        // Genetics section
        leftColumn.Widgets.Add(new Label { Text = "--- Genetics ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(leftColumn, "MutationRate", 0f, 0.2f,
            () => _pending.MutationRate, value => _pending = _pending with { MutationRate = value });
        AddSlider(leftColumn, "InitialMaxDietType", 0f, 1f,
            () => _pending.InitialMaxDietType, value => _pending = _pending with { InitialMaxDietType = value });
        AddSlider(leftColumn, "WanderlustMin", 0f, 1f,
            () => _pending.InitialWanderlustMin, v => _pending = _pending with { InitialWanderlustMin = v });
        AddSlider(leftColumn, "WanderlustMax", 0f, 1f,
            () => _pending.InitialWanderlustMax, v => _pending = _pending with { InitialWanderlustMax = v });

        // Metabolism section
        leftColumn.Widgets.Add(new Label { Text = "--- Metabolism ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(leftColumn, "EnergyDrainCoefficient", 0.001f, 1f,
            () => _pending.EnergyDrainCoefficient, value => _pending = _pending with { EnergyDrainCoefficient = value });
        AddSlider(leftColumn, "PerceptionCostCoefficient", 0f, 0.001f,
            () => _pending.PerceptionCostCoefficient, value => _pending = _pending with { PerceptionCostCoefficient = value });
        AddSlider(leftColumn, "CarnivoreTax", 0f, 2f,
            () => _pending.CarnivoreTax, v => _pending = _pending with { CarnivoreTax = v });

        // Resting section
        leftColumn.Widgets.Add(new Label { Text = "--- Resting ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(leftColumn, "RestEnergyRecovery", 0f, 0.5f,
            () => _pending.RestEnergyRecovery, value => _pending = _pending with { RestEnergyRecovery = value });
        AddSlider(leftColumn, "RestStagnationThreshold", 10f, 500f,
            () => _pending.RestStagnationThreshold, v => _pending = _pending with { RestStagnationThreshold = (int)v });
        AddSlider(leftColumn, "RestStagnationPenaltyRate", 0.01f, 0.2f,
            () => _pending.RestStagnationPenaltyRate, v => _pending = _pending with { RestStagnationPenaltyRate = v });
        AddSlider(leftColumn, "HungerDilationMultiplier", 1f, 10f,
            () => _pending.HungerDilationMultiplier, v => _pending = _pending with { HungerDilationMultiplier = v });

        // Right column
        var rightColumn = new VerticalStackPanel { Spacing = 4, Width = 260 };

        // Reproduction section
        rightColumn.Widgets.Add(new Label { Text = "--- Reproduction ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(rightColumn, "ReproductionThreshold", 0.5f, 1f,
            () => _pending.ReproductionThreshold, value => _pending = _pending with { ReproductionThreshold = value });
        AddSlider(rightColumn, "ReproductionMatchThreshold", 0f, 1f,
            () => _pending.ReproductionMatchThreshold, v => _pending = _pending with { ReproductionMatchThreshold = v });

        // Biome section
        rightColumn.Widgets.Add(new Label { Text = "--- Biome ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(rightColumn, "BiomeNoiseScale", 5f, 50f,
            () => _pending.BiomeNoiseScale, v => _pending = _pending with { BiomeNoiseScale = (int)v });
        AddSlider(rightColumn, "MudSandBalance", 0.2f, 0.8f,
            () => _pending.MudSandBalance, v => _pending = _pending with { MudSandBalance = v });
        AddSlider(rightColumn, "GrasslandDegradationChance", 0f, 0.2f,
            () => _pending.GrasslandDegradationChance, v => _pending = _pending with { GrasslandDegradationChance = v });
        AddSlider(rightColumn, "MudRecoveryRate", 0f, 0.01f,
            () => _pending.MudRecoveryRate, v => _pending = _pending with { MudRecoveryRate = v });
        AddSlider(rightColumn, "MudRecoveryCooldown", 60f, 1800f,
            () => _pending.MudRecoveryCooldown, v => _pending = _pending with { MudRecoveryCooldown = (int)v });

        // World section
        rightColumn.Widgets.Add(new Label { Text = "--- World ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(rightColumn, "WorldTilesX", 100f, 600f,
            () => _pending.WorldTilesX, v => _pending = _pending with { WorldTilesX = (int)v });
        AddSlider(rightColumn, "WorldTilesY", 100f, 600f,
            () => _pending.WorldTilesY, v => _pending = _pending with { WorldTilesY = (int)v });
        AddSlider(rightColumn, "SpatialCellPixels", 20f, 200f,
            () => _pending.SpatialCellPixels, v => _pending = _pending with { SpatialCellPixels = (int)v });

        // Display section
        rightColumn.Widgets.Add(new Label { Text = "--- Display ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(rightColumn, "WindowWidth", 800f, 2400f,
            () => _pending.WindowWidth, v => _pending = _pending with { WindowWidth = (int)v });
        AddSlider(rightColumn, "WindowHeight", 600f, 1600f,
            () => _pending.WindowHeight, v => _pending = _pending with { WindowHeight = (int)v });

        // Performance section
        rightColumn.Widgets.Add(new Label { Text = "--- Performance ---", TextColor = Microsoft.Xna.Framework.Color.LightGray });
        AddSlider(rightColumn, "StatsSmoothingTicks", 1f, 120f,
            () => _pending.StatsSmoothingTicks, v => _pending = _pending with { StatsSmoothingTicks = (int)v });

        columnsContainer.Widgets.Add(leftColumn);
        columnsContainer.Widgets.Add(rightColumn);
        panel.Widgets.Add(columnsContainer);

        // Buttons row
        var buttonRow = new HorizontalStackPanel { Spacing = 8 };
        var applyButton = new Button { Content = new Label { Text = "Apply & Reset", TextColor = Microsoft.Xna.Framework.Color.Black }, Width = 120 };
        applyButton.Click += (_, _) => _onApplyAndReset(_pending);
        buttonRow.Widgets.Add(applyButton);

        var reseedButton = new Button { Content = new Label { Text = "Reseed", TextColor = Microsoft.Xna.Framework.Color.Black }, Width = 120 };
        reseedButton.Click += (_, _) => _onReseed();
        buttonRow.Widgets.Add(reseedButton);

        panel.Widgets.Add(buttonRow);

        return panel;
    }

    private void AddSlider(VerticalStackPanel parent, string label, float min, float max, Func<float> getter, Action<float> setter)
    {
        var row = new VerticalStackPanel { Spacing = 2 };
        var nameLabel = new Label { Text = $"{label}: {getter():G3}", TextColor = Microsoft.Xna.Framework.Color.White };
        _labels[label] = nameLabel;
        var slider = new HorizontalSlider { Minimum = min, Maximum = max, Value = getter(), Width = 240 };
        slider.ValueChanged += (_, _) =>
        {
            setter(slider.Value);
            nameLabel.Text = $"{label}: {slider.Value:G3}";
        };

        row.Widgets.Add(nameLabel);
        row.Widgets.Add(slider);
        parent.Widgets.Add(row);
    }

    public void SyncPendingFromConfig(SimulationConfig current)
    {
        _pending = current;
        _seedBox.Text = current.Seed.ToString();
    }
}