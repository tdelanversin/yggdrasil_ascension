using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#nullable enable

namespace YGR
{
    public class Projectile_Slime : Projectile_Basic
    {
        // Slime projectiles have an outer and inner component
        protected AnimatedSprite _spriteInner;

        public Projectile_Slime(
            Vector2 position,
            Vector2 direction,
            AnimatedSprite spriteOuter,
            AnimatedSprite spriteInner,
            Y_Level level,
            IGameElement who,
            float scale = 0.55f,
            int damage = 1,
            float maxAge = 2500,
            float speed = 0.55f,
            float mass = 0.5f
        ) : base(position, direction, spriteOuter, level, who, scale, damage, maxAge, speed, mass)
        {
            _spriteInner = spriteInner;

            // Make Slime's projectiles have its color
            if (who is Enemy_Slime) {
                Color = ((Enemy_Slime)who).Color;
            }
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw outer projectile texture in the color of the slime
            spriteBatch.Draw(
                _sprite.Texture, _position + globalOffset,
                _sprite.SourceRectangle,
                Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

            // Draw inner projectile texture in strict white
            // HACK: only _sprite is updated in base.Update(), but we can abuse that and use it's source Rect
            Rectangle sourceRectInner = _sprite.SourceRectangle;
            sourceRectInner.Location = sourceRectInner.Location + new Point(0, (int)_sprite.SpriteDimension.Y);
            spriteBatch.Draw(
                _spriteInner.Texture, _position + globalOffset,
                sourceRectInner,
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
