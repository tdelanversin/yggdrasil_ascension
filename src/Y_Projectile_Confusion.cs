using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#nullable enable

namespace YGR
{
    public class Projectile_Confusion : Projectile_Basic
    {
        public int ConfusionDuration { get; set; }

        public Projectile_Confusion(
            Vector2 position,
            Vector2 direction,
            AnimatedSprite sprite,
            int confusionDuration,
            Y_Level level,
            IGameElement who,
            float scale,
            int damage,
            float maxAge,
            float speed,
            float mass
        ) : base(position, direction, sprite, level, who, scale, damage, maxAge, speed, mass)
        {
            ConfusionDuration = confusionDuration;
        }

        public override X_LevelElements WhatAreYou()
        {
            return X_LevelElements.ConfusionProjectile;
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _sprite.Texture, _position + globalOffset,
                _sprite.SourceRectangle,
                Manager_Confusion.GetRandomConfusionColor(), 0, Vector2.Zero, LocalScale, SpriteEffects.None, 0);
        }
    }
}
