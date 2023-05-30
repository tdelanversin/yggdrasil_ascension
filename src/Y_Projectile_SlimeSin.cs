using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#nullable enable

namespace YGR
{
    public class Projectile_SlimeSin : Projectile_Sin
    {
        // Slime projectiles have an outer and inner component
        protected AnimatedSprite _spriteInner;
        protected AnimatedSprite _spriteOuter;

        public Projectile_SlimeSin(
            Vector2 position,
            Vector2 direction,
            AnimatedSprite spriteOuter,
            AnimatedSprite spriteInner,
            Y_Level level,
            IGameElement who,
            float scale = 0.55f,
            float damage = 1,
            float maxAge = 2500,
            float speed = 0.55f,
            float mass = 0.5f,
            float fakeAcceleration = 0.0f,
            float sinAmplitude = 3f,
            float sinFrequency = 0.06f,
            float sinPhase = 0.0f
        ) : base(
            position: position,
            direction: direction,
            sprite: spriteOuter,
            level: level,
            who: who,
            scale: scale,
            damage: damage,
            maxAge: maxAge,
            speed: speed,
            mass: mass,
            fakeAcceleration: fakeAcceleration,
            sinAmplitude: sinAmplitude,
            sinFrequency: sinFrequency,
            sinPhase: sinPhase)
        {
            _spriteInner = spriteInner;
            _spriteOuter = spriteOuter;

            // Make Slime's projectiles have its color
            if (who is Enemy_Slime)
            {
                Color = ((Enemy_Slime)who).Color;
            }
            else if (who is Enemy_Slime_Spiky)
            {
                Color = ((Enemy_Slime_Spiky)who).Color;
            }
            else if (who is Enemy_Boss)
            {
                Color = ((Enemy_Boss)who).bossColor;
            }
            else if (who is Enemy_Gigachad)
            {
                Color = ((Enemy_Gigachad)who).bossColor;
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
                Color.White, 0, Vector2.Zero, LocalScale, SpriteEffects.None, 0);

            // Draw outer projectile texture in the color of the slime
            spriteBatch.Draw(
                _spriteOuter.Texture, _position + globalOffset,
                _spriteOuter.SourceRectangle,
                Color, 0, Vector2.Zero, LocalScale, SpriteEffects.None, 0);
        }
    }
}
