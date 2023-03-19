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

        //const int PLAYER_SX = 90;
        //const int PLAYER_SY = 120;
        //const float PLAYER_FRAME_DURATION = 200f;
        //const float PLAYER_VELOCITY = 200f;
        //Vector2 _playerPosition = new Vector2(200, 200);

        //Texture2D _player;
        Texture2D _background;
        Texture2D tex;

        Camera _camera;
        Room _room;
        Sprite _player;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            //_logger.Info("Initialize");
            //_logger.Warn("Warning");
            //_logger.Error("Error");
            //_logger.Debug("Debug message");
            //_logger.Info("Just printed a debug message...");

            _graphics.PreferredBackBufferWidth = RES_X;
            _graphics.PreferredBackBufferHeight = RES_Y;
            _graphics.ApplyChanges();

            _camera = new Camera(new Vector2(RES_X / 2, RES_Y / 2), RES_X, RES_Y);
            Logger.Info("Set resolution to " + RES_X.ToString() + "x" + RES_Y.ToString());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _background = Content.Load<Texture2D>("background");
            //_player = Content.Load<Texture2D>("tester");

            _room = new Room(
                Content.Load<Texture2D>("level_0"),
                new Polygon(
                    new float[][,] {
                        /*outer border*/
                        new float[,] {{ 40.0f, 51.0f }, { 40.0f , 1045.0f }, { 1884.0f , 1045.0f }, { 1884.0f, 51.0f } },
                        /*vertical rectangle on the left*/
                        new float[,] {{ 298.0f, 278.0f }, { 300.0f, 816.0f }, { 419.0f, 816.0f }, { 419, 278.0f } },
                        /*horizontal rectangle in the middle*/
                        new float[,] {{ 573.0f, 441.0f }, { 573.0f, 608.0f }, { 1482.0f, 611.0f }, { 1482, 441.0f } },
                        /*circular cylinder on the bottom right*/
                        new float[,] {{ 1653.0f, 739.0f }, { 1615.0f, 752.0f }, { 1600.0f, 796.0f }, { 1618, 860.0f }, { 1655, 868.0f }, { 1697.0f, 858.0f }, { 1712.0f, 796.0f }, { 1685.0f, 750.0f } }
                })
            );

            _player = new Sprite(
                Content.Load<Texture2D>("tester"),
                new Rectangle(0, 0, 73, 102),
                400.0f,
                new Vector2(100, 400),
                _room,
                200.0f,
                new Dictionary<string, int[]> {
                    { "stand", new int[] { 0, 1, 8, 9 } },
                    { "walk_left", new int[] { 2, 3, 4 } },
                    { "walk_right", new int[] { 5, 6, 7 } }
                }
            );
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            //_playerPosition += input * deltaTime * PLAYER_VELOCITY;

            _camera.UpdateManual(new Vector2(100, 100), 500);
            _camera.Update(gameTime);
            _player.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            int gx = _camera.posX();
            int gy = _camera.posY();

            _spriteBatch.Begin();

            _spriteBatch.Draw(
                _background,
                new Rectangle(0, 0, RES_X, RES_Y),
                _camera.GetSourceRect(),
                Color.White
            );

            //tex = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            //tex.SetData(new[] { Color.White });
            //var distance = 1000;
            //var angle = (float)(Math.PI / 2.0f);
            //var scale = new Vector2(distance, 50.0f);
            //_spriteBatch.Draw(
            //    tex, new Rectangle(500, 500, 200, 5), null, Color.White, angle, new Vector2(0, 0), SpriteEffects.None, 0);

            _room.Draw(gameTime, gx, gy, _spriteBatch);

            _player.Draw(gameTime, gx, gy, _spriteBatch);

            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
