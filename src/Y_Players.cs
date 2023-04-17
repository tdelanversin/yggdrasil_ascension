using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Ninja : IVictim // Y_Sprite
    {
        private bool _isDashing;
        private int _dashDuration;
        private float _dashSpeed;
        private int _dashTimer;
        private int _dashCooldown;
        private int _dashCooldownTimer;

        private Texture2D _sprite;
        private Texture2D _ghostSprite;
        private Texture2D _targetIndicator;

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
        PlayerIndex? _playerIndex;

        public float Scale { get; private set; }
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        public X_CollisionModel_Victim Collision { get; }
        public Vector2 Velocity { get; set; }
        public Rectangle Rect { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        IShooter _gun;

        Vector2 _maxVelocity;
        private int _controlLayout;
        private bool _isAiming;
        private Vector2 _aimDirection;

        public Ninja(
            X_CollisionModel_Victim collision,
            PlayerIndex playerIndex,
            Texture2D texture,
            Texture2D ghostTexture,
            Texture2D targetIndicator,
            float maxVelocity,
            Vector2 position,
            Y_Level level,
            IShooter gun,
            int controlLayout = 1,
            float scale = 1.0f
        )
        {
            _isDashing = false;
            _dashDuration = 100; // Dash duration in ms
            _dashSpeed = 4f; // Dash speed multiplier
            _dashTimer = 0;
            _dashCooldown = 2000; // Dash cooldown in ms
            _dashCooldownTimer = 2000;
            _sprite = texture;
            _ghostSprite = ghostTexture;
            _targetIndicator = targetIndicator;
            _playerIndex = playerIndex;
            _controlLayout = controlLayout;
            _gun = gun;
            _aimDirection = new Vector2(1, 0);

            Velocity = Vector2.Zero;
            _maxVelocity = Vector2.One * maxVelocity;

            Collision = collision;
            Level = level;
            LifePoints = 10;
            HitInLastLoop = false;
            Scale = 1.0f;
            Level.Victims.Add(this);

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
            Rect = new Rectangle(
                (int)position.X - rr.Width / 2,
                (int)position.Y - rr.Height / 2,
                rr.Width, rr.Height
            );

            // This tells the animation to start on the left-side sprite.
            previousAnimationIndex = 2;
            currentAnimationIndex = 1;

            Room = Level.GetRoom(this, Room);
        }

        public X_LevelElements WhatAreYou()
        {
            if (LifePoints < 1)
            {
                return X_LevelElements.Ghost;
            }
            else
            {
                return X_LevelElements.Victim;
            }
        }
        public void Update(GameTime gameTime)
        {
            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;

            if (HitInLastLoop)
            {
                HitInLastLoop = false;
            }

            if (LifePoints < 1)
            {
                return; // he dead
            }

            Vector2 input = Vector2.Zero;
            GamePadState gpState = GamePad.GetState(_playerIndex.Value);

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
                if (!_isAiming) // Skip if controller is already aiming
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
            Velocity = input * _maxVelocity * timeStepMS;

            // Update the cooldown timer
            if (_dashCooldownTimer < _dashCooldown)
            {
                _dashCooldownTimer += timeStepMS;
            }

            if (!_isDashing && Input.IsKeyDown(Keybinds.P1Dash) && _dashCooldownTimer >= _dashCooldown)
            {
                _isDashing = true;
                _dashTimer = 0;
                _dashCooldownTimer = 0; // Reset timer
            }
            else if (!_isDashing && gpState.IsButtonDown(Buttons.A) && _dashCooldownTimer >= _dashCooldown)
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

            //base.Update(gameTime);
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            if (Collision.Intersect(this, timeStepMS, out contactPoint, out contactNormal, out who))
            {
                Logger.Info("Collided with something");
            }

            if (_isDashing)
            {
                Velocity /= _dashSpeed;
            }

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

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (LifePoints < 1)
            {
                // Render ghosty 👻
                spriteBatch.Draw(
                    _ghostSprite,
                    new Rectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height),
                    new Rectangle(0, 0, 180, 180), Color.White);
                return;
            }

            spriteBatch.Draw(
                    _sprite,
                    new Rectangle(
                        Rect.X, Rect.Y, Rect.Width, Rect.Height),
                        sourceRectangles[currentAnimationIndex], Color.White);

            // Draw a targeting indicator if the player is actively aiming or using mouse controls
            if (_isAiming || _controlLayout > 0)
            {
                var angle = Math.Atan2(_aimDirection.Y, _aimDirection.X) + Math.PI / 2;
                spriteBatch.Draw(
                    _targetIndicator, Rect.Center.ToVector2() + _aimDirection * 60,
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
