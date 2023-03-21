using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Interface to be implemented by everything that has to be drawn and updated somehow by the main loop
 */

namespace YGR
{
    public interface IGameElement
    {
        public void Update(GameTime gameTime);
        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch);
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch);
    }
}
