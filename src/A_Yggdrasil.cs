using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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

        Texture2D _background;
        IList<IVictim> _player;
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
            Util.Initialize(this);
            Util.ToggleFullscreen();
            Menu.Initialize(this);

            var res_x = _graphics.PreferredBackBufferWidth;
            var res_y = _graphics.PreferredBackBufferHeight;
            Camera.Position = new Vector2(res_x / 2, res_y / 2);
            Camera.Bounds = _graphics.GraphicsDevice.Viewport.Bounds;

            // Set the camera mode, e.g. 'Follow' to follow players, 'Manual' for keyboard controlled
            Camera.Mode = CameraMode.Follow;

            Factory_Debug.Initialize(Content);
            Manager_Projectile.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Fonts.LoadContent(Content);
            Menu.LoadContent(Content);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>("background");
        }

        internal void StartNewGame()
        {
            _level = new Y_Level("level_0", 32, "Levels/Level_0", GraphicsDevice);
            _player = new List<IVictim>{
                new Ninja(
                    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                    PlayerIndex.One,
                    Content.Load<Texture2D>("charaset"),
                    0.02f,
                    new Vector2(200, 350),
                    _level,
                    new Y_StarterGun()
                ),
                new Y_CMSprite(
                    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                    null,
                    Content.Load<Texture2D>("tester_60"),
                    new Rectangle(0, 0, 42, 60),
                    0.004f, // acceleration
                    0.4f,  // max velocity
                    new Vector2(250, 350),
                    _level,
                    200.0f,
                    new Dictionary<string, int[]> {
                                    { "stand", new int[] { 0, 1, 8, 9 } },
                                    { "walk_left", new int[] { 2, 3, 4 } },
                                    { "walk_right", new int[] { 5, 6, 7 } }},
                    new Y_WideGun(),
                    2, // control input
                    1.0f
                )
            };

            // Pass players to camera so it can follow their positions
            Camera.Players = _player;

            // Once everything is in place, inform Update() of the new desired state
            DesiredState = GameState.InGame;
        }

        protected override void Update(GameTime gameTime)
        {
            Keyboard.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.HasBeenPressed(Keys.Escape))
                DesiredState = GameState.PreGame;
            
            // Only switch actual state during Update(), otherwise you can mess up the Draw call
            State = DesiredState;

            if (Keyboard.HasBeenPressed(Keybinds.ToggleFullscreen))
                Util.ToggleFullscreen();
                Menu.RepositionMenuItems();

            switch (State)
            {
                case GameState.PreGame:
                    Menu.Update();
                    break;
                case GameState.InGame:
                    float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    Camera.UpdateCamera(_graphics.GraphicsDevice.Viewport, deltaTime);

                    foreach (var player in _player)
                    {
                        player.Update(gameTime);
                    }

                    Manager_Projectile.Update(gameTime);
                    break;
                case GameState.Menu:
                    Menu.Update();
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            switch (State)
            {
                case GameState.PreGame:
                    _spriteBatch.Begin(
                            SpriteSortMode.Immediate, null, null, null, null, null,
                            null);
                    Menu.Draw(_spriteBatch);
                    _spriteBatch.End();
                    break;
                case GameState.InGame:
                    Vector2 zero = Vector2.Zero;

                    _spriteBatch.Begin(
                        SpriteSortMode.Immediate, null, null, null, null, null,
                        Camera.Transform);

                    _level.Draw(gameTime, Vector2.Zero, _spriteBatch);
                    _level.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);

                    Manager_Projectile.Draw(gameTime, zero, _spriteBatch);
                    Manager_Projectile.DrawOutline(gameTime, zero, _spriteBatch);

                    foreach (var player in _player)
                    {
                        player.Draw(gameTime, zero, _spriteBatch);
                        player.DrawOutline(gameTime, zero, _spriteBatch);
                    }

                    _spriteBatch.End();
                    break;
                case GameState.Menu:
                    break;
            }

            base.Draw(gameTime);
        }
    }
}
