using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public interface ICollisionModel
    {
        public void MoveTo(Point position);
        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch);
        public bool Intersect(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal);
        public bool IntersectFast(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal);
    }
}
