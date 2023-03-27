using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace YGR
{
    public class Y_Sprite
    {
        Texture2D _sprite;
        Rectangle _window;
        Dictionary<string, int[]> _animations;
        Vector2 _position;
        float _velocity;
        float _frameDuration;
        int _animationIndex;
        IShooter _gun;

        IWalkable _currentRoom;
        X_ConnectorSide _lastSide;

        public Y_Sprite(
            Texture2D texture, 
            Rectangle window,
            float velocity,
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
            _velocity = velocity;
            _position = position;
            _animationIndex = 0;
            _currentRoom = startRoom;
            _gun = gun;
        }

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {
            Vector2 input = Vector2.Zero;
            KeyboardState keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (keyboard.IsKeyDown(Keys.Right)) input.X += 1;
            if (keyboard.IsKeyDown(Keys.Left)) input.X -= 1;
            if (keyboard.IsKeyDown(Keys.Down)) input.Y += 1;
            if (keyboard.IsKeyDown(Keys.Up)) input.Y -= 1;

            if (input.LengthSquared() > 1) input.Normalize();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 dp = input * deltaTime * _velocity;

            // this is the collision detection with the room and the connectors
            IWalkable who = null;
            Vector2 where = Vector2.Zero;
            _position = _currentRoom.Clamp(_window, _position, dp, ref who, ref where);

            if (who != null) Logger.Debug("collided with someone at location " + where.ToString());

            /* ================================================ */
            /* Detect player shooting and spawn projectiles     */
            /* ================================================ */

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                var d = (mouse.Position.ToVector2() - _position);
                d.Normalize();
                _gun.Shoot(gameTime, _position, d, _currentRoom);
            }

            /* ================================================ */
            /* TODO: add collision detection with other sprites */
            /* ================================================ */

            // check where we are in
            var whatAreYou = _currentRoom.WhatAreYou();
            if (whatAreYou == X_LevelElements.Connector)
            {
                var connector = (Y_Connector)_currentRoom;
                // check if we are still inside the room
                if (!connector.IsInside(_position))
                {
                    // if not, assign the room according to the last side we were on
                    /*
                     * TODO: this is sensitive to movement speed!!!
                     */
                    var oldRoom = _currentRoom.Name;
                    _currentRoom.ResetBackgroundColor();
                    _currentRoom = connector.GetRoom(_lastSide);
                    _currentRoom.SetBackgroundColor(Color.Orange);
                    Logger.Info("Move from room [" + oldRoom + "] to room [" + _currentRoom.Name + "]");
                }
                else
                {
                    // if we are still inside the connector, check which pad and if necessary switch
                    connector.IsOnPad(_position, ref _lastSide);
                }
            }
            else if(whatAreYou == X_LevelElements.Room)
            {
                // check if we are inside one of the connector pads
                // if we are => switch the room
                var room = (Y_Room)_currentRoom;
                foreach(var conn in room.Connectors)
                {
                    if(conn.IsOnPad(_position, ref _lastSide))
                    {
                        if (conn.IsInside(_position))
                        {
                            var oldRoom = _currentRoom.Name;
                            _currentRoom.ResetBackgroundColor();
                            _currentRoom = conn;
                            _currentRoom.SetBackgroundColor(Color.LightBlue);
                            Logger.Info("Move from room [" + oldRoom + "] to room [" + _currentRoom.Name + "]");
                        }
                        break;
                    }
                }
            }
            else if(whatAreYou == X_LevelElements.Victim)
            {
                // this would be another sprite
                // perhaps with regular sprites, don't do anything
            }
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _sprite,
                new Rectangle((int)_position.X - _window.Width/2, (int)_position.Y - _window.Height/2, _window.Width, _window.Height),
                new Rectangle(_animationIndex * _window.Width, 0, _window.Width, _window.Height),
                Color.White
            );
        }
    }
}
