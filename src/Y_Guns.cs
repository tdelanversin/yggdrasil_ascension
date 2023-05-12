using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Graphics;
using Assimp;
using MonoGame.OpenGL;
#nullable enable

namespace YGR
{
    // Basic gun, does nothing special, shoots fast
    public class Gun_Basic : IShooter
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }
        public IVictim Owner { get; set; }

        public double NextShotCooldown { get; set; } = 0.0f;
        protected int ShotDelay = 240;

        public Gun_Basic(IVictim owner)
        {
            Name = "Pistol";
            Sprite = Manager_Sprites.Weapon_Pistol;
            Owner = owner;
        }

        public virtual bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Fireball.Play(0.2f, 0, 0);

            NextShotCooldown = ShotDelay;

            Manager_Projectile.AddProjectile_StarterProjectile(origin, direction, level, who);
            return true;
        }

        public virtual void Update(GameTime gameTime)
        {
            NextShotCooldown = Math.Max(0, NextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public virtual void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponPistol,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    // Slower version of basic gun for basic enemies
    public class Gun_BasicEnemy : Gun_Basic
    {
        public Gun_BasicEnemy(IVictim owner) : base(owner)
        {
            Name = "Slow Pistol";
            Sprite = Manager_Sprites.Weapon_Pistol;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Fireball.Play(0.2f, 0, 0);

            NextShotCooldown = ShotDelay;

            Manager_Projectile.AddProjectile_EnemySlimeProjectile(origin, direction, level, who);
            return true;
        }

        public override void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            /*
             * TODO: add low probability for this one
             */

            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponEnemySlowPistol,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    public class Gun_ShotGun : Gun_Basic
    {
        protected int ShotCount;
        protected double ShotSpread;

        public Gun_ShotGun(IVictim owner) : base(owner)
        {
            ShotDelay = 800;
            ShotCount = 5;
            ShotSpread = .3 / ShotCount;
            Name = string.Format("Shotgun ({0})", ShotCount);
            Sprite = Manager_Sprites.Weapon_Shotgun;
        }

        public Gun_ShotGun(IVictim owner, int shotCount) : this(owner)
        {
            ShotCount = shotCount;
            ShotSpread = .3 / ShotCount;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return true;

            Manager_Sound.Sound_Shotgun.Play(0.3f, 0, 0);

            NextShotCooldown = ShotDelay;

            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );

                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, new_dir, level, who);
                spread += ShotSpread;
            }
            return true;
        }

        public override void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponShotgun,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    public class Gun_Funky : IShooter
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }
        public IVictim Owner { get; set; }

        // Why are we not using subclassing...
        public double NextShotCooldown { get; set; } = 0.0f;

        double timeSinceShot = 1001;
        Vector2 _origin = new Vector2(0, 0);
        Vector2 _direction = new Vector2(0, 0);
        Y_Level? _level;
        IGameElement? _who;

        public Gun_Funky(IVictim owner)
        {
            // TODO: possibly find better name, but this one matches the power level and texture
            Name = "Red Devil";
            Sprite = Manager_Sprites.Weapon_RedGun;
            Owner = owner;
        }

        static int shotDelay = 1000;
        static double shotSpread = .1;

        // bulletArray is a 2D array of booleans that represent the shape of the bullet spray patter
        static bool[,] bulletArray = {
            { false, false, true, false, false },
            { false, true, true, true, false },
            { true, true, true, true, true },
            { true, true, true, true, true },
            { true, true, true, true, true },
            { true, true, true, true, true },
            { true, false, true, false, true },
            { false, true, true, true, false }
        };
        // shotTimings is an array of doubles that represent the time in milliseconds that each bullet row should be fired
        static double[] shotTimings = { 0.0, 60.0, 120.0, 180.0, 240.0, 300.0, 360.0, 420.0 };

        public virtual bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (timeSinceShot < shotDelay)
                return false;

            timeSinceShot = 0.0f;
            _origin = origin;
            _direction = direction;
            _level = level;
            _who = who;
            return true;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (timeSinceShot >= shotDelay || _who == null || _level == null)
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

                    Manager_Projectile.AddProjectile_Strong(new_origin, new_dir, _level, _who);
                    spread += shotSpread;
                }
            }
        }

        public virtual void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            /*
             * TODO: add low probability for this one
             */

            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponFunky,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    public class Gun_Wide : IShooter
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }
        public IVictim Owner { get; set; }

        // Why are we not using subclassing...
        public double NextShotCooldown { get; set; } = 0.0f;

        double timeSinceShot = 1001;
        Vector2 _origin = new Vector2(0, 0);
        Vector2 _direction = new Vector2(0, 0);
        Y_Level? _level;
        IGameElement? _who;

        public Gun_Wide(IVictim owner)
        {
            // TODO: find name
            Name = "Cannon";
            Sprite = Manager_Sprites.Weapon_RedGun;
            Owner = owner;
        }

        static int shotDelay = 1000;
        static bool[,] bulletArray = {{ false, false, true, false, false },
                                    { true, true, false, true, true },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { false, true, false, true, false },
                                    { true, false, true, false, true },
                                    { false, true, false, true, false }};
        // shotTimings is an array of doubles that represent the time in milliseconds that each bullet row should be fired
        static double[] shotTimings = { 30.0, 60.0, 120.0, 180.0, 240.0, 300.0, 360.0, 420.0 };
        static double[] positionShift = { 50, 20, 0, -20, -50 };
        static double[] shotSpread = { 0, 0, 0, 0, 0 };

        public bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {

            if (timeSinceShot < shotDelay)
                return false;

            timeSinceShot = 0.0f;
            _origin = origin;
            _direction = direction;
            _level = level;
            _who = who;
            return true;
        }

        public void Update(GameTime gameTime)
        {
            if (timeSinceShot >= shotDelay || _who == null || _level == null)
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

                    Manager_Projectile.AddProjectile_ShotGunProjectile(new_origin, new_dir, _level, _who);
                }
            }
        }

        public virtual void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponWide,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    // Gun for Gigachad
    public class Gun_Gigagun : Gun_ShotGun
    {
        public Gun_Gigagun(IVictim owner) : base(owner)
        {
            ShotCount = 256;
            ShotDelay = 5000;
            ShotSpread = 2 * Math.PI / ShotCount;
            Name = "Giga Gun";
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            // Override to make sure the direction is set, since gigachad shoots even without a target
            if (NextShotCooldown > 0.0f)
                return false;

            if (direction == Vector2.Zero)
            {
                direction = Vector2.One;
            }

            Manager_Sound.Sound_Explosion.Play();

            NextShotCooldown = ShotDelay;

            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );

                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, new_dir, level, who);
                spread += ShotSpread;
            }
            return true;
        }

        public override void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponGiga,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    // Become the one
    public class Gun_Godmode : Gun_Gigagun
    {
        public Gun_Godmode(IVictim owner) : base(owner)
        {
            ShotCount = 128;
            ShotDelay = 100;
            ShotSpread = 2 * Math.PI / ShotCount;
            Sprite = Manager_Sprites.Weapon_Keyboard; // TODO
            Name = "LoL";
        }
    }

    // Gun for ghosts. Does absolutely nothing. Just there to make other code simpler.
    public class Gun_Ghost : IShooter
    {
        public Gun_Ghost(IVictim owner)
        {
            Owner = owner;
        }

        public double NextShotCooldown { get; set; } = 0.0f;

        public string Name { get { return ""; } }

        public Texture2D Sprite { get { return Manager_Sprites.White; } }

        public IVictim Owner { get; set; }

        public bool Shoot(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who) { return false; }

        public void Update(GameTime gameTime) { }

        public void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            return;
        }
    }

    public class Gun_BossScatter : Gun_Basic
    {

        protected int ShotCount;
        protected double ShotSpread;

        public Gun_BossScatter(IVictim owner) : base(owner)
        {
            Name = "Scatter";
            ShotCount = 10;
            ShotSpread = .5 / ShotCount;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Shotgun.Play(0.3f, 0, 0);

            NextShotCooldown = ShotDelay;

            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );

                Manager_Projectile.AddProjectile_BossProjectile(origin, new_dir, level, who, 0.55f);
                spread += ShotSpread;
            }

            return true;
        }
    }

    public class Gun_BossPrecise : Gun_Basic
    {

        protected int ShotCount;
        protected double ShotSpread;
        protected double ShotSpreadCurrent; // current spread of the gun, [0,1) and used in ShotSpread * sin(ShotSpreadCurrent * 2 * pi)
        protected double ShotSpreadSpeed; // how fast the gun spreads and indicated one full sin wave per x milliseconds

        public Gun_BossPrecise(IVictim owner) : base(owner)
        {
            Name = "Precise";
            ShotDelay = 100;
            ShotSpread = 0.4f;
            ShotSpreadCurrent = 0f;
            ShotSpreadSpeed = 500f;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;

            ShotSpreadCurrent += gameTime.ElapsedGameTime.TotalMilliseconds / ShotSpreadSpeed;
            ShotSpreadCurrent %= 1;

            var new_dir = new Vector2(
                (float)(direction.X * Math.Cos(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI)) - direction.Y * Math.Sin(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI))),
                (float)(direction.X * Math.Sin(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI)) + direction.Y * Math.Cos(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI)))
            );

            Manager_Projectile.AddProjectile_BossProjectile(origin, new_dir, level, who);

            return true;
        }
    }

    public class Gun_BossAvoidPattern : Gun_BossScatter
    {

        protected double RotationSpeed;
        protected double Rotation;
        protected int Holes;
        protected int HoleSize;

        public Gun_BossAvoidPattern(IVictim owner) : base(owner)
        {
            Name = "Pattern boss gun";
            ShotDelay = 100; //Boss regulates its own AOE shooting
            ShotCount = 64;
            ShotSpread = (2 * Math.PI) / ShotCount;
            RotationSpeed = 0.0025f;
            Rotation = 0.0f;
            Holes = 2;
            HoleSize = 5;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;
            direction = -direction;

            Rotation += RotationSpeed * gameTime.ElapsedGameTime.Milliseconds;
            Rotation %= 2 * Math.PI;

            direction = new Vector2(
                (float)(direction.X * Math.Cos(Rotation) - direction.Y * Math.Sin(Rotation)),
                (float)(direction.X * Math.Sin(Rotation) + direction.Y * Math.Cos(Rotation))
            );

            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                spread += ShotSpread;
                if (i % (ShotCount / Holes) < HoleSize) continue;

                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );
                var prosition = origin + new_dir * 50f;

                Manager_Projectile.AddProjectile_BossProjectile(prosition, new_dir, level, who, 0.25f);
            }

            return true;
        }

    }

    public class Gun_BossAOE : Gun_BossScatter
    {
        public Gun_BossAOE(IVictim owner) : base(owner)
        {
            Name = "AOE boss gun";
            ShotDelay = 300; //Boss regulates its own AOE shooting
            ShotCount = 48;
            ShotSpread = 2 * Math.PI / ShotCount;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Shotgun.Play(0.3f, 0, 0);

            NextShotCooldown = ShotDelay;

            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );
                var prosition = origin + new_dir * 30f;

                Manager_Projectile.AddProjectile_BossProjectile(prosition, new_dir, level, who, 0.30f);
                spread += ShotSpread;
            }

            return true;
        }

    }
}