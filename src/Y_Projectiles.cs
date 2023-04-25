using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#nullable enable

namespace YGR
{
    public class Y_StarterProjectile : IProjectile
    {
        public double TimeCreated { get; set; }
        public string Name { get; set; }
        public bool DeleteNext { get; set; }
        public X_CollisionModel_Projectile Collision { get; set; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IGameElement WhoFiredMe { get; set; } 

        public float Scale { get; protected set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        protected Vector2 _position;
        protected Texture2D _sprite;
        public Rectangle _window;
        protected int _animationIndex;
        protected Vector2 _direction;
        protected bool _isEnemy;
        protected X_ConnectorSide _lastSide;
        protected Rectangle _rect;

        public Y_StarterProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            Y_Level level,
            IGameElement who
        )
        {
            _sprite = Manager_Projectile.projectile_textures["smaller_projectile"];
            _window = new Rectangle(0, 0, 32, 32);
            _animationIndex = 0;
            _position = position;
            _direction = direction;
            Velocity = Vector2.One * direction;
            TimeCreated = timeCreated;
            _isEnemy = false;
            Name = "StarterProjectile";
            Level = level;
            DeleteNext = false;
            Collision = new X_CollisionModel_Projectile(0.5f, 1.0f);

            WhoFiredMe = who;
            if (WhoFiredMe is IVictim)
            { // Add 50% of the players momentum to the bullet. Adds 50% more fun to the game.
                Velocity += ((IVictim)WhoFiredMe).Velocity * .5f;
                /* 
                TODO: Clamp the velocity and make sure the bullets don't go
                backwards if the player is going backwards super fast (looking
                at you, Ninja...)
                */
            }
            Room = Level.GetRoom(this, Room);
            Scale = 0.25f * Room.Scale;
            _rect = new Rectangle(
                (int)position.X - (int)((float)_window.Width / 2.0f * Scale),
                (int)position.Y - (int)((float)_window.Height / 2.0f * Scale),
                (int)(_window.Width * Scale),
                (int)(_window.Height * Scale));
        }

        public virtual void Update(GameTime gameTime)
        {
            int timeStepMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            /* ##########################################################################
             * Collision with everything handling (takes care of location update as well)
             * ########################################################################## */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            Vector2 newVelocity = Velocity;
            if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoint, out contactNormal, out who))
            {
                //Logger.Debug("Collided with something");
                foreach (var obj in who)
                {
                    // No friendly fire between entities of same kind
                    if (obj.WhatAreYou() == WhoFiredMe.WhatAreYou()) continue;

                    // Can't touch ghost
                    if (obj.WhatAreYou() == X_LevelElements.Ghost) continue;

                    // Pass through player if they are currently invincible
                    if (obj.WhatAreYou() == X_LevelElements.Invincible) continue;

                    // If victim, tell it what it was hit by
                    if (obj is IVictim)
                    {
                        IVictim victim = (IVictim)obj;
                        if (!victim.HitInLastLoop)
                        {
                            victim.HitInLastLoop = true;
                            victim.HitBy = this;
                        }
                    }

                    Velocity = newVelocity;
                }
            }
            /* ########################################################################## */

            _position += newVelocity * timeStepMS;
            _rect.Location = _position.ToPoint();
            _animationIndex = (int)(5 - (gameTime.TotalGameTime.TotalMilliseconds - TimeCreated) / 300);
        }

        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            var destinationRectangle = new Rectangle(
                _rect.X - (int)globalOffset.X,
                _rect.Y - (int)globalOffset.Y,
                _window.Width,
                _window.Height
            );
            var sourceRectangle = new Rectangle(
                _window.X + _animationIndex * _window.Width,
                _window.Y,
                _window.Width,
                _window.Height
            );
            //Logger.Debug("Drawing projectile at " + destinationRectangle.ToString() + " with source " + sourceRectangle.ToString());

            spriteBatch.Draw(
                _sprite, destinationRectangle.Location.ToVector2(),
                sourceRectangle,
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Regular DrawOutline method for debugging
        /// </summary>
        /// <param name="gameTime">Monogame GameTime object</param>
        /// <param name="globalOffset">If it's not clear, then Vector2.Zero</param>
        /// <param name="spriteBatch">Mogogame SpriteBatch</param>
        void IGameElement.DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 1, Color.BlueViolet, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public virtual X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }

    public class Y_ShotGunProjectile : Y_StarterProjectile
    {
        public Y_ShotGunProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            Y_Level level,
            IGameElement who
        ) : base(position, direction, timeCreated, level, who)
        {
            _sprite = Manager_Projectile.projectile_textures["smaller_projectile"];
            _window = new Rectangle(0, 0, 32, 32);
            _animationIndex = 0;
            _position = position;
            _direction = direction;
            Velocity = 0.7f * direction;
            TimeCreated = timeCreated;
            _isEnemy = false;
            Name = "StarterProjectile";
            Level = level;
            DeleteNext = false;
            Collision = new X_CollisionModel_Projectile(0.1f, 1.0f);

            WhoFiredMe = who;
            if (WhoFiredMe is IVictim)
            { // Add 50% of the players momentum to the bullet. Adds 50% more fun to the game.
                Velocity += ((IVictim)WhoFiredMe).Velocity * .5f;
                /* 
                TODO: Clamp the velocity and make sure the bullets don't go
                backwards if the player is going backwards super fast (looking
                at you, Ninja...)
                */
            }
            Room = Level.GetRoom(this, Room);
            Scale = 0.15f * Room.Scale;

            _rect = new Rectangle(
                (int)position.X - (int)((float)_window.Width / 2.0f * Scale),
                (int)position.Y - (int)((float)_window.Height / 2.0f * Scale),
                (int)(_window.Width * Scale),
                (int)(_window.Height * Scale));
        }
    }
}