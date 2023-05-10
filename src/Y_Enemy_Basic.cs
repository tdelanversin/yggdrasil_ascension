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
        public Color Color { get; set; }
        public IGameElement WhoKilledMe { get; set; }
        public int ElementLevel { get { return 1; } set { } }

        public EnemyState State { get; set; } = EnemyState.Inactive;
        protected int fleeingHPTreshold; // Flee if at this treshold or lower
        protected float safetyDistance { get; set; }

        public Vector2 Velocity { get; set; }
        protected float maxVelocity { get; set; }
        protected float _acceleration;
        protected float _deceleration;
        protected float _maxVelocity;

        public Vector2 FacingDirection { get; set; }
        protected float _steeringDirection;

        public AnimatedSprite CharacterSprite { get; }
        protected Vector2 CharacterOffset;
        protected float CharacterScale;
        public X_CollisionModel_Victim Collision { get; set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IShooter Gun { get; set; }
        protected IVictim Target = null;
        public bool Confused { get; set; }

        protected Vector2 _position;
        protected Color _hitColor;
        protected Color _currentColor;
        protected int _hitFrames = 30; // How many frames after being hit until the color recovers to normal
        protected int _hitFramesCounter = 0;
        protected Rectangle _rect;

        protected int _stateTimer = 0; // How long have we been idling / wandering
        protected int _stateChangeTime = 0; // How long until we change state
        protected int _stateTimerMax = 5000; // How long do we idle / wander at most

        public string Identifier;

        protected float _dropProbabilityPercent = 5;
        protected float _dropProbabilityPercentLifeSaving = 30;

        public Enemy_Basic(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        )
        {
            Name = "Mob";
            LifePointsMax = 8;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = LifePointsMax / 2;

            CharacterSprite = sprite;

            Velocity = Vector2.Zero;
            _acceleration = 0.006f;
            _deceleration = 0.04f;
            _maxVelocity = 0.06f;

            safetyDistance = 300f;
            FacingDirection = new Vector2(1, 0);
            Collision = new X_CollisionModel_Victim(1.0f, 0.0f);
            _position = position;

            // Collision bounds
            int height = 60;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);
            _rect = new Rectangle(
                (int)position.X,
                (int)position.Y,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero; // Not needed right now
            Confused = false;

            Level = level;
            Room = Level.GetRoom(this, Room);
            Gun = new Gun_BasicEnemy(this);

            Color = Color.Orange;
            _currentColor = Color.DarkSlateGray * 0.4f; // Initially we're disabled
            _hitColor = Color.DarkRed;

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

            // Return if alread dead, otherwise player kill stats are inaccurate
            if (LifePoints <= 0) { return; }

            if(projectile.WhatAreYou() == X_LevelElements.ConfusionProjectile)
            {
                Manager_Confusion.AddConfusion(this, ((Projectile_Confusion)projectile).ConfusionDuration);
            }

            LifePoints -= projectile.Damage;
            _hitFramesCounter = 1;
            _currentColor = Color.Lerp(_hitColor, Color, 0.1f);

            // @statistics
            var p = (IPlayer)projectile.WhoFiredMe;
            p.Stats.DamageDealt += projectile.Damage;
            p.Stats.TimesHit++;
            if (this is IEnemyBoss) { p.Stats.BossDamageDealt += projectile.Damage; }
            if (LifePoints <= 0) { p.Stats.Kills++; }
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
                    // Transition from the hitcolor back to the normal one
                    _currentColor = Color.Lerp(_hitColor, Color, 0.1f + 0.9f * _hitFramesCounter / _hitFrames);
                }
            }
            else
            {
                _currentColor = Color;
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
                Manager_Particles.GenParticleEffectDustCloudLight(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));

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
                // Just stand still, I guess? -> Nope, let's not make it too easy for the players
                return Wander(gameTime);
            }

            return movement;
        }

        protected virtual Vector2 ForceChace(GameTime gameTime)
        {
            // Move towards target no matter what
            if (Target != null)
            {
                FacingDirection = Target.Rect.Center.ToVector2() - Rect.Center.ToVector2();
                return FacingDirection;
            }
            else
            {
                return Chase(gameTime);
            }
        }

        protected virtual Vector2 Flee(GameTime gameTime)
        {
            Vector2 movement = Vector2.Zero;

            // No need to flee if target is super far away
            if (Target != null && Vector2.Distance(Target.Rect.Center.ToVector2(), Rect.Center.ToVector2()) > safetyDistance * 2)
            {
                return Wander(gameTime);
            }

            // Face away from closest player
            FacingDirection = Rect.Center.ToVector2() - Target.Rect.Center.ToVector2();
            FacingDirection = Vector2.Normalize(FacingDirection);

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

            CharacterSprite.Update(gameTime, movement);

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
                Manager_Sprites.AimIndicator, _rect.Center.ToVector2() + CharacterOffset + FacingDirection * _rect.Height,
                null,
                Color.White, (float)angle, new Vector2(Manager_Sprites.AimIndicator.Width / 2, 0), 0.03f, SpriteEffects.None, 0);

        }

        protected virtual void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            string str = String.Format("{0} {1} {2}", Name, LifePoints, State);
            Vector2 str_size = Fonts.Small.MeasureString(str);
            Vector2 str_pos = new Vector2(_rect.Location.X + _rect.Width / 2 - str_size.X / 2, _rect.Location.Y - str_size.Y - 2) + CharacterOffset;
            spriteBatch.DrawString(Fonts.Small, str, str_pos, Color.Wheat);
        }

        protected virtual void DrawHealthbar(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Vector2 dim = Manager_Sprites.HealthbarEmpty.Bounds.Size.ToVector2();
            float scale = Math.Min(1.25f, Rect.Width / dim.X);
            dim *= scale;
            Vector2 offset = new Vector2((Rect.Width - dim.X) / 2, -dim.Y - 5);
            Vector2 pos = _rect.Location.ToVector2() + offset;
            spriteBatch.Draw(
                texture: Manager_Sprites.HealthbarEmpty,
                position: pos,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0);

            // Fill the healthbar
            if (LifePoints > 0)
            {
                float healthPerc = LifePoints / (float)LifePointsMax;
                Rectangle infill = Manager_Sprites.HealthbarInfill.Bounds;
                infill.Width = (int)(infill.Width * healthPerc);
                spriteBatch.Draw(
                    texture: Manager_Sprites.HealthbarInfill,
                    position: pos,
                    sourceRectangle: infill,
                    color: Color.OrangeRed,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }

        protected virtual void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: CharacterSprite.Texture,
                position: _rect.Location.ToVector2() + CharacterOffset,
                sourceRectangle: CharacterSprite.SourceRectangle,
                color: _currentColor,
                rotation: 0,
                origin: Vector2.Zero,
                scale: CharacterScale,
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
                if (!((Y_CMRoom)Room).IsVisible())
                {
                    return;
                }

            }

            DrawCharacterSprite(gameTime, globalOffset, spriteBatch);
            if (Settings.DebugOutlinesEntities)
            {
                DrawFaceDirectionIndicator(gameTime, globalOffset, spriteBatch);
            }

            if (State != EnemyState.Inactive)
            {
                DrawHealthbar(gameTime, globalOffset, spriteBatch);
                // DrawOverheadString(gameTime, globalOffset, spriteBatch);
            }
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

        virtual public void DropSomethingJuicyMaybe()
        {
            if (WhoKilledMe != null)
            {
                if (WhoKilledMe.WhatAreYou() == X_LevelElements.Victim)
                {
                    if (Room.WhatAreYou() == X_LevelElements.Room)
                    {
                        var vic = (IVictim)WhoKilledMe;
                        if (vic.LifePoints < 0.1f * (float)vic.LifePointsMax)
                        {
                            var next = Util.random.Next(0, 100);
                            if (next < _dropProbabilityPercentLifeSaving)
                            {
                                // drop a life saving goodie
                                var room = (Y_CMRoom)Room;
                                var p = new Point(Rect.Location.X + Rect.Width / 2, Rect.Location.Y + Rect.Height / 2);
                                room.PickUps.Add(PickUp.Factory(Y_PowerUps.Life, p, Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale));
                            }
                        }
                        else
                        {
                            // otherwise maybe drop something that may or may not be usefull
                            var next = Util.random.Next(0, 100);
                            if (next < _dropProbabilityPercent)
                            {
                                var droppables = new Y_PowerUps[] { Y_PowerUps.Life, Y_PowerUps.Revive };
                                var ind = Util.random.Next(0, droppables.Length);
                                var room = (Y_CMRoom)Room;
                                var p = new Point(Rect.Location.X + Rect.Width / 2, Rect.Location.Y + Rect.Height / 2);
                                room.PickUps.Add(PickUp.Factory(droppables[ind], p, Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale));
                            }
                        }
                    }
                }
            }
        }
    }
}