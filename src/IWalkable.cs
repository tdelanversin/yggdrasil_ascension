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
        public void SetBackgroundColor(Color color);
        public void ResetBackgroundColor();
        public Vector2 Clamp(Rectangle rect, Vector2 pos, Vector2 dp, ref IWalkable who, ref Vector2 where);
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public bool Intersects(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal);
    }
}
