using Microsoft.Xna.Framework;

/*
 * Interface to be implemented by all elements that are a projectile
 */

namespace YGR
{
    public interface IProjectile : IGameElement
    {
        public float Age { get; set; }
        public float MaxAge { get; set; }
        public bool DeleteNext { get; set; }
        public void UpdateCollisionAndVelocity(GameTime gameTime);
        public X_CollisionModel_Projectile Collision { get; set; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IGameElement WhoFiredMe { get; set; } 
        public int Damage { get; set; }
    }
}