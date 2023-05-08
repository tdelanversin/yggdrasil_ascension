using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

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

            State = GameState.PreGame;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Fonts.LoadContent(Content);
            Menu.LoadContent(Content);
            Manager_Sound.LoadContent(Content);
            Manager_Sprites.LoadContent(Content);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Manager_Particles.LoadContent(Content, GraphicsDevice);
            Manager_Sound.PlayMainMenuMusic();

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

            // Transition the camera when switching from in-game to menu and vice versa
            if (State != DesiredState)
            {
                if (DesiredState == GameState.InGame)
                {
                    if (_level.State == Y_Level.GamePlayState.FreeRoam)
                        Camera.SetFocusPlayers(animate: true);
                    else if (_level.State == Y_Level.GamePlayState.Encounter)
                        Camera.SetFocusRoom(_level.ActiveRoom, animate: true);
                    else if (_level.State == Y_Level.GamePlayState.Start && Camera.Mode == CameraMode.Rect)
                        Camera.SetFocusRoom(_level.ActiveRoom, animate: true);
                }
                if (DesiredState == GameState.Menu)
                    Camera.SetFocusRect(_background.Rect, animate: true, animationDuration: 750);
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
                foreach (var p in Manager_Players.Players)
                {
                    p.Gun = new Gun_Godmode();
                    ((SimplePlayer)p).VelocityMax = 0.6f;
                    p.LifePoints = 9999;
                }
            }

            Camera.Update(_graphics.GraphicsDevice.Viewport, gameTime);
            // Update all entities in current game state
            switch (State)
            {
                case GameState.PreGame:
                    Menu.Update();
                    break;
                case GameState.InGame:
                    Manager_Players.Update(gameTime);
                    Manager_Projectile.Update(gameTime);
                    Manager_Enemies.Update(gameTime);
                    Manager_Light2.Update(gameTime);
                    Manager_Particles.Update(gameTime);
                    Manager_Sound.Update(gameTime);
                    _level.Update(gameTime);
                    break;
                case GameState.Menu:
                    Menu.Update();
                    _level.Update(gameTime);
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

            _background.Draw(gameTime, zero, _spriteBatch);
            switch (State)
            {
                case GameState.PreGame:
                    _background.DrawTitleText(gameTime, Vector2.Zero, _spriteBatch);
                    break;

                case GameState.InGame:
                    _level.Draw(gameTime, Vector2.Zero, _spriteBatch);

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

            // Level- / Menu UI
            switch (State)
            {
                case GameState.PreGame:
                    Menu.Draw(_spriteBatch);
                    break;

                case GameState.InGame:
                    _level.DrawUI(gameTime, _spriteBatch);
                    break;

                case GameState.Menu:
                    _level.DrawPlayerStatusUI(gameTime, _spriteBatch);
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
