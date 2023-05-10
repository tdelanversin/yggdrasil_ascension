using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Boss : Enemy_Basic, IEnemyBoss
    {        
        public BossAttack Attack {get; set; } = BossAttack.Wait;
        GameTime _attackTimer;
        Dictionary<BossAttack, IShooter> _attacks = new Dictionary<BossAttack, IShooter>();
        Dictionary<BossAttack, int> _attackLength = new Dictionary<BossAttack, int>();

        int Phase = 1;
        Dictionary<BossAttack, float> _attackWeightsPhase1 = new Dictionary<BossAttack, float>();
        Dictionary<BossAttack, float> _attackWeightsPhase2 = new Dictionary<BossAttack, float>();


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
            Gun = new Gun_BasicEnemy(this);

            _attacks = new Dictionary<BossAttack, IShooter>() {
                { BossAttack.Scatter, new Gun_BossScatter(this) },
                { BossAttack.Precise, new Gun_BossPrecise(this) },
                { BossAttack.AOE, new Gun_BossAOE(this) },
                { BossAttack.AvoidPattern, new Gun_BossAvoidPattern(this) },
                { BossAttack.Wait, new Gun_BasicEnemy(this) },
            };
            _attackLength = new Dictionary<BossAttack, int>() {
                { BossAttack.Scatter, 5000 },
                { BossAttack.Precise, 5000 },
                { BossAttack.AOE, 10000 },
                { BossAttack.AvoidPattern, 10000 },
                { BossAttack.Wait, 3000 },
            };

            _attackWeightsPhase1 = new Dictionary<BossAttack, float>() {
                { BossAttack.Scatter, 0.5f },
                { BossAttack.Precise, 0.25f },
                { BossAttack.AOE, 0.25f },
                { BossAttack.AvoidPattern, 0.0f },
                { BossAttack.Wait, 0.0f },
            };
            _attackWeightsPhase2 = new Dictionary<BossAttack, float>() {
                { BossAttack.Scatter, 0.4f },
                { BossAttack.Precise, 0.25f },
                { BossAttack.AOE, 0.25f },
                { BossAttack.AvoidPattern, 0.1f },
                { BossAttack.Wait, 0.0f },
            };


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
            switch (Attack) {
                case BossAttack.Scatter:
                    movement = Wander(gameTime);
                    break;
                case BossAttack.Precise:
                    movement = Chase(gameTime);
                    break;
                case BossAttack.AOE:
                    break;
                case BossAttack.AvoidPattern:
                    break;
                case BossAttack.Wait:
                    movement = ForceChace(gameTime);
                    break;
                default:
                    movement = Vector2.Zero;
                    break;
            }

            UpdateVelocity(movement, gameTime);
            UpdateCollision(gameTime);

            foreach (var attack in _attacks) {
                attack.Value.Update(gameTime);
            }

            CharacterSprite.Update(gameTime, movement);

            if (Attack == BossAttack.Wait) {
                return;
            }

            Vector2 targetDirection = Vector2.One;
            if (Target != null) {
                targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
            }

            _attacks[Attack].Shoot(gameTime, _rect.Center.ToVector2(), targetDirection, Level, this);
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
