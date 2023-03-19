using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public class Sprite
    {
        Texture2D _sprite;
        Rectangle _window;
        Dictionary<string, int[]> _animations;
        Vector2 _position;
        float _velocity;
        float _frameDuration;
        int _animationIndex;

        Room _currentRoom;

        public Sprite(
            Texture2D texture, 
            Rectangle window,
            float velocity,
            Vector2 position,
            Room startRoom,
            float frameDuration,
            Dictionary<string, int[]> animations
            )
        {
            _sprite = texture;
            _window = window;
            _animations = animations;
            _velocity = velocity;
            _position = position;
            _animationIndex = 0;
            _currentRoom = startRoom;
        }

        public void Update(GameTime gameTime)
        {
            Vector2 input = Vector2.Zero;
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Right))
            {
                input.X += 1;
            }
            if (keyboard.IsKeyDown(Keys.Left))
            {
                input.X -= 1;
            }
            if (keyboard.IsKeyDown(Keys.Down))
            {
                input.Y += 1;
            }
            if (keyboard.IsKeyDown(Keys.Up))
            {
                input.Y -= 1;
            }

            if (input.LengthSquared() > 1)
            {
                input.Normalize();
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 dp = input * deltaTime * _velocity;

            ICollidable who = null;
            _position = _currentRoom.Clamp(_window, _position, dp, ref who);

            if (who != null)
            {
                Logger.Info("collided with someone");
            }
        }

        public void Draw(GameTime gameTime, int ox, int oy, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _sprite,
                new Rectangle((int)_position.X - ox - _window.Width/2, (int)_position.Y - oy - _window.Height/2, _window.Width, _window.Height),
                new Rectangle(_animationIndex * _window.Width, 0, _window.Width, _window.Height),
                Color.White
            );
        }
    }
}
