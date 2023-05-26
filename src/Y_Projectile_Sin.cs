using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
#nullable enable

namespace YGR
{
    public class Projectile_Sin : Projectile_Basic
    {
        // Slime projectiles have an outer and inner component

        Vector2 _mainAxisPosition;
        Vector2 _mainAxisVelocity;
        float _sinAmplitude;
        float _sinFrequency;
        float _sinPhase;

        public Projectile_Sin(
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
            float fakeAcceleration = 1.0f,
            float sinAmplitude = 0.5f,
            float sinFrequency = 0.01f,
            float sinPhase = 0.0f
        ) : base(
            position: position,
            direction: direction,
            sprite: sprite,
            level: level,
            who: who,
            scale: scale,
            damage: damage,
            maxAge: maxAge,
            speed: speed,
            mass: mass,
            fakeAcceleration: fakeAcceleration)
        {
            _mainAxisPosition = position;
            _mainAxisVelocity = direction * speed;
            _sinAmplitude = sinAmplitude;
            _sinFrequency = sinFrequency;
            _sinPhase = sinPhase;

            Color = Color.White;
        }

        public override void UpdateCollisionAndVelocity(GameTime gameTime)
        {
            // Update age
            int timeStepMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            Age += timeStepMS;
            
            // Update main axis position
            _mainAxisPosition += _mainAxisVelocity * timeStepMS;

            // Update position
            var mainAxisNormal = new Vector2(-_direction.Y, _direction.X);
            var newPosition = _mainAxisPosition + mainAxisNormal * (float)Math.Sin(Age * _sinFrequency + _sinPhase * 2 * Math.PI) * _sinAmplitude;

            // Calculate new velocity
            Velocity = (newPosition - _position) / timeStepMS;

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

                    // Generate wall impact particles
                    if ((obj.WhatAreYou() == X_LevelElements.Room || obj.WhatAreYou() == X_LevelElements.Door) && WhoFiredMe is IPlayer)
                        Manager_Particles.MakeWallImpactParticle(contactPoint[0].ToVector2(), contactNormal[0]);

                    // Hit players and enemies
                    if (obj is IVictim)
                    {
                        lock (this)
                        {
                            ImpactParticles((IVictim)obj, contactNormal[0]);
                            ((IVictim)obj).Hit(this);
                        }
                    }
                }
                Velocity = newVelocity;
            }
            _position += Velocity * timeStepMS;
            _rect.Location = _position.ToPoint();
        }
    }
}
