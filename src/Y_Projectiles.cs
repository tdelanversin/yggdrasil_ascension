using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#nullable enable

namespace YGR
{
    public class Y_StarterProjectile: IProjectile
    {
        public float Scale { get; private set; }
        public bool DeleteNext { get; set; }
        public string Name { get; set; }
        public double TimeCreated { get; set; }
        public X_CollisionModel_Projectile Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }
        public IGameElement WhoFiredMe { get; }

        private Vector2 _position;
        private Texture2D _sprite;
        public Rectangle _window;
        private int _animationIndex;
        private Vector2 _direction;
        private bool _isEnemy;
        X_ConnectorSide _lastSide;
        Rectangle _rect;

        public Y_StarterProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            Y_Level level,
            IGameElement who
        ) {
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
            Room = Level.GetRoom(this, Room);
            Scale = 0.25f*Room.Scale;
            _rect = new Rectangle(
                (int)position.X - (int)((float)_window.Width / 2.0f * Scale), 
                (int)position.Y - (int)((float)_window.Height / 2.0f * Scale), 
                (int)(_window.Width * Scale), 
                (int)(_window.Height * Scale));
        }

        public void Update(GameTime gameTime) {
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
                foreach(var obj in who)
                {
                    //Logger.Info(obj.WhatAreYou().ToString());
                    if (obj.WhatAreYou() == WhoFiredMe.WhatAreYou()) continue;

                    if(obj.WhatAreYou() == X_LevelElements.Victim)
                    {
                        ((IVictim)obj).LifePoints = ((IVictim)obj).LifePoints - 1;
                        ((IVictim)obj).HitInLastLoop = true;
                    }
                    else if (obj.WhatAreYou() == X_LevelElements.Enemy)
                    {
                        //Logger.Info("### " + WhoFiredMe.WhatAreYou().ToString() + " hit an " + obj.WhatAreYou().ToString() + " with id " + ((Y_SimpleEnemy)obj).Identifier + " | lifepoints: " + ((IEnemy)obj).LifePoints.ToString());
                        ((IEnemy)obj).LifePoints = ((IEnemy)obj).LifePoints - 1;
                        //Logger.Info("#2# " + WhoFiredMe.WhatAreYou().ToString() + " hit an " + obj.WhatAreYou().ToString() + " | lifepoints: " + ((IEnemy)obj).LifePoints.ToString());
                        ((IEnemy)obj).HitInLastLoop = true;
                    }

                    Velocity = newVelocity;
                }

                //Rectangle rect = me.Rect;
                //rect.Location += (me.Velocity * timeStepMS).ToPoint();
                //me.Rect = rect;
            }
            /* ########################################################################## */

            _position += newVelocity * timeStepMS;
            _rect.Location = _position.ToPoint();
            _animationIndex = (int)(5 - (gameTime.TotalGameTime.TotalMilliseconds - TimeCreated) / 300);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) {
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

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }

    public class Y_ShotGunProjectile: IProjectile
    {
        public float Scale { get; private set; }
        public bool DeleteNext { get; set; }
        public string Name { get; set; }
        public double TimeCreated { get; set; }
        public X_CollisionModel_Projectile Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        //public Rectangle Rect { get; set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }
        public IGameElement WhoFiredMe { get; }

        private Vector2 _position;
        private Texture2D _sprite;
        public Rectangle _window;
        private int _animationIndex;
        private Vector2 _direction;
        private bool _isEnemy;
        X_ConnectorSide _lastSide;
        Rectangle _rect;

        public Y_ShotGunProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            Y_Level level,
            IGameElement who
        ) {
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
            Room = Level.GetRoom(this, Room);
            Scale = 0.15f * Room.Scale;

            _rect = new Rectangle(
                (int)position.X - (int)((float)_window.Width / 2.0f * Scale), 
                (int)position.Y - (int)((float)_window.Height / 2.0f * Scale), 
                (int)(_window.Width * Scale), 
                (int)(_window.Height * Scale));
        }

        public void Update(GameTime gameTime) {

            /* ##########################################################################
             * Collision with everything handling (takes care of location update as well)
             * ########################################################################## */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            Vector2 newVelocity = Velocity;
            int timeStepMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoint, out contactNormal, out who))
            {
                //Logger.Debug("Collided with something");
                foreach (var obj in who)
                {
                    if (obj.WhatAreYou() == WhoFiredMe.WhatAreYou()) continue;

                    if (obj.WhatAreYou() == X_LevelElements.Victim)
                    {
                        ((IVictim)obj).LifePoints = ((IVictim)obj).LifePoints - 1;
                        ((IVictim)obj).HitInLastLoop = true;
                    }
                    else if (obj.WhatAreYou() == X_LevelElements.Enemy)
                    {
                        ((IEnemy)obj).LifePoints = ((IEnemy)obj).LifePoints - 1;
                        ((IEnemy)obj).HitInLastLoop = true;
                    }
                }

                Velocity = newVelocity;
            }
            _position += newVelocity * timeStepMS;
            _rect.Location = _position.ToPoint();
            _animationIndex = (int)(5 - (gameTime.TotalGameTime.TotalMilliseconds - TimeCreated) / 300);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) {
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
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }
}