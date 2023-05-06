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
        public Color Color { get; set; }
        public X_CollisionModel_Victim Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }

        // Class fields
        public ControlLayout ControlLayout { get; set; }
        public IShooter Gun { get; set; }
        public PlayerIndex PlayerIndex { get; }
        protected Rectangle _rect;
        protected AnimatedSprite GhostSprite;
        protected AnimatedSprite CharacterSprite;
        protected Vector2 CharacterOffset;
        protected Vector2 GhostOffset;
        protected float CharacterScale;
        protected float GhostScale;
        protected Color _characterColor;
        protected Color _ghostColor;
        protected Texture2D _spriteAimIndicator;
        protected float _acceleration;
        protected Vector2 _aimDirection;
        protected float _deceleration;
        protected float _maxVelocity;
        protected Vector2 Position;
        protected ParticleEffect pE;
        protected bool _isAiming;
        protected bool _invincible;
        protected float _invincibleDuration;
        protected float _invincibleTimer;
        protected float _cr;
        protected float _mass;
        protected InputType _currentAimInput;
        protected float _actionTimer;
        protected float _actionTreshold;

        // Dash
        protected bool _dashing;
        protected int _dashDuration;
        protected float _dashSpeed;
        protected int _dashTimer;
        protected int _dashCooldown;
        protected int _dashCooldownTimer;

        protected enum InputType
        {
            Controller = 0,
            KeyboardMouse,
        }

        public SimplePlayer(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            AnimatedSprite sprite,
            Y_Level level,
            IShooter gun,
            ControlLayout controlLayout = ControlLayout.ControllerOnly,
            float scale = 1.0f
            )
        {
            // Use all constructor arguments
            Level = level;
            PlayerIndex = playerIndex;
            Position = initialPosition;
            CharacterSprite = sprite;
            Gun = gun;
            ControlLayout = controlLayout;
            Scale = scale;

            // Balancing knobs
            LifePointsMax = 15;
            LifePoints = LifePointsMax;
            _invincibleDuration = 1250;

            // Colors
            // `Color` is the color of the indicators and healthbar, indicated by controller index
            _spriteAimIndicator = Manager_Sprites.AimIndicator;
            switch ((int)playerIndex)
            {
                case 0:
                    Color = Color.Red;
                    break;
                case 1:
                    Color = Color.Blue;
                    break;
                case 2:
                    Color = Color.Green;
                    break;
                case 3:
                    Color = Color.Yellow;
                    break;
                default:
                    Color = Color.White;
                    break;
            }
            // Character color is the color of the character sprite, currently neutral white
            _characterColor = Color.White;
            _ghostColor = Color.Lerp(Color.White, Color, 0.5f);

            // Collision bounds
            int height = 60;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);
            _rect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Scale * Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero; // Not needed right now

            // Set up animated sprite for the ghost
            // It will be slightly higher than players due to floating and shadows.
            GhostSprite = Manager_Sprites.NewAnimatedSprite_Ghost();
            GhostScale = Scale * _rect.Width / GhostSprite.SpriteDimension.X;
            // Make the ghost peak out of the collision bounds at the top instead of bottom
            GhostOffset = _rect.Size.ToVector2() - GhostSprite.SpriteDimension * GhostScale;

            // Movement related
            Velocity = Vector2.Zero;
            _acceleration = 0.008f;
            _deceleration = 0.004f;
            _maxVelocity = 0.35f;

            // Dash
            _dashing = false;
            _dashDuration = 200; // Dash duration in ms
            _dashSpeed = 4f; // Dash speed multiplier
            _dashTimer = _dashDuration;
            _dashCooldown = 500 - _dashDuration; // Dash cooldown in ms
            _dashCooldownTimer = _dashCooldown;

            _mass = 1.0f;
            _cr = 0.0f; // elastic impact
            Collision = new X_CollisionModel_Victim(_mass, _cr);

            // Character state
            _isAiming = false;
            _aimDirection = new Vector2(1, 0);
            _invincible = false;

            Room = Level.GetRoom(this, Room);
        }

        public X_LevelElements WhatAreYou()
        {
            if (_invincible || _dashing)
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

        public void Heal(int healAmount = 999)
        {
            // Don't heal a dead player
            if (!IsAlive()) { return; }

            LifePoints = Math.Min(LifePoints + healAmount, LifePointsMax);
        }

        public void Revive(int healAmount = 999)
        {
            LifePoints = LifePointsMax;
        }

        /* Deal with being hit by projectile, basically physical therapy */
        public void Hit(IProjectile projectile)
        {
            if (_invincible) { return; }

            LifePoints -= projectile.Damage;
            _invincible = true;
            _invincibleTimer = 0;
        }

        protected void UpdateRoom(GameTime gameTime)
        {
            if (Room.WhatAreYou() == X_LevelElements.Room)
            {
                ((Y_CMRoom)Room).SetVisited(true);
            }
        }

        protected void UpdateInvincibility(GameTime gameTime)
        {
            if (_invincible)
            {
                if (_invincibleTimer > _invincibleDuration)
                {
                    _invincible = false;
                }
                else
                {
                    _invincibleTimer += gameTime.ElapsedGameTime.Milliseconds;
                }
            }
        }

        protected void UpdateColor(GameTime gameTime)
        {
            if (_invincible)
            {
                // Blinking while invincible
                _characterColor = Color.DimGray * (float)((Math.Sin(_invincibleTimer / 50) + 1) / 2);
            }
            else if (_dashing)
            {
                // Transient invisibility while dashing
                _characterColor = Color.White * (_dashTimer / (float)_dashDuration);
            }
            else
            {
                _characterColor = Color.White;
            }
        }

        protected virtual void UpdateDash(GameTime gameTime)
        {
            if (_dashing)
            {
                if (_dashTimer > _dashDuration)
                {
                    _dashing = false;
                }
                else
                {
                    float velocityMultiplier = 1f + (float)Math.Sin(Math.PI * (_dashTimer / (float)_dashDuration) / 2 + Math.PI / 2) * _dashSpeed;
                    Velocity *= velocityMultiplier;
                    _dashTimer += gameTime.ElapsedGameTime.Milliseconds;
                }
            }
            else
            {
                // Wait for cooldown
                if (_dashCooldownTimer < _dashCooldown)
                {
                    _dashCooldownTimer += gameTime.ElapsedGameTime.Milliseconds;
                    return;
                }

                // Ghosts don't need to dash
                if (!IsAlive())
                {
                    return;
                }

                // Don't dash if we're not moving
                if (Velocity == Vector2.Zero)
                {
                    return;
                }

                if (Input.IsKeyDown(Keybinds.ActionOne) || Input.IsButtonDown(PlayerIndex, Keybinds.GamePadAction))
                {
                    Manager_Sound.Sound_Dash.Play();
                    _dashing = true;
                    _dashTimer = 0;
                    _dashCooldownTimer = 0; // Reset timer
                }
            }
        }

        /* Handle GamePad movement, aiming and shooting */
        protected void HandleGamepadInput(GameTime gameTime, ref Vector2 input)
        {
            GamePadState gpState = GamePad.GetState(PlayerIndex);
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
                if ((Input.IsButtonDown(PlayerIndex, Keybinds.GamePadShoot)) && IsAlive())
                {
                    _isAiming = true; // Show the aim indicator when firing
                    _currentAimInput = InputType.Controller;
                    Gun.Shoot(gameTime, Rect.Center.ToVector2(), _aimDirection, Level, this);
                }
            }
        }

        /* Handle Keyboard & Mouse movement, aiming and shooting */
        protected void HandleMouseKeyboardInput(GameTime gameTime, ref Vector2 input)
        {
            if (ControlLayout > 0)
            {
                if (Room == null) return;
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
                if (Input.IsLeftMousePressed() && IsAlive())
                {
                    Gun.Shoot(gameTime, playerCenter, _aimDirection, Level, this);
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

            Position += Velocity * timeStepMS;
            _rect.Location = Position.ToPoint();
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateRoom(gameTime);
            Manager_Particles.Update(gameTime);
            UpdateInvincibility(gameTime);
            Vector2 input = Vector2.Zero;
            HandleGamepadInput(gameTime, ref input);
            HandleMouseKeyboardInput(gameTime, ref input);

            if (IsAlive()) { CharacterSprite.Update(gameTime, input); }
            else { GhostSprite.Update(gameTime, input); }

            UpdateVelocity(input, gameTime);
            UpdateDash(gameTime);
            UpdateCollision(gameTime);

            UpdateColor(gameTime);
            Gun.Update(gameTime);
        }

        // Render ghosty 👻
        protected virtual void DrawGhost(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: GhostSprite.Texture,
                position: _rect.Location.ToVector2() + GhostOffset,
                sourceRectangle: GhostSprite.SourceRectangle,
                color: _ghostColor,
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
                position: _rect.Location.ToVector2() + CharacterOffset,
                sourceRectangle: CharacterSprite.SourceRectangle,
                color: _characterColor,
                rotation: 0,
                origin: Vector2.Zero,
                scale: CharacterScale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        protected virtual void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            string str = "P" + (int)PlayerIndex + ": " + LifePoints.ToString();
            float str_width = Fonts.Normal.MeasureString(str).X;
            spriteBatch.DrawString(Fonts.Normal, str, new Vector2(_rect.Location.X + _rect.Width / 2 - str_width / 2, _rect.Location.Y - 16), Color.Wheat);
        }

        protected virtual void DrawHealthbar(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Vector2 dim = Manager_Sprites.HealthbarEmpty.Bounds.Size.ToVector2();
            float scale = Rect.Height / dim.X;
            dim *= scale;
            Vector2 offset = new Vector2((Rect.Width - dim.X) / 2, -dim.Y - 5);
            Vector2 pos = _rect.Location.ToVector2() + offset;
            spriteBatch.Draw(
                texture: Manager_Sprites.HealthbarEmpty,
                position: pos,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0);

            // Fill the healthbar
            if (LifePoints > 0)
            {
                float healthPerc = LifePoints / (float)LifePointsMax;
                Rectangle infill = Manager_Sprites.HealthbarInfill.Bounds;
                infill.Width = (int)(infill.Width * healthPerc);
                spriteBatch.Draw(
                    texture: Manager_Sprites.HealthbarInfill,
                    position: pos,
                    sourceRectangle: infill,
                    color: Color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }

        protected virtual void DrawAimIndicator(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw an indicator only if a) the player is actively aiming on the gamepad or b) is using mouse to aim
            if (_currentAimInput == InputType.Controller && _isAiming || _currentAimInput == InputType.KeyboardMouse)
            {
                var angle = Math.Atan2(_aimDirection.Y, _aimDirection.X) + Math.PI / 2;
                spriteBatch.Draw(
                    _spriteAimIndicator, _rect.Location.ToVector2() + _rect.Size.ToVector2() / 2f + _aimDirection * (int)(_rect.Height * 1.5),
                    null,
                    Color, (float)angle, new Vector2(_spriteAimIndicator.Width / 2, 0), 0.1f, SpriteEffects.None, 0);
            }
        }

        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (IsAlive())
            {
                DrawCharacterSprite(gameTime, globalOffset, spriteBatch);
                DrawAimIndicator(gameTime, globalOffset, spriteBatch);
                DrawHealthbar(gameTime, globalOffset, spriteBatch);
                // DrawOverheadString(gameTime, globalOffset, spriteBatch);
            }
            else
            {
                DrawGhost(gameTime, globalOffset, spriteBatch);
            }
        }

        public virtual void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }
    }
}
