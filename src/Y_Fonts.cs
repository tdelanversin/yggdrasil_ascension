using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

/*
 * Static fonts, because we need them everywhere
 */

namespace YGR
{
    public static class Fonts
    {
        // Very original font names
        public static SpriteFont Normal;
        public static SpriteFont Large;

        internal static void LoadContent(ContentManager content)
        {
            Normal = content.Load<SpriteFont>("Fonts/font_normal");
            Large = content.Load<SpriteFont>("Fonts/font_large");
        }
    }
}
