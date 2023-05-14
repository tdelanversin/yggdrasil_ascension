using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public class Y_Confusion : IGameElement
    {
        public IVictim Target { get; set; }

        public Rectangle Rect { get; set; }

        public int ElementLevel { get; set; }

        AnimatedSprite _sprite;
        int _durationMS;

        public Y_Confusion(IVictim target, AnimatedSprite sprite, int durationMS)
        {
            Target = target;
            _sprite = sprite;
            setRect();
            _durationMS = durationMS;
        }

        private void setRect()
        {
            var width = (int)(Target.Rect.Width * (5.0f / 3.0f));
            var posx = Target.Rect.X + Target.Rect.Width/2 - width/2;
            var posy = Target.Rect.Y - Target.Rect.Height * (1.0f / 3.0f);
            Rect = new Rectangle((int)posx, (int)posy, width, width);
        }

        public void Update(GameTime gameTime)
        {
            setRect();
            _sprite.Update(gameTime, AnimationState.Idle);
            _durationMS -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public bool TimeUp() => _durationMS <= 0;

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.Bisque, spriteBatch);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _sprite.Texture, Rect.Location.ToVector2(),
                _sprite.SourceRectangle,
                Manager_Confusion.GetRandomConfusionColor(),
                0, Vector2.Zero, (float)Rect.Width / (float)_sprite.SourceRectangle.Width, SpriteEffects.None, 0); ;
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Confusion;
        }
    }
}
