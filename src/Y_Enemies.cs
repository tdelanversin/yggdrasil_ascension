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
        public IProjectile HitBy { get; set; }
        protected float maxVelocity { get; set; }
        protected float safetyDistance { get; set; }

        public Vector2 Velocity { get; set; }
        protected Vector2 _acceleration;
        protected Vector2 _deceleration;
        protected Vector2 _maxVelocity;

        public Vector2 FacingDirection { get; set; }
        protected float SteeringDirection;

        public Texture2D Sprite { get; set; }
        public Rectangle SpriteRect = new Rectangle(0, 0, 42, 60);
        public X_CollisionModel_Victim Collision { get; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IShooter Gun { get; set; }
        protected IList<IVictim> Players;
        protected IVictim Target = null;
        public float Scale { get; set; }

        public string Name { get; set; }

        protected Vector2 _position;
        protected Color _hitColor;
        protected Color _regularColor;
        protected Color _color;
        protected int _hitFrames = 10;
        protected int _hitFramesCounter = 0;
        protected Rectangle _rect;

        public string Identifier;

        public Y_SimpleEnemy(
            Vector2 position,
            Y_Level level,
            IList<IVictim> players
        )
        {
            LifePoints = 3;
            HitInLastLoop = false;

            Velocity = Vector2.Zero;
            _acceleration = Vector2.One * 0.002f;
            _deceleration = Vector2.One * 0.02f;
            _maxVelocity = Vector2.One * 0.2f;

            safetyDistance = 300f;
            FacingDirection = new Vector2(1, 0);
            SteeringDirection = 0.0f;
            Sprite = Manager_Enemies.enemy_textures["default_enemy"];
            Collision = new X_CollisionModel_Victim(1.0f, 0.0f);
            _position = position;

            _rect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                SpriteRect.Width, SpriteRect.Height
            );

            Scale = Math.Min(
                _rect.Width / (float)SpriteRect.Width,
                _rect.Height / (float)SpriteRect.Height
            );

            Level = level;
            Room = Level.GetRoom(this, Room);
            Gun = new Y_SimpleEnemyGun();
            Players = players;

            Name = "base enemy";

            _hitColor = Color.Blue;
            _regularColor = Color.Red;
            _color = _regularColor;

            var rand = new Random();
            Identifier = DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Second.ToString() + "-" + DateTime.Now.Millisecond.ToString() + "-" + rand.NextSingle().ToString();
        }


        protected bool FindTargetAndVisibility()
        {
            List<Tuple<float, IVictim>> inRange = new List<Tuple<float, IVictim>>();
            foreach (IVictim player in Players)
            {
                if (player.WhatAreYou() == X_LevelElements.Ghost)
                {
                    continue;
                }

                if (!LineOfSight(player.Rect.Center.ToVector2()))
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

            return true;
        }

        protected bool LineOfSight(Vector2 target)
        {
            if (Room == null) return false;
            if (target == null) return false;

            Point origin = Rect.Center;
            Vector2 targetDirection = target - Rect.Center.ToVector2();
            targetDirection = Vector2.Normalize(targetDirection);
            float targetDistance = Vector2.Distance(target, Rect.Center.ToVector2());
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

        public virtual void UpdateVelocity(Vector2 input, GameTime gameTime)
        {
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;

            /* ##########################################################################
             * Speed and velocity handling based on control input
             *  => must happen before collision handling <=
             * ########################################################################## */
            if (input != Vector2.Zero)
            {
                Manager_Particles._particleEffects[0].Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));

                if (input.LengthSquared() > 1)
                {
                    input.Normalize();
                }
                Velocity += input * _acceleration * timeStepMS;
            }
            else
            {
                Velocity = new Vector2(
                    Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration.X * timeStepMS),
                    Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration.Y * timeStepMS));
            }

            Velocity = Vector2.Clamp(Velocity, -_maxVelocity, _maxVelocity);
        }

        public virtual void Update(GameTime gameTime)
        {
            Room = Level.GetRoom(this, Room);

            if (HitInLastLoop)
            {
                _color = _hitColor;
                _hitFramesCounter++;
                if (_hitFramesCounter > _hitFrames)
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
                else
                {
                    Room = Level.GetRoom(this, Room);
                    FacingDirection += new Vector2(Util.random.NextSingle() - 0.5f, Util.random.NextSingle() - 0.5f);
                    if (FacingDirection.LengthSquared() > 1)
                    {
                        FacingDirection = Vector2.Normalize(FacingDirection);
                    }
                }
                Velocity += FacingDirection * _acceleration * (float)timeStepMS;
                Velocity = Vector2.Clamp(Velocity, -_maxVelocity, _maxVelocity);

                _position += Velocity * timeStepMS;
                _rect.Location = _position.ToPoint();
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

                // Centered steering model wandering: Move a point on a circle around the entity, always face that point
                // Offset circle steering model wandering: Move a point on a circle in front of the entity, always face that point
                // Vector2 steeringCenter = _position + Vector2.Normalize(FacingDirection) / 2;
                // Vector2 steeringPoint = steeringCenter + new Vector2((float)Math.Cos(SteeringDirection), (float)Math.Sin(SteeringDirection)) / 2;
                // FacingDirection = steeringPoint - _position;
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
            //Rectangle rect = me.Rect;
            //rect.Location += (me.Velocity * timeStepMS).ToPoint();
            //me.Rect = rect;

            _position += Velocity * timeStepMS;
            _rect.Location = _position.ToPoint();


            if (canSee)
            {
                Point origin = _rect.Center;
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun.Shoot(gameTime, origin.ToVector2(), targetDirection, Level, this);
            }
        }

        protected virtual void DrawFaceDirectionIndicator(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            var angle = Math.Atan2(FacingDirection.Y, FacingDirection.X) + Math.PI / 2;
            spriteBatch.Draw(
                Manager_Players.SpriteAimIndicator[3], _rect.Center.ToVector2() + FacingDirection * _rect.Height,
                null,
                Color.White, (float)angle, new Vector2(Manager_Players.SpriteAimIndicator[3].Width / 2, 0), 0.03f, SpriteEffects.None, 0);

        }

        protected virtual void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // TODO: Improve
            string str = LifePoints.ToString();
            float str_width = Fonts.Normal.MeasureString(str).X;
            spriteBatch.DrawString(Fonts.Normal, str, new Vector2(_rect.Location.X + _rect.Width / 2 - str_width / 2, _rect.Location.Y - 16), Color.OrangeRed);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: Sprite,
                position: _rect.Location.ToVector2(),
                sourceRectangle: SpriteRect,
                color: _color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Scale,
                effects: SpriteEffects.None,
                layerDepth: 0);
            if (Settings.Outlines)
            {
                DrawFaceDirectionIndicator(gameTime, globalOffset, spriteBatch);
            }
            DrawOverheadString(gameTime, globalOffset, spriteBatch);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, Rect.Width, _rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public X_LevelElements WhatAreYou() => X_LevelElements.Enemy;
    }
}