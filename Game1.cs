using DigitalTerrarium.Core;
using DigitalTerrarium.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;

namespace DigitalTerrarium;

public class Game1 : Game
{
    private SimulationConfig _config = SimulationConfig.WithWallClockSeed();

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Simulation _simulation = null!;
    private RenderSystem _renderSystem = null!;
    private PerceptionRadar _perceptionRadar = null!;
    private TimeController _time = null!;
    private KeyboardState _prevKeyboard;
    private Dashboard _dashboard = null!;
    private Desktop _desktop = null!;
    private DevPanel _devPanel = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth  = _config.WindowWidth,
            PreferredBackBufferHeight = _config.WindowHeight
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _simulation = new Simulation(_config);
        _time = new TimeController();
        MyraEnvironment.Game = this;
        _desktop = new Desktop();
        _devPanel = new DevPanel(
            _simulation.Config,
            onApplyAndReset: cfg => _simulation.ApplyConfigAndReset(cfg),
            onReseed: () => _simulation.ApplyConfigAndReset(_simulation.Config));
        _devPanel.Root.HorizontalAlignment = HorizontalAlignment.Right;
        _devPanel.Root.VerticalAlignment = VerticalAlignment.Bottom;
        _desktop.Root = _devPanel.Root;
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderSystem = new RenderSystem(GraphicsDevice);
        _perceptionRadar = new PerceptionRadar(_renderSystem.Pixel);
        SpriteFont font = Content.Load<SpriteFont>("Fonts/DashboardFont");
        _dashboard = new Dashboard(font);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (keyboard.IsKeyDown(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.Space)) _time.TogglePause();
        if (keyboard.IsKeyDown(Keys.OemOpenBrackets) && _prevKeyboard.IsKeyUp(Keys.OemOpenBrackets)) _time.StepDown();
        if (keyboard.IsKeyDown(Keys.OemCloseBrackets) && _prevKeyboard.IsKeyUp(Keys.OemCloseBrackets)) _time.StepUp();
        if (keyboard.IsKeyDown(Keys.R) && _prevKeyboard.IsKeyUp(Keys.R))
        {
            _simulation.ApplyConfigAndReset(_simulation.Config);
        }

        if (keyboard.IsKeyDown(Keys.F3) && _prevKeyboard.IsKeyUp(Keys.F3))
        {
            _devPanel.IsVisible = !_devPanel.IsVisible;
            if (_devPanel.IsVisible)
            {
                _devPanel.SyncPendingFromConfig(_simulation.Config);
            }
        }

        if (keyboard.IsKeyDown(Keys.V) && _prevKeyboard.IsKeyUp(Keys.V))
        {
            _perceptionRadar.Visible = !_perceptionRadar.Visible;
        }

        _time.RunPendingTicks(gameTime.ElapsedGameTime.TotalSeconds, _simulation.Tick);

        _prevKeyboard = keyboard;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(8, 8, 12));

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        int viewportSize = _simulation.Config.ViewportSize;
        int windowHeight = _simulation.Config.WindowHeight;
        _renderSystem.Draw(
            _spriteBatch,
            _simulation.World,
            _simulation.Organisms,
            new Rectangle(0, 0, viewportSize, viewportSize));
        _perceptionRadar.Draw(
            _spriteBatch,
            _simulation.Organisms,
            new Rectangle(0, 0, viewportSize, viewportSize));
        _dashboard.Draw(_spriteBatch, new Rectangle(viewportSize, 0, 224, windowHeight), _simulation, _time);
        _spriteBatch.End();

        _desktop.Render();

        base.Draw(gameTime);
    }
}
