using Microsoft.Xna.Framework;

/*
 * Interface to be implemented by all elements where a player can move around inside
 */

namespace YGR
{
    public interface IWalkable : ILevelElement
    {
        public string Name { get; }
        public void SetBackgroundColor(Color color);
        public void ResetBackgroundColor();
        public Vector2 Clamp(Rectangle rect, Vector2 pos, Vector2 dp, ref IWalkable who, ref Vector2 where);
    }
}
