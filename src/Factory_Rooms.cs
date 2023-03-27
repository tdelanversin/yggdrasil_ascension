using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using YRG;
using System.Diagnostics;


namespace YGR
{
    /// <summary>
    /// Class <c>Y_RoomFactory</c> contains methods to create instances of every available room.
    /// </summary>
    public static class Factory_Rooms
    {

        private static Csv_reader csv_reader = new();
        private static Level _level;
        private static bool _initialized = false;

        private static Texture2D _room_0;
        private static Texture2D _room_1;
        private static Texture2D _room_2;
        public static void collision_init()
        {
            var x=_level.x;
            var y=_level.y;
            var width=_level.width;
            var height=_level.height;
            var collision_values = Csv_reader.Csvread();
            Logger.Info("yuhuuuuuuuuuuuuuuuu" + collision_values[0]);
            X_Polygon[][] outline=new X_Polygon[collision_values.Length][];
            for (int i=0; i < collision_values.Length; i++)
            {
                var collision = collision_values[i];
                X_Polygon tile = new X_Polygon(new float[,] { { x + i }, { x + i }, { x + i }, { x + i } });

            }
        }

        public static void Initialize(ContentManager content)
        {
            _room_0 = content.Load<Texture2D>("room_0");
            _room_1 = content.Load<Texture2D>("room_1");
            _room_2 = content.Load<Texture2D>("room_2");

            _initialized = true;
        }

        private static void check()
        {
            if (!_initialized) Logger.Error("Factory_Rooms not initialized: call Factory_Rooms.Initialize(ContentManager) somewhere!");
        }

        public static Y_Room Room_0(string name)
        {
            collision_init();
            check();
            return new Y_Room(
                /* name of the room */
                name,
                /* texture of the room */
                _room_0,
                /* collision detection polygon, has multiple parts and has to surround all collidable parts of the room */
                new X_Polygon(
                    new float[][,] {
                        /*outer border*/

                        //new float[,] {{ 40.0f, 51.0f }, { 40.0f , 1045.0f }, { 1884.0f , 1045.0f }, { 1884.0f, 51.0f } },
                        ///*vertical rectangle on the left*/
                        //new float[,] {{ 298.0f, 278.0f }, { 300.0f, 816.0f }, { 419.0f, 816.0f }, { 419, 278.0f } },
                        ///*horizontal rectangle in the middle*/
                        //new float[,] {{ 573.0f, 441.0f }, { 573.0f, 608.0f }, { 1482.0f, 611.0f }, { 1482, 441.0f } },
                        ///*circular cylinder on the bottom right*/
                        //new float[,] {{ 1653.0f, 739.0f }, { 1615.0f, 752.0f }, { 1600.0f, 796.0f }, { 1618, 860.0f }, { 1655, 868.0f }, { 1697.0f, 858.0f }, { 1712.0f, 796.0f }, { 1685.0f, 750.0f } }
                }),
                /* attaching points for connectors */
                new Dictionary<X_ConnectorSide, int[,]>
                {
                    { X_ConnectorSide.Left, new int[,] { { 17, 277 }, {17, 671} } },
                    { X_ConnectorSide.Right, new int[,] { { 1903, 262 }, { 1903, 652 } } },
                    { X_ConnectorSide.Bottom, new int[,] { { 714, 1062 }, { 1220, 1062 } } },
                    { X_ConnectorSide.Top, new int[,] { { 627, 17 }, { 1032, 17 }, { 1630, 17 } } }
                }
            );
        }

        public static Y_Room Room_1(string name)
        {
            check();

            return new Y_Room(
                name,
                _room_1,
                new X_Polygon(
                    new float[][,] {
                        /*outer border*/
                        new float[,] {{ 38, 38 }, { 38 , 557 }, { 614 , 557 }, { 616, 38 } },
                        /*vertical rectangle on the left*/
                        new float[,] {{ 38, 296 }, { 38, 347 }, { 314, 347 }, { 314, 296 } },
                        /*square cylinder in the middle*/
                        new float[,] {{ 416, 200 }, { 416, 287 }, { 512, 287 }, { 512, 200 } }
                }),
                new Dictionary<X_ConnectorSide, int[,]>
                {
                    { X_ConnectorSide.Left, new int[,] { { 19, 178 } } },
                    { X_ConnectorSide.Right, new int[,] { { 636, 393 } } },
                    { X_ConnectorSide.Bottom, new int[,] { { 327, 576 } } },
                    { X_ConnectorSide.Top, new int[,] { { 221, 19 } } }
                }
            );
        }

        public static Y_Room Room_2(string name)
        {
            check();

            return new Y_Room(
                name,
                _room_2,
                new X_Polygon(
                    new float[][,] {
                        /*outer border*/
                        new float[,] {{ 38, 38 }, { 38 , 557 }, { 425 , 557 }, { 425, 38 } },
                        /*horizontal wall on the left*/
                        new float[,] {{ 38, 295 }, { 38, 349 }, { 314, 349 }, { 314, 296 } },
                }),
                new Dictionary<X_ConnectorSide, int[,]>
                {
                    { X_ConnectorSide.Left, new int[,] { { 19, 191 } } },
                    { X_ConnectorSide.Right, new int[,] { { 446, 185 } } },
                    { X_ConnectorSide.Bottom, new int[,] { { 169, 575 } } },
                    { X_ConnectorSide.Top, new int[,] { { 208, 19 } } }
                }
            );
        }
    }
}
