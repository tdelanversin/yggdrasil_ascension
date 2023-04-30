using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public enum Y_PowerUps
    {
        Revive,
        Life
    }

    public class Y_PowerUp : IGameElement
    {
        public float Scale { get; set; }

        public Rectangle Rect { get; set; }

        public Action<IVictim> Action { get; }

        private int _animIntervalMs;
        private int _animIntervalCounterMs;
        private int _animSpriteCount;
        private int _animCount;
        private int _animSpriteOffsetX;
        private int _animSpriteOffsetY;
        private int _tileSize;
        private Texture2D _texture;
        
        private static Dictionary<Y_PowerUps, Texture2D> _textures;
        private static ContentManager _content;

        public static void Initialize(ContentManager content)
        {
            _content = content;
            _textures = new Dictionary<Y_PowerUps, Texture2D>();
            _textures.Add(Y_PowerUps.Life, _content.Load<Texture2D>("SpritesOther/heart"));
            _textures.Add(Y_PowerUps.Revive, _content.Load<Texture2D>("SpritesOther/star"));
        }

        public static Y_PowerUp Factory(Y_PowerUps type, Point location, int width, int height, int tileSize, float scale, Dictionary<string, dynamic> properties)
        {
            if (_content == null) Logger.Error("Y_PowerUp not initialized");

            switch (type)
            {
                case Y_PowerUps.Revive:
                    return new Y_PowerUp(type, location, width, height, tileSize, scale, player => player.LifePoints = player.LifePoints + 15, 100, 5, 32, 32);
                default:
                    return new Y_PowerUp(
                        type, location, width, height, tileSize, scale, 
                        (player) => {
                            var ghosts = Manager_Players.Players.Where(x => x != player && x.WhatAreYou() == X_LevelElements.Ghost && x.Room == player.Room).ToArray();
                            if(ghosts.Length > 0)
                            {
                                Random rand = new Random();
                                var g = ghosts[rand.Next(0, ghosts.Length)];
                                ((SimplePlayer)g).Revive();
                            }
                        }, 100, 7, 32, 32);
            }
        }

        public Y_PowerUp(Y_PowerUps type, Point location, int width, int height, int tileSize, float scale, Action<IVictim> action, int animIntervalMs, int animSpriteCount, int animSpriteOffsetX, int animSpriteOffsetY)
        {
            Scale = scale;
            Rect = new Rectangle((int)(location.X*scale), (int)(location.Y*scale), (int)(scale * width), (int)(scale * height));
            Action = action;
            _animIntervalMs = animIntervalMs;
            _animSpriteCount = animSpriteCount;
            _animSpriteOffsetX = animSpriteOffsetX;
            _animSpriteOffsetY = animSpriteOffsetY;
            _animCount = 0;
            _animIntervalCounterMs = 0;
            _tileSize = tileSize;
            _texture = _textures[type];
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                    _texture, Rect.Location.ToVector2(),
                    new Rectangle(_animCount * _animSpriteOffsetX, 0, _animSpriteOffsetX, _animSpriteOffsetY),
                    //new Rectangle(0, 0, _animSpriteOffsetX, _animSpriteOffsetY),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 4, Color.Purple, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            _animIntervalCounterMs += gameTime.ElapsedGameTime.Milliseconds;
            if(_animIntervalCounterMs >= _animIntervalMs)
            {
                _animIntervalCounterMs = 0;
                _animCount++;
                if(_animCount >= _animSpriteCount)
                {
                    _animCount = 0;
                }
            }
        }

        public void MoveBy(Point offset)
        {
            Rect = new Rectangle(Rect.X + offset.X, Rect.Y + offset.Y, Rect.Width, Rect.Height);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.PowerUp;
        }
    }
}
