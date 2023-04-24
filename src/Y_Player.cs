using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace YGR
{
    public enum ControlLayout
    {
        ControllerOnly = 0,
        KeyboardWASD,
        KeyboardArrows,
    }

    public class SimplePlayer : IVictim
    {
        // IGameElement fields
        public float Scale { get; protected set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        // IVictim fields
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        public X_CollisionModel_Victim Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }

        // private fields
        protected bool _isAiming;
        protected ControlLayout _controlLayout;
        protected Dictionary<string, int[]> _animations;
        protected float _cr;
        protected float _mass;
        protected InputType _currentAimInput;
        protected int _animationIndex;
        protected IShooter _gun;
        protected PlayerIndex _playerIndex;
        protected Rectangle _rect;
        protected Rectangle _spriteDimensions;
        protected Texture2D _spriteAimIndicator;
        protected Texture2D _spriteGhost;
        protected Texture2D _spritePlayer;
        protected Vector2 _acceleration;
        protected Vector2 _aimDirection;
        protected Vector2 _deceleration;
        protected Vector2 _maxVelocity;
        protected Vector2 _position;
        protected ParticleEffect pE;

        protected enum InputType
        {
            Controller = 0,
            KeyboardMouse,
        }

        public SimplePlayer(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            ControlLayout controlLayout = ControlLayout.ControllerOnly,
            float scale = 1.0f
            )
        {
            // Use all constructor arguments
            Level = level;
            _playerIndex = playerIndex;
            _position = initialPosition;
            _gun = gun;
            _controlLayout = controlLayout;

            // Set all the sprites
            _spritePlayer = Manager_Players.SpriteBasic;
            _spriteGhost = Manager_Players.SpriteGhost;
            _spriteAimIndicator = Manager_Players.SpriteAimIndicator[(int)playerIndex];

            _spriteDimensions = new Rectangle(0, 0, 42, 60);
            _animationIndex = 0;
            _animations = new Dictionary<string, int[]> {
                { "stand", new int[] { 0, 1, 8, 9 } },
                { "walk_left", new int[] { 2, 3, 4 } },
                { "walk_right", new int[] { 5, 6, 7 } }};

            Velocity = Vector2.Zero;
            _acceleration = Vector2.One * 0.008f;
            _deceleration = Vector2.One * 0.008f;
            _maxVelocity = Vector2.One * 0.4f;

            _mass = 1.0f;
            _cr = 0.0f; // elastic impact
            Collision = new X_CollisionModel_Victim(_mass, _cr);

            _isAiming = false;
            _aimDirection = Vector2.Zero;

            LifePoints = 30;
            HitInLastLoop = false;

            _rect = new Rectangle(
                (int)_position.X - (int)(scale * _spriteDimensions.Width / 2),
                (int)_position.Y - (int)(scale * _spriteDimensions.Height / 2),
                (int)(scale * _spriteDimensions.Width), (int)(scale * _spriteDimensions.Height)
            );

            Scale = (float)Rect.Width / (float)_spriteDimensions.Width;
            Level.Victims.Add(this);
            Room = Level.GetRoom(this, Room);

            //Walking particles
            Manager_Particles.GenParticleEffect();

        }

        public X_LevelElements WhatAreYou()
        {
            if (IsAlive())
            {
                return X_LevelElements.Victim;
            }
            else
            {
                return X_LevelElements.Ghost;
            }
        }

        public bool IsAlive()
        {
            return LifePoints > 0;
        }

        /* Handle GamePad movement, aiming and shooting */
        public void HandleGamepadInput(GameTime gameTime, ref Vector2 input)
        {
            GamePadState gpState = GamePad.GetState(_playerIndex);
            if (gpState.IsConnected)
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
                    _currentAimInput = InputType.Controller;
                }
                else
                {
                    _isAiming = false;
                }
                if ((gpState.IsButtonDown(Buttons.RightShoulder) || gpState.IsButtonDown(Buttons.RightTrigger)) && IsAlive())
                {
                    _isAiming = true; // Show the aim indicator when firing
                    _currentAimInput = InputType.Controller;
                    _gun.Shoot(gameTime, Rect.Center.ToVector2(), _aimDirection, Level, this);
                }
            }
        }

        /* Handle Keyboard & Mouse movement, aiming and shooting */
        public void HandleMouseKeyboardInput(GameTime gameTime, ref Vector2 input)
        {

            if (_controlLayout > 0)
            {
                if (_controlLayout == ControlLayout.KeyboardWASD)
                {


                    if (Input.IsKeyDown(Keybinds.P1Right)) input.X += 1;
                    if (Input.IsKeyDown(Keybinds.P1Left)) input.X -= 1;
                    if (Input.IsKeyDown(Keybinds.P1Down)) input.Y += 1;
                    if (Input.IsKeyDown(Keybinds.P1Up)) input.Y -= 1;
                }
                else if (_controlLayout == ControlLayout.KeyboardArrows)
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
                if (mouse.LeftButton == ButtonState.Pressed && IsAlive())
                {
                    _gun.Shoot(gameTime, playerCenter, _aimDirection, Level, this);
                }

                if (Input.HasMouseStateChanged())
                {
                    _currentAimInput = InputType.KeyboardMouse;
                }
            }
        }

        public virtual void UpdateVelocity(Vector2 input, GameTime gameTime)
        {
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;

            /* ##########################################################################
             * Speed and velocity handling based on control input
             *  => must happen before collision handling <=
             * ########################################################################## */
            if (input != Vector2.Zero)
            {
                Manager_Particles._particleEffects[0].Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));

                if (input.LengthSquared() > 1)
                {
                    input.Normalize();
                }
                Velocity += input * _acceleration * timeStepMS;
            }
            else
            {
                Velocity = new Vector2(
                    Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration.X * timeStepMS),
                    Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration.Y * timeStepMS));
            }

            Velocity = Vector2.Clamp(Velocity, -_maxVelocity, _maxVelocity);
        }

        public virtual void UpdateCollision(GameTime gameTime)
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
                Velocity = newVelocity;
            }

            _position += newVelocity * timeStepMS;
            _rect.Location = _position.ToPoint();
        }

        public virtual void Update(GameTime gameTime)
        {
            Manager_Particles.Update(gameTime);
            Vector2 input = Vector2.Zero;
            HandleGamepadInput(gameTime, ref input);
            HandleMouseKeyboardInput(gameTime, ref input);
            UpdateVelocity(input, gameTime);
            UpdateCollision(gameTime);
            _gun.Update(gameTime);

        }

        // Render ghosty 👻
        protected virtual void DrawGhost(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: _spriteGhost,
                position: _rect.Location.ToVector2(),
                sourceRectangle: new Rectangle(0, 0, 160, 160),
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: (float)Rect.Width / 160,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        protected virtual void DrawPlayer(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: _spritePlayer,
                position: _rect.Location.ToVector2(),
                sourceRectangle: new Rectangle(_animationIndex * _spriteDimensions.Width, 0, _spriteDimensions.Width, _spriteDimensions.Height),
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: Scale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        protected virtual void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // TODO: Improve
            string str = "P" + (int)_playerIndex + ": " + LifePoints.ToString();
            float str_width = Fonts.Normal.MeasureString(str).X;
            spriteBatch.DrawString(Fonts.Normal, str, new Vector2(_rect.Location.X + _rect.Width / 2 - str_width / 2, _rect.Location.Y - 16), Color.Wheat);
        }

        protected virtual void DrawAimIndicator(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw an indicator only if a) the player is actively aiming on the gamepad or b) is using mouse to aim
            if (_currentAimInput == InputType.Controller && _isAiming || _currentAimInput == InputType.KeyboardMouse)
            {
                var angle = Math.Atan2(_aimDirection.Y, _aimDirection.X) + Math.PI / 2;
                spriteBatch.Draw(
                    _spriteAimIndicator, _rect.Location.ToVector2() + _spriteDimensions.Center.ToVector2() + _aimDirection * _spriteDimensions.Height,
                    null,
                    Color.White, (float)angle, new Vector2(_spriteAimIndicator.Width / 2, 0), 0.1f, SpriteEffects.None, 0);
            }
        }
        protected virtual void DrawParticles(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            Manager_Particles.Draw(gameTime, spriteBatch);
        }


        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            if (IsAlive())
            {
                DrawParticles(gameTime, globalOffset, spriteBatch);
                DrawPlayer(gameTime, globalOffset, spriteBatch);
                DrawAimIndicator(gameTime, globalOffset, spriteBatch);
                DrawOverheadString(gameTime, globalOffset, spriteBatch);
            }
            else
            {
                DrawGhost(gameTime, globalOffset, spriteBatch);
            }
        }

        /// <summary>
        /// Regular DrawOutline method for debugging
        /// </summary>
        /// <param name="gameTime">Monogame GameTime object</param>
        /// <param name="globalOffset">If it's not clear, then Vector2.Zero</param>
        /// <param name="spriteBatch">Mogogame SpriteBatch</param>
        public virtual void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }
    }
    public class Ninja : SimplePlayer // Y_Sprite
    {
        private bool _isDashing;
        private int _dashDuration;
        private float _dashSpeed;
        private int _dashTimer;
        private int _dashCooldown;
        private int _dashCooldownTimer;

        // A timer that stores milliseconds.
        float timer;
        // An int that is the threshold for the timer.
        int threshold;
        // A Rectangle array that stores sourceRectangles for animations.
        Rectangle[] sourceRectangles;
        Dictionary<string, Rectangle[]> directionSourceRectangles;
        // These bytes tell the spriteBatch.Draw() what sourceRectangle to display.
        byte previousAnimationIndex;
        byte currentAnimationIndex;

        public Ninja(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            ControlLayout controlLayout = ControlLayout.ControllerOnly,
            float scale = 1.0f
            ) : base(playerIndex, initialPosition, level, gun, controlLayout, scale)
        {
            /* Overrides from base class */
            _spritePlayer = Manager_Players.SpriteNinja;
            LifePoints = 8; // Ninja squishy
            _maxVelocity = Vector2.One * 0.5f; // Ninja go fast

            /* Class specifics */
            _isDashing = false;
            _dashDuration = 100; // Dash duration in ms
            _dashSpeed = 3f; // Dash speed multiplier
            _dashTimer = 0;
            _dashCooldown = 1000; // Dash cooldown in ms
            _dashCooldownTimer = _dashCooldown;

            // Set a default timer value.
            timer = 0;
            // Set an initial threshold of 250ms, you can change this to alter the speed of the animation (lower number = faster animation).
            threshold = 250;
            // Three sourceRectangles contain the coordinates of Alex's three down-facing sprites on the charaset.
            directionSourceRectangles = new Dictionary<string, Rectangle[]>
            {
                {
                    "down", new Rectangle[]
                    {
                        new Rectangle(0, 128, 48, 64),
                        new Rectangle(48, 128, 48, 64),
                        new Rectangle(96, 128, 48, 64)
                    }
                },
                {
                    "up", new Rectangle[]
                    {
                        new Rectangle(0, 0, 48, 64),
                        new Rectangle(48, 0, 48, 64),
                        new Rectangle(96, 0, 48, 64)
                    }
                },
                {
                    "right", new Rectangle[]
                    {
                        new Rectangle(0, 64, 48, 64),
                        new Rectangle(48, 64, 48, 64),
                        new Rectangle(96, 64, 48, 64)
                    }
                },
                {
                    "left", new Rectangle[]
                    {
                        new Rectangle(0, 192, 48, 64),
                        new Rectangle(48, 192, 48, 64),
                        new Rectangle(96, 192, 48, 64)
                    }
                },
                {
                    "idle", new Rectangle[]
                    {
                        new Rectangle(0, 128, 48, 64),
                        new Rectangle(0, 128, 48, 64),
                        new Rectangle(0, 128, 48, 64)
                    }
                }
            };

            Rectangle rr = directionSourceRectangles["down"][0];
            _spriteDimensions = new Rectangle(0, 0, rr.Width, rr.Height);
            _rect = new Rectangle(
                (int)_position.X - (int)(scale * _spriteDimensions.Width / 2),
                (int)_position.Y - (int)(scale * _spriteDimensions.Height / 2),
                (int)(scale * _spriteDimensions.Width), (int)(scale * _spriteDimensions.Height)
            );

            Scale = (float)Rect.Width / (float)_spriteDimensions.Width;

            // This tells the animation to start on the left-side sprite.
            previousAnimationIndex = 2;
            currentAnimationIndex = 1;
        }

        private void UpdateDash(GameTime gameTime)
        {
            // Handle dash
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
            // Update the cooldown timer
            if (_dashCooldownTimer < _dashCooldown)
            {
                _dashCooldownTimer += timeStepMS;
            }

            if (!_isDashing && (Input.IsKeyDown(Keybinds.ActionOne) || Input.IsButtonDown(_playerIndex, Buttons.A)) && _dashCooldownTimer >= _dashCooldown)
            {
                _isDashing = true;
                _dashTimer = 0;
                _dashCooldownTimer = 0; // Reset timer
            }

            if (_isDashing)
            {
                _dashTimer += timeStepMS;

                if (_dashTimer >= _dashDuration)
                {
                    _isDashing = false;
                }
                else
                {
                    Velocity *= _dashSpeed;
                }
            }
        }

        override public void Update(GameTime gameTime)
        {
            Vector2 input = Vector2.Zero;
            HandleGamepadInput(gameTime, ref input);
            HandleMouseKeyboardInput(gameTime, ref input);

            UpdateVelocity(input, gameTime);
            UpdateDash(gameTime); // Updates Velocity directly for now, so call before UpdateCollision()
            UpdateCollision(gameTime);

            _gun.Update(gameTime);

            // TODO: Improve sprite/animation stuff
            string direction = "down";
            if (input.X > 0)
            {
                direction = "right";
            }
            else if (input.X < 0)
            {
                direction = "left";
            }
            else if (input.Y > 0)
            {
                direction = "down";
            }
            else if (input.Y < 0)
            {
                direction = "up";
            }
            else
            {
                direction = "idle";
            }

            // Update the sourceRectangles array based on direction
            sourceRectangles = directionSourceRectangles[direction];

            // Check if the timer has exceeded the threshold.
            if (timer > threshold)
            {
                // If Alex is in the middle sprite of the animation.
                if (currentAnimationIndex == 1)
                {
                    // If the previous animation was the left-side sprite, then the next animation should be the right-side sprite.
                    if (previousAnimationIndex == 0)
                    {
                        currentAnimationIndex = 2;
                    }
                    else
                    // If not, then the next animation should be the left-side sprite.
                    {
                        currentAnimationIndex = 0;
                    }
                    // Track the animation.
                    previousAnimationIndex = currentAnimationIndex;
                }
                // If Alex was not in the middle sprite of the animation, he should return to the middle sprite.
                else
                {
                    currentAnimationIndex = 1;
                }
                // Reset the timer.
                timer = 0;
            }
            // If the timer has not reached the threshold, then add the milliseconds that have past since the last Update() to the timer.
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

        }

        override protected void DrawPlayer(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(
                    _spritePlayer,
                    new Rectangle(
                        _rect.X, _rect.Y, _rect.Width, _rect.Height),
                        sourceRectangles[currentAnimationIndex], Color.White);
        }
    }
}
