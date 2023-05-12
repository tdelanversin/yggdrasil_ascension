using Microsoft.Xna.Framework;
#nullable enable

namespace YGR
{
    public class Weapon_PinkHammer : Gun_Basic
    {
        public Weapon_PinkHammer(IVictim owner) : base(owner)
        {
            ShotDelay = 1000;
            Name = "Pink Hammer";
            Sprite = Manager_Sprites.Weapon_Hammer;
        }

        public override bool Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Fireball.Play(0.2f, 0, 0);

            NextShotCooldown = ShotDelay;

            Manager_Projectile.AddProjectile_PinkHammer(origin, direction, level, who);
            return true;
        }

        public override void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location)
        {
            room.PickUps.Add(PickUp.Factory(
                Y_PowerUps.WeaponPinkHammer,
                location,
                Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale, lastOwner));
        }
    }
}