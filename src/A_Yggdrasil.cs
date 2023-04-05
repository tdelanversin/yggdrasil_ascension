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
        Y_Camera _camera;
        IList<IVictim> _player;
        IDictionary<string, IWalkable> _rooms;

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
            _camera = new Y_Camera(_graphics.GraphicsDevice.Viewport, new Vector2(res_x / 2, res_y / 2));

            // Set the camera mode, e.g. 'Follow' to follow players, 'Manual' for keyboard controlled
            _camera.Mode = CameraMode.Follow;

            Factory_Rooms.Initialize(Content);
            Factory_Connectors.Initialize(Content);
            Factory_Debug.Initialize(Content);
            Manager_Projectile.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>("background");

            _rooms = new Dictionary<string, IWalkable> {
                { "room_center", new Y_CMRoom("hello", new X_CollisionModel_Room("Rooms/Collisions2.csv", 40, 40, new Point(0,0)))}
            };

            _player = new List<IVictim>{
                new Ninja(
                    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                    PlayerIndex.One,
                    Content.Load<Texture2D>("charaset"),
                    0.02f,
                    new Vector2(200, 350),
                    _rooms["room_center"],
                    new Y_StarterGun()
                ),
                new Y_CMSprite(
                    new X_CollisionModel_Victim(1.0f /* mass */, 0.0f /* elastic impact */),
                    null,
                    Content.Load<Texture2D>("tester_30"),
                    new Rectangle(0, 0, 21, 30),
                    0.004f, // acceleration
                    0.4f,  // max velocity
                    new Vector2(250, 350),
                    _rooms["room_center"],
                    200.0f,
                    new Dictionary<string, int[]> {
                                    { "stand", new int[] { 0, 1, 8, 9 } },
                                    { "walk_left", new int[] { 2, 3, 4 } },
                                    { "walk_right", new int[] { 5, 6, 7 } }},
                    new Y_WideGun(),
                    2 // control input
                )
            };
            _camera.Players = _player;
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

            _camera.UpdateCamera(_graphics.GraphicsDevice.Viewport, deltaTime);

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
                _camera.Transform);

            _spriteBatch.Draw(
                _background,
                new Rectangle(0, 0, 3840, 2160),
                new Rectangle(0, 0, 3840, 2160),
                Color.White
            );

            foreach (var room in _rooms.Values)
            {
                room.Draw(gameTime, zero, _spriteBatch);
                room.DrawOutline(gameTime, zero, _spriteBatch);
                // uncomment for debugging
                //room.DrawOutline(gameTime, zero, _spriteBatch);
            }

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
