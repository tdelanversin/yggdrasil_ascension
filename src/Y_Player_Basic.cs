using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Particles;
using System;
using System.Collections.Generic;
using System.Timers;

namespace YGR
{
    public enum ControlLayout
    {
        ControllerOnly = 0,
        KeyboardWASD,
        KeyboardArrows,
    }

    public class Player_Basic : IPlayer
    {
        // IGameElement fields
        public float LocalScale { get; protected set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }
        public IGameElement WhoKilledMe { get; set; }

        // IVictim fields
        public float LifePoints { get; protected set; }
        public float LifePointsMax { get; set; }
        public Color Color { get; set; }
        public X_CollisionModel_Victim Collision { get; protected set; }
        public Vector2 Velocity { get; set; }
        public Vector2 ImpactVelocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public string Name { get; set; }
        public int ElementLevel { get; set; }
        public bool Confused { get; set; }

        // IPLayer fields
        public ControlLayout ControlLayout { get; set; }
        public IShooter Gun { get; set; }
        public IAbility Ability { get; set; }

        public PlayerIndex PlayerIndex { get; protected set; }
        public bool IsActive { get; protected set; }
        public bool IsInvincible { get; protected set; }
        public bool IsDashing { get; protected set; }
        public PlayerType Type { get; protected set; }
        public Statistics Stats { get; set; }

        // Class fields
        public float VelocityMax;
        protected Rectangle _rect;
        protected AnimatedSprite GhostSprite;
        protected AnimatedSprite CharacterSprite;
        protected Vector2 CharacterOffset;
        protected Vector2 GhostOffset;
        protected float CharacterDrawScale;
        protected float GhostDrawScale;
        protected Color _characterColor;
        protected Color _ghostColor;
        protected Texture2D _spriteAimIndicator;
        protected float _acceleration;
        public Vector2 AimDirection;
        protected float _deceleration;
        protected float _impactDeceleration;
        protected Vector2 Position;
        protected ParticleEffect pE;
        protected bool _isAiming;
        protected float _invincibleDuration;
        protected float _invincibleTimer;
        protected float _cr;
        protected float _mass;
        public InputType CurrentAimInput;
        protected float _actionTimer;
        protected float _actionTreshold;

        // Dash
        protected int _dashDuration;
        protected float _dashSpeed;
        protected int _dashTimer;
        protected int _dashCooldown;
        protected int _dashCooldownTimer;

        public enum InputType
        {
            Controller = 0,
            KeyboardMouse,
        }

        public Player_Basic(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            PlayerType type,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
            )
        {
            // Use constructor arguments
            Level = level;
            PlayerIndex = playerIndex;
            Position = initialPosition;
            ElementLevel = 1;

            // Set Name
            Name = "Basic Dude";

            // Yes, bring him back <3
            CharacterSprite = Manager_Sprites.NewAnimatedSprite_TestCharacter();

            // He deserves the best weapon in the game
            Gun = new Weapon_PinkHammer(this);
            Ability = new Ability_Ghost();
            Type = type;
            ControlLayout = controlLayout;
            LocalScale = Y_Level.GlobalScale;

            // Balancing knobs
            LifePointsMax = IPlayer.PlayerBaseHealth;
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
                    // Keep the last player keyboard/mouse controllable, that
                    // way one extra person can participate if players don't
                    // have four controllers.
                    ControlLayout = ControlLayout.KeyboardWASD;
                    break;
                default:
                    Color = Color.White;
                    break;
            }
            // Character color is the color of the character sprite, currently neutral white
            _characterColor = Color.White;
            _ghostColor = Color.Lerp(Color.White, Color, 0.5f);

            SetupPlayerRect(height: IPlayer.PlayerBaseHeight);

            // Set up animated sprite for the ghost
            // It will be slightly higher than players due to floating and shadows.
            GhostSprite = Manager_Sprites.NewAnimatedSprite_Ghost();
            GhostDrawScale = LocalScale * _rect.Width / GhostSprite.SpriteDimension.X;
            // Make the ghost peak out of the collision bounds at the top instead of bottom
            GhostOffset = _rect.Size.ToVector2() - GhostSprite.SpriteDimension * GhostDrawScale;

            // can be confused, but normally isn't
            Confused = false;

            // Movement related
            Velocity = Vector2.Zero;
            ImpactVelocity = Vector2.Zero;
            VelocityMax = IPlayer.PlayerBaseVelocity;
            _acceleration = IPlayer.PlayerBaseAcceleration;
            _deceleration = IPlayer.PlayerBaseDeceleration;
            _impactDeceleration = IPlayer.PlayerBaseImpactDeceleration;

            // Dash
            IsDashing = false;
            _dashDuration = 200; // Dash duration in ms
            _dashSpeed = 4f; // Dash speed multiplier
            _dashTimer = _dashDuration;
            _dashCooldown = 500 - _dashDuration; // Dash cooldown in ms
            _dashCooldownTimer = _dashCooldown;

            _mass = IPlayer.PlayerBaseMass;
            _cr = 0.0f; // elastic impact
            Collision = new X_CollisionModel_Victim(_mass, _cr);

            // Character state
            _isAiming = false;
            AimDirection = new Vector2(1, 0);
            IsInvincible = false;
            IsActive = true;
            Stats = new Statistics();

            Room = Level.GetRoom(this, Room);
        }

        public virtual bool LevelUp()
        {
            if (ElementLevel == 3) return false;

            ElementLevel = ElementLevel + 1;

            LifePointsMax += 15;
            LifePoints += 15;

            return true;
        }

        public virtual void TeleportTo(Point target)
        {
            _rect.Location = target;
            Position = target.ToVector2();
        }

        public virtual X_LevelElements WhatAreYou()
        {
            if (IsInvincible || IsDashing)
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

        /// <summary>
        /// Set the player Rect and the CharacterScale while taking into account LocalScale
        /// </summary>
        protected virtual void SetupPlayerRect(float height)
        {
            height *= LocalScale;
            float width = height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X;
            _rect = new Rectangle(
                (int)Position.X,
                (int)Position.Y,
                (int)width,
                (int)height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterDrawScale = LocalScale * Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
        }

        public virtual bool IsAlive()
        {
            return LifePoints > 0;
        }


        // Private heal method, this does NOT check if player is alive
        protected virtual void heal(float healAmount)
        {
            if (LifePointsMax - LifePoints < healAmount)
            {
                healAmount = LifePointsMax - LifePoints;
            }
            LifePoints += healAmount;
            Stats.AmountHealed += healAmount;
        }

        public virtual void Heal()
        {
            if (!IsAlive()) { return; } // Don't heal a dead player
            heal(LifePointsMax);
        }

        public virtual void Heal(float healAmount)
        {
            if (!IsAlive()) { return; } // Don't heal a dead player
            heal(healAmount);
        }

        public virtual void Revive()
        {
            if (IsAlive()) { return; }
            LifePoints = LifePointsMax;
            Stats.Revives++;
            heal(LifePointsMax / 2);
        }

        public virtual void Revive(float healAmount)
        {
            if (IsAlive()) { return; }
            Stats.Revives++;
            heal(healAmount);
        }

        public virtual void Godmode()
        {
            LifePoints = LifePointsMax = 999;
            Gun = new Gun_Godmode(this);
            VelocityMax = 0.6f;
        }

        /* Deal with being hit by projectile, basically physical therapy */
        public virtual void Hit(IProjectile projectile)
        {
            if (IsInvincible || !IsAlive()) { return; }

            LifePoints -= projectile.Damage;
            IsInvincible = true;
            _invincibleTimer = 0;

            // @statistics
            Stats.DamageTaken += projectile.Damage;
            if (projectile.WhoFiredMe is IEnemyBoss) { Stats.BossDamageTaken += projectile.Damage; }
            if (LifePoints <= 0)
            {
                Manager_Sound.Sound_PlayerDeath.Play();
                Stats.Deaths++;
                LifePoints = 0;
                // drop a grave stone as a pickup
                // if (Room.WhatAreYou() == X_LevelElements.Room)
                // {
                //     var p = new Point(Rect.Location.X + Rect.Width / 2, Rect.Location.Y + Rect.Height / 2);
                //     ((Y_CMRoom)Room).PickUps.Add(PickUp.Factory(Y_PowerUps.Gravestone, p, Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale));
                // }
                // Drop a grave stone as an enemy
                // Manager_Enemies.AddGraveStone(Rect.Center.ToVector2(), Level);
            }
        }

        protected virtual void UpdateRoom(GameTime gameTime)
        {
            if (Room.WhatAreYou() == X_LevelElements.Room)
            {
                ((Y_CMRoom)Room).SetVisited(true);
            }
        }

        protected virtual void UpdateInvincibility(GameTime gameTime)
        {
            if (IsInvincible)
            {
                if (_invincibleTimer > _invincibleDuration)
                {
                    IsInvincible = false;
                }
                else
                {
                    _invincibleTimer += gameTime.ElapsedGameTime.Milliseconds;
                }
            }
        }

        public virtual AnimatedSprite GetSprite()
        {
            if (IsAlive())
                return CharacterSprite;
            else
                return GhostSprite;
        }

        protected virtual void UpdateColor(GameTime gameTime)
        {
            if (IsInvincible)
            {
                // Blinking while invincible
                _characterColor = Color.DimGray * (float)((Math.Sin(_invincibleTimer / 50) + 1) / 2);
            }
            else if (IsDashing)
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
            if (IsDashing)
            {
                Manager_Particles.GetParticleEffect(Manager_Particles.Effect.Dash).Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));
                //Manager_Particles.GenParticleEffectDash(new Vector2(_rect.Location.X+_rect.Width/2, _rect.Location.Y+_rect.Height));
                if (_dashTimer > _dashDuration)
                {
                    IsDashing = false;
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

                if ((ControlLayout != ControlLayout.ControllerOnly && Input.IsKeyDown(Keybinds.ActionOne)) || Input.IsButtonDown(PlayerIndex, Keybinds.GamePadAction))
                {
                    Manager_Sound.Sound_Dash.Play();
                    IsDashing = true;
                    _dashTimer = 0;
                    _dashCooldownTimer = 0; // Reset timer

                    // @statistics
                    Stats.TimesDashed++;
                }
            }
        }

        /* Handle GamePad movement, aiming and shooting */
        protected virtual void HandleGamepadInput(GameTime gameTime, ref Vector2 input)
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
                    AimDirection = newAimDirection;
                    _isAiming = true;
                    CurrentAimInput = InputType.Controller;
                }
                else
                {
                    _isAiming = false;
                }
                if ((Input.IsButtonDown(PlayerIndex, Keybinds.GamePadShoot)) && IsAlive())
                {
                    _isAiming = true; // Show the aim indicator when firing
                    CurrentAimInput = InputType.Controller;
                    bool shot = Gun.Shoot(gameTime, Rect.Center.ToVector2(), AimDirection, Level, this);
                    if (shot) { Stats.TimesFired++; }
                }

                if (Input.IsButtonDown(PlayerIndex, Keybinds.GamePadAbility) && IsAlive())
                {
                    _isAiming = true; // Show the aim indicator when firing
                    CurrentAimInput = InputType.Controller;
                    bool triggered = Ability.Trigger(gameTime, Rect.Center.ToVector2(), AimDirection, Level, this);
                    if (triggered) { Stats.TimesAbilitated++; }
                }
            }
        }

        /* Handle Keyboard & Mouse movement, aiming and shooting */
        protected virtual void HandleMouseKeyboardInput(GameTime gameTime, ref Vector2 input)
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
                    Vector2 newAimDirection = Input.GetMousePositionInGame() - playerCenter;
                    newAimDirection.Normalize();
                    AimDirection = newAimDirection;
                }
                if (Input.IsLeftMousePressed() && IsAlive())
                {
                    bool shot = Gun.Shoot(gameTime, playerCenter, AimDirection, Level, this);
                    if (shot) { Stats.TimesFired++; }
                }
                if (Input.IsRightMousePressed() && IsAlive())
                {
                    bool triggered = Ability.Trigger(gameTime, playerCenter, AimDirection, Level, this);
                    if (triggered) { Stats.TimesAbilitated++; }
                }

                if (Input.HasMouseStateChanged())
                {
                    CurrentAimInput = InputType.KeyboardMouse;
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
                if (Settings.ParticleEffects)
                {
                    //Manager_Particles.GenParticleEffectDustCloudLight(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));
                    Manager_Particles.GetParticleEffect(Manager_Particles.Effect.DustCloudLight).Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height));
                }

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

            if (!handleImpact(timeStepMS)) Velocity = Util.ClampMagnitude(Velocity, VelocityMax);
        }

        private bool handleImpact(int timeStepMS)
        {
            if (ImpactVelocity.X > 0 || ImpactVelocity.Y > 0)
            {
                Logger.Info("lol A:" + ImpactVelocity.ToString());
                Velocity = ImpactVelocity;

                Velocity = Util.ClampMagnitude(Velocity, MathHelper.Max(Math.Abs(ImpactVelocity.X), Math.Abs(ImpactVelocity.Y)));

                ImpactVelocity = new Vector2(
                    Math.Sign(ImpactVelocity.X) * Math.Max(0.0f, Math.Abs(ImpactVelocity.X) - _impactDeceleration * timeStepMS),
                    Math.Sign(ImpactVelocity.Y) * Math.Max(0.0f, Math.Abs(ImpactVelocity.Y) - _impactDeceleration * timeStepMS));

                return true;
            }
            ImpactVelocity = Vector2.Zero;
            return false;
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
            Stats.DistanceTravelled += Velocity.Length() * timeStepMS;
            _rect.Location = Position.ToPoint();
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateRoom(gameTime);
            UpdateInvincibility(gameTime);
            Vector2 input = Vector2.Zero;
            HandleGamepadInput(gameTime, ref input);
            HandleMouseKeyboardInput(gameTime, ref input);

            if (IsAlive()) { CharacterSprite.Update(gameTime, input); }
            else { GhostSprite.Update(gameTime, input); }

            if (Y_Level.State == Y_Level.GamePlayState.Tutorial)
            {
                /* During the tutorial we don't want the players to do anything yet */
                return;
            }

            UpdateVelocity(input, gameTime);
            UpdateDash(gameTime);
            UpdateCollision(gameTime);

            UpdateColor(gameTime);
            Gun.Update(gameTime);
            Ability.Update(gameTime);
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
                scale: GhostDrawScale,
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
                scale: CharacterDrawScale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        protected virtual void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            string str = "P" + (int)PlayerIndex + ": " + LifePoints.ToString();
            float str_width = Fonts.Small.MeasureString(str).X;
            spriteBatch.DrawString(Fonts.Small, str, new Vector2(_rect.Location.X + _rect.Width / 2 - str_width / 2, _rect.Location.Y - 16), Color.Wheat);
        }

        protected virtual void DrawHealthbar(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Vector2 dim = Manager_Sprites.HealthbarEmpty.Bounds.Size.ToVector2();
            float scale = IPlayer.PlayerBaseHeight / dim.X;
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
            if (CurrentAimInput == InputType.Controller && _isAiming || CurrentAimInput == InputType.KeyboardMouse)
            {
                var angle = Math.Atan2(AimDirection.Y, AimDirection.X) + Math.PI / 2;
                spriteBatch.Draw(
                    _spriteAimIndicator, _rect.Location.ToVector2() + _rect.Size.ToVector2() / 2f + AimDirection * (int)(_rect.Height * 1.5),
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
                Ability.Draw(gameTime, globalOffset, spriteBatch);
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
