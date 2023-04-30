using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YGR
{
    public enum GameState
    {
        PreGame,
        InGame,
        Menu,
    }

    public class A_Yggdrasil : Game
    {
        public Clearcove.Logging.Logger logger;

        public GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter = new FrameCounter();

        Y_Level _level;
        public GameState State;
        public GameState DesiredState;
        public A_Yggdrasil()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            State = GameState.PreGame;
            Settings.Initialize(this);
            Input.Initialize(this);
            Util.Initialize(this);
            Menu.Initialize(this);
            Settings.ApplyScreenConfiguration();
            Camera.Initialize(_graphics, CameraMode.Follow);

            string level = "Level_3";
            Factory_Debug.Initialize(Content);
            Manager_Projectile.Initialize(Content);
            Manager_Enemies.Initialize(Content);
            Manager_Players.Initialize();
            Manager_Particles.Initialize();
            Manager_Light2.Initialize("./Levels/" + level + "/simplified", Content, GraphicsDevice);
            X_AutoTiler.Initialize("./Doors/", "data.json", GraphicsDevice, Y_Door.MapJsonName);
            X_AutoTiler.Initialize("./Levels/", "doors.json", GraphicsDevice, Y_CMRoom.MapJsonName);
            Y_PowerUp.Initialize(Content);
            Y_MultiPowerUp.Initialize(Content);

            _level = new Y_Level(level, 32, 32, "./Levels/", "./Doors");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Fonts.LoadContent(Content);
            Menu.LoadContent(Content);
            Manager_Sound.LoadContent(Content);
            Manager_Players.LoadContent(Content);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Manager_Particles.LoadContent(Content);
            Manager_Sound.PlayMainMenuMusic();
        }

        protected override void UnloadContent()
        {
            Manager_Particles.Dispose();
        }

        internal void StartNewGame()
        {
            // Stop any playing songs and play a viking horn, just because
            Manager_Sound.StopMusic();
            Manager_Sound.Sound_VikingHorn.Play();

            Notifications.Clear();

            Manager_Light2.Platform = Manager_Light2.Type.GPU;
            _level.Create(GraphicsDevice);

            // Once everything is in place, inform Update() of the new desired state
            DesiredState = GameState.InGame;
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();
            Notifications.Update(gameTime);

            // Keybind to switch in and out of the menu screen
            if (Input.IsKeyTriggered(Keys.Escape) || Input.IsButtonTriggered(0, Buttons.Start))
            {
                if (State == GameState.InGame)
                {
                    DesiredState = GameState.Menu;
                }
                if (State == GameState.Menu)
                {
                    DesiredState = GameState.InGame;
                }
                if (State == GameState.PreGame)
                {
                    // Nothing for now
                }
            }

            // Only switch actual state during Update(), otherwise you can mess up the Draw call
            State = DesiredState;

            // Keybind to toggle fullscreen
            if (Input.IsKeyTriggered(Keybinds.ToggleFullscreen))
            {
                Settings.ToggleFullscreen();
            }

            // Keybind to cycle camera mode
            if (Input.IsKeyTriggered(Keybinds.CycleCameraMode))
            {
                Camera.CycleCameraMode();
                Notifications.New("Camera mode switched to " + Camera.Mode);
            }

            // Update all entities in current game state
            switch (State)
            {
                case GameState.PreGame:
                    Menu.Update();
                    break;
                case GameState.InGame:
                    Camera.Update(_graphics.GraphicsDevice.Viewport, gameTime);
                    Manager_Players.Update(gameTime);
                    Manager_Projectile.Update(gameTime);
                    Manager_Enemies.Update(gameTime);
                    _level.Update(gameTime);
                    break;
                case GameState.Menu:
                    Menu.Update();
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Vector2 zero = Vector2.Zero;

            switch (State)
            {
                case GameState.PreGame:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _spriteBatch.Begin(
                            SpriteSortMode.Immediate, null, null, null, null, null,
                            null);
                    Menu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;

                case GameState.InGame:
                    GraphicsDevice.Clear(_level.OutsideColor);
                    _spriteBatch.Begin(
                        SpriteSortMode.Immediate, null, null, null, null, null,
                        Camera.Transform);

                    _level.Draw(gameTime, Vector2.Zero, _spriteBatch);

                    Manager_Particles.Draw(gameTime, zero, _spriteBatch);
                    Manager_Projectile.Draw(gameTime, zero, _spriteBatch);

                    Manager_Enemies.Draw(gameTime, zero, _spriteBatch);
                    Manager_Players.Draw(gameTime, zero, _spriteBatch);

                    if (Settings.Outlines)
                    {
                        _level.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);
                        Manager_Projectile.DrawOutline(gameTime, zero, _spriteBatch);
                        Manager_Players.DrawOutline(gameTime, zero, _spriteBatch);
                        Manager_Enemies.DrawOutline(gameTime, zero, _spriteBatch);
                    }
                    _spriteBatch.End();
                    break;

                case GameState.Menu:
                    GraphicsDevice.Clear(Color.CornflowerBlue);
                    _spriteBatch.Begin(
                            SpriteSortMode.Immediate, null, null, null, null, null,
                            null);
                    Menu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;
            }

            _frameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            string fps = string.Format("FPS: {0:0}", _frameCounter.AverageFramesPerSecond);
            var fpsColor = Color.BlanchedAlmond;
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);
            _spriteBatch.DrawString(Fonts.Normal, fps, new Vector2(1, 1), fpsColor);
            Notifications.Draw(gameTime, zero, _spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
