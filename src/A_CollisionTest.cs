//#define T_LARGE
//#define T_60
#define T_40
//#define T_30

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace YGR
{

    public struct Line
    {
        public Point origin;
        public Vector2 direction;
    }

    public class A_CollisionTest : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int RES_X = 1920;
        private const int RES_Y = 1000;

        private Y_CMRoom _room;
        private List<Y_CMSprite> _player;

        public A_CollisionTest()
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

            Factory_Rooms.Initialize(Content);
            Factory_Connectors.Initialize(Content);
            Factory_Debug.Initialize(Content);
            Manager_Projectile.Initialize(Content);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //TextFieldParser parser = Content.Load<TextFieldParser>("Level_0/Collisions.csv");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //string[] lines = File.ReadAllLines("./Level/Level_0/Collisions.csv");

            _room = new Y_CMRoom(
                "hello",
                new X_CollisionModel_Room("Rooms/Collisions.csv", 40, 40, new Point(0,0))
                );

            _player = new List<Y_CMSprite>();

            for(int i=1; i<=2; ++i)
            {
                _player.Add(
                new Y_CMSprite(
                    new X_CollisionModel_Victim(1.0f*i /* mass */, 1.0f /* elastic impact */),
                    null,
#if T_LARGE
                    Content.Load<Texture2D>("tester"),
                    new Rectangle(0, 0, 73, 102),
#elif T_60
                    Content.Load<Texture2D>("tester_60"),
                    new Rectangle(0, 0, 42, 60),
#elif T_40
                    Content.Load<Texture2D>("tester_40"),
                    new Rectangle(0, 0, 28, 40),
#elif T_30
                    Content.Load<Texture2D>("tester_30"),
                    new Rectangle(0, 0, 21, 30),
#endif
                    0.004f, // acceleration
                    0.5f / i,  // max velocity
                    new Vector2(200*i, 350),
                    _room,
                    200.0f,
                    new Dictionary<string, int[]> {
                                    { "stand", new int[] { 0, 1, 8, 9 } },
                                    { "walk_left", new int[] { 2, 3, 4 } },
                                    { "walk_right", new int[] { 5, 6, 7 } }},
                    new Y_StarterGun(),
                    i // control input
                ));
            }

            Mouse.SetPosition(175, 380);
        }

        protected override void Update(GameTime gameTime)
        {
            foreach(var player in _player)
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

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null,
                Matrix.CreateTranslation(0, 0, 0));

            _room.Draw(gameTime, Vector2.Zero, _spriteBatch);
            _room.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);

            foreach(var player in _player)
            {
                player.Draw(gameTime, Vector2.Zero, _spriteBatch);
                player.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);
            }

            Manager_Projectile.Draw(gameTime, Vector2.Zero, _spriteBatch);
            Manager_Projectile.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
