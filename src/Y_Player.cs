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
        public int LifePointsMax { get; set; }
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
        protected Vector2 CharacterSpriteDimension;
        protected Vector2 GhostSpriteDimension;
        protected AnimatedSprite GhostSprite;
        protected AnimatedSprite CharacterSprite;
        protected float CharacterScale;
        protected float GhostScale;
        protected Texture2D _spriteAimIndicator;
        protected float _acceleration;
        protected Vector2 _aimDirection;
        protected float _deceleration;
        protected float _maxVelocity;
        protected Vector2 _position;
        protected ParticleEffect pE;
        protected Color _color;

        protected float _actionTimer;
        protected float _actionTreshold;

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
            Scale = scale;

            // Set up sprites
            _spriteAimIndicator = Manager_Sprites.AimIndicator[(int)playerIndex];
            _color = Color.White;

            _rect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                40, 60
            );

            CharacterSpriteDimension = new Vector2(44, 62);
            CharacterScale = Scale * Util.GetSpriteScale(_rect, CharacterSpriteDimension);
            CharacterSprite = new AnimatedSprite(
                texture: Manager_Sprites.Player_Simple,
                spriteDimension: CharacterSpriteDimension,
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.WalkLeft, new int[] { 1, 2 } },
                    { AnimationState.IdleLeft, new int[] { 1 } },
                    { AnimationState.WalkRight, new int[] { 3, 4 } },
                    { AnimationState.IdleRight, new int[] { 3 } },
                }
            );

            GhostSpriteDimension = new Vector2(Manager_Sprites.Player_Ghost.Width / 8, Manager_Sprites.Player_Ghost.Height);
            GhostScale = Scale * _rect.Width / GhostSpriteDimension.X; // Ghost will be slightly higher than players, due to floating and shadows
            GhostSprite = new AnimatedSprite(
                texture: Manager_Sprites.Player_Ghost,
                spriteDimension: GhostSpriteDimension,
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.WalkRight, new int[] { 0, 1, 2, 3, 2, 1 } },
                    { AnimationState.IdleRight, new int[] { 0, 1, 2, 3, 2, 1 } },
                    { AnimationState.WalkLeft, new int[] { 7, 6, 5, 4, 5, 6 } },
                    { AnimationState.IdleLeft, new int[] { 7, 6, 5, 4, 5, 6 } },
                }
            );


            // Movement related
            Velocity = Vector2.Zero;
            _acceleration = 0.008f;
            _deceleration = 0.004f;
            _maxVelocity = 0.35f;

            _mass = 1.0f;
            _cr = 0.0f; // elastic impact
            Collision = new X_CollisionModel_Victim(_mass, _cr);

            // Character related
            _isAiming = false;
            _aimDirection = new Vector2(1, 0);
            _invincible = false;
            _invincibleDuration = 1250;

            LifePointsMax = 30;
            LifePoints = LifePointsMax;

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

        public void Revive()
        {
            LifePoints = LifePointsMax;
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
                if (Room.WhatAreYou() == X_LevelElements.Room)
                {
                    ((Y_CMRoom)Room).SuppliedRoomFunctions();

                }
                Level.SuppliedRoomFunctions();
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
                if ((Input.HasMouseMoved() || Input.IsLeftMousePressed()) && !_isAiming) // Skip if controller is already aiming
                {
                    Vector2 mouseInGamePosition = Input.GetMousePosition().ToVector2() / Camera.Zoom + Camera.VisibleArea.Location.ToVector2();
                    Vector2 newAimDirection = mouseInGamePosition - playerCenter;
                    newAimDirection.Normalize();
                    _aimDirection = newAimDirection;
                }
                if (Input.IsLeftMousePressed() && IsAlive() && !_invincible)
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
                    Math.Sign(Velocity.X) * Math.Max(0.0f, Math.Abs(Velocity.X) - _deceleration * timeStepMS),
                    Math.Sign(Velocity.Y) * Math.Max(0.0f, Math.Abs(Velocity.Y) - _deceleration * timeStepMS));
            }

            Velocity = Util.ClampMagnitude(Velocity, _maxVelocity);
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

            if (IsAlive()) { CharacterSprite.Update(gameTime, input); }
            else { GhostSprite.Update(gameTime, input); }

            UpdateVelocity(input, gameTime);
            UpdateCollision(gameTime);
            _gun.Update(gameTime);

        }

        // Render ghosty 👻
        protected virtual void DrawGhost(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Vector2 ghostOffset = new Vector2(0, -GhostSpriteDimension.Y * GhostScale + _rect.Height);
            spriteBatch.Draw(
                texture: GhostSprite.Texture,
                position: _rect.Location.ToVector2() + ghostOffset,
                sourceRectangle: GhostSprite.SourceRectangle,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: GhostScale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        protected virtual void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: CharacterSprite.Texture,
                position: _rect.Location.ToVector2(),
                sourceRectangle: CharacterSprite.SourceRectangle,
                color: _color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: CharacterScale,
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
                    _spriteAimIndicator, _rect.Location.ToVector2() + CharacterSpriteDimension / 2f + _aimDirection * (int)(_rect.Height * 1.5),
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
            LifePointsMax = 20;
            LifePoints = LifePointsMax;

            /* Class specifics */
            _isDashing = false;
            _dashDuration = 100; // Dash duration in ms
            _dashSpeed = 3f; // Dash speed multiplier
            _dashTimer = 0;
            _dashCooldown = 1000; // Dash cooldown in ms
            _dashCooldownTimer = _dashCooldown;

            CharacterSpriteDimension = new Vector2(48, 64);
            CharacterScale = Scale * Util.GetSpriteScale(_rect, CharacterSpriteDimension);
            CharacterSprite = new AnimatedSprite(
                texture: Manager_Sprites.Player_Ninja,
                spriteDimension: CharacterSpriteDimension,
                animationSourceRects: new Dictionary<AnimationState, Rectangle[]> {
                    {
                        AnimationState.WalkDown, new Rectangle[]
                        {
                            new Rectangle(0, 128, 48, 64),
                            new Rectangle(48, 128, 48, 64),
                            new Rectangle(96, 128, 48, 64)
                        }
                    },
                    {
                        AnimationState.WalkUp, new Rectangle[]
                        {
                            new Rectangle(0, 0, 48, 64),
                            new Rectangle(48, 0, 48, 64),
                            new Rectangle(96, 0, 48, 64)
                        }
                    },
                    {
                        AnimationState.WalkRight, new Rectangle[]
                        {
                            new Rectangle(0, 64, 48, 64),
                            new Rectangle(48, 64, 48, 64),
                            new Rectangle(96, 64, 48, 64)
                        }
                    },
                    {
                        AnimationState.WalkLeft, new Rectangle[]
                        {
                            new Rectangle(0, 192, 48, 64),
                            new Rectangle(48, 192, 48, 64),
                            new Rectangle(96, 192, 48, 64)
                        }
                    },
                        {
                        AnimationState.Idle, new Rectangle[]
                        {
                            new Rectangle(0, 128, 48, 64),
                            new Rectangle(0, 128, 48, 64),
                            new Rectangle(0, 128, 48, 64)
                        }
                    }
                },
                animationDuration: 750
            );
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

            if (!_isDashing && (ControlLayout != ControlLayout.ControllerOnly && Input.IsKeyDown(Keybinds.ActionOne) || Input.IsButtonDown(_playerIndex, Keybinds.GamePadAction)) && _dashCooldownTimer >= _dashCooldown)
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

            if (IsAlive())
            { CharacterSprite.Update(gameTime, input); }
            else { GhostSprite.Update(gameTime, input); }

        }

        override protected void DrawCharacterSprite(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: Manager_Sprites.Player_Ninja,
                position: _rect.Location.ToVector2(),
                sourceRectangle: CharacterSprite.SourceRectangle,
                color: _color,
                rotation: 0,
                origin: Vector2.Zero,
                scale: CharacterScale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }
    }
}
