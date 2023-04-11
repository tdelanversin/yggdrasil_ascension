using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace YGR
{

    public class A_Yggdrasil : Game
    {
        public Clearcove.Logging.Logger logger;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Texture2D _background;
        IList<IVictim> _player;
        Y_Level _level;
        //IDictionary<string, IWalkable> _rooms;

        public A_Yggdrasil()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Util.ToggleFullscreen(_graphics, Window);

            var res_x = _graphics.PreferredBackBufferWidth;
            var res_y = _graphics.PreferredBackBufferHeight;
            Camera.Position = new Vector2(res_x / 2, res_y / 2);
            Camera.Bounds = _graphics.GraphicsDevice.Viewport.Bounds;

            // Set the camera mode, e.g. 'Follow' to follow players, 'Manual' for keyboard controlled
            Camera.Mode = CameraMode.Follow;

            //Factory_Rooms.Initialize(Content);
            //Factory_Connectors.Initialize(Content);
            Factory_Debug.Initialize(Content);
            Manager_Projectile.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>("background");

            _level = new Y_Level("level_0", 32, "Levels/Level_0", GraphicsDevice);

            _player = new List<IVictim>{
                //new Ninja(
                //    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                //    PlayerIndex.One,
                //    Content.Load<Texture2D>("charaset"),
                //    0.02f,
                //    new Vector2(200, 350),
                //    _level,
                //    new Y_StarterGun()
                //),
                new Y_CMSprite(
                    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                    null,
                    Content.Load<Texture2D>("tester_60"),
                    //new Rectangle(0, 0, 21, 30),
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

            Camera.Players = _player;
        }

        protected override void Update(GameTime gameTime)
        {
            Keyboard.Update();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.IsPressed(Keys.Escape))
                Exit();

            if (Keyboard.HasBeenPressed(Keybinds.ToggleFullscreen))
                Util.ToggleFullscreen(_graphics, Window);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Camera.UpdateCamera(_graphics.GraphicsDevice.Viewport, deltaTime);

            foreach (var player in _player)
            {
                player.Update(gameTime);
            }

            Manager_Projectile.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            Vector2 zero = Vector2.Zero;

            _spriteBatch.Begin(
                SpriteSortMode.Immediate, null, null, null, null, null,
                Camera.Transform);

            //_spriteBatch.Draw(
            //    _background,
            //    new Rectangle(0, 0, 3840, 2160),
            //    new Rectangle(0, 0, 3840, 2160),
            //    Color.White
            //);

            _level.Draw(gameTime, Vector2.Zero, _spriteBatch);
            _level.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);

            //foreach (var room in _rooms.Values)
            //{
            //    room.Draw(gameTime, zero, _spriteBatch);
            //    room.DrawOutline(gameTime, zero, _spriteBatch);
            //    // uncomment for debugging
            //    //room.DrawOutline(gameTime, zero, _spriteBatch);
            //}

            Manager_Projectile.Draw(gameTime, zero, _spriteBatch);
            Manager_Projectile.DrawOutline(gameTime, zero, _spriteBatch);

            foreach (var player in _player)
            {
                player.Draw(gameTime, zero, _spriteBatch);
                player.DrawOutline(gameTime, zero, _spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
