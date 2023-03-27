using Microsoft.Xna.Framework;

/*
 * Interface to be implemented by all elements that shoot bullet sprays
 */

namespace YGR
{
    public interface IShooter 
    {
        public void Shoot(GameTime gametime, Vector2 origin, Vector2 direction);
    }
}
