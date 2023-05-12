
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
        public float LifePoints { get; }
        public float LifePointsMax { get; }
        public Color Color { get; set; }
        public bool Confused { get; set; }
        public X_CollisionModel_Victim Collision { get; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        void Hit(IProjectile projectile);
        public string Name { get; set; }
        IGameElement WhoKilledMe { get; set; }
    }
}
