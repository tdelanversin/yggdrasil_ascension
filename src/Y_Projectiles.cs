using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#nullable enable

namespace YGR
{
    public class Y_StarterProjectile: IProjectile
    {
        public bool DeleteNext { get; set; }
        public string Name { get; set; }
        public double TimeCreated { get; set; }
        public Vector2 Position { get; private set; }

        private Texture2D _sprite;
        public Rectangle _window;
        private int _animationIndex;
        private Vector2 _direction;
        private double _speed;
        private bool _isEnemy;
        private IWalkable _room;
        X_ConnectorSide _lastSide;
        private IGameElement _who;

        public Y_StarterProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            IWalkable room,
            IGameElement who
        ) {
            _sprite = X_ProjectileManager.projectile_textures["default_projectile"];
            _window = new Rectangle(0, 0, 64, 64);
            _animationIndex = 0;
            Position = position;
            _direction = direction;
            _speed = 1;
            TimeCreated = timeCreated;
            _isEnemy = false;
            Name = "StarterProjectile";
            _room = room;
            DeleteNext = false;

            _room.Projectiles.Add(this);
            _who = who;
        }

        public void Update(GameTime gameTime) {
            IWalkable who = null;
            Vector2 where = Vector2.Zero;
            if (checkColWPlayer())
            {
                DeleteNext = true;
                return;
            }

            Position = _room.Clamp(_window, Position, _direction * (float)(_speed * gameTime.ElapsedGameTime.TotalMilliseconds), ref who, ref where );
            if(who != null)
            {
                DeleteNext = true;
            }
            _animationIndex = (int)(5 - (gameTime.TotalGameTime.TotalMilliseconds - TimeCreated) / 300);

            var whatAreYou = _room.WhatAreYou();
            if (whatAreYou == X_LevelElements.Connector)
            {
                var connector = (Y_Connector)_room;
                // check if we are still inside the room
                if (!connector.IsInside(Position))
                {
                    // if not, assign the room according to the last side we were on
                    /*
                     * TODO: this is sensitive to movement speed!!!
                     */
                    var oldRoom = _room.Name;
                    _room = connector.GetRoom(_lastSide);
                    Logger.Info("Projectile moves from room [" + oldRoom + "] to room [" + _room.Name + "]");
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
                var room = (Y_Room)_room;
                foreach (var conn in room.Connectors)
                {
                    if (conn.IsOnPad(Position, ref _lastSide))
                    {
                        if (conn.IsInside(Position))
                        {
                            var oldRoom = _room.Name;
                            _room = conn;
                            Logger.Info("Projectile move from room [" + oldRoom + "] to room [" + _room.Name + "]");
                        }
                        break;
                    }
                }
            }
        }

        bool checkColWPlayer()
        {
            Rectangle rect = GetRect();
            foreach(var victim in _room.Victims)
            {
                if (victim == _who) continue;

                if (victim.Intersects(rect))
                {
                    victim.HitInLastLoop = true;
                    return true;
                }
            }
            return false;
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) {
            var destinationRectangle = new Rectangle(
                (int)(Position.X - globalOffset.X - _window.Width / 2),
                (int)(Position.Y - globalOffset.Y - _window.Height / 2),
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
            Factory_Debug.DrawRectangle((int)Position.X - _window.Width/2, (int)Position.Y - _window.Height/2, _window.Width, _window.Height, 3, Color.BlueViolet, spriteBatch);
        }

        public bool Intersects(Rectangle other)
        {
            return false;
        }

        public Vector2 IntersectionPoint(Vector2 pos, Vector2 dp)
        {
            return Vector2.Zero;
        }

        public Rectangle GetRect()
        {
            return new Rectangle((int)Position.X - _window.Width/2, (int)Position.Y - _window.Height/2, _window.Width, _window.Height);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }
}