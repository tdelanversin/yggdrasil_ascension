using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public class Enemy_Gigachad : Enemy_Basic, IEnemyBoss
    {
        IShooter Gun2; // Gigachad needs moar guns
        public BossAttack Attack { get; set; } = BossAttack.Scatter;

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
            Gun = new Gun_ShotGun(this, 13);
            Gun2 = new Gun_BossAOEGigachad(this);

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

        public void DrawBossHealthBar(GameTime gameTime, SpriteBatch spriteBatch) { }
    }
}
