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

        protected int ShotDelay = 5000;

        public bool Triggered { get; private set; }

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

            // decide on the duration of the confusion in the Hit() method of the respective enemy!!!
            Manager_Projectile.AddProjectile_Confusion(origin, direction, 0, level, who);
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

        public bool Triggered { get; private set; }
    }

    public class Ability_Shield : IAbility
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }
        public Player_Basic Owner { get; set; }
        public bool Triggered { get; private set; }

        AnimatedSprite _sprite;
        Point _center;
        float _a;
        float _b;
        float _hOffset;

        float _scale;
        // float _angle;

        //bool _reloading;
        int _maxDuration;
        // int _maxReloadTime;
        int _currentDuration = 0;

        int _level_1_duration = 3000;
        int _level_2_duration = 6000;
        int _level_3_duration = 8000;

        int _shortestWaitTimeMS = 1000;
        int _shortestWaitTimeCounter = 0;

        Vector2[] _collisionModel;
        int _outerCollisionModelPrecision = 20;
        int _innerCollisionModelPrecision = 10;
        int _middleCollisionModelPrecision = 5;

        bool _everySecondFrame;

        Color[] _goodLevelColors = new Color[] { Color.Orange, Color.Green, Color.Blue };
        Color _crap = Color.Red;

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

            Triggered = false;
            _collisionModel = new Vector2[_outerCollisionModelPrecision + _innerCollisionModelPrecision + _middleCollisionModelPrecision];
            _everySecondFrame = true;
        }

        private void setShieldDuration()
        {
            if (Owner.ElementLevel == 1) _maxDuration = _level_1_duration;
            else if (Owner.ElementLevel == 2) _maxDuration = _level_2_duration;
            else _maxDuration = _level_3_duration;
        }

        public bool Trigger(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (_shortestWaitTimeCounter > 0) return false;
            if (Triggered) return false;
            setShieldDuration();
            Triggered = true;

            return true;
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw an indicator only if a) the player is actively aiming on the gamepad or b) is using mouse to aim
            if(Triggered)
            {
                Color good = _goodLevelColors[Math.Max(_goodLevelColors.Length - 1, Owner.ElementLevel - 1)];
                float p = 1.0f / _level_3_duration * _currentDuration;
                Color gradient = new Color(
                    (byte)(_crap.R * (1.0f-p) + good.R * p),
                    (byte)(_crap.G * (1.0f-p) + good.G * p),
                    (byte)(_crap.B * (1.0f-p) + good.B * p)
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
            if (!Triggered) return false;

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

        public void Hit(float damage)
        {
            // no damage to the shield is possible
            // could register impacts for the statistics though
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (!Triggered) return;

            foreach (var cm in _collisionModel)
            {
                Factory_Debug.DrawPoint((int)cm.X, (int)cm.Y, 5, Color.Yellow, spriteBatch);
            }
        }

        public void Update(GameTime gameTime)
        {
            _everySecondFrame = !_everySecondFrame;

            // do some cooldown to prevent flickering
            if(_shortestWaitTimeCounter > 0)
            {
                // wait but reload at the same time... otherwise it's unfair
                _shortestWaitTimeCounter -= gameTime.ElapsedGameTime.Milliseconds;
                if (_currentDuration < _maxDuration && _everySecondFrame)
                {
                    _currentDuration += gameTime.ElapsedGameTime.Milliseconds;
                }
                return;
            }
            _shortestWaitTimeCounter = 0;


            // as long as the user keeps the button pressed... if not => stop
            if (!(Input.IsKeyDown(Keybinds.KeyboardAbility) || Input.IsButtonDown(Owner.PlayerIndex, Keybinds.GamePadAbility)) || _currentDuration <= 0)
            {
                // If we were running the shield... set the shortest wait time to 1s
                if (Triggered) _shortestWaitTimeCounter = _shortestWaitTimeMS;

                // set trigger to false
                Triggered = false;

                // make sure we are up to speed
                setShieldDuration();

                if(_currentDuration < _maxDuration && _everySecondFrame)
                {
                    _currentDuration += gameTime.ElapsedGameTime.Milliseconds;
                }
                return;
            }

            _currentDuration -= gameTime.ElapsedGameTime.Milliseconds;

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
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Shield;
        }
    }

    public class Ability_Gunslinger : IAbility
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; }
        protected double NextShotCooldown = 0.0f;
        protected int ShotDelay = 10000;
        protected int Duration = 3000;
        public bool Triggered { get; private set; }
        protected IPlayer owner;
        protected IShooter GunShot;
        protected double oldShotDelay;

        public Ability_Gunslinger(IPlayer owner)
        {
            Name = "Gunslinger";
            Sprite = Manager_Sprites.Effect_Gunslinger;
            this.owner = owner;
            Triggered = false;
        }

        public bool Trigger(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Gunslinger.Play(0.5f, 0, 0);

            NextShotCooldown = ShotDelay;

            GunShot = owner.Gun;
            oldShotDelay = GunShot.ShotDelay;
            GunShot.ShotDelay = oldShotDelay / 2.5;
            return true;
        }
        

        public virtual void Update(GameTime gameTime)
        {
            NextShotCooldown = Math.Max(0, NextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
            if (NextShotCooldown <= ShotDelay - Duration && GunShot != null)
            {
                GunShot.ShotDelay = oldShotDelay;
                GunShot = null;
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) { }
    }
}

