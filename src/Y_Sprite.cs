using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Y_Sprite : IVictim
    {
        Texture2D _sprite;
        Rectangle _window;
        Dictionary<string, int[]> _animations;
        float _frameDuration;
        int _animationIndex;
        IShooter _gun;
        PlayerIndex? _playerIndex;

        IWalkable _currentRoom;
        X_ConnectorSide _lastSide;
        int _hitCounter;
        int _maxHitCounter;

        public Vector2 Position { get; private set; }
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        private Rectangle _rect;
        private Vector2 _velocity;
        private Vector2 _acceleration;
        private Vector2 _deceleration;
        private Vector2 _maxVelocity;

        public Y_Sprite(
            PlayerIndex? playerIndex,
            Texture2D texture,
            Rectangle window,
            float acceleration,
            float deceleration,
            float maxVelocity,
            Vector2 position,
            IWalkable startRoom,
            float frameDuration,
            Dictionary<string, int[]> animations,
            IShooter gun
            )
        {
            _sprite = texture;
            _window = window;
            _animations = animations;
            _animationIndex = 0;
            _currentRoom = startRoom;
            _gun = gun;
            _playerIndex = playerIndex;

            _hitCounter = 0;
            _maxHitCounter = 750 / 16;

            _velocity = Vector2.Zero;
            _acceleration = Vector2.One * acceleration;
            _deceleration = Vector2.One * deceleration;
            _maxVelocity = Vector2.One * maxVelocity;
            Position = position;
            _rect = new Rectangle(
                (int)position.X - _window.Width/2,
                (int)position.Y - _window.Height/2,
                _window.Width, _window.Height
            );
            LifePoints = 100;
            HitInLastLoop = false;

            //startRoom.Victims.Add(this);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Victim;
        }

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {
            Vector2 input = Vector2.Zero;
            MouseState mouse = Mouse.GetState();

            if (HitInLastLoop)
            {
                _hitCounter = 1;
                HitInLastLoop = false;
            }
            if (_hitCounter > 0 && _hitCounter < _maxHitCounter) _hitCounter++;
            else _hitCounter = 0;

            bool controls = false;
            if(_playerIndex == null)
            {
                KeyboardState keyboard = Keyboard.GetState();
                if (keyboard.IsKeyDown(Keys.D)) input.X += 1;
                if (keyboard.IsKeyDown(Keys.A)) input.X -= 1;
                if (keyboard.IsKeyDown(Keys.S)) input.Y += 1;
                if (keyboard.IsKeyDown(Keys.W)) input.Y -= 1;

                /* ================================================ */
                /* Detect player shooting and spawn projectiles     */
                /* ================================================ */

                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    var d = (mouse.Position.ToVector2() - Position);
                    d.Normalize();
                    _gun.Shoot(gameTime, Position, d, _currentRoom, this);
                }
            }
            else
            {
                GamePadState gpState = GamePad.GetState(_playerIndex.Value);
                if (gpState.IsButtonDown(Buttons.LeftThumbstickRight)) input.X += gpState.ThumbSticks.Left.X;
                if (gpState.IsButtonDown(Buttons.LeftThumbstickLeft)) input.X += gpState.ThumbSticks.Left.X;
                if (gpState.IsButtonDown(Buttons.LeftThumbstickDown)) input.Y -= gpState.ThumbSticks.Left.Y;
                if (gpState.IsButtonDown(Buttons.LeftThumbstickUp)) input.Y -= gpState.ThumbSticks.Left.Y;

                if (gpState.IsButtonDown(Buttons.RightShoulder) || gpState.IsButtonDown(Buttons.RightTrigger))
                {
                    Vector2 shootDir = Vector2.One;
                    if (
                        gpState.IsButtonDown(Buttons.RightThumbstickRight) ||
                        gpState.IsButtonDown(Buttons.RightThumbstickLeft) ||
                        gpState.IsButtonDown(Buttons.RightThumbstickDown) ||
                        gpState.IsButtonDown(Buttons.RightThumbstickUp) 
                    )
                    {
                        shootDir.X *= gpState.ThumbSticks.Right.X;
                        shootDir.Y *= -gpState.ThumbSticks.Right.Y;
                        shootDir.Normalize();
                    }
                    else
                    {
                        shootDir.X = -1.0f;
                        shootDir.Y = 0.0f;
                    }

                    _gun.Shoot(gameTime, Position, shootDir, _currentRoom, this);
                }
            }

            if (input != Vector2.Zero) controls = true;
            if (input.LengthSquared() > 1)
            {
                input.Normalize();
            }

            // only check collision if we actually have some input...
            int deltaTime = gameTime.ElapsedGameTime.Milliseconds;
            //Logger.Info(Velocity.ToString() + "    " + MaxVelocity.ToString());
            if (controls)
            {
                _velocity += input * _acceleration * deltaTime;
            }
            else
            {
                _velocity.X = Math.Sign(_velocity.X) * Math.Max(0.0f, Math.Abs(_velocity.X) - _deceleration.X * deltaTime);
                _velocity.Y = Math.Sign(_velocity.Y) * Math.Max(0.0f, Math.Abs(_velocity.Y) - _deceleration.Y * deltaTime);
            }
            _velocity = Vector2.Clamp(_velocity, -_maxVelocity, _maxVelocity);

            //Vector2 where = Vector2.Zero;
            //IGameElement with;
            //if(!checkColWSprite(Velocity, out with, out where)){
            //    // this is the collision detection with the room and the connectors
            //    IWalkable who = null;
            //    Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, _window.Width, _window.Height);
            //    Position = _currentRoom.Clamp(rect, Position, Velocity, ref who, ref where);

            //    if (who != null) Logger.Debug("collided with some room at location " + where.ToString());
            //}

            Vector2 contactNormal;
            Point contactPoint;
            if (_currentRoom.Intersects(ref _rect, ref _velocity, deltaTime, out contactPoint, out contactNormal))
            {
                Logger.Info("impacted at " + contactPoint.ToString());
            }

            //_velocity += input * deltaTime * _acceleration;
            _rect.Location += (_velocity * deltaTime).ToPoint();

            //// check where we are in
            //var whatAreYou = _currentRoom.WhatAreYou();
            //if (whatAreYou == X_LevelElements.Connector)
            //{
            //    var connector = (Y_Connector)_currentRoom;
            //    // check if we are still inside the room
            //    if (!connector.IsInside(Position))
            //    {
            //        // if not, assign the room according to the last side we were on
            //        /*
            //         * TODO: this is sensitive to movement speed!!!
            //         */
            //        var oldRoom = _currentRoom.Name;
            //        _currentRoom.Victims.Remove(this);
            //        _currentRoom = connector.GetRoom(_lastSide);
            //        _currentRoom.Victims.Add(this);
            //        Logger.Info("Move from room [" + oldRoom + "] to room [" + _currentRoom.Name + "]");
            //    }
            //    else
            //    {
            //        // if we are still inside the connector, check which pad and if necessary switch
            //        connector.IsOnPad(Position, ref _lastSide);
            //    }
            //}
            //else if(whatAreYou == X_LevelElements.Room)
            //{
            //    // check if we are inside one of the connector pads
            //    // if we are => switch the room
            //    var room = (Y_Room)_currentRoom;
            //    foreach(var conn in room.Connectors)
            //    {
            //        if(conn.IsOnPad(Position, ref _lastSide))
            //        {
            //            if (conn.IsInside(Position))
            //            {
            //                var oldRoom = _currentRoom.Name;
            //                _currentRoom.Victims.Remove(this);
            //                _currentRoom = conn;
            //                _currentRoom.Victims.Add(this);
            //                Logger.Info("Move from room [" + oldRoom + "] to room [" + _currentRoom.Name + "]");
            //            }
            //            break;
            //        }
            //    }
            //}
        }

        //private bool checkColWSprite(Vector2 dp, out IGameElement who, out Vector2 where)
        //{
        //    // check collision with some victims
        //    Rectangle rect = GetRect();
        //    rect.X += (int)Math.Ceiling(dp.X + Math.Sign(dp.X));
        //    rect.Y += (int)Math.Ceiling(dp.Y + Math.Sign(dp.Y));

        //    where = Vector2.Zero;
        //    who = null;
        //    foreach (var victim in _currentRoom.Victims)
        //    {
        //        if (victim == this) continue;

        //        if (victim.Intersects(rect))
        //        {
        //            //Rectangle intersect = Manager_Collision.Intersect(rect, victim.GetRect());
        //            //who = victim;
        //            //where = new Vector2(intersect.X + intersect.Width / 2, intersect.Y + intersect.Height / 2);
        //            return true;
        //        }
        //    }
        //    Position += dp;
        //    return false;
        //}

        //public Rectangle GetRect()
        //{
        //    return new Rectangle((int)Position.X - _window.Width/2, (int)Position.Y - _window.Height/2, _window.Width, _window.Height);
        //}

        public bool Intersects(Rectangle other)
        {
            return other.Intersects(_rect);
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Color color = Color.White;
            if (_hitCounter != 0)
            {
                color = Color.OrangeRed;
            }
            spriteBatch.Draw(
                _sprite,
                _rect,
                new Rectangle(_animationIndex * _window.Width, 0, _window.Width, _window.Height),
                color
            );
        }

        /// <summary>
        /// Regular DrawOutline method for debugging
        /// </summary>
        /// <param name="gameTime">Monogame GameTime object</param>
        /// <param name="globalOffset">If it's not clear, then Vector2.Zero</param>
        /// <param name="spriteBatch">Mogogame SpriteBatch</param>
        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height, 3, Color.OrangeRed, spriteBatch);
        }

        public Rectangle GetRect()
        {
            return _rect;
        }
    }
}
