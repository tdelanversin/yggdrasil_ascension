using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        public void MoveTo(Point position);
        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name="");
        public ref Texture2D GetFloor();
        public void Illuminate(X_Light light);
        public int TextureTileSize { get; }
    }
}
