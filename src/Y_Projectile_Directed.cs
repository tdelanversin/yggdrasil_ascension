using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
#nullable enable

namespace YGR
{
    public class Projectile_Directed : Projectile_Basic
    {
        protected float Angle;

        public Projectile_Directed(
            Vector2 position,
            Vector2 direction,
            AnimatedSprite sprite,
            Y_Level level,
            IGameElement who,
            float scale = 0.55f,
            float damage = 1,
            float maxAge = 2500,
            float speed = 0.55f,
            float mass = 0.5f,
            float fakeAcceleration = 1.0f
        ) : base(position, direction, sprite, level, who, scale, damage, maxAge, speed, mass, fakeAcceleration)
        {
            Angle = (float)(Math.Atan2(direction.Y, direction.X) + Math.PI / 2);
            Color = Color.LightPink;
            _direction = Vector2.Normalize(_direction); // just to be sure;

            _rect = new Rectangle(
                (int)(position.X + _direction.X * _size.X),
                (int)(position.Y + _direction.Y * _size.Y),
                (int)(_size.X * 2.0f / 3.0f * Y_Level.GlobalScale),
                (int)(_size.Y * 2.0f / 3.0f * Y_Level.GlobalScale));
        }

        /* Particle handling */
        public override void UpdateParticles(GameTime gameTime)
        {
            var particlePosition = _rect.Center.ToVector2() - _direction * _sprite.SpriteDimension.X * LocalScale / 2;
            Manager_Particles._particleEffects[(int)Manager_Particles.Effect.ProjectileTrails].Emitters.ForEach(emitter => { emitter.Parameters.Color = Color.ToHsl(); });//new MonoGame.Extended.Range<HslColor>(Color.ToHsl());
            Manager_Particles._particleEffects[(int)Manager_Particles.Effect.ProjectileTrails].Trigger(particlePosition);
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            var direction = _direction;
            if (WhoFiredMe is Player_Basic)
            { // Add a "little twist" to it ...get it?
                direction = ((Player_Basic)WhoFiredMe)._aimDirection;
            }
            Angle = (float)(Math.Atan2(direction.Y, direction.X) + Math.PI / 2);
            spriteBatch.Draw(
                _sprite.Texture, _rect.Center.ToVector2() + globalOffset,
                _sprite.SourceRectangle,
                Color, Angle, _sprite.SpriteDimension * 0.5f, LocalScale, SpriteEffects.None, 0);
        }
    }
}
