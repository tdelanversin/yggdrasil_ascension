using Microsoft.Xna.Framework;

namespace YGR
{
    public class Collision
    {
        private ICollidable Obj { get; }
        private Vector2 Position { get; }

        public Collision(ICollidable obj, Vector2 position)
        {
            Position = position;
            Obj = obj;
        }
    }
}
