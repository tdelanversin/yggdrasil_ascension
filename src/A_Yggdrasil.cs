using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

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

            Settings.ToggleFullscreen();

            var res_x = _graphics.PreferredBackBufferWidth;
            var res_y = _graphics.PreferredBackBufferHeight;
            Camera.Position = new Vector2(res_x / 2, res_y / 2);
            Camera.Bounds = _graphics.GraphicsDevice.Viewport.Bounds;
            Camera.Mode = CameraMode.Follow; /* 'Follow' to follow players, 'Manual' for keyboard controlled */

            Factory_Debug.Initialize(Content);
            Manager_Projectile.Initialize(Content);
            Manager_Enemies.Initialize(Content);
            Manager_Players.Initialize();
            Manager_Particles.Initialize();
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
            // Uncomment to play intro sound in a loop
            //MediaPlayer.Play(Manager_Sound.AddSong_Intro());
            MediaPlayer.IsRepeating = true;
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;
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

            _level = new Y_Level("level_0", 48, "Levels/Level_0", GraphicsDevice);

            Manager_Players.ClearPlayers();

            Manager_Players.AddPlayer_Ninja(PlayerIndex.One, new Vector2(200, 180), _level, ControlLayout.KeyboardWASD);
            Manager_Players.AddPlayer_SimplePlayer(PlayerIndex.Two, new Vector2(200, 360), _level, ControlLayout.KeyboardArrows);

            for (int i = 2; i < 4; i++)
            {
                PlayerIndex playerIndex = (PlayerIndex)i;
                if (GamePad.GetState(playerIndex).IsConnected)
                {
                    Manager_Players.AddPlayer_SimplePlayer(playerIndex, position: new Vector2(200, 180 + i * 180), _level);
                }
            }
            
            // Pass players to camera so it can follow their positions
            Camera.Players = Manager_Players.Players;

            for (int i = 0; i < 6; i++)
            {
                Manager_Enemies.AddEnemy_SimpleEnemy(new Vector2(1050 + i * 200, 350), _level, Manager_Players.Players);
            }

            // Once everything is in place, inform Update() of the new desired state
            DesiredState = GameState.InGame;
        }
        void MediaPlayer_MediaStateChanged(object sender, System.
                                   EventArgs e)
        {
            // 0.0f is silent, 1.0f is full volume
            // MediaPlayer.Volume -= 0.1f;
            // MediaPlayer.Play(song);
        }
        protected override void Update(GameTime gameTime)
        {
            Input.Update();

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
                    float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    Camera.UpdateCamera(_graphics.GraphicsDevice.Viewport, deltaTime);

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
            base.Draw(gameTime);
        }
    }
}
