using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Gigachad : Enemy_Basic
    {
        IShooter Gun2; // Gigachad needs moar guns

        public Enemy_Gigachad(
            Vector2 position,
            Y_Level level,
            IList<IVictim> players
        ) : base(position, level, players)
        {
            LifePoints = 999;
            fleeingHPTreshold = 0; // Gigachad never flees
            Sprite = Manager_Enemies.enemy_textures["gigachad"];
            SpriteRect = Sprite.Bounds;

            Name = "Gigachad";
            Gun = new Gun_ShotGun(13);
            Gun2 = new Gun_Gigagun();

            _hitColor = Color.OrangeRed;
            _regularColor = Color.White;
            _color = _regularColor;

            _maxVelocity = 0.075f;

            var _mass = 8.0f; // Heavier than other entities
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            _rect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                180, 180
            );

            Scale = Math.Min(
                _rect.Width / (float)SpriteRect.Width,
                _rect.Height / (float)SpriteRect.Height
            );
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

            var playersInSameRoom = ((List<IVictim>)Manager_Players.Players).FindAll(x => x.Room == Room);
            if (playersInSameRoom.Count < 1) return;

            // Gigachad shoot Big Gun no matter what (as long as there are players in the same room)
            Gun2.Shoot(gameTime, _rect.Center.ToVector2(), Vector2.One, Level, this);
        }

        protected override void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: Sprite,
                position: _rect.Location.ToVector2(),
                sourceRectangle: SpriteRect,
                color: _color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Scale,
                effects: Velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                layerDepth: 0);
        }
    }
}
