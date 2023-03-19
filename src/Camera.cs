using Clearcove.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace YGR
{
    public class Camera : ICamera
    {
        private Vector2 _dPos;
        private Vector2 _dSize;
        private Rectangle _rect;
        private Rectangle _newRect;
        
        private float _dt;

        private bool _animate;
        private float _animationTime;
        private int _animationCounter;

        private DateTime _lastTS;

        int _coolDownCounter = 0;
        int _coolDown = 10;

        public Camera(Vector2 pos, int res_x, int res_y)
        {
            _rect = new Rectangle((int)(pos.X - res_x/2), (int)(pos.Y - res_y/2), res_x, res_y);
            _newRect = _rect;

            _dPos = Vector2.Zero;
            _dSize = Vector2.Zero;

            _animate = false;
            _animationTime = 0.0f;
            _animationCounter = 0;
            _dt = 0.0f;
        }

        public void SetDt(float dt)
        {
            if(dt > 0.0f)
            {
                _dt = dt;
            }
        }

        public void MoveBy(float dx, float dy, float dt)
        {
            _newRect.X = (int)dx;
            _newRect.Y = (int)dy;
            resetAnimation(dt);
        }

        public void UpdateManual(Vector2 dPos, float dt)
        {
            //if (coolDown()) return;

            Vector2 input = Vector2.Zero;
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.A))
            {
                input.X -= 1;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                input.X += 1;
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                input.Y += 1;
            }
            if (keyboard.IsKeyDown(Keys.W))
            {
                input.Y -= 1;
            }

            if (input.LengthSquared() > 1)
            {
                input.Normalize();
            }

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

        public void ResizeSourceRect(int dw, int dh, float dt)
        {
            _newRect.Width = dw;
            _newRect.Height = dh;
            resetAnimation(dt);
        }

        public Rectangle GetSourceRect()
        {
            return _rect;
        }

        public int posX()
        {
            return _rect.X;
        }

        public int posY()
        {
            return _rect.Y;
        }

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
            _animationCounter++;
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

        private bool coolDown()
        {
            if (_coolDownCounter < _coolDown)
            {
                _coolDownCounter++;
                return true;
            }
            
            _coolDownCounter = 0;
            return false;
        }

        private void resetAnimation(float dt)
        {
            _animate = true;
            _animationTime = 0.0f;
            _animationCounter = 0;
            _dt = dt;
        }
    }
}
