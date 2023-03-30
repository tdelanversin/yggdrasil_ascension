using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
#nullable enable

namespace YGR
{
    public class Y_StarterGun : IShooter
    {
        double nextShotCooldown = 0.0f;
        static int shotDelay = 1000;

        public Y_StarterGun() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, IWalkable room, IGameElement who)
        {
            if (nextShotCooldown > 0.0f)
                return;

            nextShotCooldown = shotDelay;

            var projectile = new Y_StarterProjectile(
                origin,
                direction,
                gameTime.TotalGameTime.TotalMilliseconds,
                room,
                who
            );
            Manager_Projectile.AddProjectile(projectile);
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

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, IWalkable room, IGameElement who)
        {
            if (nextShotCooldown > 0.0f)
                return;

            nextShotCooldown = shotDelay;

            double spread = -shotCount / 2 * shotSpread;
            for (int i = 0; i < shotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );
                var projectile = new Y_ShotGunProjectile(
                    origin,
                    new_dir,
                    gameTime.TotalGameTime.TotalMilliseconds,
                    room,
                    who
                );
                Manager_Projectile.AddProjectile(projectile);
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
        IWalkable _room;
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
        // shotSpeeds is an array of doubles that represent the time in milliseconds that each bullet row should be fired
        // static double[] shotSpeeds = { 0.0, 60.0, 120.0 };
        static double[] shotSpeeds = { 0.0, 60.0, 120.0, 180.0, 240.0, 300.0, 360.0, 420.0 };

        public Y_FunkyGun() { }

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, IWalkable room, IGameElement who)
        {
            if (timeSinceShot < shotDelay)
                return;

            timeSinceShot = 0.0f;
            _origin = origin;
            _direction = direction;
            _room = room;
            _who = who;
        }

        public void Update(GameTime gameTime)
        {
            if (timeSinceShot >= shotDelay)
                return;

            var lastUpdate = timeSinceShot;
            timeSinceShot += gameTime.ElapsedGameTime.TotalMilliseconds;

            shotSpeeds.Last();
            if (lastUpdate >= shotSpeeds.Last())
                return;

            for (int i = 0; i < shotSpeeds.GetLength(0); ++i)
            {
                if (lastUpdate > shotSpeeds[i] || timeSinceShot <= shotSpeeds[i])
                    continue;

                double timedelta = timeSinceShot - shotSpeeds[i];
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
                        (float)(_origin.X + timedelta * new_dir.X),
                        (float)(_origin.Y + timedelta * new_dir.Y)
                    );
                    var projectile = new Y_ShotGunProjectile(
                        new_origin,
                        new_dir,
                        gameTime.TotalGameTime.TotalMilliseconds,
                        _room,
                        _who
                    );
                    Manager_Projectile.AddProjectile(projectile);
                    spread += shotSpread;
                }
            }
        }


        public class Y_SimpleEnemyGun : IShooter
        {
            double nextShotCooldown = 0.0f;
            static int shotDelay = 1000;

            public Y_SimpleEnemyGun() { }

            public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, IWalkable room, IGameElement who)
            {
                if (nextShotCooldown > 0.0f)
                    return;

                nextShotCooldown = shotDelay;

                var projectile = new Y_StarterProjectile(
                    origin,
                    direction,
                    gameTime.TotalGameTime.TotalMilliseconds,
                    room,
                    who
                );
            }


            public void Update(GameTime gameTime)
            {
                nextShotCooldown = Math.Max(0, nextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
            }

        }
    }
}