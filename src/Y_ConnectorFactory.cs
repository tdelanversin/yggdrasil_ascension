using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YGR
{
    /// <summary>
    /// Class <c>Y_ConnectorFactory</c> contains methods to create instances of all available connectors.
    /// </summary>
    public class Y_ConnectorFactory
    {
        Texture2D _hConnector;
        Texture2D _vConnector;

        public Y_ConnectorFactory(ContentManager content)
        {
            _vConnector = content.Load<Texture2D>("v_connector");
            _hConnector = content.Load<Texture2D>("h_connector");
        }

        public Y_Connector VConnector(string name)
        {
            return new Y_Connector(
                /* name of the connector */
                name,
                /* direction of the connector */
                Y_ConnectorDirection.Vertical,
                /* texture of the connector */
                _vConnector,
                /* sprite animation window */
                new Rectangle(0, 0, 120, 322),
                /* connecting point on the connector (left for V_ConnectionDirection.Horizontal, bottom for V_ConnectionDirection.Vertical) */
                new Vector2(59, 150),
                /* connecting point (right for V_ConnectionDirection.Horizontal, top for V_ConnectionDirection.Vertical) */
                new Vector2(59, 89),
                /* collision polygon for closed state */
                new X_Polygon(
                    new float[][,]
                    {
                        new float[,] {{0, 70}, { 0, 191}, { 120, 191}, { 120, 70 } }
                    }),
                /* collision polygon for opened state */
                new X_Polygon(
                    new float[][,]
                    {
                        /* left limiter */
                        new float[,] {{0, 70}, { 0, 244}, { 11, 244}, { 11, 70 } },
                        /* right limiter */
                        new float[,] {{108, 70}, { 108, 244}, { 120, 244}, { 120, 70 } }
                    }),
                /* rectangle for left or bottom connector pad */
                new Rectangle(-50, 153, 210, 250),
                /* rectangle for right or top connector pad */
                new Rectangle(-50, -80, 220, 200),
                /* induced spacing distance between two Y_Rooms  */
                28,
                /* animation indices for opening and closing animations */
                new Dictionary<string, int[]> {
                    { "open", new int[] { 0, 1, 2, 3, 4 } },
                    { "close", new int[] { 4, 3, 2, 1, 0 } }
                }
            );
        }

        public Y_Connector HConnector(string name)
        {
            return new Y_Connector(
                name,
                Y_ConnectorDirection.Horizontal,
                _hConnector,
                new Rectangle(0, 0, 217, 126),
                new Vector2(80, 63),
                new Vector2(136, 63),
                /* collision polygon for closed state */
                new X_Polygon(
                    new float[][,]
                    {
                        new float[,] {{62, 0}, { 62, 126}, { 154, 126}, { 154, 0 } }
                    }),
                /* collision polygon for opened state */
                new X_Polygon(
                    new float[][,]
                    {
                        /* top limiter */
                        new float[,] {{62, 0}, { 62, 10}, { 154, 10}, { 154, 0 } },
                        /* bottom limiter */
                        new float[,] {{62, 114}, { 62, 126}, { 154, 126}, { 154, 114 } }
                    }),
                    new Rectangle(-80, -50, 180, 226),
                    new Rectangle(127, -50, 180, 226),
                    19,
                    new Dictionary<string, int[]> {
                        { "open", new int[] { 0, 1, 2, 3, 4 } },
                        { "close", new int[] { 4, 3, 2, 1, 0 } }
                    }
                );
        }
    }
}
