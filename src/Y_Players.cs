using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace YGR
{
    public class Ninja : Y_Sprite
    {
        private bool _isDashing;
        private float _dashDuration;
        private float _dashSpeed;
        private float _dashTimer;
        private float _dashCooldown;
        private float _dashCooldownTimer;

        private Texture2D _sprite;

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
            PlayerIndex? playerIndex,
            Texture2D texture,
            Rectangle window,
            float velocity,
            Vector2 position,
            IWalkable startRoom,
            float frameDuration,
            Dictionary<string, int[]> animations,
            IShooter gun
        ) : base(playerIndex, texture, window, velocity, position, startRoom, frameDuration, animations, gun)
        {
            _isDashing = false;
            _dashDuration = 0.1f; // Dash duration in seconds
            _dashSpeed = 4f; // Dash speed multiplier
            _dashTimer = 0.0f;
            _dashCooldown = 3.0f; // Dash cooldown in seconds
            _dashCooldownTimer = 0.0f;
            _sprite = texture;

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
            // This tells the animation to start on the left-side sprite.
            previousAnimationIndex = 2;
            currentAnimationIndex = 1;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update the cooldown timer
            if (_dashCooldownTimer < _dashCooldown)
            {
                _dashCooldownTimer += deltaTime;
            }

            if (_playerIndex == null)
            {
                KeyboardState keyboard = Keyboard.GetState();
                if (!_isDashing && keyboard.IsKeyDown(Keys.Q) && _dashCooldownTimer >= _dashCooldown)
                {
                    _isDashing = true;
                    _dashTimer = 0.0f;
                    _dashCooldownTimer = 0.0f; // Reset timer
                }
            }
            else
            {
                GamePadState gpState = GamePad.GetState(_playerIndex.Value);
                if (!_isDashing && gpState.IsButtonDown(Buttons.A) && _dashCooldownTimer >= _dashCooldown)
                {
                    _isDashing = true;
                    _dashTimer = 0.0f;
                    _dashCooldownTimer = 0.0f; // Reset timer
                }
            }

            if (_isDashing)
            {
                _dashTimer += deltaTime;

                if (_dashTimer >= _dashDuration)
                {
                    _isDashing = false;
                }
                else
                {
                    _velocity *= _dashSpeed;
                }
            }

            base.Update(gameTime);

            if (_isDashing)
            {
                _velocity /= _dashSpeed;
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
           else{
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

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch){
            spriteBatch.Draw( _sprite, new Rectangle((int)Position.X - _window.Width/2, (int)Position.Y - _window.Height/2, _window.Width, _window.Height), sourceRectangles[currentAnimationIndex], Color.White);
        }
    }
}
