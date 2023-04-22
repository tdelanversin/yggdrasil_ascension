using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Audio;
#nullable enable

namespace YGR
{
    public class Y_StarterGun : IShooter
    {
        double nextShotCooldown = 0.0f;
        static int shotDelay = 1000;

        public Y_StarterGun() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (nextShotCooldown > 0.0f)
                return;

            Manager_Sound.AddSound_Fireball().Play();

            nextShotCooldown = shotDelay;

            Manager_Projectile.AddProjectile_StarterProjectile(origin, direction, gameTime.TotalGameTime.TotalMilliseconds, level, who);
        }

        public void Update(GameTime gameTime)
        {
            nextShotCooldown = Math.Max(0, nextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }
    }

    public class Y_ShotGun : IShooter
    {
        double nextShotCooldown = 0.0f;
        static int shotDelay = 1000;
        static int shotCount = 3;
        static double shotSpread = .1;


        public Y_ShotGun() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            
            if (nextShotCooldown > 0.0f)
                return;

            Manager_Sound.AddSound_Shotgun().Play();

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

    public class Y_FunkyGun : IShooter
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

        public Y_FunkyGun() { }

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

    public class Y_WideGun : IShooter
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


        public Y_WideGun() { }

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

    public class Y_SimpleEnemyGun : IShooter
    {
        double nextShotCooldown = 0.0f;
        static int shotDelay = 1000;

        public Y_SimpleEnemyGun() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (nextShotCooldown > 0.0f)
                return;

            nextShotCooldown = shotDelay;

            Manager_Projectile.AddProjectile_StarterProjectile(origin, direction, gameTime.TotalGameTime.TotalMilliseconds, level, who);
        }

        public void Update(GameTime gameTime)
        {
            nextShotCooldown = Math.Max(0, nextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }
    }
}