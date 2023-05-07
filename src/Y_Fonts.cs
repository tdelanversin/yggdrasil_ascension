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
        public static SpriteFont Small;
        public static SpriteFont Medium;
        public static SpriteFont Large;

        internal static void LoadContent(ContentManager content)
        {
            Small = content.Load<SpriteFont>("Fonts/font_small");
            Medium = content.Load<SpriteFont>("Fonts/font_medium");
            Large = content.Load<SpriteFont>("Fonts/font_large");
        }

        /// <summary>
        /// Add an ounce of HiDPI support to the game
        /// </summary>
        internal static SpriteFont GetDecentlySizedFont()
        {
            int windowHeight = Camera.Bounds.Height;

            if (windowHeight < 1080)
            {
                return Small;
            }
            else if (windowHeight < 1440)
            {
                return Medium;
            }
            else // if (windowHeight >= huge)
            {
                return Large;
            }
        }
    }
}
