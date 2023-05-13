using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.Sprites;
using System.Security.Cryptography;
using MonoGame.Extended;

namespace YGR
{
    // Basic gun, does nothing special, shoots fast
    public class Ability_Confusion : IAbility
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }

        protected double NextShotCooldown = 0.0f;
        protected int ShotDelay = 240;

        public Ability_Confusion()
        {
            Name = "Confusion";
            Sprite = Manager_Sprites.Effect_Confusion;
        }

        public virtual bool Trigger(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Confusion.Play(0.5f, 0, 0);

            NextShotCooldown = ShotDelay;

            var duration = 0;
            if (who.ElementLevel == 1) duration = 3000;
            else if (who.ElementLevel == 2) duration = 5000;
            else duration = 8000;
            Manager_Projectile.AddProjectile_Confusion(origin, direction, duration, level, who);
            return true;
        }

        public virtual void Update(GameTime gameTime)
        {
            NextShotCooldown = Math.Max(0, NextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) { }
    }

    // Ability for Ghosts. Does as much as absolutely nothing but make lives easier for stupid programmers
    public class Ability_Ghost : IAbility
    {
        public string Name { get { return ""; } }

        public Texture2D Sprite { get { return Manager_Sprites.White; } }

        public bool Trigger(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who) { return false; }

        public void Update(GameTime gameTime) { }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) { }
    }

    public class Ability_Shield : IAbility
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }
        public Player_Basic Owner { get; set; }

        AnimatedSprite _sprite;
        Point _center;
        float _a;
        float _b;
        float _hOffset;

        float _scale;
        float _angle;
        bool _triggered;
        float _strength;
        float _maxStrength;
        bool _reloading;
        int _reloadingTimeMS;
        int _reloadingTimeCounter;

        Vector2[] _collisionModel;
        int _outerCollisionModelPrecision = 20;
        int _innerCollisionModelPrecision = 10;
        int _middleCollisionModelPrecision = 5;

        int _triggerCountDown;
        int _triggerCountDownMax;

        Color _good;
        Color _crap;

        public Ability_Shield(Player_Basic owner)
        {
            Name = "Shield";
            Sprite = null;

            Owner = owner;
            Sprite = Manager_Sprites.Effect_Shield;

            _sprite = Manager_Sprites.NewAnimatedSprite_Shield();
            _scale = 1.0f;
            _center = new Point(128, 64);
            _a = 128.0f * Y_Level.GlobalScale; // must be half the texture width from the Python script
            _b = 80.0f * Y_Level.GlobalScale; // must be half the texture height from the Python script
            _hOffset = 16.0f * Y_Level.GlobalScale; // must be the same as h_offset from the Python script

            _triggered = false;
            _reloading = false;
            _reloadingTimeCounter = 0;
            _reloadingTimeMS = 10000;

            _collisionModel = new Vector2[_outerCollisionModelPrecision + _innerCollisionModelPrecision + _middleCollisionModelPrecision];
             
            _triggerCountDown = 0;
            _triggerCountDownMax = 10;

            _good = Color.Blue;
            _crap = Color.Red;
            _strength = Owner.LifePointsMax;
            _maxStrength = _strength;
        }

        public bool Trigger(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if(_triggerCountDown > 0)
            {
                return false;
            }
            _triggerCountDown = _triggerCountDownMax;
            if (!_triggered && !_reloading)
            {
                _triggered = true;
                return true;
            }
            else
            {
                _triggered = false;
                return false;
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw an indicator only if a) the player is actively aiming on the gamepad or b) is using mouse to aim
            if(_triggered)
            {
                float p = 1.0f / _maxStrength * _strength;
                Color gradient = new Color(
                    (byte)(_crap.R * (1.0f-p) + _good.R * p),
                    (byte)(_crap.G * (1.0f-p) + _good.G * p),
                    (byte)(_crap.B * (1.0f-p) + _good.B * p)
                );
                float angle = (float)(Math.Atan2(Owner.AimDirection.Y, Owner.AimDirection.X) + Math.PI / 2);
                spriteBatch.Draw(
                    _sprite.Texture, Owner.Rect.Center.ToVector2() + 0.25f * Owner.AimDirection * Owner.Rect.Size.ToVector2(),
                    _sprite.SourceRectangle,
                    gradient * 0.5f, angle, _sprite.SpriteDimension * 0.5f, _scale * Y_Level.GlobalScale, SpriteEffects.None, 0
                );
            }
        }

        public bool HitByProjectile(IProjectile projectile, int timeStepMS)
        {
            if (!_triggered) return false;

            foreach(var p in _collisionModel)
            {
                if (projectile.Rect.Contains(p)) return true;
            }

            /*
             * Screw this!!! Maybe another time...
             */
            //var i = -projectile.Velocity;
            //i.Normalize();
            //var d = Owner.AimDirection;
            //d.Normalize();
            //float angle = (float)Math.Atan2(d.Y * i.X - d.X * i.Y, d.X * i.X + d.Y * i.Y);

            //float minAngle = (float)(-Math.PI / 2 + Math.PI / 16);
            //float maxAngle = (float)(Math.PI / 2 - Math.PI / 16);
            //Logger.Info(minAngle.ToString() + " " + maxAngle.ToString() + " " + angle.ToString());
            //if (angle < minAngle || angle > maxAngle)
            //{
            //    Logger.Info("We suck...");
            //    return false;
            //}

            //Logger.Info("We passed the first hurdle...");

            //float baseAngle = (float)(Math.Atan2(Owner.AimDirection.Y, Owner.AimDirection.X));

            //// two points between which the projectile has to be within this time interval
            //Vector2 Ap = projectile.Rect.Center.ToVector2() - Owner.Rect.Center.ToVector2();
            //Vector2 Bp = Ap + projectile.Velocity * timeStepMS;

            //Vector2 A = new Vector2(
            //    (float)(Ap.X * Math.Cos(-baseAngle) - Ap.Y * Math.Sin(-baseAngle)),
            //    (float)(Ap.X * Math.Sin(-baseAngle) + Ap.Y * Math.Cos(-baseAngle))
            //);

            //Vector2 B = new Vector2(
            //    (float)(Bp.X * Math.Cos(-baseAngle) - Bp.Y * Math.Sin(-baseAngle)),
            //    (float)(Bp.X * Math.Sin(-baseAngle) + Bp.Y * Math.Cos(-baseAngle))
            //);

            //// using this stuff:
            //// https://www.xarg.org/book/computer-graphics/line-segment-ellipse-intersection/
            //float rx2 = _b * _b;
            //float ry2 = _a * _a;
            //float By_M_Ay = (B.Y - A.Y);
            //float Bx_M_Ax = (B.X - A.X);
            //float Ax2 = A.X * A.X;
            //float Ay2 = A.Y * A.Y;
            //float a = rx2 * By_M_Ay * By_M_Ay + ry2 * Bx_M_Ax * Bx_M_Ax;
            //float b = 2 * rx2 * A.Y * By_M_Ay + 2 * A.X * Bx_M_Ax;
            //float c = rx2 * Ay2 + ry2 * Ax2 - rx2 * ry2;
            //float D = b * b - 4 * a * c;

            //if(D < 0)
            //{
            //    // no intersection
            //    Logger.Info("No intersection because the determinant is negative...");
            //}
            //else
            //{
            //    float sqrt = (float)Math.Sqrt(D);
            //    float t1 = -b + sqrt / (2 * a);
            //    float t2 = -b - sqrt / (2 * a);

            //    Logger.Info("Maybe... " + t1.ToString() + " " + t2.ToString());

            //    // if we are somewhere in between our two points, we collided, otherwise we didnt
            //    if (Math.Min(t1, t2) < 1) return true;
            //}

            return false;
        }

        public float Strength()
        {
            return _strength;
        }

        public void SetReloadingTime(int reloadingTimeMS)
        {
            _reloadingTimeMS = reloadingTimeMS;
        }

        public void Hit(float damage)
        {
            _strength -= damage;
            if (_strength <= 0)
            {
                _reloading = true;
                _triggered = false;
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (!_triggered) return;

            foreach (var cm in _collisionModel)
            {
                Factory_Debug.DrawPoint((int)cm.X, (int)cm.Y, 5, Color.Yellow, spriteBatch);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_triggerCountDown > 0) _triggerCountDown--;

            if (_reloading)
            {
                _reloadingTimeCounter += gameTime.ElapsedGameTime.Milliseconds;
                if(_reloadingTimeCounter >= _reloadingTimeMS)
                {
                    _reloadingTimeCounter = 0;
                    _strength = _maxStrength;
                    _reloading = false;
                }
            }
            else
            {
                var p = Owner.Rect.Center;
                float baseAngle = (float)(Math.Atan2(Owner.AimDirection.Y, Owner.AimDirection.X));

                float minAngle = (float)(-Math.PI / 2 + Math.PI / 16);
                float maxAngle = (float)(Math.PI / 2 - Math.PI / 16);
                float dAngle = (maxAngle - minAngle) / _outerCollisionModelPrecision;

                float angle = minAngle;
                for(int i=0; i<_outerCollisionModelPrecision; ++i)
                {
                    float xp = (float)(_b * Math.Cos(angle));
                    float yp = (float)(_a * Math.Sin(angle));
                    float x = (float)(xp * Math.Cos(baseAngle) - yp * Math.Sin(baseAngle) + Math.Cos(baseAngle) * _hOffset);
                    float y = (float)(xp * Math.Sin(baseAngle) + yp * Math.Cos(baseAngle) + Math.Sin(baseAngle) * _hOffset);
                    _collisionModel[i] = new Vector2(x + p.X, y + p.Y);
                    angle += dAngle;
                }

                dAngle = (maxAngle - minAngle) / _innerCollisionModelPrecision;
                angle = minAngle;
                int len = _outerCollisionModelPrecision + _innerCollisionModelPrecision;
                for(int i=_outerCollisionModelPrecision; i<len; ++i)
                {
                    float xp = (float)(0.5f * _b * Math.Cos(angle));
                    float yp = (float)(0.5f * _a * Math.Sin(angle));
                    float x = (float)(xp * Math.Cos(baseAngle) - yp * Math.Sin(baseAngle) + Math.Cos(baseAngle) * _hOffset);
                    float y = (float)(xp * Math.Sin(baseAngle) + yp * Math.Cos(baseAngle) + Math.Sin(baseAngle) * _hOffset);
                    _collisionModel[i] = new Vector2(x + p.X, y + p.Y);
                    angle += dAngle;
                }

                dAngle = (maxAngle - minAngle) / _middleCollisionModelPrecision;
                angle = minAngle;
                int len2 = _outerCollisionModelPrecision + _innerCollisionModelPrecision + _middleCollisionModelPrecision;
                for (int i = len; i < len2; ++i)
                {
                    float xp = (float)(0.75f * _b * Math.Cos(angle));
                    float yp = (float)(0.75f * _a * Math.Sin(angle));
                    float x = (float)(xp * Math.Cos(baseAngle) - yp * Math.Sin(baseAngle) + Math.Cos(baseAngle) * _hOffset);
                    float y = (float)(xp * Math.Sin(baseAngle) + yp * Math.Cos(baseAngle) + Math.Sin(baseAngle) * _hOffset);
                    _collisionModel[i] = new Vector2(x + p.X, y + p.Y);
                    angle += dAngle;
                }

                _sprite.Update(gameTime, AnimationState.Idle);
            }
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Shield;
        }
    }
}

