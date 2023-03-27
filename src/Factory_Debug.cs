using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace YGR
{
    /// <summary>
    /// This class contains a bunch of methods intended to draw debugging structures.
    /// 
    /// For new methods, make sure to use the preloaded textures since otherwise the drawing process will be laggy as hell
    /// </summary>
    public static class Factory_Debug
    {
        private static bool _initialized = false;

        private static Texture2D[] _strokes;

        /// <summary>
        /// Initialize the factory
        /// </summary>
        /// <param name="content">ContentManager from Monogame</param>
        public static void Initialize(ContentManager content)
        {
            _strokes = new Texture2D[]{
                content.Load<Texture2D>("stroke_1px"),
                content.Load<Texture2D>("stroke_2px"),
                content.Load<Texture2D>("stroke_3px"),
                content.Load<Texture2D>("stroke_4px"),
                content.Load<Texture2D>("stroke_5px"),
                content.Load<Texture2D>("stroke_6px"),
                content.Load<Texture2D>("stroke_7px"),
                content.Load<Texture2D>("stroke_8px"),
                content.Load<Texture2D>("stroke_9px"),
                content.Load<Texture2D>("stroke_10px")
            };

            _initialized = true;
        }

        /// <summary>
        /// Check if the factory is initialized. Call this at the beginning of every public drawing method
        /// </summary>
        private static void check()
        {
            if (!_initialized)
            {
                Logger.Error("Factory_Debug is not initialized: call Factory_Debug.Initialize() first!");
            }
        }

        /// <summary>
        /// Draw a rectangle: top-left: (x,y), make sure to use global coordinates
        /// </summary>
        /// <param name="x">top-left x coord</param>
        /// <param name="y">top-left y coord</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="lineWidth">Strength of the line</param>
        /// <param name="color"></param>
        /// <param name="spriteBatch">SpriteBatch from monogame</param>
        public static void DrawRectangle(int x, int y, int width, int height, int lineWidth, Color color, SpriteBatch spriteBatch)
        {
            check();

            if (lineWidth > _strokes.Length) {
                lineWidth = _strokes.Length;
            }
            int index = lineWidth - 1;

            spriteBatch.Draw(_strokes[index], new Rectangle(x, y, lineWidth, height + lineWidth), color);
            spriteBatch.Draw(_strokes[index], new Rectangle(x, y, width + lineWidth, lineWidth), color);
            spriteBatch.Draw(_strokes[index], new Rectangle(x + width, y, lineWidth, height + lineWidth), color);
            spriteBatch.Draw(_strokes[index], new Rectangle(x, y + height, width + lineWidth, lineWidth), color);
        }

        /// <summary>
        /// Draw a line from (x,y) that is 'distance' long at 'angle' relative to horizontal (counter-clock-wise).
        /// </summary>
        /// <param name="x">origin-x (has to be global coordinates)</param>
        /// <param name="y">origin-y (has to be global coordinates)</param>
        /// <param name="distance">length of the line</param>
        /// <param name="angle">angle relative to horizontal and counter-clockwise</param>
        /// <param name="lineWidth">width of the line</param>
        /// <param name="color">color :-)</param>
        /// <param name="spriteBatch">SpriteBatch from Monogame</param>
        public static void DrawLine(int x, int y, int distance, float angle, int lineWidth, Color color, SpriteBatch spriteBatch)
        {
            check();

            if (lineWidth > _strokes.Length)
            {
                lineWidth = _strokes.Length;
            }
            int index = lineWidth - 1;

            spriteBatch.Draw(_strokes[index], new Rectangle(x, y, distance, lineWidth), null, color, angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draw a point at (x,y)
        /// </summary>
        /// <param name="x">x-coord (must be global coordinates)</param>
        /// <param name="y">y-coord (must be global coordinates)</param>
        /// <param name="size">width and height value</param>
        /// <param name="color">color :-D</param>
        /// <param name="spriteBatch">SpriteBatch of Monogame</param>
        public static void DrawPoint(int x, int y, int size, Color color, SpriteBatch spriteBatch)
        {
            check();

            if (size > _strokes.Length)
            {
                size = _strokes.Length;
            }
            int index = size - 1;

            spriteBatch.Draw(_strokes[index], new Rectangle(x - size/2, y - size/2, size, size), null, color, 0, new Vector2(0, 0), SpriteEffects.None, 0);
        }
    }
}
