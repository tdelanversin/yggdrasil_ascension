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
        public X_RoomGraph Graph { get; set; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        public void MoveTo(Point position);
        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name="");
        public string ResourceFolder { get; }
        
        public List<PickUp> PickUps { get; }
        public string Category { get; }
        public void ResetRoom();

        public X_IlluminationResources IlluminationResources { get; set; }
        public Color[] Shade { get; set; }
        public Texture2D[] ShadeTexture { get; set; }
        public int ShadeIndex { get; set; }
        public int GetTargetShadeIndex();
        public void SwitchTargetShadeIndex();
        public Vector3 Offset { get; set; }
        public List<X_Light> Lights { get; set; }
        public List<X_Light> GetAllRelevantLights();
        public bool IsVisible();
        public void SetVisible(bool yes);
    }
}
