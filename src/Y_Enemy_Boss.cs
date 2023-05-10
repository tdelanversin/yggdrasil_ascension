using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Boss : Enemy_Basic, IEnemyBoss
    {
        IShooter Gun_Scatter;
        IShooter Gun_Precise;
        IShooter Gun_AOE;
        IShooter Gun_AvoidPattern;
        BossAttack Attack {get; set; } = BossAttack.Wait;
        float[] GunWeightsPhase1 = { 0.5f, 0.25f, 0.25f, 0.0f }; // Weights for the gun selection

        public Enemy_Boss(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        ) : base(position, sprite, level)
        {
            LifePointsMax = 120;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = 0; // Gigachad never flees

            Name = "Slime Boss";
            Gun = new Gun_BasicEnemy();
            Gun_Scatter = new Gun_BossScatter();
            Gun_Precise = new Gun_BossPrecise();
            Gun_AOE = new Gun_BossAOE();
            Gun_AvoidPattern = new Gun_BossAvoidPattern();

            _hitColor = Color.OrangeRed;
            Color = new Color(255, 255, 98);
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
            Gun_Scatter.Update(gameTime);
            Gun_Precise.Update(gameTime);
            Gun_AOE.Update(gameTime);
            Gun_AvoidPattern.Update(gameTime);

            CharacterSprite.Update(gameTime, movement);

            switch (Attack)
            {
                case BossAttack.Scatter:
                    Gun = Gun_Scatter;
                    break;
                case BossAttack.Precise:
                    Gun = Gun_Precise;
                    break;
                case BossAttack.AOE:
                    var playersInSameRoom = Manager_Players.Players.FindAll(x => x.LifePoints > 0 && x.Room == Room);
                    if (playersInSameRoom.Count < 1) return;

                    // Gigachad shoot Big Gun no matter what (as long as there are players in the same room)
                    Gun_AOE.Shoot(gameTime, _rect.Center.ToVector2(), Vector2.One, Level, this);
                    break;
                case BossAttack.AvoidPattern:
                    Gun = Gun_AvoidPattern;
                    break;
                case BossAttack.Wait:
                    return;
                default:
                    return;
            }


            if (Target != null)
            {
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun.Shoot(gameTime, _rect.Center.ToVector2(), targetDirection, Level, this);
            }
        }

        protected override void UpdateVelocity(Vector2 input, GameTime gameTime)
        {
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;

            /* ##########################################################################
             * Speed and velocity handling based on control input
             *  => must happen before collision handling <=
             * ########################################################################## */
            if (input != Vector2.Zero)
            {
                //Manager_Particles.GenParticleEffectDustCloudLight(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));
                Manager_Particles._particleEffects[(int)Manager_Particles.Effect.GigaChad].Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));

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

            Velocity = Util.ClampMagnitude(Velocity, _maxVelocity);
        }
    }
}
