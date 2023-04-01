
/*
 * Interface to be implemented by everything that can be a victim
 * 
 * For example, if a character is impacted by a projectile, the character has to be a victim
 * 
 * The idea is, that this interface lets the projectile do something to the victim, like deduce points etc.
 */

using Microsoft.Xna.Framework;

namespace YGR
{
    public interface IVictim : IGameElement
    {
        public int LifePoints { get; set; }
        public bool HitInLastLoop { get; set; }
        public X_CollisionModelVictim Collision { get; }
        public Vector2 Velocity { get; set; }
        public IWalkable Room { get; set; }
    }
}
