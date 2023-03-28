using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Interface to be implemented by all elements that are a projectile
 */

namespace YGR
{
    public interface IProjectile : IGameElement
    {
        //public Texture2D _sprite { get; set; }
        //public Rectangle _window { get; set; }
        //public Vector2 _position { get; set; }
        //public Vector2 _direction { get; set; }
        public double TimeCreated { get; set; }

        //public bool _isEnemy { get; set; }
        public string Name { get; set; }
        public bool DeleteNext { get; set; }

        //public void Update(GameTime gameTime);
        //public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch);
    }
}