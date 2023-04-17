using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Y_CMSprite : IVictim
    {
        Texture2D _sprite;
        Texture2D _targetIndicator;
        Rectangle _window;
        Dictionary<string, int[]> _animations;
        int _animationIndex;
        IShooter _gun;
        PlayerIndex? _playerIndex;

        X_ConnectorSide _lastSide;
        int _hitCounter;
        int _maxHitCounter;
        bool _isAiming;
        Vector2 _aimDirection;

        public float Scale { get; private set; }
        public Vector2 Position { get; private set; }
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        public X_CollisionModel_Victim Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public Rectangle Rect { get; set; }

        private Vector2 _acceleration;
        private Vector2 _deceleration;
        private Vector2 _maxVelocity;
        private float _mass;
        private float _cr;

        private int _controlLayout;

        public Y_CMSprite(
            X_CollisionModel_Victim collision,
            PlayerIndex? playerIndex,
            Texture2D texture,
            Texture2D targetIndicator,
            Rectangle window,
            float acceleration,
            float maxVelocity,
            Vector2 position,
            Y_Level level,
            float frameDuration,
            Dictionary<string, int[]> animations,
            IShooter gun,
            int controlLayout = 1,
            float scale = 1.0f
            )
        {
            _sprite = texture;
            _targetIndicator = targetIndicator;
            _window = window;
            _animations = animations;
            _animationIndex = 0;
            _gun = gun;
            _playerIndex = playerIndex;
            _controlLayout = controlLayout;

            _hitCounter = 0;
            _maxHitCounter = 750 / 16;

            Velocity = Vector2.Zero;
            _acceleration = Vector2.One * acceleration;
            _deceleration = Vector2.One * acceleration;
            _maxVelocity = Vector2.One * maxVelocity;
            Position = position;

            _isAiming = false;
            _aimDirection = Vector2.Zero;

            Level = level;
            LifePoints = 100;
            HitInLastLoop = false;

            Collision = collision;

            Rect = new Rectangle(
                (int)position.X - (int)(scale * _window.Width / 2),
                (int)position.Y - (int)(scale * _window.Height / 2),
                (int)(scale * _window.Width), (int)(scale * _window.Height)
            );

            Scale = (float)Rect.Width / (float)_window.Width;
            Level.Victims.Add(this);
            Room = Level.GetRoom(this, Room);
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
            GamePadState gpState = GamePad.GetState(_playerIndex.Value);

            if (HitInLastLoop)
            {
                _hitCounter = 1;
                HitInLastLoop = false;
            }
            if (_hitCounter > 0 && _hitCounter < _maxHitCounter) _hitCounter++;
            else _hitCounter = 0;

            // Gamepad control
            if (_playerIndex != null)
            {
                if (gpState.IsButtonDown(Buttons.LeftThumbstickRight)) input.X += gpState.ThumbSticks.Left.X;
                if (gpState.IsButtonDown(Buttons.LeftThumbstickLeft)) input.X += gpState.ThumbSticks.Left.X;
                if (gpState.IsButtonDown(Buttons.LeftThumbstickDown)) input.Y -= gpState.ThumbSticks.Left.Y;
                if (gpState.IsButtonDown(Buttons.LeftThumbstickUp)) input.Y -= gpState.ThumbSticks.Left.Y;

                if (
                    gpState.IsButtonDown(Buttons.RightThumbstickRight) ||
                    gpState.IsButtonDown(Buttons.RightThumbstickLeft) ||
                    gpState.IsButtonDown(Buttons.RightThumbstickDown) ||
                    gpState.IsButtonDown(Buttons.RightThumbstickUp)
                )
                {
                    Vector2 newAimDirection = Vector2.One;
                    newAimDirection.X *= gpState.ThumbSticks.Right.X;
                    newAimDirection.Y *= -gpState.ThumbSticks.Right.Y;
                    newAimDirection.Normalize();
                    _aimDirection = newAimDirection;
                    _isAiming = true;
                }
                else
                {
                    _isAiming = false;
                }
                if (gpState.IsButtonDown(Buttons.RightShoulder) || gpState.IsButtonDown(Buttons.RightTrigger))
                {
                    _isAiming = true; // Show the aim indicator when firing
                    _gun.Shoot(gameTime, Rect.Center.ToVector2(), _aimDirection, Level, this);
                }
            }

            // Mouse & Keyboard control
            if (_controlLayout > 0)
            {
                if (_controlLayout == 1)
                {
                    if (Input.IsKeyDown(Keybinds.P1Right)) input.X += 1;
                    if (Input.IsKeyDown(Keybinds.P1Left)) input.X -= 1;
                    if (Input.IsKeyDown(Keybinds.P1Down)) input.Y += 1;
                    if (Input.IsKeyDown(Keybinds.P1Up)) input.Y -= 1;
                }
                else if (_controlLayout == 2)
                {
                    if (Input.IsKeyDown(Keybinds.P2Right)) input.X += 1;
                    if (Input.IsKeyDown(Keybinds.P2Left)) input.X -= 1;
                    if (Input.IsKeyDown(Keybinds.P2Down)) input.Y += 1;
                    if (Input.IsKeyDown(Keybinds.P2Up)) input.Y -= 1;
                }
                MouseState mouse = Mouse.GetState();
                Vector2 playerCenter = Rect.Center.ToVector2();
                if (Input.HasMouseMoved() && !_isAiming) // Skip if controller is already aiming
                {
                    Vector2 mouseInGamePosition = mouse.Position.ToVector2() / Camera.Zoom + Camera.VisibleArea.Location.ToVector2();
                    Vector2 newAimDirection = mouseInGamePosition - playerCenter;
                    newAimDirection.Normalize();
                    _aimDirection = newAimDirection;
                }
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    _gun.Shoot(gameTime, playerCenter, _aimDirection, Level, this);
                }
            }

            _gun.Update(gameTime);

            bool controls = false;
            if (input != Vector2.Zero) controls = true;
            if (input.LengthSquared() > 1)
            {
                input.Normalize();
            }


            /* ##########################################################################
             * Speed and velocity handling based on control input
             *  => must happen before collision handling <=
             * ########################################################################## */
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
            //Logger.Info(Velocity.ToString() + "    " + MaxVelocity.ToString());
            if (controls)
            {
                Velocity += input * _acceleration * timeStepMS;
            }
            else
            {
                Velocity = new Vector2(
                    Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration.X * timeStepMS),
                    Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration.Y * timeStepMS));
            }
            Velocity = Vector2.Clamp(Velocity, -_maxVelocity, _maxVelocity);

            /* ##########################################################################
             * Collision with everything handling (takes care of location update as well)
             * ########################################################################## */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            if (Collision.Intersect(this, timeStepMS, out contactPoint, out contactNormal, out who))
            {
                Logger.Info("Collided with something");
            }
            /* ########################################################################## */
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
            //spriteBatch.Draw(
            //    _sprite, Rect,
            //    new Rectangle(_animationIndex * _window.Width, 0, _window.Width, _window.Height),
            //    color
            //);

            spriteBatch.Draw(
                _sprite, Rect.Location.ToVector2(), 
                new Rectangle(_animationIndex * _window.Width, 0, _window.Width, _window.Height), 
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

            // Draw a targeting indicator if the player is actively aiming or using mouse controls
            if (_isAiming || _controlLayout > 0)
            {
                var angle = Math.Atan2(_aimDirection.Y, _aimDirection.X) + Math.PI / 2;
                spriteBatch.Draw(
                    _targetIndicator, Rect.Location.ToVector2() + _window.Center.ToVector2() + _aimDirection * _sprite.Height,
                    null,
                    Color.White, (float)angle, new Vector2(_targetIndicator.Width / 2, 0), 0.1f, SpriteEffects.None, 0);
            }
        }

        /// <summary>
        /// Regular DrawOutline method for debugging
        /// </summary>
        /// <param name="gameTime">Monogame GameTime object</param>
        /// <param name="globalOffset">If it's not clear, then Vector2.Zero</param>
        /// <param name="spriteBatch">Mogogame SpriteBatch</param>
        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }
    }
}
