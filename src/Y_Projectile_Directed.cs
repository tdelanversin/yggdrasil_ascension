using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
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

        public override void UpdateCollisionAndVelocity(GameTime gameTime)
        {
            int timeStepMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            Age += timeStepMS;

            /* Collision / Velocity handling */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            Vector2 newVelocity = Velocity;
            if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoint, out contactNormal, out who))
            {
                //Logger.Debug("Collided with something");
                foreach (var obj in who)
                {
                    // No friendly fire between entities of same kind
                    if (obj.WhatAreYou() == WhoFiredMe.WhatAreYou()) continue;

                    // Can't touch ghost
                    if (obj.WhatAreYou() == X_LevelElements.Ghost) continue;

                    // Pass through player if they are currently invincible
                    if (obj.WhatAreYou() == X_LevelElements.Invincible) continue;

                    else if (obj is IWalkable)
                    {
                        Velocity = newVelocity;
                    }
                    else
                    {
                        DeleteNext = false;
                    }

                    // Hit players and enemies
                    if (obj is IVictim)
                    {
                        if (Settings.ParticleEffects)
                        {
                            Manager_Particles.GetParticleEffect(Manager_Particles.Effect.Impact).Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));
                        }
                        ((IVictim)obj).Hit(this);
                    }
                }
            }
            _position += Velocity * timeStepMS;
            _rect.Location = _position.ToPoint();
        }

        /* Particle handling */
        public override void UpdateParticles(GameTime gameTime)
        {
            if (!Settings.ParticleEffects)
            {
                return;
            }
            var particlePosition = _rect.Center.ToVector2() - _direction * _sprite.SpriteDimension.X * LocalScale / 2;
            Manager_Particles.GetParticleEffect(Manager_Particles.Effect.ProjectileTrails).Emitters.ForEach(emitter => { emitter.Parameters.Color = Color.ToHsl(); });//new MonoGame.Extended.Range<HslColor>(Color.ToHsl());
            Manager_Particles.GetParticleEffect(Manager_Particles.Effect.ProjectileTrails).Trigger(particlePosition);
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
