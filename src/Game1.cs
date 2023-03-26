using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;

namespace YGR
{

    public class Game1 : Game
    {
        public Clearcove.Logging.Logger logger;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int RES_X = 1920;
        private const int RES_Y = 1080;

        Texture2D _background;
        Y_Camera _camera;
        Y_Sprite _player;
        List<Y_Connector> _connectors;
        Dictionary<string, Y_Room> _rooms;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = RES_X;
            _graphics.PreferredBackBufferHeight = RES_Y;
            _graphics.ApplyChanges();

            _camera = new Y_Camera(new Vector2(RES_X / 2, RES_Y / 2), RES_X, RES_Y);
            Logger.Info("Set resolution to " + RES_X.ToString() + "x" + RES_Y.ToString());

            Factory_Rooms.Initialize(Content);
            Factory_Connectors.Initialize(Content);
            ProjectileManager.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _background = Content.Load<Texture2D>("background");

            _rooms = new Dictionary<string, Y_Room> {
                { "room_center", Factory_Rooms.Room_0("room_center") },
                { "room_right", Factory_Rooms.Room_1("room_right") },
                { "room_left", Factory_Rooms.Room_2("room_left") },
                { "room_bottom", Factory_Rooms.Room_1("room_bottom") },
                { "room_top", Factory_Rooms.Room_1("room_top") },
            };

            _player = new Y_Sprite(
                Content.Load<Texture2D>("tester"),
                new Rectangle(0, 0, 73, 102),
                400.0f,
                new Vector2(200, 450),
                _rooms["room_center"],
                200.0f,
                new Dictionary<string, int[]> {
                    { "stand", new int[] { 0, 1, 8, 9 } },
                    { "walk_left", new int[] { 2, 3, 4 } },
                    { "walk_right", new int[] { 5, 6, 7 } }},
                new Y_StarterGun()
            );

            Random random = new Random(3);
            _connectors = new List<Y_Connector> { 
                Factory_Connectors.HConnector("con_center_right").Connect(X_ConnectorSide.Left, _rooms["room_center"], X_ConnectorSide.Right, _rooms["room_right"], random),
                Factory_Connectors.HConnector("con_center_left").Connect(X_ConnectorSide.Right, _rooms["room_center"], X_ConnectorSide.Left, _rooms["room_left"], random),
                Factory_Connectors.VConnector("con_center_bottom").Connect(X_ConnectorSide.Top, _rooms["room_center"], X_ConnectorSide.Bottom, _rooms["room_bottom"], random),
                Factory_Connectors.VConnector("con_center_top").Connect(X_ConnectorSide.Bottom, _rooms["room_center"], X_ConnectorSide.Top, _rooms["room_top"], random)
            };

            _rooms["room_center"].SetBackgroundColor(Color.LightSalmon);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _camera.UpdateManual(new Vector2(100, 100), 500);
            _camera.Update(gameTime);
            _player.Update(gameTime);

            ProjectileManager.Update(gameTime);

            foreach(var con in _connectors)
            {
                con.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            Vector2 zero = Vector2.Zero;
            int gx = _camera.PosX;
            int gy = _camera.PosY;

            _spriteBatch.Begin(
                SpriteSortMode.Immediate, null, null, null, null, null,
                Matrix.CreateTranslation(gx, gy, 0));

            _spriteBatch.Draw(
                _background,
                new Rectangle(0, 0, 3840, 2160),
                new Rectangle(0, 0, 3840, 2160),
                Color.White
            );

            foreach(var room in _rooms.Values)
            {
                room.Draw(gameTime, zero, _spriteBatch);
                // uncomment for debugging
                //room.DrawOutline(gameTime, zero, _spriteBatch);
            }

            foreach (var con in _connectors)
            {
                con.Draw(gameTime, zero, _spriteBatch);
                // uncomment for debugging
                //con.DrawOutline(gameTime, zero, _spriteBatch);
            }

            ProjectileManager.Draw(gameTime, zero, _spriteBatch);

            _player.Draw(gameTime, zero, _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
