using Microsoft.Xna.Framework;
using System.Linq;
using System;
#nullable enable

namespace YGR
{
    // Basic gun, does nothing special, shoots fast
    public class Gun_Basic : IShooter
    {
        protected double NextShotCooldown = 0.0f;
        protected int ShotDelay = 240;

        public Gun_Basic() { }

        public virtual void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return;

            Manager_Sound.Sound_Fireball.Play(0.2f, 0, 0);

            NextShotCooldown = ShotDelay;

            Manager_Projectile.AddProjectile_StarterProjectile(origin, direction, gameTime.TotalGameTime.TotalMilliseconds, level, who);
        }

        public virtual void Update(GameTime gameTime)
        {
            NextShotCooldown = Math.Max(0, NextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }
    }

    // Slower version of basic gun for basic enemies
    public class Gun_BasicSlow : Gun_Basic
    {
        public Gun_BasicSlow() { 
            ShotDelay = 1000;
        }
    }

    public class Gun_ShotGun : Gun_Basic
    {
        protected int ShotCount;
        protected double ShotSpread;

        public Gun_ShotGun()
        {
            ShotDelay = 1200;
            ShotCount = 5;
            ShotSpread = .3 / ShotCount;
        }

        public Gun_ShotGun(int shotCount) : this()
        {
            ShotCount = shotCount;
            ShotSpread = .3 / ShotCount;
        }

        public override void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return;

            Manager_Sound.Sound_Shotgun.Play(0.3f, 0, 0);

            NextShotCooldown = ShotDelay;

            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );

                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, new_dir, gameTime.TotalGameTime.TotalMilliseconds, level, who);
                spread += ShotSpread;
            }
        }
    }

    public class Gun_Funky : IShooter
    {
        double timeSinceShot = 1001;
        Vector2 _origin = new Vector2(0, 0);
        Vector2 _direction = new Vector2(0, 0);
        Y_Level _level;
        IGameElement _who;

        static int shotDelay = 1000;
        // bulletArray is a 2D array of booleans that represent the shape of the bullet spray patter
        static double shotSpread = .1;
        // static bool[,] bulletArray = {{false, true, false},
        //                               {true, false, true},
        //                               {false, true, false}};
        static bool[,] bulletArray = {{ false, false, true, false, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { true, false, true, false, true },
                                    { false, true, false, true, false }};
        // shotTimings is an array of doubles that represent the time in milliseconds that each bullet row should be fired
        // static double[] shotTimings = { 0.0, 60.0, 120.0 };
        static double[] shotTimings = { 0.0, 60.0, 120.0, 180.0, 240.0, 300.0, 360.0, 420.0 };

        public Gun_Funky() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {

            if (timeSinceShot < shotDelay)
                return;

            timeSinceShot = 0.0f;
            _origin = origin;
            _direction = direction;
            _level = level;
            _who = who;
        }

        public void Update(GameTime gameTime)
        {
            if (timeSinceShot >= shotDelay)
                return;

            var lastUpdate = timeSinceShot;
            timeSinceShot += gameTime.ElapsedGameTime.TotalMilliseconds;

            shotTimings.Last();
            if (lastUpdate >= shotTimings.Last())
                return;

            for (int i = 0; i < shotTimings.GetLength(0); ++i)
            {
                if (lastUpdate > shotTimings[i] || timeSinceShot <= shotTimings[i])
                    continue;

                double timedelta = timeSinceShot - shotTimings[i];
                double spread = -bulletArray.GetLength(1) / 2 * shotSpread;
                for (int j = 0; j < bulletArray.GetLength(1); j++)
                {
                    if (!bulletArray[i, j])
                    {
                        spread += shotSpread;
                        continue;
                    }

                    var new_dir = new Vector2(
                        (float)(_direction.X * Math.Cos(spread) - _direction.Y * Math.Sin(spread)),
                        (float)(_direction.X * Math.Sin(spread) + _direction.Y * Math.Cos(spread))
                    );
                    var new_origin = new Vector2(
                        (float)(_who.Rect.Center.X + timedelta * new_dir.X),
                        (float)(_who.Rect.Center.Y + timedelta * new_dir.Y)
                    );

                    Manager_Projectile.AddProjectile_ShotGunProjectile(new_origin, new_dir, gameTime.TotalGameTime.TotalMilliseconds, _level, _who);
                    spread += shotSpread;
                }
            }
        }
    }

    public class Gun_Wide : IShooter
    {
        double timeSinceShot = 1001;
        Vector2 _origin = new Vector2(0, 0);
        Vector2 _direction = new Vector2(0, 0);
        Y_Level _level;
        IGameElement _who;

        static int shotDelay = 1000;
        // static bool[,] bulletArray = {{false, true, false},
        //                               {true, false, true},
        //                               {false, true, false}};
        static bool[,] bulletArray = {{ false, false, true, false, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { true, false, true, false, true },
                                    { false, true, false, true, false }};
        // shotTimings is an array of doubles that represent the time in milliseconds that each bullet row should be fired
        // static double[] shotTimings = { 0.0, 60.0, 120.0 };
        static double[] shotTimings = { 30.0, 60.0, 120.0, 180.0, 240.0, 300.0, 360.0, 420.0 };
        static double[] positionShift = { 50, 20, 0, -20, -50 };
        static double[] shotSpread = { 0, 0, 0, 0, 0 };


        public Gun_Wide() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {

            if (timeSinceShot < shotDelay)
                return;

            timeSinceShot = 0.0f;
            _origin = origin;
            _direction = direction;
            _level = level;
            _who = who;
        }

        public void Update(GameTime gameTime)
        {
            if (timeSinceShot >= shotDelay)
                return;

            var lastUpdate = timeSinceShot;
            timeSinceShot += gameTime.ElapsedGameTime.TotalMilliseconds;

            shotTimings.Last();
            if (lastUpdate >= shotTimings.Last())
                return;

            for (int i = 0; i < shotTimings.GetLength(0); ++i)
            {
                if (lastUpdate > shotTimings[i] || timeSinceShot <= shotTimings[i])
                    continue;

                double timedelta = timeSinceShot - shotTimings[i];
                for (int j = 0; j < bulletArray.GetLength(1); j++)
                {
                    if (!bulletArray[i, j])
                        continue;

                    var spread = shotSpread[j];
                    var shift = positionShift[j];

                    var new_dir = new Vector2(
                        (float)(_direction.X * Math.Cos(spread) - _direction.Y * Math.Sin(spread)),
                        (float)(_direction.X * Math.Sin(spread) + _direction.Y * Math.Cos(spread))
                    );

                    var perp = new Vector2(-new_dir.Y, new_dir.X);
                    var new_origin = new Vector2(
                        (float)(_who.Rect.Center.X + timedelta * new_dir.X + shift * perp.X),
                        (float)(_who.Rect.Center.Y + timedelta * new_dir.Y + shift * perp.Y)
                    );

                    Manager_Projectile.AddProjectile_ShotGunProjectile(new_origin, new_dir, gameTime.TotalGameTime.TotalMilliseconds, _level, _who);
                }
            }
        }
    }

    // Gun for Gigachad
    public class Gun_Gigagun : IShooter
    {
        double nextShotCooldown = 0.0f;
        static int shotDelay = 3000;
        static int shotCount = 256;
        static double shotSpread = 2 * Math.PI / shotCount;

        public Gun_Gigagun() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {

            if (nextShotCooldown > 0.0f)
                return;

            // direction does not matter, just make sure it's sensible
            direction = Vector2.One;

            Manager_Sound.Sound_Explosion.Play(1f, 0, 0);

            nextShotCooldown = shotDelay;

            double spread = -shotCount / 2 * shotSpread;
            for (int i = 0; i < shotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );

                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, new_dir, gameTime.TotalGameTime.TotalMilliseconds, level, who);
                spread += shotSpread;
            }
        }

        public void Update(GameTime gameTime)
        {
            nextShotCooldown = Math.Max(0, nextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }

    }
}