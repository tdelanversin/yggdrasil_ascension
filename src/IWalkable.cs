using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Yggdrasil;

/*
 * Interface to be implemented by all elements where a player can move around inside
 */

namespace YGR
{
    public interface IWalkable : IGameElement
    {
        public string Name { get; }
        public X_CollisionModelRoom Collision { get; }
        public void SetBackgroundColor(Color color);
        public void ResetBackgroundColor();
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
    }
}
