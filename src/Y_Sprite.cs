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
        public Rectangle _window;
        Dictionary<string, int[]> _animations;
        public float _velocity;
        float _frameDuration;
        int _animationIndex;
        IShooter _gun;
        public PlayerIndex? _playerIndex;

        public IWalkable Room { get; set; }
        X_ConnectorSide _lastSide;
        int _hitCounter;
        int _maxHitCounter;

        public Vector2 Position { get; private set; }
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        public X_CollisionModel_Victim Collision { get; }

        public Rectangle Rect { get; set; }
        public Vector2 Velocity { get; set; }

        private Vector2 _acceleration;
        private Vector2 _deceleration;
        private Vector2 _maxVelocity;

        public Vector2 input;

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
            _velocity = velocity;
            Position = position;
            _animationIndex = 5;
            _currentRoom = startRoom;
            _gun = gun;
            _playerIndex = playerIndex;

            _hitCounter = 0;
            _maxHitCounter = 750 / 16;

            Velocity = Vector2.Zero;
            _acceleration = Vector2.One * acceleration;
            _deceleration = Vector2.One * deceleration;
            _maxVelocity = Vector2.One * maxVelocity;
            Position = position;
            Rect = new Rectangle(
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
        public virtual void Update(GameTime gameTime)
        {
            input = Vector2.Zero;
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
                if (Keyboard.IsPressed(Keys.D)) input.X += 1;
                if (Keyboard.IsPressed(Keys.A)) input.X -= 1;
                if (Keyboard.IsPressed(Keys.S)) input.Y += 1;
                if (Keyboard.IsPressed(Keys.W)) input.Y -= 1;

                /* ================================================ */
                /* Detect player shooting and spawn projectiles     */
                /* ================================================ */

                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    var d = (mouse.Position.ToVector2() - Position);
                    d.Normalize();
                    _gun.Shoot(gameTime, Rect.Location.ToVector2(), d, Room, this);
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

                    _gun.Shoot(gameTime, Rect.Location.ToVector2(), shootDir, Room, this);
                }
            }

            if (input != Vector2.Zero) controls = true;
            
            // update gun for special shooting effects
            _gun.Update(gameTime);

            if (input.LengthSquared() > 1)
            {
                input.Normalize();
            }

            // only check collision if we actually have some input...
            int deltaTime = gameTime.ElapsedGameTime.Milliseconds;
            //Logger.Info(Velocity.ToString() + "    " + MaxVelocity.ToString());
            if (controls)
            {
                Velocity += input * _acceleration * deltaTime;
            }
            else
            {
                Vector2 newVelocity = Vector2.Zero;
                newVelocity.X = Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration.X * deltaTime);
                newVelocity.Y = Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration.Y * deltaTime);
                Velocity = newVelocity;
            }
            Velocity = Vector2.Clamp(Velocity, -_maxVelocity, _maxVelocity);

            Vector2 contactNormal;
            Point contactPoint;
            Vector2 velocity = Velocity;
            Rectangle rect = Rect;
            if (Room.Collision.Intersect(ref rect, ref velocity, deltaTime, out contactPoint, out contactNormal))
            {
                Velocity = velocity;
                Logger.Info("impacted at " + contactPoint.ToString());
            }

            //Velocity += input * deltaTime * _acceleration;
            rect.Location += (Velocity * deltaTime).ToPoint();
            Rect = rect;
        }

        public bool Intersects(Rectangle other)
        {
            return other.Intersects(Rect);
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Color color = Color.White;
            if (_hitCounter != 0)
            {
                color = Color.OrangeRed;
            }

            spriteBatch.Draw(
                _sprite,
                Rect,
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
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.OrangeRed, spriteBatch);
        }
    }
}
