using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Interface to be implemented by all elements that shoot bullet sprays
 */

namespace YGR
{
    public interface IShooter 
    {
        public bool Shoot(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who);

        public void Update(GameTime gameTime);

        public string Name { get; }

        public Texture2D Sprite { get; }
    }
}
