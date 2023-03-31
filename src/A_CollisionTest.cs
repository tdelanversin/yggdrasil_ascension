//#define T_LARGE
//#define T_60
//#define T_40
#define T_30

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Linq;
using Assimp.Configs;

namespace YGR
{
    //public struct Set
    //{
    //    public float Distance;
    //    public Vector2 ContactNormal;
    //    public float UHit;

    //    public Set(float dist, Vector2 contactNormal, float uHit)
    //    {
    //        Distance = dist; ContactNormal = contactNormal; UHit = uHit;
    //    }
    //}

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

        private Y_TestRoom _room;
        private Y_Sprite _player;

        //private Rectangle[] _rects;
        //Color[] _rectColors;
        //Point[] _contactPoints;
        //Vector2[] _contactNormals;

        //private Rectangle _myRect;
        //private Vector2 _velocity;
        //private IList<Tuple<Rectangle, Rectangle>> _collidedRects;
        //private float _uHit;

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

            _room = new Y_TestRoom("hello", "Room_0.zip", tileWidth:40, tileHeight:40);


            _player = new Y_Sprite(
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
                    0.004f,   // deceleration
                    0.75f,  // max acceleration
                    new Vector2(200, 350),
                    _room,
                    200.0f,
                    new Dictionary<string, int[]> {
                        { "stand", new int[] { 0, 1, 8, 9 } },
                        { "walk_left", new int[] { 2, 3, 4 } },
                        { "walk_right", new int[] { 5, 6, 7 } }},
                    new Y_StarterGun()
                );

            //_line = new Line();
            //_line.origin = new Point(150, 350);
            //_line.direction = new Vector2(1, 1);

            //_myRect = new Rectangle(120, 350, 50, 60);
            //_velocity = Vector2.Zero;

            //_rects = new Rectangle[] {
            //    new Rectangle(200, 150, 150, 250),
            //    new Rectangle(380, 400, 200, 100),
            //    new Rectangle(200, 700, 200, 200),
            //    new Rectangle(400, 700, 200, 200),
            //    new Rectangle(600, 700, 200, 200),
            //    new Rectangle(800, 700, 200, 200),
            //    new Rectangle(800, 500, 200, 200),
            //    new Rectangle(800, 300, 200, 200),
            //    new Rectangle(800, 100, 200, 200)
            // };

            //_rectColors = new Color[_rects.Length];
            //_contactPoints = new Point[_rects.Length];
            //_contactNormals = new Vector2[_rects.Length];
            //for(int i=0; i<_rectColors.Length; ++i)
            //{
            //    _rectColors[i] = Color.Red;
            //    _contactPoints[i] = Point.Zero;
            //    _contactNormals[i] = Vector2.Zero;
            //}

            Mouse.SetPosition(175, 380);
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            //_room.Update(gameTime);
            _player.Update(gameTime);

            //var mouse = Mouse.GetState();

            //_line.direction = (mouse.Position - _line.origin).ToVector2();

            //Vector2 input = Vector2.Zero;
            //KeyboardState keyboard = Keyboard.GetState();
            //if (keyboard.IsKeyDown(Keys.Right)) input.X += 1;
            //if (keyboard.IsKeyDown(Keys.Left)) input.X -= 1;
            //if (keyboard.IsKeyDown(Keys.Down)) input.Y += 1;
            //if (keyboard.IsKeyDown(Keys.Up)) input.Y -= 1;

            //if(input.Length() != 0) input.Normalize();

            //int timeStep = gameTime.ElapsedGameTime.Milliseconds;
            //_velocity += input * 0.005f * timeStep;



            //float uHit;
            //List<Set> collided = new List<Set>();
            //for (int i=0; i<_rects.Length; ++i)
            //{
            //    bool result = Manager_Collision.DynamicRectVsRect(
            //        ref _myRect, _velocity, timeStep, 
            //        ref _rects[i], out _contactPoints[i], out _contactNormals[i], out uHit);
            //    //ref _myRect, ref _velocity, timeStep, ref _rects[i]);

            //    if (result)
            //    {
            //        Vector2 om = new Vector2(_myRect.Width / 2, _myRect.Height / 2);
            //        Vector2 on = new Vector2(_rects[i].Width / 2, _rects[i].Height / 2);
            //        float dist = (_myRect.Location.ToVector2() + om - _rects[i].Location.ToVector2() + on).LengthSquared();
            //        collided.Add(new Set(dist, _contactNormals[i], uHit));
            //        _rectColors[i] = Color.Yellow;
            //    }
            //    else
            //    {
            //        _rectColors[i] = Color.Red;
            //        _contactPoints[i] = Point.Zero;
            //    }
            //}

            //collided.Sort((x,y) => Math.Sign(y.UHit - x.UHit));

            //foreach(var col in collided)
            //{
            //    //var m = collided.MinBy(x => x.UHit);
            //    Vector2 v = new Vector2(Math.Abs(_velocity.X), Math.Abs(_velocity.Y)) * (1 - col.UHit);
            //    _velocity += col.ContactNormal * v;
            //}

            //_myRect.Location += (_velocity * timeStep).ToPoint();

            //Logger.Info("######################### " + dv.ToString() + "          " + _myRect.Location.ToString() + "           " + _velocity.ToString() + "      " + (_velocity * timeStep).ToString());

            //if (_myRect.X > RES_X-_myRect.Width) _myRect.X = RES_X-_myRect.Width;
            //if (_myRect.X < 0) _myRect.X = 0;
            //if (_myRect.Y > RES_Y-_myRect.Height) _myRect.Y = RES_Y-_myRect.Height;
            //if (_myRect.Y < 0) _myRect.Y = 0;

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

            _player.Draw(gameTime, Vector2.Zero, _spriteBatch);
            _player.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);

            Manager_Projectile.Draw(gameTime, Vector2.Zero, _spriteBatch);
            Manager_Projectile.DrawOutline(gameTime, Vector2.Zero, _spriteBatch);

            //Factory_Debug.DrawLine(
            //    _line.origin.X, _line.origin.Y, (int)_line.direction.Length(),
            //    (float)Math.Atan2(_line.direction.Y, _line.direction.X),
            //    3, Color.Blue, _spriteBatch
            //);

            //for (int i = 0; i < _rects.Length; ++i)
            //{
            //    Factory_Debug.DrawRectangle(
            //        _rects[i].X, _rects[i].Y,
            //        _rects[i].Width, _rects[i].Height,
            //        3, _rectColors[i], _spriteBatch
            //    );

            //    Factory_Debug.DrawPoint(
            //        _contactPoints[i].X, _contactPoints[i].Y, 15, Color.Blue, _spriteBatch);
            //}

            //Factory_Debug.DrawRectangle(
            //        _myRect.X, _myRect.Y,
            //        _myRect.Width, _myRect.Height,
            //        3, Color.Blue, _spriteBatch
            //    );

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
