using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public class Enemy_Gigachad : Enemy_Basic, IEnemyBoss
    {
        Dictionary<BossAttack, IShooter> Guns; // Gigachad needs moar guns
        public BossAttack Attack { get; set; } = BossAttack.Scatter;
        protected int _attackTimer = 0;
        public Color bossColor;

        public Enemy_Gigachad(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        ) : base(position, sprite, level)
        {
            LifePointsMax = 300;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = 0; // Gigachad never flees

            Name = "Gigachad";
            Guns = new Dictionary<BossAttack, IShooter>(){
                {BossAttack.Scatter, new Gun_GigachadScatter(this, 13)},
                {BossAttack.AOE, new Gun_GigachadAOE(this)},
                {BossAttack.AvoidPattern1, new Gun_GigachadHammer(this)},
                {BossAttack.Precise, new Gun_GigachadSin(this)}
            };

            Gun = Guns[Attack];

            _hitColor = Color.OrangeRed;
            Color = Color.White;
            _currentColor = Color;

            _maxVelocity = 0.075f;

            var _mass = 8.0f; // Heavier than other entities
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 250;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);

            // Offset the enitity to center it on the spawner tile
            _position = position - new Vector2(width / 2, height / 2);
            _rect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero;

            bossColor = new Color(95, 255, 56);
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
            foreach (var gun in Guns.Values)
            {
                gun.Update(gameTime);
            }

            // find target
            Vector2 targetDirection = new Vector2(0, -1);
            if (Target != null)
            {
                targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
            }
            
            switch (Attack)
            {
                case BossAttack.Scatter:
                    if (_attackTimer > 5000)
                    {
                        _attackTimer = -1000;
                        Attack = BossAttack.AOE;
                        Gun = Guns[Attack];
                    }
                    break;
                case BossAttack.AOE:
                    if (_attackTimer > 10000)
                    {
                        _attackTimer = -1000;
                        Attack = BossAttack.AvoidPattern1;
                        Gun = Guns[Attack];
                    }
                    break;
                case BossAttack.AvoidPattern1:
                    if (_attackTimer > 3000)
                    {
                        _attackTimer = -1000;
                        Attack = BossAttack.Precise;
                        Gun = Guns[Attack];
                    }
                    break;
                case BossAttack.Precise:
                    if (_attackTimer > 5000)
                    {
                        _attackTimer = -1000;
                        Attack = BossAttack.Scatter;
                        Gun = Guns[Attack];
                    }
                    break;
                default:
                    break;
            }

            if (_attackTimer > 0)
                Gun.Shoot(gameTime, Rect.Center.ToVector2(), targetDirection, Level, this);
                
            _attackTimer += gameTime.ElapsedGameTime.Milliseconds;
        }

        public void DrawBossHealthBar(GameTime gameTime, SpriteBatch spriteBatch) { }
    }
}
