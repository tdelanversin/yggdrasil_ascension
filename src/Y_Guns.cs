using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended.Collections;
#nullable enable

namespace YGR
{
    // Basic gun, does nothing special, shoots fast
    public class Gun_Basic : IShooter
    {
        public string Name { get; protected set; } = "Pistol";
        public Texture2D Sprite { get; protected set; }  = Manager_Sprites.Weapon_Pistol;
        public IVictim Owner { get; set; }
        public Y_PowerUps PowerUpType { get; set; } = Y_PowerUps.WeaponPistol;
        public double ShotDelay { get; set; } = 240;
        public double NextShotCooldown { get; set; } = 0.0f;

        public Gun_Basic(IVictim owner)
        {
            Owner = owner;
        }

        public virtual bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Fireball.Play(0.2f, 0, 0);
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
                PowerUpType,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }

    // Slower version of basic gun for basic enemies
    public class Gun_BasicEnemy : Gun_Basic
    {
        public Gun_BasicEnemy(IVictim owner) : base(owner)
        {
            Name = "Pistol Enemy";
            Sprite = Manager_Sprites.Weapon_Pistol;
            PowerUpType = Y_PowerUps.WeaponEnemySlowPistol;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Slime4, Manager_Sound.Sound_Slime5, Manager_Sound.Sound_Slime6, Manager_Sound.Sound_Slime7, Manager_Sound.Sound_Slime8 };
            sound.Shuffle(Util.random).First().Play(.25f, 0f, 0f);

            Manager_Projectile.AddProjectile_EnemySlimeProjectile(origin, direction, level, who);
            return true;
        }
    }

    public class Gun_ShotGun : Gun_Basic
    {
        protected int ShotCount { get; set; }
        protected double ShotSpread { get; set; }
        public Gun_ShotGun(IVictim owner) : this(owner, 5) { }
        public Gun_ShotGun(IVictim owner, int shotCount) : base(owner)
        {
            ShotDelay = 800;
            ShotCount = shotCount;
            ShotSpread = .3 / shotCount;
            Name = string.Format("Shotgun ({0})", shotCount);
            Sprite = Manager_Sprites.Weapon_Shotgun;
            PowerUpType = Y_PowerUps.WeaponShotgun;
        }
        protected IEnumerable<Vector2> IterateDirections(Vector2 direction)
        {
            double spread = -ShotCount / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                var new_dir = new Vector2(
                    (float)(direction.X * Math.Cos(spread) - direction.Y * Math.Sin(spread)),
                    (float)(direction.X * Math.Sin(spread) + direction.Y * Math.Cos(spread))
                );
                yield return new_dir;
                spread += ShotSpread;
            }
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Shotgun.Play(0.3f, 0, 0);
            foreach (var dir in IterateDirections(direction))
                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, dir, level, who);

            return true;
        }
    }

    public class Gun_ShotGunEnemy : Gun_ShotGun
    {
        public Gun_ShotGunEnemy(IVictim owner, int shotCount = 5) : base(owner, shotCount)
        {
            Name = string.Format("Shotgun Enemy ({0})", ShotCount);
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return true;
            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Slime4, Manager_Sound.Sound_Slime5, Manager_Sound.Sound_Slime6 };
            sound.Shuffle(Util.random).First().Play(.25f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
            {
                Manager_Projectile.AddProjectile_EnemySlimeProjectile(origin, dir, level, who);
            }
            return true;
        }
    }

    public class Gun_Keyboard : Gun_Basic
    {
        public Gun_Keyboard(IVictim owner) : base(owner)
        {
            Name = "Keyboard Gun";
            Sprite = Manager_Sprites.Weapon_Keyboard_Pink;
            PowerUpType = Y_PowerUps.WeaponKeyboardPink;
            ShotDelay = 800;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Keyboard1, Manager_Sound.Sound_Keyboard2, Manager_Sound.Sound_Keyboard3, Manager_Sound.Sound_Keyboard4, Manager_Sound.Sound_Keyboard5 };
            sound.Shuffle(Util.random).First().Play(.2f, 0f, 0f);

            Manager_Projectile.AddProjectile_Keyboard(origin, direction, level, who, damage: 1.5f);
            return true;
        }
    }

    public class Gun_Letter : Gun_ShotGun
    {
        public Gun_Letter(IVictim owner) : base(owner, 3)
        {
            Name = "Letter Gun";
            Sprite = Manager_Sprites.Weapon_Letter;
            PowerUpType = Y_PowerUps.WeaponLetter;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Letter1, Manager_Sound.Sound_Letter2, Manager_Sound.Sound_Letter3, Manager_Sound.Sound_Letter4 };
            sound.Shuffle(Util.random).First().Play(.3f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
                Manager_Projectile.AddProjectile_Letter(origin, dir, level, who, damage: .8f);
            return true;
        }
    }

    public class Gun_Book : Gun_Basic
    {
        public Gun_Book(IVictim owner) : base(owner)
        {
            Name = "Book Gun";
            Sprite = Manager_Sprites.Weapon_Book;
            PowerUpType = Y_PowerUps.WeaponBook;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Book1, Manager_Sound.Sound_Book2, Manager_Sound.Sound_Book3 };
            sound.Shuffle(Util.random).First().Play(.3f, 0f, 0f);

            Manager_Projectile.AddProjectile_Book(origin, direction, level, who, damage: 2f);
            return true;
        }
    }

    public class Gun_NinjaStar : Gun_Basic
    {
        public Gun_NinjaStar(IVictim owner) : base(owner)
        {
            Name = "Ninja Star Gun";
            Sprite = Manager_Sprites.Weapon_Ninja_Star;
            PowerUpType = Y_PowerUps.WeaponNinjaStar;
            ShotDelay = 300;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Ninja1, Manager_Sound.Sound_Ninja2, Manager_Sound.Sound_Ninja3 };
            sound.Shuffle(Util.random).First().Play(0.1f, 0f, 0f);

            Manager_Projectile.AddProjectile_NinjaStar(origin, direction, level, who, damage: .7f);
            return true;
        }
    }

    public class Gun_Helix : Gun_Basic
    {
        public Gun_Helix(IVictim owner) : base(owner)
        {
            Name = "Helix Gun";
            Sprite = Manager_Sprites.Weapon_Helix;
            PowerUpType = Y_PowerUps.WeaponHelix;
            ShotDelay = 100;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Blaster.Play(0.1f, 0, 0);

            Manager_Projectile.AddProjectile_Helix(origin, direction, level, who, damage: .25f, phase: 0.0f);
            Manager_Projectile.AddProjectile_Helix(origin, direction, level, who, damage: .25f, phase: 0.5f);
            return true;
        }
    }

    public class Gun_Blunderbuss : Gun_ShotGun
    {
        public Gun_Blunderbuss(IVictim owner, int shotCount = 5) : base(owner, shotCount)
        {
            Name = "Blunderbuss";
            Sprite = Manager_Sprites.Weapon_Blunderbuss;
            PowerUpType = Y_PowerUps.WeaponBlunderbuss;
            ShotDelay = 1000;
            ShotSpread = 0.002f;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_PlasmaPistol.Play(0.85f, 0, 0);
            double spread = -(ShotCount - 1) / 2 * ShotSpread;
            for (int i = 0; i < ShotCount; i++)
            {
                Manager_Projectile.AddProjectile_Blunderbuss(origin, direction, level, who, damage: 2, drift: (float)spread);
                spread += ShotSpread;
            }
            return true;
        }
    }

    public class Gun_RedDevil : Gun_ShotGun
    {
        public Gun_RedDevil(IVictim owner, int shotCount = 48) : base(owner, shotCount)
        {
            Name = "Red Devil";
            Sprite = Manager_Sprites.Weapon_RedDevil;
            PowerUpType = Y_PowerUps.WeaponRedDevil;
            ShotDelay = 1500;
            ShotSpread = 2 * Math.PI / ShotCount;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return true;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_OmniShotGun.Play(0.7f, 0, 0);
            foreach (var dir in IterateDirections(direction))
                Manager_Projectile.AddProjectile_RedDevil(origin, dir, level, who, damage: .75f);
            return true;
        }
    }

    public class Gun_Sniper : Gun_Basic
    {
        public Gun_Sniper(IVictim owner) : base(owner)
        {
            Name = "Sniper";
            Sprite = Manager_Sprites.Weapon_Sniper;
            PowerUpType = Y_PowerUps.WeaponSniper;
            ShotDelay = 1500;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Camera.Shake(-direction, 0.0001f, ShakeStrength.Light);
            Manager_Sound.Sound_SniperShot.Play(0.85f, 0, 0);
            Manager_Projectile.AddProjectile_Sniper(origin, direction, level, who, damage: 10f);
            return true;
        }
    }

    public class Gun_Wide : Gun_Basic
    {
        Vector2 _origin = new Vector2(0, 0);
        Vector2 _direction = new Vector2(0, 0);
        Y_Level? _level;
        IGameElement? _who;
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

        public Gun_Wide(IVictim owner) : base(owner)
        {
            Name = "Canon";
            Sprite = Manager_Sprites.Weapon_RedDevil;
            PowerUpType = Y_PowerUps.WeaponWide;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Explosion.Play(0.85f, 0, 0);

            _origin = origin;
            _direction = direction;
            _level = level;
            _who = who;
            return true;
        }

        protected IEnumerable<(Vector2, Vector2)> IterateOriginDirections(double lastUpdate, double currentUpdate)
        {
            if (_who == null || _level == null)
                yield break;

            var origin = _who.Rect.Center.ToVector2();

            for (int i = 0; i < shotTimings.GetLength(0); ++i)
            {
                if (lastUpdate > shotTimings[i])
                    continue;
                if (currentUpdate <= shotTimings[i])
                    break;

                double timedelta = currentUpdate - shotTimings[i];
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

                    var dir_normal = new Vector2(-new_dir.Y, new_dir.X);
                    var new_origin = new Vector2(
                        (float)(origin.X + timedelta * new_dir.X + shift * dir_normal.X),
                        (float)(origin.Y + timedelta * new_dir.Y + shift * dir_normal.Y)
                    );

                    yield return (new_origin, new_dir);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            var lastUpdate = ShotDelay - NextShotCooldown;
            base.Update(gameTime);
            var currentUpdate = ShotDelay - NextShotCooldown;

            if (_who == null || _level == null)
                return;

            foreach (var (origin, direction) in IterateOriginDirections(lastUpdate, currentUpdate))
                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, direction, _level, _who);
        }
    }

    // Gun for Gigachad
    public class Gun_Gigagun : Gun_ShotGun
    {
        public Gun_Gigagun(IVictim owner, int shotCount = 256) : base(owner, shotCount)
        {
            ShotDelay = 5000;
            ShotSpread = 2 * Math.PI / ShotCount;
            Name = "Giga Gun";
            PowerUpType = Y_PowerUps.WeaponGiga;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            // Override to make sure the direction is set, since gigachad shoots even without a target
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            if (direction == Vector2.Zero)
                direction = new Vector2(0, 1);

            Manager_Sound.Sound_Explosion.Play(1.0f, 0f, 0f);
            foreach (var dir in IterateDirections(direction))
                Manager_Projectile.AddProjectile_ShotGunProjectile(origin, dir, level, who);
            return true;
        }
    }

    // Become the one
    public class Gun_Godmode : Gun_Gigagun
    {
        public Gun_Godmode(IVictim owner, int shotCount = 128) : base(owner, shotCount)
        {
            ShotDelay = 100;
            Sprite = Manager_Sprites.Weapon_Keyboard; // TODO
            PowerUpType = Y_PowerUps.WeaponGodmode;
            Name = "LoL";
        }
    }

    // Gun for ghosts. Does absolutely nothing. Just there to make other code simpler.
    public class Gun_Ghost : Gun_Basic
    {
        public Gun_Ghost(IVictim owner) : base(owner)
        {
            Sprite = Manager_Sprites.White;
            Name = "Ghost Gun";
        }

        public override bool Shoot(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who) { return false; }

        public override void Update(GameTime gameTime) { }

        public override void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location) { }
    }

    public class Gun_BossScatter : Gun_ShotGun
    {

        public Gun_BossScatter(IVictim owner, int shotCount = 10) : base(owner, shotCount)
        {
            Name = "Boss Scatter";
            ShotSpread = .5 / ShotCount;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Slime4, Manager_Sound.Sound_Slime5, Manager_Sound.Sound_Slime6 };
            sound.Shuffle(Util.random).First().Play(1.0f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
                Manager_Projectile.AddProjectile_BossProjectile(origin, dir, level, who, 0.55f);
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

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Slime7, Manager_Sound.Sound_Slime8};
            sound.Shuffle(Util.random).First().Play(1.0f, 0f, 0f);

            var new_dir = new Vector2(
                (float)(direction.X * Math.Cos(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI)) - direction.Y * Math.Sin(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI))),
                (float)(direction.X * Math.Sin(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI)) + direction.Y * Math.Cos(ShotSpread * Math.Sin(ShotSpreadCurrent * 2 * Math.PI)))
            );

            Manager_Projectile.AddProjectile_BossProjectile(origin, new_dir, level, who);

            return true;
        }
    }

    public class Gun_BossAvoidPattern1 : Gun_BossScatter
    {

        protected double RotationSpeed;
        protected double Rotation;
        protected int Holes;
        protected int HoleSize;

        public Gun_BossAvoidPattern1(IVictim owner) : base(owner)
        {
            Name = "Pattern boss gun";
            ShotDelay = 100;
            ShotCount = 64;
            ShotSpread = (2 * Math.PI) / ShotCount;
            RotationSpeed = 0.0025f;
            Rotation = 0.0f;
            Holes = 3;
            HoleSize = 6;
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

            int i = 0;
            foreach (var dir in (IterateDirections(direction)))
            {
                if (i++ % (ShotCount / Holes) < HoleSize) continue;
                var position = origin + dir * 50f;
                Manager_Projectile.AddProjectile_BossProjectile(position, dir, level, who, 0.25f);
            }

            return true;
        }

    }

    public class Gun_BossAvoidPattern2 : Gun_BossAvoidPattern1
    {

        public Gun_BossAvoidPattern2(IVictim owner) : base(owner)
        {
            Name = "Pattern boss gun";
            ShotDelay = 800;
            ShotCount = 18;
            ShotSpread = (2 * Math.PI) / ShotCount;
            RotationSpeed = 0.01f;
            Rotation = 0.0f;
            Holes = 1;
            HoleSize = -1;
        }
    }

    public class Gun_BossAOE : Gun_BossScatter
    {
        public Gun_BossAOE(IVictim owner, int shotCount = 48) : base(owner, shotCount)
        {
            Name = "AOE boss gun";
            ShotDelay = 300; //Boss regulates its own AOE shooting
            ShotSpread = 2 * Math.PI / ShotCount;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            foreach (var dir in IterateDirections(direction))
            {
                var position = origin + dir * 30f;
                Manager_Projectile.AddProjectile_BossProjectile(position, dir, level, who, 0.30f);
            }

            return true;
        }

    }

    public class Gun_GigachadScatter : Gun_BossScatter
    {
        public Gun_GigachadScatter(IVictim owner, int shotCount = 10) : base(owner, shotCount)
        {
            Name = "Gigachad Scatter";
            ShotSpread = .5 / ShotCount;
            ShotDelay = 1000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Fireball.Play(1.0f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
                Manager_Projectile.AddProjectile_BossProjectile(origin, dir, level, who, 0.55f);
            return true;
        }
    }

    public class Gun_GigachadAOE : Gun_BossAOE
    {
        public Gun_GigachadAOE(IVictim owner, int shotCount = 48) : base(owner, shotCount)
        {
            Name = "Gigachad AOE gun";
            ShotDelay = 2000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            var sound = new List<SoundEffect>() { Manager_Sound.Sound_Slime6 };
            Manager_Sound.Sound_PlasmaPistol.Play(1.0f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
            {
                var position = origin + dir * 30f;
                Manager_Projectile.AddProjectile_BossProjectile(position, dir, level, who, 0.30f);
            }

            return true;
        }
    }

    public class Gun_GigachadHammer : Gun_BossAOE
    {
        public Gun_GigachadHammer(IVictim owner, int shotCount = 16) : base(owner, shotCount)
        {
            Name = "Gigachad Hammer";
            ShotDelay = 2000;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Slime6.Play(1.0f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
            {
                var position = origin + dir * 30f;
                Manager_Projectile.AddProjectile_PinkHammer(position, dir, level, who);
            }

            return true;
        }
    }

    public class Gun_GigachadSin : Gun_BossScatter
    {
        public Gun_GigachadSin(IVictim owner, int shotCount = 5) : base(owner, shotCount)
        {
            Name = "Gigachad Sin";
            ShotDelay = 300;
            ShotSpread = .5 / ShotCount;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;
            NextShotCooldown = ShotDelay;

            Manager_Sound.Sound_Slime6.Play(1.0f, 0f, 0f);

            foreach (var dir in IterateDirections(direction))
            {
                var position = origin + dir * 30f;
                Manager_Projectile.AddProjectile_BossSin(position, dir, level, who);
            }

            return true;
        }
    }
}