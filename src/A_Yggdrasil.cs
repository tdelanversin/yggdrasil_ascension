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
            Input.Initialize();
            Util.Initialize(this);
            Menu.Initialize(this);
            Settings.ApplyScreenConfiguration();

            var res_x = _graphics.PreferredBackBufferWidth;
            var res_y = _graphics.PreferredBackBufferHeight;
            Camera.Initialize(new Vector2(res_x / 2, res_y / 2), _graphics.GraphicsDevice.Viewport, CameraMode.Follow);

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
            /*
            # PLAN

            ## Basics
            - Setup a small starting/tutorial room
            - Initialize 4 (random) players, place them nicely spaced out
            - Have a starting room section (marked rectangle)
                - Every player that wants to play moves their character into the
                  rectangle
            - To start the game, all players shoot a start button / pillar
                - Needs to be hit by >= X different players, where X is the
                  amount of players standing in the marked rectangle

            ## Further points
            - Have either a pillar to shoot that switches a players character to
              a different one, either random, or cycle through, or possibly have
              different pillars to shoot at to select the character
            - WASD+Mouse could either be
                - Always the secondary controls for Player 1, alongside the
                  first controller
                - Configurable, so that if you have for example only a single
                  controller, one can play with keyboard+mouse and the other
                  with controller
            - Start level generation directly from and with this initial room,
              for a smooth transition.
            */

            Manager_Light2.Platform = Manager_Light2.Type.GPU;
            _level.Create(GraphicsDevice);

            // Once everything is in place, inform Update() of the new desired state
            DesiredState = GameState.InGame;
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();
            Notifications.Update(gameTime);

            if (Input.IsKeyTriggered(Keys.Escape) || Input.IsButtonTriggered(0, Buttons.Back))
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

            if (Input.IsKeyTriggered(Keybinds.ToggleFullscreen))
            {
                Settings.ToggleFullscreen();
            }

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
