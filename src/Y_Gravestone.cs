using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace YGR
{
    public class Gravestone : Enemy_Basic
    {
        public Texture2D Sprite { get; }

        public Gravestone(
            Vector2 position,
            Y_Level level
        ) : base(position, null, level)
        {
            Name = "Gravestone";
            LifePointsMax = 50;
            LifePoints = LifePointsMax;

            Sprite = Manager_Sprites.Gravestone;

            // Collision bounds
            int height = IPlayer.PlayerBaseHeight;
            int width = (int)(height * Sprite.Width / (float)Sprite.Height);

            // Offset the enitity to center it on the spawner tile
            _position = position - new Vector2(width / 2, height / 2);
            _rect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                width,
                height
            );
        }

        public override void Hit(IProjectile projectile)
        {
            lock (this)
            {
                // Return if alread dead, otherwise player kill stats are inaccurate
                if (LifePoints <= 0) { return; }

                LifePoints -= projectile.Damage;
                _hitFramesCounter = 1;
                _currentColor = Color.Lerp(_hitColor, Color, 0.1f);
            }
        }

        public override void Update(GameTime gameTime)
        {

            UpdateHitCounters(gameTime);
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Culling
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty)
            {
                return;
            }

            if (Room.WhatAreYou() == X_LevelElements.Room)
            {
                if (!((Y_CMRoom)Room).IsVisible())
                {
                    return;
                }

            }

            spriteBatch.Draw(Sprite, _rect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            // DrawHealthbar(gameTime, globalOffset, spriteBatch);
        }

        public override X_LevelElements WhatAreYou() => X_LevelElements.Gravestone;

        public override void DropSomethingJuicyMaybe() { }
    }
}