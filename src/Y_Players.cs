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
        }
    }
}
