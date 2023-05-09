using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YGR
{
    public enum GameState
    {
        Intro,
        PreGame,
        InGame,
        Menu
    }

    public class A_Yggdrasil : Game
    {
        public Clearcove.Logging.Logger logger;

        public GraphicsDeviceManager _graphics;
        public SpriteBatch _spriteBatch;
        private FrameCounter _frameCounter = new FrameCounter();
        private Background _background;

        public static GraphicsDevice GraphicsDevice_;

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
            Settings.Initialize(this);
            Input.Initialize(this);
            Util.Initialize(this);
            Menu.Initialize(this);
            Settings.ApplyScreenConfiguration();
            Camera.Initialize(_graphics, CameraMode.Follow);
            Manager_Video.Initialize(GraphicsDevice, "./Intro/Intro.mp4");

            string level = "Level_3";
            Factory_Debug.Initialize(Content);
            Manager_Players.Initialize();
            Manager_Particles.Initialize();
            Manager_Light2.Initialize("./Levels/" + level + "/simplified", Content, GraphicsDevice);
            X_AutoTiler.Initialize("./Doors/", "data-brown.json", GraphicsDevice, Y_Door.MapJsonName);
            X_AutoTiler.Initialize("./Doors/", "data-green.json", GraphicsDevice, Y_Door.MapJsonName);
            Y_Door.Initialize(Content);

            GraphicsDevice_ = GraphicsDevice;
            _background = new Background();
            _level = new Y_Level(level, 32, 32, "./Levels/", "./Doors", Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Fonts.LoadContent(Content);
            Manager_Sound.LoadContent(Content);
            Manager_Sprites.LoadContent(Content);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Manager_Particles.LoadContent(Content, GraphicsDevice);

            Manager_Video.Play();

            _background.LoadContent(Content);
            _level.Preprocess(GraphicsDevice);
        }

        internal void StartNewGame()
        {
            // Stop any playing songs and play a viking horn, just because
            // Manager_Sound.StopMusic();
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
            if (Input.IsKeyTriggered(Keys.Escape) || Input.IsButtonTriggeredAny(Buttons.Start))
            {
                if (State == GameState.Intro)
                {
                    DesiredState = GameState.PreGame;
                }
                else if (State == GameState.InGame)
                {
                    DesiredState = GameState.Menu;
                }
                else if (State == GameState.Menu)
                {
                    DesiredState = GameState.InGame;
                }
                else if (State == GameState.PreGame)
                {
                    // Nothing for now
                }
                else
                {
                    Logger.Error("Invalid state requested");
                }
            }

            // Transition the camera when switching from in-game to menu and vice versa
            if (State != DesiredState)
            {
                if (State == GameState.Intro)
                {
                    Manager_Video.Stop();
                    Manager_Video.Dispose();
                    Manager_Sound.PlayMainMenuMusic();
                }
                if (DesiredState == GameState.InGame)
                {
                    if (_level.State == Y_Level.GamePlayState.Start && Camera.Mode == CameraMode.Room)
                    {
                        // Don't override the slow camera transition on game starts
                    }
                    else
                    {
                        Camera.RestorePreviousMode();
                    }
                    // if (_level.State == Y_Level.GamePlayState.FreeRoam)
                    //     Camera.SetFocusPlayers(animate: true);
                    // else if (_level.State == Y_Level.GamePlayState.Encounter)
                    //     Camera.SetFocusRoom(_level.ActiveRoom, animate: true);
                    // else if (_level.State == Y_Level.GamePlayState.Start && Camera.Mode == CameraMode.Menu)
                    //     Camera.SetFocusRoom(_level.ActiveRoom, animate: true);
                }
                if (DesiredState == GameState.Menu)
                {
                    Camera.SetFocusMenu(_background.Rect, animate: true, animationDuration: 750);
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

            if (Input.IsKeyTriggered(Keybinds.GodMode))
            {
                foreach (var p in Manager_Players.Players) { p.Godmode(); }
            }

            if (Input.IsKeyTriggered(Keybinds.KillAllEnemies))
            {
                Manager_Enemies.KillAllNormalEnemies();
            }

            Camera.Update(_graphics.GraphicsDevice.Viewport, gameTime);

            // Update all entities in current game state
            switch (State)
            {
                case GameState.Intro:
                    // Handled entirely in Draw()
                    break;
                case GameState.PreGame:
                    _background.Update(gameTime);
                    Menu.Update();
                    break;
                case GameState.InGame:
                    _background.Update(gameTime);
                    Manager_Players.Update(gameTime);
                    Manager_Projectile.Update(gameTime);
                    Manager_Enemies.Update(gameTime);
                    Manager_Light2.Update(gameTime);
                    Manager_Particles.Update(gameTime);
                    Manager_Sound.Update(gameTime);
                    _level.Update(gameTime);
                    break;
                case GameState.Menu:
                    _background.Update(gameTime);
                    Menu.Update();
                    break;
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Vector2 zero = Vector2.Zero;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            /* ### Draw everything that is an in-game level element and zoomable, e.g. not UI / text ### */
            _spriteBatch.Begin(
                   SpriteSortMode.Immediate, null, null, null, null, null,
                   Camera.Transform);

            switch (State)
            {
                case GameState.PreGame:
                    _background.Draw(gameTime, zero, _spriteBatch);
                    _background.DrawTitleText(gameTime, Vector2.Zero, _spriteBatch);
                    break;

                case GameState.InGame:
                    _background.Draw(gameTime, zero, _spriteBatch);
                    _level.Draw(gameTime, Vector2.Zero, _spriteBatch);
                    _background.DrawTitleText(gameTime, Vector2.Zero, _spriteBatch);

                    Manager_Particles.Draw(gameTime, _spriteBatch);
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
                    break;

                case GameState.Menu:
                    _background.Draw(gameTime, zero, _spriteBatch);
                    _level.Draw(gameTime, Vector2.Zero, _spriteBatch);
                    _background.DrawTitleText(gameTime, Vector2.Zero, _spriteBatch);
                    break;
            }
            _spriteBatch.End();


            /* ### Draw everything that is NOT an in-game level element ### */
            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, null);

            // Fps Counter
            _frameCounter.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            string fps = string.Format("FPS: {0:0}", _frameCounter.AverageFramesPerSecond);
            var fpsColor = Color.BlanchedAlmond;
            _spriteBatch.DrawString(Fonts.Small, fps, new Vector2(1, 1), fpsColor);

            // Level- / Menu UI / Intro Video
            switch (State)
            {
                case GameState.Intro:
                    bool finished = Manager_Video.Draw(gameTime, _spriteBatch);
                    if (finished)
                    {
                        DesiredState = GameState.PreGame;
                    }
                    break;
                case GameState.PreGame:
                    Menu.Draw(_spriteBatch);
                    break;

                case GameState.InGame:
                    if (_level.State == Y_Level.GamePlayState.Start)
                    {
                        UI.DrawPlayerSelection(gameTime, _spriteBatch);
                    }
                    UI.DrawPlayerStatus(gameTime, _spriteBatch);
                    break;

                case GameState.Menu:
                    UI.DrawPlayerStatus(gameTime, _spriteBatch);
                    // UI.DrawPlayerStatistics(gameTime, _spriteBatch);
                    Menu.Draw(_spriteBatch);
                    break;
            }

            // Finally, notifications
            Notifications.Draw(gameTime, zero, _spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
