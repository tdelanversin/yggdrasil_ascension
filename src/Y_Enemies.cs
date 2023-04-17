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
            safetyDistance = 500f;
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

            Name = "base enemy";

            Level.Victims.Add(this);
        }


        private void FindTarget() {
            IVictim closestPlayer = null;
            float closestDistance = float.MaxValue;
            foreach (IVictim player in Players)
            {
                float distance = Vector2.Distance(player.Rect.Center.ToVector2(), Rect.Center.ToVector2());
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            if (Target == null) {
                Target = closestPlayer;
                return;
            }

            float distanceToTarget = Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2());

            if (distanceToTarget > 2 * closestDistance) {
                Target = closestPlayer;
                return;
            }
        }

        public void Update(GameTime gameTime)
        {
            FindTarget();

            if (Target == null)
            {
                return;
            }

            // FacingDirection = Room.Graph.GetDirectionToTarget(this, Target);
            FacingDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();

            float safetyPoint = (FacingDirection.Length() - safetyDistance);
            if (safetyPoint < 0)
            {
                FacingDirection = Vector2.Zero;
            } else {
                FacingDirection = Vector2.Normalize(FacingDirection);
            }

            Logger.Info("FacingDirection: " + FacingDirection.ToString() + " safetyPoint: " + safetyPoint.ToString());

            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;

            Velocity = FacingDirection * maxVelocity * timeStepMS;

            Logger.Info("Velocity: " + Velocity.ToString());

            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            if (Collision.Intersect(this, timeStepMS, out contactPoint, out contactNormal, out who))
            {
                Logger.Info("Collided with something");
            }

            // Check line of sight to target and shoot if possible
            if (Room.Graph.LineOfSight(this, Target))
            {
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                targetDirection = Vector2.Normalize(targetDirection);
                Gun.Shoot(gameTime, Rect.Center.ToVector2(), targetDirection, Level, this);
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