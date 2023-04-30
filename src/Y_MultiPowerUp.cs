using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace YGR
{
    public enum Y_MultiPowerUps
    {
        Radio
    }

    public class PowerUpItem
    {
        public bool Active { get; set; }
        public Y_PowerUp Item { get; }
        public MultiPowerUpItem MultiPowerUp { get; set; }
        public PowerUpItem(Y_PowerUp item)
        {
            Item = item;
            Active = true;
        }
    }

    public class MultiPowerUpItem
    {
        public bool Active { get; set; }
        public Y_MultiPowerUp Item { get; }
        public MultiPowerUpItem(Y_MultiPowerUp item)
        {
            Item = item;
            Active = true;
        }
    }

    public class Y_MultiPowerUp : IGameElement
    {
        public float Scale { get; set; }

        public Rectangle Rect { get; set; }

        public Action<PowerUpItem, List<PowerUpItem>> Action { get; }

        private int _colorTimeMs;
        private int _totalTimeMs;
        private int _animIntervalTimeMs;
        private int _animIntervalCounterMs;
        Color _finalColor;
        private int _tileSize;
        Texture2D _texture;

        private static ContentManager _content;
        private static Texture2D _whiteBackground;

        public static void Initialize(ContentManager content)
        {
            _content = content;
            _whiteBackground = _content.Load<Texture2D>("SpritesOther/white");
        }

        public static Y_MultiPowerUp Factory(Y_MultiPowerUps type, Point location, int width, int height, int tileSize, float scale, Dictionary<string, dynamic> properties)
        {
            if (_content == null) Logger.Error("Y_PowerUp not initialized");

            return new Y_MultiPowerUp(location, width, height, tileSize, scale,
                (hitPowerUp, allPowerUps) =>
                {
                    if(hitPowerUp.MultiPowerUp != null)
                    {
                        foreach(var pu in allPowerUps)
                        {
                            if(pu.MultiPowerUp == hitPowerUp.MultiPowerUp)
                            {
                                pu.Active = false;
                            }
                        }
                    }
                }, 25, 1000, 2000, Color.Purple);
        }

        public Y_MultiPowerUp(Point location, int width, int height, int tileSize, float scale, Action<PowerUpItem, List<PowerUpItem>> action, int animIntervalTimeMs, int colorTimeMs, int totalTimeMs, Color finalColor)
        {
            Scale = scale;
            Rect = new Rectangle((int)(location.X*scale), (int)(location.Y*scale), (int)(scale * width), (int)(scale * height));
            Action = action;
            _animIntervalTimeMs = animIntervalTimeMs;
            _colorTimeMs = colorTimeMs;
            _totalTimeMs = totalTimeMs;
            _animIntervalCounterMs = 0;
            _tileSize = tileSize;
            _finalColor = finalColor;
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            var c = _finalColor;
            if(_animIntervalCounterMs > _colorTimeMs)
            {
                c.A = 45;
            }
            else
            {
                c.A = (byte)(45.0f / (float)_colorTimeMs * (float)_animIntervalCounterMs);
            }
            spriteBatch.Draw(
                    _whiteBackground, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, Rect.Width, Rect.Height),
                    c, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 4, Color.Blue, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            _animIntervalCounterMs += gameTime.ElapsedGameTime.Milliseconds;
            if(_animIntervalCounterMs >= _totalTimeMs)
            {
                _animIntervalCounterMs = 0;
            }
        }

        public void MoveBy(Point offset)
        {
            Rect = new Rectangle(Rect.X + offset.X, Rect.Y + offset.Y, Rect.Width, Rect.Height);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.MultiPowerUp;
        }
    }
}
