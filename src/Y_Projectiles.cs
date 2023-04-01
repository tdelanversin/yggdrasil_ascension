using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#nullable enable

namespace YGR
{
    public class Y_StarterProjectile: IProjectile
    {
        public bool DeleteNext { get; set; }
        public string Name { get; set; }
        public double TimeCreated { get; set; }
        public Vector2 Position { get; private set; }
        public X_CollisionModel_Projectile Collision { get; }
        public Vector2 Velocity { get; set; }
        public IWalkable Room { get; set; }
        public Rectangle Rect { get; set; }
        public IGameElement WhoFiredMe { get; }

        private Texture2D _sprite;
        public Rectangle _window;
        private int _animationIndex;
        private Vector2 _direction;
        private bool _isEnemy;
        X_ConnectorSide _lastSide;

        public Y_StarterProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            IWalkable room,
            IGameElement who
        ) {
            _sprite = Manager_Projectile.projectile_textures["default_projectile"];
            _window = new Rectangle(0, 0, 64, 64);
            _animationIndex = 0;
            Position = position;
            _direction = direction;
            Velocity = Vector2.One * direction;
            TimeCreated = timeCreated;
            _isEnemy = false;
            Name = "StarterProjectile";
            Room = room;
            DeleteNext = false;
            Collision = new X_CollisionModel_Projectile(0.5f, 1.0f);

            Rect = new Rectangle((int)position.X - _window.Width/2, (int)position.Y - _window.Height/2, _window.Width, _window.Height);

            Room.Projectiles.Add(this);
            WhoFiredMe = who;
        }

        public void Update(GameTime gameTime) {
            int timeStepMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            /* ##########################################################################
             * Collision with everything handling (takes care of location update as well)
             * ########################################################################## */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            if (Collision.Intersect(this, timeStepMS, out contactPoint, out contactNormal, out who))
            {
                Logger.Info("Collided with something");
                foreach(var obj in who)
                {
                    if(obj.WhatAreYou() == X_LevelElements.Victim)
                    {
                        ((IVictim)obj).HitInLastLoop = true;
                    }
                }
            }
            /* ########################################################################## */

            _animationIndex = (int)(5 - (gameTime.TotalGameTime.TotalMilliseconds - TimeCreated) / 300);

            var whatAreYou = Room.WhatAreYou();
            if (whatAreYou == X_LevelElements.Connector)
            {
                var connector = (Y_Connector)Room;
                // check if we are still inside the room
                if (!connector.IsInside(Position))
                {
                    // if not, assign the room according to the last side we were on
                    /*
                     * TODO: this is sensitive to movement speed!!!
                     */
                    var oldRoom = Room.Name;
                    Room = connector.GetRoom(_lastSide);
                    Logger.Info("Projectile moves from room [" + oldRoom + "] to room [" + Room.Name + "]");
                }
                else
                {
                    // if we are still inside the connector, check which pad and if necessary switch
                    connector.IsOnPad(Position, ref _lastSide);
                }
            }
            else if (whatAreYou == X_LevelElements.Room)
            {
                // check if we are inside one of the connector pads
                // if we are => switch the room
                var room = (Y_Room)Room;
                foreach (var conn in room.Connectors)
                {
                    if (conn.IsOnPad(Position, ref _lastSide))
                    {
                        if (conn.IsInside(Position))
                        {
                            var oldRoom = Room.Name;
                            Room = conn;
                            Logger.Info("Projectile move from room [" + oldRoom + "] to room [" + Room.Name + "]");
                        }
                        break;
                    }
                }
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) {
            var destinationRectangle = new Rectangle(
                Rect.X - (int)globalOffset.X,
                Rect.Y - (int)globalOffset.Y,
                _window.Width,
                _window.Height
            );
            var sourceRectangle = new Rectangle(
                _window.X + _animationIndex * _window.Width,
                _window.Y,
                _window.Width,
                _window.Height
            );
            Logger.Debug("Drawing projectile at " + destinationRectangle.ToString() + " with source " + sourceRectangle.ToString());
            spriteBatch.Draw(
                _sprite,
                destinationRectangle,
                sourceRectangle,
                Color.White
            );
        }

        /// <summary>
        /// Regular DrawOutline method for debugging
        /// </summary>
        /// <param name="gameTime">Monogame GameTime object</param>
        /// <param name="globalOffset">If it's not clear, then Vector2.Zero</param>
        /// <param name="spriteBatch">Mogogame SpriteBatch</param>
        void IGameElement.DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, _window.Width, _window.Height, 3, Color.BlueViolet, spriteBatch);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }
}