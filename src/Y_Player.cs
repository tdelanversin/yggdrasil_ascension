using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Particles;
using System;
using System.Collections.Generic;

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
        public X_CollisionModel_Victim Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }

        // Class fields
        public ControlLayout ControlLayout;
        protected bool _isAiming;
        protected bool _invincible;
        protected float _invincibleDuration;
        protected float _invincibleTimeLeft;
        protected float _cr;
        protected float _mass;
        protected InputType _currentAimInput;
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
        protected Color _color;

        protected float _actionTimer;
        protected float _actionTreshold;

        protected Dictionary<string, int[]> _animations;
        protected string _animationDirection;
        protected int _animationIndex;
        protected float _animationTimer;
        protected float _animationTreshold;

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
            ControlLayout = controlLayout;

            // Set all the sprites
            _spritePlayer = Manager_Players.SpriteBasic;
            _spriteGhost = Manager_Players.SpriteGhost;
            _spriteAimIndicator = Manager_Players.SpriteAimIndicator[(int)playerIndex];
            _color = Color.White;

            _spriteDimensions = new Rectangle(0, 0, 44, 62);
            _animationTimer = 0;
            _animationTreshold = 250; // After how many ms to cycle through sprites
            _animationIndex = 0;
            _animations = new Dictionary<string, int[]> {
                { "idle", new int[] { 0 } },
                { "left", new int[] { 1, 2 } },
                { "right", new int[] { 3, 4 } }};
            _animationDirection = "idle";

            _rect = new Rectangle(
                (int)_position.X - (int)(scale * _spriteDimensions.Width / 2),
                (int)_position.Y - (int)(scale * _spriteDimensions.Height / 2),
                (int)(scale * _spriteDimensions.Width), (int)(scale * _spriteDimensions.Height)
            );

            Scale = Math.Min(
                _rect.Width / (float)_spriteDimensions.Width,
                _rect.Height / (float)_spriteDimensions.Height
            );

            // Movement related
            Velocity = Vector2.Zero;
            _acceleration = Vector2.One * 0.008f;
            _deceleration = Vector2.One * 0.008f;
            _maxVelocity = Vector2.One * 0.4f;

            _mass = 1.0f;
            _cr = 0.0f; // elastic impact
            Collision = new X_CollisionModel_Victim(_mass, _cr);

            // Character related
            _isAiming = false;
            _aimDirection = Vector2.Zero;
            _invincible = false;
            _invincibleDuration = 1250;

            LifePoints = 30;

            Room = Level.GetRoom(this, Room);
        }

        public X_LevelElements WhatAreYou()
        {
            if (_invincible)
            {
                return X_LevelElements.Invincible;
            }
            else if (IsAlive())
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

        /* Deal with being hit by projectile, basically physical therapy */
        public void Hit(IProjectile projectile)
        {
            if (_invincible) { return; }

            LifePoints -= projectile.Damage;
            _invincible = true;
            _invincibleTimeLeft = _invincibleDuration;
        }

        protected void UpdateInvincibility(GameTime gameTime)
        {
            if (_invincible)
            {
                _invincibleTimeLeft -= gameTime.ElapsedGameTime.Milliseconds;
                if (_invincibleTimeLeft < 0)
                {
                    _invincible = false;
                    _color = Color.White;
                }
                else
                {
                    _color = Color.DimGray * (float)((Math.Sin(_invincibleTimeLeft / 50) + 1) / 2);
                }
            }
        }

        protected void UpdateAnimation(GameTime gameTime)
        {
            string newAnimationDirection = _animationDirection;
            if (Velocity.X > 0 && _animations.ContainsKey("right"))
            {
                newAnimationDirection = "right";
            }
            else if (Velocity.X < 0 && _animations.ContainsKey("left"))
            {
                newAnimationDirection = "left";
            }
            else if (Velocity.Y > 0 && _animations.ContainsKey("up"))
            {
                newAnimationDirection = "up";
            }
            else if (Velocity.Y < 0 && _animations.ContainsKey("down"))
            {
                newAnimationDirection = "down";
            }
            else if (_animations.ContainsKey("idle"))
            {
                newAnimationDirection = "idle";
            }

            if (newAnimationDirection == _animationDirection)
            {
                _animationTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (_animationTimer > _animationTreshold)
                {
                    _animationTimer = 0;
                    _animationIndex = (_animationIndex + 1) % _animations[_animationDirection].Length;
                }
            }
            else
            {
                _animationTimer = 0;
                _animationIndex = 0;
                _animationDirection = newAnimationDirection;
            }
        }

        /* Handle GamePad movement, aiming and shooting */
        protected void HandleGamepadInput(GameTime gameTime, ref Vector2 input)
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
                if ((Input.IsButtonDown(_playerIndex, Keybinds.GamePadShoot)) && IsAlive() && !_invincible)
                {
                    _isAiming = true; // Show the aim indicator when firing
                    _currentAimInput = InputType.Controller;
                    _gun.Shoot(gameTime, Rect.Center.ToVector2(), _aimDirection, Level, this);
                }
            }
        }

        /* Handle Keyboard & Mouse movement, aiming and shooting */
        protected void HandleMouseKeyboardInput(GameTime gameTime, ref Vector2 input)
        {
            if (ControlLayout > 0)
            {
                if (ControlLayout == ControlLayout.KeyboardWASD)
                {
                    if (Input.IsKeyDown(Keybinds.P1Right)) input.X += 1;
                    if (Input.IsKeyDown(Keybinds.P1Left)) input.X -= 1;
                    if (Input.IsKeyDown(Keybinds.P1Down)) input.Y += 1;
                    if (Input.IsKeyDown(Keybinds.P1Up)) input.Y -= 1;
                }
                else if (ControlLayout == ControlLayout.KeyboardArrows)
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
                if (mouse.LeftButton == ButtonState.Pressed && IsAlive() && !_invincible)
                {
                    _gun.Shoot(gameTime, playerCenter, _aimDirection, Level, this);
                }

                if (Input.HasMouseStateChanged())
                {
                    _currentAimInput = InputType.KeyboardMouse;
                }
            }
        }

        protected virtual void UpdateVelocity(Vector2 input, GameTime gameTime)
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

        protected virtual void UpdateCollision(GameTime gameTime)
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
            UpdateInvincibility(gameTime);
            Vector2 input = Vector2.Zero;
            HandleGamepadInput(gameTime, ref input);
            HandleMouseKeyboardInput(gameTime, ref input);
            UpdateVelocity(input, gameTime);
            UpdateCollision(gameTime);
            UpdateAnimation(gameTime);
            _gun.Update(gameTime);

        }

        // Render ghosty 👻
        protected virtual void DrawGhost(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            int width = _spriteGhost.Width / 4;
            int height = _spriteGhost.Height;
            // TODO: use the proper animation framework to pingpong through the frames
            int totalMS = gameTime.TotalGameTime.Milliseconds / 200;
            int step = totalMS % 6;
            int animationIndex = 3 - Math.Abs(3 - step);
            spriteBatch.Draw(
                texture: _spriteGhost,
                position: _rect.Location.ToVector2(),
                sourceRectangle: new Rectangle(width * animationIndex, 0, width, height),
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: (float)Rect.Width / width,
                effects: Velocity.X >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                layerDepth: 0);
        }

        protected virtual void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: _spritePlayer,
                position: _rect.Location.ToVector2(),
                sourceRectangle: new Rectangle(_animations[_animationDirection][_animationIndex] * (_spriteDimensions.Width), 0, _spriteDimensions.Width, _spriteDimensions.Height),
                color: _color,
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
                    _spriteAimIndicator, _rect.Location.ToVector2() + _spriteDimensions.Center.ToVector2() + _aimDirection * (int)(_rect.Height * 1.5),
                    null,
                    Color.White, (float)angle, new Vector2(_spriteAimIndicator.Width / 2, 0), 0.1f, SpriteEffects.None, 0);
            }
        }

        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            if (IsAlive())
            {
                DrawCharacterSprite(gameTime, globalOffset, spriteBatch);
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
            LifePoints = 20;

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

            if (!_isDashing && (Input.IsKeyDown(Keybinds.ActionOne) || Input.IsButtonDown(_playerIndex, Keybinds.GamePadAction)) && _dashCooldownTimer >= _dashCooldown)
            {
                Manager_Sound.Sound_Dash.Play();
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

            UpdateInvincibility(gameTime);
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

        override protected void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(
                    _spritePlayer,
                    new Rectangle(
                        _rect.X, _rect.Y, _rect.Width, _rect.Height),
                        sourceRectangles[currentAnimationIndex], _color);
        }
    }
}
