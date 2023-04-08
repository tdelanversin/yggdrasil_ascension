using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    /// <summary>
    /// Enum <e>X_ConnectorSide</e> enumerates all the sides that X_ConnectorPoint can be located on a Y_Room
    /// </summary>
    public enum X_ConnectorSide
    {
        Left = 0,
        Top,
        Right,
        Bottom
    }

    /// <summary>
    /// Class <c>X_ConnectorPoint</c> models the points on the Y_Room walls where a Y_Connector can be attached to.
    /// </summary>
    public class X_ConnectorPoint
    {
        // Specifiey the side of the Y_Room the point is located
        // Left: the point should be located on the left wall of the Y_Room
        public X_ConnectorSide ConnectorSide { get; }
        // The point itself in local coordinates
        public Point Point { get; private set; }

        public X_ConnectorPoint(X_ConnectorSide side, Point point)
        {

            ConnectorSide = side;
            Point = point;
        }

        public void MoveTo(Point position)
        {
            Point = position + Point;
        }

        /// <summary>
        /// This Method will draw all internal structures for debugging purposes
        /// Note: potentially heavy impact on performace
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            var color = Color.White;
            int size = 11;
            switch (ConnectorSide)
            {
                case X_ConnectorSide.Left:
                    color = Color.Red;
                    break;
                case X_ConnectorSide.Right:
                    color = Color.Blue;
                    size = 7;
                    break;
                case X_ConnectorSide.Top:
                    color = Color.Green;
                    break;
                case X_ConnectorSide.Bottom:
                    color = Color.Yellow;
                    size = 7;
                    break;
            }

            Factory_Debug.DrawPoint((int)(Point.X + globalOffset.X), (int)(Point.Y + globalOffset.Y), size, color, spriteBatch);
        }
    }
}
