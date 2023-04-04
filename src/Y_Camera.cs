using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace YGR
{
    /// <summary>
    /// Class <c>Y_Camera</c> is a primitive implementation of a camera view that can pan using W,A,S,D. It pans with some simple animation.
    /// </summary>
    public class Y_Camera
    {
        // Global X position
        public int PosX { get { return _rect.X; } }
        // Global X position
        public int PosY { get { return _rect.Y; } }
        // Rectangle that will be shown in the Viewport
        public Rectangle SourceRect { get { return _rect; } }

        private Vector2 _dPos;
        private Rectangle _rect;
        private Rectangle _newRect;
        
        private float _dt;
        private bool _animate;
        private float _animationTime;

        public Y_Camera(Vector2 pos, int res_x, int res_y)
        {
            _rect = new Rectangle((int)(pos.X - res_x/2), (int)(pos.Y - res_y/2), res_x, res_y);
            _newRect = _rect;

            _dPos = Vector2.Zero;

            _animate = false;
            _animationTime = 0.0f;
            _dt = 0.0f;
        }

        /// <summary>
        /// Update the animation time
        /// </summary>
        /// <param name="dt">Time duration for the animation</param>
        public void SetDt(float dt)
        {
            if(dt > 0.0f) _dt = dt;
        }

        /// <summary>
        /// Pan the camera by [dx, dy] within animation time dt
        /// </summary>
        /// <param name="dx">Delta x</param>
        /// <param name="dy">Delta Y</param>
        /// <param name="dt">Animation time</param>
        public void MoveBy(float dx, float dy, float dt)
        {
            _newRect.X = (int)dx;
            _newRect.Y = (int)dy;
            resetAnimation(dt);
        }

        /// <summary>
        /// Poll the keyboard and pan the camera in the direction polled from the keyboard using dPos as update speeds.
        /// Finish the paning animation within time dt.
        /// If the user pans the camera again before the animation is finished, the animation will restart in the new direction.
        /// </summary>
        /// <param name="dPos">Position update delta for each manual update</param>
        /// <param name="dt">Animation time</param>
        public void UpdateManual(Vector2 dPos, float dt)
        {
            //if (coolDown()) return;

            Vector2 input = Vector2.Zero;

            if (Keyboard.IsPressed(Keys.F)) input.X += 1;
            if (Keyboard.IsPressed(Keys.H)) input.X -= 1;
            if (Keyboard.IsPressed(Keys.G)) input.Y -= 1;
            if (Keyboard.IsPressed(Keys.T)) input.Y += 1;

            if (input.LengthSquared() > 1) input.Normalize();

            if(input != Vector2.Zero)
            {
                _dPos.X = input.X * dPos.X;
                _dPos.Y = input.Y * dPos.Y;
                _newRect.X = (int)(_rect.X + _dPos.X);
                _newRect.Y = (int)(_rect.Y + _dPos.Y);
                resetAnimation(dt);
                Logger.Debug("set new position diff: " + _dPos.ToString());
            }
        }

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {
            float dt = gameTime.ElapsedGameTime.Milliseconds;

            if (_animate)
            {
                moveAnimation(dt);
            }
            //_logger.Debug(_dt.ToString() + " " + dt.ToString() + " " + _rect.ToString());
        }

        private void moveAnimation(float dt)
        {
            if (_animationTime <= _dt)
            {
                float percent = 1.0f / _dt * dt;
                average(percent);
                _animationTime += dt;
            }
            if(_animationTime >= _dt)
            {
                _animate = false;
            }
            //_logger.Debug(_newRect.ToString() + " " + dt.ToString() + " " + (_animationTime).ToString() + " " + _dt.ToString() + " " + _rect.ToString());
        }

        private int newPosition(int oldPos, int dp, int newPos)
        {
            int temp = oldPos + dp;
            if (dp < 0.0f)
            {
                if (temp >= newPos) return temp;
                return newPos;
            }
            else
            {
                if (temp <= newPos) return temp;
                return newPos;
            }
        }

        private void average(float a)
        {
            int dx = (int)(Math.Ceiling(_dPos.X * a));
            _rect.X = newPosition(_rect.X, dx, _newRect.X);

            int dy = (int)(Math.Ceiling(_dPos.Y * a));
            _rect.Y = newPosition(_rect.Y, dy, _newRect.Y);
        }

        private void resetAnimation(float dt)
        {
            _animate = true;
            _animationTime = 0.0f;
            _dt = dt;
        }
    }
}
