using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

        private AnimatedSprite _sprite;
        private float _spriteScale;

        public static Y_PowerUp Factory(Y_PowerUps type, Point location, int width, int height, float scale)
        {
            switch (type)
            {
                case Y_PowerUps.Life:
                    return new Y_PowerUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_SpinningHeart(),
                        (player) => {
                            var alives = Manager_Players.Players.Where(x => x.WhatAreYou() == X_LevelElements.Victim).ToArray();
                            foreach (var alive in alives)
                            {
                                alive.LifePoints = Math.Min(alive.LifePointsMax, alive.LifePoints + 15);
                            }
                        });
                default: // case Y_PowerUps.Revive:
                    return new Y_PowerUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_SpinningPlus(),
                        (player) =>
                        {
                            var ghosts = Manager_Players.Players.Where(x => x.WhatAreYou() == X_LevelElements.Ghost).ToArray();
                            foreach (var ghost in ghosts)
                            {
                                ((SimplePlayer)ghost).Revive();
                            }
                        });
            }
        }

        public Y_PowerUp(Y_PowerUps type, Point location, int width, int height, float scale, AnimatedSprite sprite, Action<IVictim> action)
        {
            Scale = scale;
            float heightNew = (int)(height * 1.75);
            float widthNew = (int)((heightNew * sprite.SpriteDimension.X / sprite.SpriteDimension.Y));
            Rect = new Rectangle((int)(location.X * scale - (widthNew - width) / 2.0), (int)(location.Y * scale - (heightNew - height) - 10), (int)(scale * widthNew), (int)(scale * heightNew));
            Action = action;
            _sprite = sprite;
            _spriteScale = Scale * Util.GetSpriteScale(Rect, _sprite.SpriteDimension);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                    _sprite.Texture, Rect.Location.ToVector2(),
                    _sprite.SourceRectangle,
                    Color.White, 0, Vector2.Zero, _spriteScale, SpriteEffects.None, 0);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 4, Color.Purple, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime, AnimationState.Idle);
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
