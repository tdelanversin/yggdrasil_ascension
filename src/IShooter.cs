using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Interface to be implemented by all elements that shoot bullet sprays
 */

namespace YGR
{
    public interface IShooter
    {
        public string Name { get; }
        public Texture2D Sprite { get; }
        public IVictim Owner { get; set; }
        public Y_PowerUps PowerUpType { get; set; }
        public double ShotDelay { get; set; }
        public double NextShotCooldown { get; set; }

        public bool Shoot(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who);
        public void Update(GameTime gameTime);
        public void DropAsPickUp(IVictim lastOwner, IWalkable room, Point location);
    }
}
