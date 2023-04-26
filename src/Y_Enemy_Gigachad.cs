using Microsoft.Xna.Framework;
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
            Sprite = Manager_Enemies.enemy_textures["gigachad"];
            SpriteRect = Sprite.Bounds;

            _color = Color.White;

            Velocity = Vector2.Zero;
            _acceleration = Vector2.One * 0.0008f;
            _deceleration = Vector2.One * 0.02f;
            _maxVelocity = Vector2.One * 0.12f;

            _rect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                180, 180
            );

            Scale = Math.Min(
                _rect.Width / (float)SpriteRect.Width,
                _rect.Height / (float)SpriteRect.Height
            );

            Gun = new Y_GigaGun();
            Gun2 = new Y_FunkyGun();

            Name = "Gigachad";
        }

        public override void Update(GameTime gameTime)
        {
            Room = Level.GetRoom(this, Room);
            Gun.Update(gameTime);
            Gun2.Update(gameTime);

            if (HitInLastLoop)
            {
                LifePoints -= 1;
                HitInLastLoop = false;
            }

            Vector2 movement = Vector2.Zero;
            bool canSee = FindTargetAndVisibility();
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
            if (canSee && Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2()) > safetyDistance)
            {
                if (Target != null)
                {
                    FacingDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                    movement = FacingDirection;
                }
            }
            else // No target in line of sight, just wander
            {
                float steeringDiff = (Util.random.NextSingle() - 0.5f) / 4f;

                // Don't stupidly try walking into walls
                int counter = 0;
                do
                {
                    SteeringDirection += steeringDiff;
                    steeringDiff *= 2;
                    counter++;
                    FacingDirection = new Vector2((float)Math.Cos(SteeringDirection), (float)Math.Sin(SteeringDirection));
                } while (counter < 8 && !LineOfSight(Rect.Center.ToVector2() + FacingDirection * Rect.Height * 2));

                movement = FacingDirection;
            }


            UpdateVelocity(movement, gameTime);

            IList<Vector2> contactNormals;
            IList<Point> contactPoints;
            IList<IGameElement> who;
            Vector2 newVelocity;
            if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoints, out contactNormals, out who))
            {
                Velocity = newVelocity;
            }

            _position += Velocity * timeStepMS;
            _rect.Location = _position.ToPoint();


            // Gigachad sees players, Gigachad shoots player
            if (canSee)
            {
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun2.Shoot(gameTime, _rect.Center.ToVector2(), targetDirection, Level, this);
            }

            var playersInSameRoom = ((List<IVictim>)Manager_Players.Players).FindAll(x => x.Room == Room);
            if (playersInSameRoom.Count < 1) return;

            // Gigachad shoot Big Gun no matter what (as long as there are players in the same room)
            Gun.Shoot(gameTime, _rect.Center.ToVector2(), Vector2.One, Level, this);
        }
    }
}
