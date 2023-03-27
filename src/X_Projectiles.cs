using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#nullable enable

namespace YGR
{
    public class X_StarterProjectile: IProjectile
    {
        public bool DeleteNext { get; set; }
        public string Name { get; set; }

        public double TimeCreated { get; set; }

        private Texture2D _sprite;
        public Rectangle _window;
        private int _animationIndex;
        private Vector2 _position;
        private Vector2 _direction;
        private double _speed;
        private bool _isEnemy;
        private IWalkable _room;
        X_ConnectorSide _lastSide;

        public X_StarterProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated,
            IWalkable room
        ) {
            _sprite = ProjectileManager.projectile_textures["default_projectile"];
            _window = new Rectangle(0, 0, 64, 64);
            _animationIndex = 0;
            _position = position;
            _direction = direction;
            _speed = 1;
            TimeCreated = timeCreated;
            _isEnemy = false;
            Name = "StarterProjectile";
            _room = room;
            DeleteNext = false;
        }

        public void Update(GameTime gameTime) {
            IWalkable who = null;
            Vector2 where = Vector2.Zero;
            _position = _room.Clamp(_window, _position, _direction * (float)(_speed * gameTime.ElapsedGameTime.TotalMilliseconds), ref who, ref where );
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
                if (!connector.IsInside(_position))
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
                    connector.IsOnPad(_position, ref _lastSide);
                }
            }
            else if (whatAreYou == X_LevelElements.Room)
            {
                // check if we are inside one of the connector pads
                // if we are => switch the room
                var room = (Y_Room)_room;
                foreach (var conn in room.Connectors)
                {
                    if (conn.IsOnPad(_position, ref _lastSide))
                    {
                        if (conn.IsInside(_position))
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

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) {
            var destinationRectangle = new Rectangle(
                (int)(_position.X - globalOffset.X - _window.Width / 2),
                (int)(_position.Y - globalOffset.Y - _window.Height / 2),
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
    }
}