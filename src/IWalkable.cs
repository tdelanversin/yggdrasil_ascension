using Microsoft.Xna.Framework;
using System.Collections.Generic;

/*
 * Interface to be implemented by all elements where a player can move around inside
 */

namespace YGR
{
    public interface IWalkable : IGameElement
    {
        public string Name { get; }
        public X_CollisionModel_Room Collision { get; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public void MoveTo(Point position);
        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name="");
    }
}
