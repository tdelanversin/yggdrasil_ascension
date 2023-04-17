using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace YGR
{
    public class Y_SimpleEnemy : IEnemy
    {
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        private float maxVelocity { get; set; }
        private float safetyDistance { get; set; }

        public Vector2 Velocity { get; set; }
        public Vector2 FacingDirection { get; set; }
        public Texture2D Sprite { get; set; }
        public Rectangle SpriteRect = new Rectangle(0, 0, 42, 60);
        public X_CollisionModel_Victim Collision { get; }
        public Rectangle Rect { get; set; }

        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IShooter Gun { get; set; }
        private IList<IVictim> Players;
        private IVictim Target = null;
        public float Scale { get; }

        public string Name { get; set; }

        public Y_SimpleEnemy(
            Vector2 position,
            Y_Level level,
            IList<IVictim> players
        )
        {
            LifePoints = 1;
            HitInLastLoop = false;

            Velocity = new Vector2(0, 0);
            maxVelocity = 0.01f;
            safetyDistance = 300f;
            FacingDirection = new Vector2(0, 0);
            Sprite = Manager_Enemies.enemy_textures["default_enemy"];
            Collision = new X_CollisionModel_Victim(1.0f, 0.0f);
            Rect = new Rectangle(
                (int)position.X - SpriteRect.Width / 2,
                (int)position.Y - SpriteRect.Height / 2,
                SpriteRect.Width, SpriteRect.Height
            );

            Level = level;
            Room = Level.GetRoom(this, Room);
            Gun = new Y_SimpleEnemyGun();
            Players = players;
            Scale = 1.0f;

            Name = "base enemy";

            Level.Victims.Add(this);
        }


        private bool FindTargetAndVisibility()
        {
            IVictim closestPlayer = null;
            float closestDistance = float.MaxValue;
            foreach (IVictim player in Players)
            {
                if (player.WhatAreYou() == X_LevelElements.Ghost)
                {
                    continue;
                }
                float distance = Vector2.Distance(player.Rect.Center.ToVector2(), Rect.Center.ToVector2());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
            Target = closestPlayer;

            if (Target == null)
            {
                return LineOfSight();
            }

            float distanceToTarget = Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2());

            bool canSee = LineOfSight();
            if (distanceToTarget > 2 * closestDistance && !canSee)
            {
                Target = closestPlayer;
                return LineOfSight();
            }
            return canSee;
        }

        private bool LineOfSight()
        {
            Point origin = Rect.Center;
            Vector2 targetDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
            targetDirection = Vector2.Normalize(targetDirection);
            float targetDistance = Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2());
            foreach (Rectangle rects in Room.Collision.GetCollisionRectangles())
            {
                Rectangle rect = rects;
                Point contactPoint = Point.Zero;
                Vector2 contactNormal = Vector2.Zero;
                float uHit = 0.0f;
                if (Manager_Collision.RayVsRect(ref origin, ref targetDirection, ref rect, out contactPoint, out contactNormal, out uHit))
                {
                    if (Vector2.Distance(contactPoint.ToVector2(), origin.ToVector2()) < targetDistance)
                        return false;
                }
            }
            return true;
        }

        public void Update(GameTime gameTime)
        {
            Gun.Update(gameTime);
            bool canSee = FindTargetAndVisibility();

            if (!canSee || Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2()) > safetyDistance)
            {
                FacingDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                FacingDirection = Vector2.Normalize(FacingDirection);
                int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
                Velocity = FacingDirection * maxVelocity * (float)timeStepMS;

                IList<Vector2> contactNormals;
                IList<Point> contactPoints;
                IList<IGameElement> who;
                if (Collision.Intersect(this, timeStepMS, out contactPoints, out contactNormals, out who))
                {
                    Logger.Info("Collided with something");
                }
            }

            if (canSee)
            {
                Point origin = Rect.Center;
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun.Shoot(gameTime, origin.ToVector2(), targetDirection, Level, this);
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, Rect, SpriteRect, Color.Red);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public X_LevelElements WhatAreYou() => X_LevelElements.Enemy;
    }
}