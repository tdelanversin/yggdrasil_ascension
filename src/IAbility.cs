using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public interface IAbility
    {
        public bool Trigger(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who);

        public void Update(GameTime gameTime);

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch);

        public string Name { get; }

        public Texture2D Sprite { get; }

        public bool Triggered { get; }
    }
}
