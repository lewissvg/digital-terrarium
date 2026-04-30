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
            Spacing = 4,
            Padding = new Myra.Graphics2D.Thickness(8),
            Background = new SolidBrush(new Microsoft.Xna.Framework.Color(20, 20, 30, 220))
        };

        panel.Widgets.Add(new Label { Text = "DEV PANEL  (F3)", TextColor = Microsoft.Xna.Framework.Color.White });

        var seedRow = new HorizontalStackPanel { Spacing = 4 };
        seedRow.Widgets.Add(new Label { Text = "Seed", Width = 80, TextColor = Microsoft.Xna.Framework.Color.White });
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

        AddSlider(panel, "InitialFoodDensity", 0f, 1f,
            () => _pending.InitialFoodDensity, value => _pending = _pending with { InitialFoodDensity = value });
        AddSlider(panel, "FoodRegenRate", 0f, 20f,
            () => _pending.FoodRegenRate, value => _pending = _pending with { FoodRegenRate = value });
        AddSlider(panel, "FoodEnergyValue", 1f, 100f,
            () => _pending.FoodEnergyValue, value => _pending = _pending with { FoodEnergyValue = value });
        AddSlider(panel, "StartingPopulation", 10f, 500f,
            () => _pending.StartingPopulation, value => _pending = _pending with { StartingPopulation = (int)value });
        AddSlider(panel, "MutationRate", 0f, 0.2f,
            () => _pending.MutationRate, value => _pending = _pending with { MutationRate = value });
        AddSlider(panel, "InitialMaxDietType", 0f, 1f,
            () => _pending.InitialMaxDietType, value => _pending = _pending with { InitialMaxDietType = value });
        AddSlider(panel, "PerceptionCostCoefficient", 0f, 0.001f,
            () => _pending.PerceptionCostCoefficient, value => _pending = _pending with { PerceptionCostCoefficient = value });
        AddSlider(panel, "ReproductionThreshold", 0.5f, 1f,
            () => _pending.ReproductionThreshold, value => _pending = _pending with { ReproductionThreshold = value });
        AddSlider(panel, "EnergyDrainCoefficient", 0.001f, 1f,
            () => _pending.EnergyDrainCoefficient, value => _pending = _pending with { EnergyDrainCoefficient = value });
        AddSlider(panel, "RestEnergyRecovery", 0f, 0.5f,
            () => _pending.RestEnergyRecovery, value => _pending = _pending with { RestEnergyRecovery = value });

        var applyButton = new Button { Content = new Label { Text = "Apply & Reset", TextColor = Microsoft.Xna.Framework.Color.Black } };
        applyButton.Click += (_, _) => _onApplyAndReset(_pending);
        panel.Widgets.Add(applyButton);

        var reseedButton = new Button { Content = new Label { Text = "Reseed Same Config", TextColor = Microsoft.Xna.Framework.Color.Black } };
        reseedButton.Click += (_, _) => _onReseed();
        panel.Widgets.Add(reseedButton);

        return panel;
    }

    private void AddSlider(VerticalStackPanel parent, string label, float min, float max, Func<float> getter, Action<float> setter)
    {
        var row = new VerticalStackPanel { Spacing = 2 };
        var nameLabel = new Label { Text = $"{label}: {getter():G3}", TextColor = Microsoft.Xna.Framework.Color.White };
        var slider = new HorizontalSlider { Minimum = min, Maximum = max, Value = getter(), Width = 200 };
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
