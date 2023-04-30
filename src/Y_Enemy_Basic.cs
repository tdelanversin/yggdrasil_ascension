using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public class Enemy_Basic : IEnemy
    {
        public string Name { get; set; }
        public int LifePoints { get; set; }
        public int LifePointsMax { get; set; }

        public EnemyState State { get; set; } = EnemyState.Inactive;
        protected int fleeingHPTreshold = 2; // Flee if at this treshold or lower
        protected float safetyDistance { get; set; }

        public Vector2 Velocity { get; set; }
        protected float maxVelocity { get; set; }
        protected float _acceleration;
        protected float _deceleration;
        protected float _maxVelocity;

        public Vector2 FacingDirection { get; set; }
        protected float _steeringDirection;

        public Texture2D Sprite { get; set; }
        public Rectangle SpriteRect = new Rectangle(0, 0, 42, 60);
        public X_CollisionModel_Victim Collision { get; set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IShooter Gun { get; set; }
        protected IVictim Target = null;
        public float Scale { get; set; }
        protected Vector2 _position;
        protected Color _hitColor;
        protected Color _regularColor;
        protected Color _color;
        protected int _hitFrames = 10; // = 8 How many frame do we show the hit color
        protected int _hitFramesCounter = 0;
        protected Rectangle _rect;

        protected Rectangle _spriteDimensions;
        protected Dictionary<string, int[]> _animations;
        protected string _animationDirection;
        protected int _animationIndex;
        protected float _animationTimer;
        protected float _animationTreshold;

        protected int _stateTimer = 0; // How long have we been idling / wandering
        protected int _stateChangeTime = 0; // How long until we change state
        protected int _stateTimerMax = 5000; // How long do we idle / wander at most

        public string Identifier;

        public Enemy_Basic(
            Vector2 position,
            Y_Level level,
            IList<IVictim> players
        )
        {
            Name = "Mob";
            LifePointsMax = 8;
            LifePoints = LifePointsMax;

            Velocity = Vector2.Zero;
            _acceleration = 0.006f;
            _deceleration = 0.04f;
            _maxVelocity = 0.15f;

            safetyDistance = 150f;
            FacingDirection = new Vector2(1, 0);
            Sprite = Manager_Sprites.Enemy_Basic;
            Collision = new X_CollisionModel_Victim(1.0f, 0.0f);
            _position = position;

            _spriteDimensions = new Rectangle(0, 0, 44, 62);
            _animationTimer = 0;
            _animationTreshold = 250; // After how many ms to cycle through sprites
            _animationIndex = 0;
            _animations = new Dictionary<string, int[]> {
                { "idle", new int[] { 0 } },
                { "left", new int[] { 1, 2 } },
                { "right", new int[] { 3, 4 } }};
            _animationDirection = "idle";

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
            Gun = new Gun_BasicEnemy();

            _hitColor = Color.OrangeRed;
            _regularColor = Color.Orange;
            _color = Color.DarkSlateGray; // Initially we're disabled

            var rand = new Random();
            Identifier = DateTime.Now.Hour.ToString() + "-" + DateTime.Now.Second.ToString() + "-" + DateTime.Now.Millisecond.ToString() + "-" + rand.NextSingle().ToString();
        }

        // Sets Target to closest visible (and alive) player
        protected void FindTarget()
        {
            Target = null;
            float prevDistance = 0f;
            foreach (IVictim player in Manager_Players.Players)
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

                if (Target == null || distance < prevDistance)
                {
                    Target = player;
                    prevDistance = distance;
                }
            }
        }

        protected bool LineOfSight(Vector2 target)
        {
            if (Room == null) { return false; }
            if (!Room.Rect.Contains(target)) { return false; }

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

        /* Deal with being hit by projectile, basically physical therapy */
        public void Hit(IProjectile projectile)
        {
            if (State == EnemyState.Inactive) { return; }

            LifePoints -= projectile.Damage;
            _hitFramesCounter = 1;
            _color = _hitColor;
        }

        protected void UpdateHitCounters(GameTime gameTime)
        {
            if (_hitFramesCounter > 0)
            {
                if (_hitFramesCounter > _hitFrames)
                {
                    _hitFramesCounter = 0;
                }
                else
                {
                    _hitFramesCounter++;
                }
            }
            else
            {
                _color = _regularColor;
            }
        }

        protected void UpdateAnimation(GameTime gameTime)
        {
            string newAnimationDirection = _animationDirection;
            if (Velocity.X > 0 && _animations.ContainsKey("right"))
            {
                newAnimationDirection = "right";
            }
            else if (Velocity.X < 0 && _animations.ContainsKey("left"))
            {
                newAnimationDirection = "left";
            }
            else if (Velocity.Y > 0 && _animations.ContainsKey("up"))
            {
                newAnimationDirection = "up";
            }
            else if (Velocity.Y < 0 && _animations.ContainsKey("down"))
            {
                newAnimationDirection = "down";
            }
            else if (_animations.ContainsKey("idle"))
            {
                newAnimationDirection = "idle";
            }

            if (newAnimationDirection == _animationDirection)
            {
                _animationTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (_animationTimer > _animationTreshold)
                {
                    _animationTimer = 0;
                    _animationIndex = (_animationIndex + 1) % _animations[_animationDirection].Length;
                }
            }
            else
            {
                _animationTimer = 0;
                _animationIndex = 0;
                _animationDirection = newAnimationDirection;
            }
        }

        protected virtual void UpdateVelocity(Vector2 input, GameTime gameTime)
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
                    Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration * timeStepMS),
                    Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration * timeStepMS));
            }

            Velocity = Util.ClampMagnitude(Velocity, _maxVelocity);
        }

        protected virtual void UpdateCollision(GameTime gameTime)
        {
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
            /* ##########################################################################
             * Collision with everything handling (takes care of location update as well)
             * ########################################################################## */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            Vector2 newVelocity = Velocity;
            if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoint, out contactNormal, out who))
            {
                //Logger.Info("Collided with something");

                if (float.IsNaN(newVelocity.X))
                {
                    Logger.Error("newVelocity contains NaNs");
                }
                else
                {
                    Velocity = newVelocity;
                }
            }

            _position += newVelocity * timeStepMS;
            _rect.Location = _position.ToPoint();
        }

        protected virtual Vector2 Wander(GameTime gameTime)
        {
            float steeringDiff = (Util.random.NextSingle() - 0.5f) / 4f;

            // Don't stupidly try walking into walls
            int counter = 0;
            do
            {
                _steeringDirection += steeringDiff;
                steeringDiff *= 2;
                counter++;
                FacingDirection = new Vector2((float)Math.Cos(_steeringDirection), (float)Math.Sin(_steeringDirection));
            } while (counter < 8 && !LineOfSight(Rect.Center.ToVector2() + FacingDirection * Rect.Height * 2));

            /*
            // Alternative Circle steering model wandering: Move a point on a circle in front of the entity, then face that point
            Vector2 steeringCenter = _position + Vector2.Normalize(FacingDirection) / 2;
            Vector2 steeringPoint = steeringCenter + new Vector2((float)Math.Cos(SteeringDirection), (float)Math.Sin(SteeringDirection)) / 2;
            FacingDirection = steeringPoint - _position;
            */

            return FacingDirection;
        }

        protected virtual Vector2 Chase(GameTime gameTime)
        {
            Vector2 movement = Vector2.Zero;

            // Move towards target if it's further than safety distance away
            if (Target != null && Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2()) > safetyDistance)
            {
                FacingDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                movement = FacingDirection;
            }
            else
            {
                // Just stand still, I guess?
            }

            return movement;
        }

        protected virtual Vector2 Flee(GameTime gameTime)
        {
            Vector2 movement = Vector2.Zero;

            // Face away from closest player
            FacingDirection = Rect.Center.ToVector2() - Target.Rect.Center.ToVector2();

            // If we're running into an obstacle, try and face away from it, like we do when wandering
            // _steeringDirection = (float)Math.Tan(FacingDirection.X / FacingDirection.Y);
            float steeringDiff = (Util.random.NextSingle() - 0.5f) / 4f;

            // Don't stupidly try walking into walls
            int counter = 0;
            while (counter < 8 && !LineOfSight(Rect.Center.ToVector2() + FacingDirection * Rect.Height * 2))
            {
                _steeringDirection += steeringDiff;
                steeringDiff *= 2;
                counter++;
                FacingDirection = new Vector2((float)Math.Cos(_steeringDirection), (float)Math.Sin(_steeringDirection));
            }
            movement = FacingDirection;

            return movement;
        }

        public virtual void UpdateState(GameTime gameTime)
        {
            if (Target != null)
            {
                if (LifePoints > fleeingHPTreshold)
                {
                    State = EnemyState.Chase;
                }
                else
                {
                    State = EnemyState.Flee;
                }
            }
            else if (State == EnemyState.Wander)
            { /* Flip states with higher likelyhood as time passes on */
                _stateTimer -= gameTime.ElapsedGameTime.Milliseconds;
                if (-_stateTimer > _stateChangeTime)
                {
                    State = EnemyState.Idle;
                    _stateTimer = 0;
                    _stateChangeTime = Util.random.Next(_stateTimerMax);
                }
            }
            else if (State == EnemyState.Idle)
            {
                _stateTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (_stateTimer > _stateChangeTime)
                {
                    State = EnemyState.Wander;
                    _stateTimer = 0;
                    _stateChangeTime = Util.random.Next(_stateTimerMax);
                }
            }
            else
            { // Target left the room
                State = EnemyState.Wander;
            }
        }


        public virtual void Update(GameTime gameTime)
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
            UpdateAnimation(gameTime);

            Gun.Update(gameTime);
            if (Target != null)
            {
                Vector2 targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
                Gun.Shoot(gameTime, _rect.Center.ToVector2(), targetDirection, Level, this);
            }
        }

        protected virtual void DrawFaceDirectionIndicator(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            var angle = Math.Atan2(FacingDirection.Y, FacingDirection.X) + Math.PI / 2;
            spriteBatch.Draw(
                Manager_Sprites.AimIndicator[3], _rect.Center.ToVector2() + FacingDirection * _rect.Height,
                null,
                Color.White, (float)angle, new Vector2(Manager_Sprites.AimIndicator[3].Width / 2, 0), 0.03f, SpriteEffects.None, 0);

        }

        protected virtual void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            string str = String.Format("{0} {1}", Name, LifePoints);
            Vector2 str_size = Fonts.Normal.MeasureString(str);
            Vector2 str_pos = new Vector2(_rect.Location.X + _rect.Width / 2 - str_size.X / 2, _rect.Location.Y - str_size.Y - 2);
            spriteBatch.DrawString(Fonts.Normal, str, str_pos, Color.OrangeRed);
        }

        protected virtual void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: Sprite,
                position: _rect.Location.ToVector2(),
                sourceRectangle: new Rectangle(_animations[_animationDirection][_animationIndex] * (_spriteDimensions.Width), 0, _spriteDimensions.Width, _spriteDimensions.Height),
                color: _color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Scale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Culling
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty)
            {
                return;
            }

            if (Room.WhatAreYou() == X_LevelElements.Room)
            {
                if (((Y_CMRoom)Room).State == X_RoomState.Closed)
                {
                    return;
                }

            }
            else if (Room.WhatAreYou() == X_LevelElements.Door)
            {
                if (((Y_Door)Room).State == X_DoorState.Closed)
                {
                    return;
                }

            }

            DrawCharacterSprite(gameTime, globalOffset, spriteBatch);
            if (Settings.Outlines)
            {
                DrawFaceDirectionIndicator(gameTime, globalOffset, spriteBatch);
            }
            DrawOverheadString(gameTime, globalOffset, spriteBatch);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Culling
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty)
            {
                return;
            }

            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, Rect.Width, _rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public X_LevelElements WhatAreYou() => X_LevelElements.Enemy;
    }
}