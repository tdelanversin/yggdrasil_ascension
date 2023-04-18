using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IShooter Gun { get; set; }
        private IList<IVictim> Players;
        private IVictim Target = null;
        public float Scale { get; }

        public string Name { get; set; }

        private Vector2 _position;
        private Color _hitColor;
        private Color _regularColor;
        private Color _color;
        int _hitFrames = 10;
        int _hitFramesCounter = 0;
        Rectangle _rect;

        public string Identifier;

        public Y_SimpleEnemy(
            Vector2 position,
            Y_Level level,
            IList<IVictim> players
        )
        {
            LifePoints = 3;
            HitInLastLoop = false;

            Velocity = new Vector2(0, 0);
            maxVelocity = 0.01f;
            safetyDistance = 300f;
            FacingDirection = new Vector2(0, 0);
            Sprite = Manager_Enemies.enemy_textures["default_enemy"];
            Collision = new X_CollisionModel_Victim(1.0f, 0.0f);
            _position = position;
            _rect = new Rectangle(
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

            _hitColor = Color.Blue;
            _regularColor = Color.Red;
            _color = _regularColor;

            var rand = new Random();
            Identifier = DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Second.ToString() + "-" + DateTime.Now.Millisecond.ToString() + "-" + rand.NextSingle().ToString();
        }


        private bool FindTargetAndVisibility()
        {
            List<Tuple<float, IVictim>> inRange = new List<Tuple<float, IVictim>>();
            foreach (IVictim player in Players)
            {
                if (player.WhatAreYou() == X_LevelElements.Ghost)
                {
                    continue;
                }
                float distance = Vector2.Distance(player.Rect.Center.ToVector2(), Rect.Center.ToVector2());

                inRange.Add(new Tuple<float, IVictim>(distance, player));
            }

            Target = null;
            if (inRange.Count == 0) return false;

            inRange = inRange.OrderBy(t => t.Item1).ToList();
            Target = inRange.First().Item2;

            return LineOfSight(Target);
        }

        private bool LineOfSight(IVictim target)
        {
            Point origin = Rect.Center;
            Vector2 targetDirection = target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
            targetDirection = Vector2.Normalize(targetDirection);
            float targetDistance = Vector2.Distance(target.Rect.Center.ToVector2(), Rect.Center.ToVector2());
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
            Room = Level.GetRoom(this, Room);

            if (HitInLastLoop)
            {
                _color = _hitColor;
                _hitFramesCounter++;
                if(_hitFramesCounter > _hitFrames)
                {
                    HitInLastLoop = false;
                    _hitFramesCounter = 0;
                }
            }
            else
            {
                _color = _regularColor;
            }

            Gun.Update(gameTime);
            bool canSee = FindTargetAndVisibility();
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
            if (!canSee || Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2()) > safetyDistance)
            {
                if(Target != null)
                {
                    FacingDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                    FacingDirection = Vector2.Normalize(FacingDirection);
                    Velocity = FacingDirection * maxVelocity * (float)timeStepMS;
                }
                else
                {
                    Room = Level.GetRoom(this, Room);
                    FacingDirection = Room.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                    FacingDirection = Vector2.Normalize(FacingDirection);
                    Velocity = FacingDirection * maxVelocity * (float)timeStepMS;
                }

                IList<Vector2> contactNormals;
                IList<Point> contactPoints;
                IList<IGameElement> who;
                Vector2 newVelocity;
                if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoints, out contactNormals, out who))
                {
                    Velocity = newVelocity;
                }
                //Rectangle rect = me.Rect;
                //rect.Location += (me.Velocity * timeStepMS).ToPoint();
                //me.Rect = rect;

                _position += Velocity * timeStepMS;
                _rect.Location = _position.ToPoint();
            }

            if (canSee)
            {
                Point origin = _rect.Center;
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun.Shoot(gameTime, origin.ToVector2(), targetDirection, Level, this);
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Sprite, _rect, SpriteRect, _color);
            spriteBatch.DrawString(Fonts.Normal, LifePoints.ToString(), new Vector2(_rect.Location.X + 30 / 2, _rect.Location.Y - 10), Color.Wheat);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, Rect.Width, _rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public X_LevelElements WhatAreYou() => X_LevelElements.Enemy;
    }
}