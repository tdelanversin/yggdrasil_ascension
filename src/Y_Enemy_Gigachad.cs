using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Gigachad : Enemy_Basic
    {
        IShooter Gun2; // Gigachad needs moar guns
        public float VelocityMax;

        public Enemy_Gigachad(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        ) : base(position, sprite, level)
        {
            LifePointsMax = 120;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = 0; // Gigachad never flees

            Name = "Gigachad";
            Gun = new Gun_ShotGun(13);
            Gun2 = new Gun_Gigagun();

            _hitColor = Color.OrangeRed;
            Color = Color.White;
            _currentColor = Color;

            _maxVelocity = 0.075f;

            var _mass = 8.0f; // Heavier than other entities
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 250;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);

            // Offset the boss to center it on the spawner tile that is only 32x32
            // TODO: standardize boss size and adjust that in the level editor as well 
            _rect = new Rectangle(
                (int)position.X - (height - 32) / 2,
                (int)position.Y - (width - 32) / 2 - 30,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero;
            VelocityMax = 0.35f;

        }

        public override void Update(GameTime gameTime)
        {
            if (State == EnemyState.Inactive) { return; }

            UpdateHitCounters(gameTime);
            FindTarget();
            UpdateState(gameTime);

            Vector2 movement = Vector2.Zero;
            switch (State)
            {
                case EnemyState.Chase:
                    movement = Chase(gameTime);
                    break;
                case EnemyState.Flee:
                    movement = Flee(gameTime);
                    break;
                case EnemyState.Wander:
                    movement = Wander(gameTime);
                    break;
                case EnemyState.Idle:
                    break;
                default:
                    break;
            }

            UpdateVelocity(movement, gameTime);
            UpdateCollision(gameTime);
            Gun.Update(gameTime);
            Gun2.Update(gameTime);

            // Gigachad sees players, Gigachad shoots player
            if (Target != null)
            {
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun.Shoot(gameTime, _rect.Center.ToVector2(), targetDirection, Level, this);
            }

            CharacterSprite.Update(gameTime, movement);

            var playersInSameRoom = Manager_Players.Players.FindAll(x => x.LifePoints > 0 && x.Room == Room);
            if (playersInSameRoom.Count < 1) return;

            // Gigachad shoot Big Gun no matter what (as long as there are players in the same room)
            Gun2.Shoot(gameTime, _rect.Center.ToVector2(), Vector2.One, Level, this);
        }
        protected virtual void UpdateVelocity(Vector2 input, GameTime gameTime)
        {
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;

            /* ##########################################################################
             * Speed and velocity handling based on control input
             *  => must happen before collision handling <=
             * ########################################################################## */
            if (input != Vector2.Zero)
            {
                //Manager_Particles.GenParticleEffectDustCloudLight(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));
                Manager_Particles._particleEffects[1].Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));

                if (input.LengthSquared() > 1)
                {
                    input.Normalize();
                }
                Velocity += input * _acceleration * timeStepMS;
            }
            else
            {
                Velocity = new Vector2(
                    Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration * timeStepMS),
                    Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration * timeStepMS));
            }

            Velocity = Util.ClampMagnitude(Velocity, VelocityMax);
        }
    }
}
