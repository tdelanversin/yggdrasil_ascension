using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Clearcove.Logging;
using System;

namespace YGR
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Logger _logger;

        private const int RES_X = 1920;
        private const int RES_Y = 1080;
        const int PLAYER_SX = 100;
        const int PLAYER_SY = 128;
        const float PLAYER_FRAME_DURATION = 200f;
        const float PLAYER_VELOCITY = 200f;
        Vector2 _playerPosition = new Vector2(200, 200);

        Texture2D _room;
        Texture2D _player;
        Texture2D _background;

        Camera _camera;

        public Game1(Logger logger)
        {
            _logger = logger;
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

            _camera = new Camera(_logger, new Vector2(RES_X / 2, RES_Y / 2), RES_X, RES_Y);
            _logger.Info("Set resolution to " + RES_X.ToString() + "x" + RES_Y.ToString());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _room = Content.Load<Texture2D>("level_0");
            _background = Content.Load<Texture2D>("background");
            _player = Content.Load<Texture2D>("tester");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Vector2 input = Vector2.Zero;

            //if (pad.IsConnected)
            //{
            //    input = GamePad.GetState(PlayerIndex.One).ThumbSticks.Left;
            //    input.Y *= -1;
            //}

            //var state = GamePad.GetState(PlayerIndex.One);
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Right))
            {
                input.X += 1;
            }
            if (keyboard.IsKeyDown(Keys.Left))
            {
                input.X -= 1;
            }
            if (keyboard.IsKeyDown(Keys.Down))
            {
                input.Y += 1;
            }
            if (keyboard.IsKeyDown(Keys.Up))
            {
                input.Y -= 1;
            }

            if (input.LengthSquared() > 1)
            {
                input.Normalize();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _playerPosition += input * deltaTime * PLAYER_VELOCITY;

            _camera.MoveByManual(new Vector2(960, 540), 500);

            // TODO: Add your update logic here

            _camera.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            int playerFrameIndex = 0;

            int gx = _camera.posX();
            int gy = _camera.posY();

            _spriteBatch.Begin();

            _spriteBatch.Draw(
                _background,
                new Rectangle(0, 0, RES_X, RES_Y),
                _camera.GetSourceRect(),
                Color.White
            );

            _spriteBatch.Draw(
                _player,
                new Rectangle((int)_playerPosition.X - gx, (int)_playerPosition.Y - gy, PLAYER_SX, PLAYER_SY),
                new Rectangle(playerFrameIndex * PLAYER_SX, 0, PLAYER_SX, PLAYER_SY),
                Color.White
            );

            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
