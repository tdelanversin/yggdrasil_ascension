using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#nullable enable

namespace YGR
{
    public class Projectile_Slime : Projectile_Basic
    {
        // Slime projectiles have an outer and inner component
        protected AnimatedSprite _spriteInner;
        protected AnimatedSprite _spriteOuter;

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
            _spriteOuter = spriteOuter;

            // Make Slime's projectiles have its color
            if (who is Enemy_Slime)
            {
                Color = ((Enemy_Slime)who).Color;
            }
        }

        // Override to update both AnimatedSprites
        public override void UpdateSprites(GameTime gameTime)
        {
            _spriteInner.Update(gameTime, AnimationState.Idle);
            _spriteOuter.Update(gameTime, AnimationState.Idle);
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw inner projectile texture in strict white
            spriteBatch.Draw(
                _spriteInner.Texture, _position + globalOffset,
                _spriteInner.SourceRectangle,
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

            // Draw outer projectile texture in the color of the slime
            spriteBatch.Draw(
                _spriteOuter.Texture, _position + globalOffset,
                _spriteOuter.SourceRectangle,
                Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
