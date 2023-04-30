using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YGR
{
    public enum X_DoorDirection
    {
        Horizontal = 0,
        Vertical,
        Corner
    }

    public enum X_TileType
    {
        Roof=2,
        Wall,
        Floor,
        Outside,
        DontCare
    }

    public enum X_DoorState
    {
        Open = 1,
        Opening,
        Closing,
        Closed,
        LockedClosed,
        LockedOpen
    }

    public enum X_DoorTextureLayer
    {
        Floor,
        Wall,
        Door,
        Mechanism1,
        Mechanism2
    }

    public class Y_Door : IWalkable
    {
        public float Scale { get; private set; }
        public Rectangle Rect { get; set; }
        public string Name { get; }
        public X_CollisionModel_Room Collision { get; }
        public X_RoomGraph Graph { get; set; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        public int TextureTileSize { get; }
        public string ResourceFolder { get; }

        private new Dictionary<X_DoorState, List<Rectangle>> _doorCollisionRectangles;

        public X_DoorState State { get; set; }

        private X_DoorDirection _direction;
        //private Texture2D _floor;
        //private Texture2D _wall;
        //private Texture2D _door;
        Dictionary<X_DoorTextureLayer, List<X_AutoTiler.X_AutoTileTexture>> _tileTextures;
        Dictionary<X_DoorTextureLayer, List<X_AutoTiler.X_AutoTileColor>> _tileColors;
        static public int NumTilesDoorWidth { get { return 5; } }

        private int _halfHeight;

        private int _currentDoorOpenOffset = 0;
        private float _doorOpeningTime = 2000.0f;
        private float _animationTime = 0.0f;
        int _tileSize;
        int _tileOffset;
        bool[] _illuminated;
        //private Y_Door _door;

        bool _closingTheDoor;


        //Rectangle _outsideRect1;
        //Rectangle _outsideRect2;

        public Y_Door(
            X_DoorDirection direction, 
            int numTilesLength, 
            int tileWidth, 
            int tileHeight, 
            int tileOffset,
            GraphicsDevice graphicsDevice,
            string resourceFolder,
            string tileJsonFile
            )
        {
            _direction = direction;
            _tileOffset = tileOffset;

            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);
            collision = flipToPosition(collision, direction, tileOffset);
            collision = getDoorPoints(collision, tileWidth, tileHeight);
            _illuminated = null;

            Collision = new X_CollisionModel_Room(collision, tileWidth, tileHeight, isRoomCollisionModel: false);

            Rect = new Rectangle(0, 0, tileWidth * collision[0].Length, tileHeight * collision.Length);
            Scale = 1.0f;
            _doorCollisionRectangles = new Dictionary<X_DoorState, List<Rectangle>>();
            ResourceFolder = Util.PathOsNormalization(resourceFolder);

            X_AutoTiler.Resolve<X_DoorTextureLayer>(
                ResourceFolder, tileJsonFile, 
                graphicsDevice, 
                Collision.GetCollisionTemplate(), 
                MapTexture,
                out _tileTextures, out _tileColors);

            Scale = (float)tileHeight / _tileTextures.First().Value.First().Texture().Height;
            TextureTileSize = _tileTextures.First().Value.First().Texture().Height;
            _tileSize = (int)(TextureTileSize * Scale);
            State = X_DoorState.Closed;
            _closingTheDoor = false;

            int width = Collision.GetCollisionTemplate()[0].Length;
            int height = Collision.GetCollisionTemplate().Length;
            Color[] trans = Enumerable.Repeat<Color>(Color.Transparent, _tileSize * _tileSize * width * height).ToArray();
        }

        public static X_ConnectorSide ParseFromSide(string side)
        {
            if (side.ToLower().StartsWith("t")) return X_ConnectorSide.Top;
            if (side.ToLower().StartsWith("b")) return X_ConnectorSide.Bottom;
            if (side.ToLower().StartsWith("l")) return X_ConnectorSide.Left;
            if (side.ToLower().StartsWith("r")) return X_ConnectorSide.Right;

            Logger.Error("Invalid side to parse");

            return X_ConnectorSide.Bottom;
        }

        public static Tuple<int, X_ConnectorSide> ParseToSide(string side)
        {
            if (side == "") return null;

            X_ConnectorSide cs = X_ConnectorSide.Left;
            int index = 0;
            if (side.ToLower().StartsWith("t")) cs = X_ConnectorSide.Top;
            else if (side.ToLower().StartsWith("b")) cs = X_ConnectorSide.Bottom;
            else if (side.ToLower().StartsWith("l")) cs = X_ConnectorSide.Left;
            else if (side.ToLower().StartsWith("r")) cs = X_ConnectorSide.Right;

            if(!int.TryParse(side.Substring(1), out index))
            {
                Logger.Error("Invalid index passed");
            }

            return new Tuple<int, X_ConnectorSide>(index, cs);
        }

        public static void GetDoorType(
            X_ConnectorPoint from, 
            X_ConnectorPoint to, 
            int tileSize,
            out X_DoorDirection direction,
            out int numTilesLength,
            out int tileOffset
            )
        {
            direction = X_DoorDirection.Horizontal;
            numTilesLength = 0;
            tileOffset = 0;
            int lOffset = 3 * tileSize;

            Point fromP = from.Point + new Point(1000000, 1000000);
            Point toP = to.Point + new Point(1000000, 1000000);
            if(from.ConnectorSide == X_ConnectorSide.Top && to.ConnectorSide == X_ConnectorSide.Bottom)
            {
                direction = X_DoorDirection.Vertical;
                tileOffset = toP.X - fromP.X;
                numTilesLength = Math.Abs(fromP.Y - toP.Y) + lOffset;
            }
            else if (from.ConnectorSide == X_ConnectorSide.Bottom && to.ConnectorSide == X_ConnectorSide.Top)
            {
                direction = X_DoorDirection.Vertical;
                tileOffset = fromP.X - toP.X;
                numTilesLength = Math.Abs(fromP.Y - toP.Y) + lOffset;
            }
            else if (from.ConnectorSide == X_ConnectorSide.Left && to.ConnectorSide == X_ConnectorSide.Right)
            {
                direction = X_DoorDirection.Horizontal;
                tileOffset = fromP.Y - toP.Y;
                numTilesLength = Math.Abs(fromP.X - toP.X) + lOffset;
            }
            else if (from.ConnectorSide == X_ConnectorSide.Right && to.ConnectorSide == X_ConnectorSide.Left)
            {
                direction = X_DoorDirection.Horizontal;
                tileOffset = toP.Y - fromP.Y;
                numTilesLength = Math.Abs(fromP.X - toP.X) + lOffset;
            }
            else if (
                (from.ConnectorSide == X_ConnectorSide.Left   && to.ConnectorSide == X_ConnectorSide.Bottom) ||
                (from.ConnectorSide == X_ConnectorSide.Right  && to.ConnectorSide == X_ConnectorSide.Bottom)
                )
            {
                direction = X_DoorDirection.Corner;
                numTilesLength = toP.X - fromP.X;
                tileOffset = Math.Abs(fromP.Y - toP.Y); // Math.Sign(toP.Y - fromP.Y) * 8 * tileSize; // toP.Y - fromP.Y;
                numTilesLength += Math.Sign(numTilesLength) * (lOffset + tileSize);
                tileOffset += Math.Sign(tileOffset) * (lOffset + tileSize);
            }
            else if(
                (from.ConnectorSide == X_ConnectorSide.Top && to.ConnectorSide == X_ConnectorSide.Left) ||
                (from.ConnectorSide == X_ConnectorSide.Top && to.ConnectorSide == X_ConnectorSide.Right)
                )
            {
                direction = X_DoorDirection.Corner;
                numTilesLength = fromP.X - toP.X;
                tileOffset = -Math.Abs(toP.Y - fromP.Y); // Math.Sign(toP.Y - fromP.Y) * 8 * tileSize; // toP.Y - fromP.Y;
                numTilesLength += Math.Sign(numTilesLength) * (lOffset + tileSize);
                tileOffset += Math.Sign(tileOffset) * (lOffset + tileSize);
            }
            else if(
                (from.ConnectorSide == X_ConnectorSide.Bottom && to.ConnectorSide == X_ConnectorSide.Left) ||
                (from.ConnectorSide == X_ConnectorSide.Bottom && to.ConnectorSide == X_ConnectorSide.Right)
                )
            {
                direction = X_DoorDirection.Corner;
                numTilesLength = toP.X - fromP.X;
                tileOffset = Math.Abs(fromP.Y - toP.Y); // Math.Sign(toP.Y - fromP.Y) * 8 * tileSize; // toP.Y - fromP.Y;
                numTilesLength += Math.Sign(numTilesLength) * (lOffset + tileSize);
                tileOffset += Math.Sign(tileOffset) * (lOffset + tileSize);
            }
            else if(
                (from.ConnectorSide == X_ConnectorSide.Left && to.ConnectorSide == X_ConnectorSide.Top) ||
                (from.ConnectorSide == X_ConnectorSide.Right && to.ConnectorSide == X_ConnectorSide.Top)
                )
            {
                direction = X_DoorDirection.Corner;
                numTilesLength = toP.X - fromP.X;
                tileOffset = -Math.Abs(fromP.Y - toP.Y); // Math.Sign(toP.Y - fromP.Y) * 8 * tileSize; // toP.Y - fromP.Y;
                numTilesLength += Math.Sign(numTilesLength) * (lOffset + tileSize);
                tileOffset += Math.Sign(tileOffset) * (lOffset + tileSize);
            }

            if(numTilesLength == 0)
            {
                Logger.Error("Invalid connector setting requested");
            }

            numTilesLength /= tileSize;
            tileOffset /= tileSize;
        }

        public Tuple<X_ConnectorSide, Y_CMRoom> GetOtherDoor(Y_CMRoom room)
        {
            var p = new Point(room.Rect.X + room.Rect.Width / 2, room.Rect.Y + room.Rect.Height / 2);
            foreach (var adjRoom in DoorRooms)
            {
                if (!adjRoom.Value.First().Rect.Contains(p))
                {
                    return new Tuple<X_ConnectorSide, Y_CMRoom>(adjRoom.Key, (Y_CMRoom)adjRoom.Value.First());
                }
            }
            return null;
        }

        public void Illuminate()
        {
            if (!Settings.Lighting) return;

            var lights = new List<X_Light>();
            foreach (var room in DoorRooms)
            {
                var r = (Y_CMRoom)room.Value.First();
                lights.AddRange(r.Lights);
            }

            Vector3 offset = new Vector3(Rect.Location.X / Scale, Rect.Location.Y / Scale, 0);
            _illuminated = Manager_Light.Illuminate(lights, Collision.GetCollisionTemplate(), TextureTileSize, offset, true);

            Color[] red = Enumerable.Repeat<Color>(Color.Green, _tileSize * _tileSize).ToArray();
            foreach (var tile in _tileColors)
            {
                if (tile.Key != X_DoorTextureLayer.Floor) continue;
                for(int t=0; t<tile.Value.Count; ++t) // (var t in tile.Value)
                {

                    var location = tile.Value[t].Location();
                    Rectangle rect = new Rectangle(location.X * _tileSize, location.Y * _tileSize, _tileSize, _tileSize);

                    var light = getRectFromArray(_illuminated, rect);
                    for (int i = 0; i < tile.Value[t].Color().Length; ++i)
                    {
                        var col = tile.Value[t].Color()[i];
                        if (!light[i])
                        {
                            Color nCol = Color.White;
                            nCol.R = (byte)((1 - 0.4f) * col.R + 0.4f * Color.Black.R);
                            nCol.G = (byte)((1 - 0.4f) * col.G + 0.4f * Color.Black.G);
                            nCol.B = (byte)((1 - 0.4f) * col.B + 0.4f * Color.Black.B);
                            tile.Value[t].Color()[i] = nCol;
                        }
                    }
                }
            }

            var colors = _tileColors.Where(x => x.Key == X_DoorTextureLayer.Floor).Select(x => x.Value).First();
            var textures = _tileTextures.Where(x => x.Key == X_DoorTextureLayer.Floor).Select(x => x.Value).First();
            for (int t = 0; t < colors.Count(); ++t)
            {
                textures[t].Texture().SetData<Color>(colors[t].Color());
            };
        }

        private bool[] getRectFromArray(bool[] lighted, Rectangle rect)
        {
            bool[] res = new bool[rect.Width*rect.Height];

            int index = rect.Location.Y * Rect.Width + rect.Location.X;
            for (int r = 0; r < res.Length; r+=_tileSize)
            {
                for(int c=0; c<_tileSize; ++c)
                {
                    res[r+c] = lighted[index+c];
                }
                index += Rect.Width;
            }

            return res;
        }

        public static X_DoorTextureLayer MapTexture(string textureType)
        {
            if (textureType.ToLower().Contains("wall") || textureType.ToLower().Contains("floor")) return X_DoorTextureLayer.Floor;
            if (textureType.ToLower().Contains("roof")) return X_DoorTextureLayer.Wall;
            if (textureType.ToLower().Contains("mechanism1")) return X_DoorTextureLayer.Mechanism1;
            if (textureType.ToLower().Contains("mechanism2")) return X_DoorTextureLayer.Mechanism2;
            return X_DoorTextureLayer.Door;
        }

        public static X_TileType MapJsonName(string tileType)
        {
            if (tileType.ToLower().Contains("floor")) return X_TileType.Floor;
            if (tileType.ToLower().Contains("wall")) return X_TileType.Wall;
            if (tileType.ToLower().Contains("roof")) return X_TileType.Roof;
            if (tileType.ToLower().Contains("out")) return X_TileType.Outside;
            return X_TileType.DontCare;
        }

        private IList<Tuple<int, int>> findMatch(int[,] match, int[][] pattern, int ox, int oy)
        {
            int lx = match.GetLength(0);
            int ly = match.GetLength(1);
            int px = pattern.Length - lx + 1;
            int py = pattern[0].Length - ly + 1;
            IList<Tuple<int, int>> matchPos = new List<Tuple<int, int>>();

            for (int x = 0; x < px; ++x)
            {
                for (int y = 0; y < py; ++y)
                {
                    for (int mx = 0; mx < lx; ++mx)
                    {
                        for (int my = 0; my < ly; ++my)
                        {
                            var p = pattern[x + mx][y + my];
                            var m = match[mx, my];
                            if (p != m && !(p==-1 && m==0)) goto no_match;
                        }
                    }
                    matchPos.Add(new Tuple<int, int>(x+ox, y+oy));
                    no_match: continue;
                }
            }

            return matchPos;
        }

        private int[][] getDoorPoints(int[][] collision, int tileWidth, int tileHeight)
        {
            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            for (int x = 0; x < collision.Length; ++x)
            {
                for (int y = 0; y < collision[0].Length; ++y)
                {
                    var val = collision[x][y];
                    if (val == 2 || val == 4)
                    {
                        var side = (val == 2) ? X_ConnectorSide.Bottom : X_ConnectorSide.Left;
                        Doors.Add(side, new List<X_ConnectorPoint>{ new X_ConnectorPoint(side, new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2))});
                        collision[x][y] = 0;
                    }
                    if (val == 3 || val == 5)
                    {
                        var side = (val == 3) ? X_ConnectorSide.Top : X_ConnectorSide.Right;
                        Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2)) });
                        collision[x][y] = 0;
                    }
                }
            }

            return collision;
        }

        private int[][] flipToPosition(int[][] collision, X_DoorDirection direction, int tileOffset)
        {
            int[][] newCol = null;
            if (direction == X_DoorDirection.Vertical)
            {
                if(tileOffset < 0)
                {
                    // No need to do anything. This is the generation case
                    newCol = collision;
                }
                else
                {
                    // flip horizontally
                    newCol = new int[collision.Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision[0].Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            newCol[x][newCol[0].Length - 1 - y] = collision[x][y];
                        }
                    }
                }
            }
            else if(direction == X_DoorDirection.Horizontal)
            {
                if(tileOffset < 0)
                {
                    // rotate by 90 degrees clock-wise
                    newCol = new int[collision[0].Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision.Length];
                    }
                    for (int x = 0; x < newCol[0].Length; ++x)
                    {
                        for (int y = 0; y < newCol.Length; ++y)
                        {
                            var val = collision[x][y];
                            if (val == 2) val = 4;
                            else if (val == 3) val = 5;
                            newCol[y][newCol[0].Length - 1 - x] = val;
                        }
                    }
                }
                else
                {
                    // rotate by 90 degrees clock-wise
                    // then flip vertically
                    newCol = new int[collision[0].Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision.Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            var val = collision[y][x];
                            if (val == 2) val = 4;
                            else if (val == 3) val = 5;
                            newCol[newCol.Length - 1 - x][newCol[0].Length - 1 - y] = val;
                        }
                    }
                }
            }
            else
            {
                if (tileOffset < 0)
                {
                    newCol = new int[collision.Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision[0].Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            var val = collision[x][y];
                            newCol[newCol.Length - 1 - x][y] = val;
                        }
                    }
                }
                else
                {
                    newCol = collision;
                }
                //output(newCol, "./logs/pattern.csv");
            }

            return newCol;
        }

        private int[][] createDoorTemplate(int numTilesLength, int tileOffset)
        {
            if(_direction != X_DoorDirection.Corner)
            {
                int width = numTilesLength;
                int doorWidth = NumTilesDoorWidth;
                int height = doorWidth + Math.Abs(tileOffset);

                int[][] collision = new int[width][];
                for (int x = 0; x < width; ++x)
                {
                    collision[x] = new int[height];
                    for (int y = 0; y < height; ++y)
                    {
                        collision[x][y] = 0;
                    }
                }

                int half1 = (int)Math.Floor((float)width / 2.0f) + 2;
                _halfHeight = half1;

                for (int x = 1; x < half1; ++x)
                {
                    collision[x][0] = 1;
                }

                for (int y = doorWidth; y < collision[0].Length; ++y)
                {
                    collision[0][y] = -1;
                    collision[1][y] = -1;
                }
                for (int x = 1; x < half1 - doorWidth; ++x)
                {
                    collision[x][doorWidth - 1] = 1;
                    for (int y = doorWidth; y < collision[0].Length; ++y) collision[x][y] = -1;
                }

                for (int x = half1 - doorWidth; x < width - 1; ++x)
                {
                    collision[x][height - 1] = 1;
                }

                for (int x = half1; x < width - 1; ++x)
                {
                    collision[x][Math.Abs(tileOffset)] = 1;
                    for (int y = 0; y <= Math.Abs(tileOffset) - 1; ++y) collision[x][y] = -1;
                }
                for (int y = 0; y <= Math.Abs(tileOffset) - 1; ++y)
                {
                    collision[width - 1][y] = -1;
                    collision[width - 2][y] = -1;
                }

                for (int y = 0; y < Math.Abs(tileOffset); ++y)
                {
                    collision[half1 - 1][y + 1] = 1;
                    collision[half1 - doorWidth][height - y - 2] = 1;
                }
                for (int y = 0; y < collision[0].Length; ++y)
                {
                    if (collision[1][y] != 0) collision[0][y] = -1;
                    if (collision[width - 2][y] != 0) collision[width - 1][y] = -1;
                }
                //for (int y = 0; y < collision[0].Length; ++y)
                //{
                //    collision[0][y] = -1;
                //    collision[width - 1][y] = -1;
                //}

                collision[width - 2][height - 1 - doorWidth / 2] = 2;
                collision[1][doorWidth / 2] = 3;

                return collision;
            }
            else
            {
                int width = Math.Abs(numTilesLength);
                int height = Math.Abs(tileOffset);

                int[][] collision = new int[height][];

                for (int x = 0; x < height; ++x)
                {
                    collision[x] = new int[width];
                    for (int y = 0; y < width; ++y)
                    {
                        collision[x][y] = 0;
                    }
                }

                for(int i=1; i < width-1; ++i)
                {
                    collision[height - 1][i] = 1;
                }

                int openingOffset = width - NumTilesDoorWidth;
                if(numTilesLength < 0)
                {
                    for (int i = NumTilesDoorWidth-1; i < width-1; ++i)
                    {
                        collision[height - NumTilesDoorWidth][i] = 1;
                    }
                    for (int i = 1; i < height; ++i)
                    {
                        collision[i][0] = 1;
                    }
                    for (int i = 1; i < height-NumTilesDoorWidth; ++i)
                    {
                        collision[i][NumTilesDoorWidth-1] = 1;
                    }
                    for (int i = 0; i < height - NumTilesDoorWidth; ++i)
                    {
                        for (int j = NumTilesDoorWidth; j < width; ++j)
                        {
                            collision[i][j] = -1;
                        }
                    }
                    // vertical part on the left => horizontal entry on the right
                    collision[height - NumTilesDoorWidth/2 - 1][width-2] = 5;
                    collision[1][NumTilesDoorWidth / 2] = (tileOffset > 0 ? 3 : 2);
                    collision[0][0] = -1;
                    collision[0][NumTilesDoorWidth-1] = -1;
                    collision[height-1][width-1] = -1;
                    collision[height - NumTilesDoorWidth][width-1] = -1;
                }
                else
                {
                    for (int i = 1; i < width- NumTilesDoorWidth; ++i)
                    {
                        collision[height - NumTilesDoorWidth][i] = 1;
                    }

                    for (int i = 1; i < height; ++i)
                    {
                        collision[i][width-1] = 1;
                    }
                    for (int i = 1; i < height - NumTilesDoorWidth+1; ++i)
                    {
                        collision[i][width - NumTilesDoorWidth] = 1;
                    }
                    for (int i = 0; i < height - NumTilesDoorWidth; ++i)
                    {
                        for(int j=0; j<width- NumTilesDoorWidth; ++j)
                        {
                            collision[i][j] = -1;
                        }
                    }
                    // vertical part on the right => horizontal entry on the left
                    collision[height - NumTilesDoorWidth / 2 - 1][1] = 4;
                    collision[1][width - 1 - NumTilesDoorWidth / 2] = (tileOffset > 0 ? 3 : 2);
                    collision[0][width - 1] = -1;
                    collision[0][width - NumTilesDoorWidth] = -1;
                    collision[height-1][0] = -1;
                    collision[height - NumTilesDoorWidth][0] = -1;
                }

                //output(collision, "./logs/pattern.csv");

                return collision;
            }
        }

        public bool DoorIsOpen()
        {
            return State == X_DoorState.Open || State == X_DoorState.LockedOpen;
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!Doors.ContainsKey(side)) return null;
            return Doors[side].First();
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms. The method acts out of the PERSPECTIVE OF THE CONNECTOR!</para>
        /// <para>The connector is in the middle</para>
        /// 
        /// <br>-------------------- </br>
        /// <br>left |||XXX||| right </br>
        /// <br>-------------------- </br>
        ///  
        /// <para>OR</para>
        /// 
        /// <br>  top   </br>
        /// <br> |||||  </br>
        /// <br> |XXX|  </br>
        /// <br> |||||  </br>
        /// <br> bottom </br>
        /// 
        /// <para>side1 = X_ConnectorSide.Left means, that room1 is on the left side of the connector.</para>
        /// </summary>
        /// <param name="side1">The origin side: the connector will place the other side with respect to this side</param>
        /// <param name="from">The Y_Room on side1 (the origin side)</param>
        /// <param name="side2">The adjusted side: the connector will place this side with respect to the other side</param>
        /// <param name="to">The Y_Room on side 2 (the adjusted side)</param>
        /// <param name="random">Random generator for the random connector point selection</param>
        /// <returns></returns>
        public IWalkable Connect(
            X_ConnectorSide connectorSide1,
            IWalkable from,
            X_ConnectorSide connectorSide2,
            IWalkable to
        )
        {
            /*
             * If we use 
             *    connect(Left, room1, Right, room2)
             * we have to FLIP the sides for the next call because for the left side of the connector
             * we need a point on the right side of room 1 and for the right side of the connector, we need
             * a point on the left side of room 2!!!!!!!
             */

            if (connectorSide1 == X_ConnectorSide.Left) connectorSide1 = X_ConnectorSide.Right;
            else if (connectorSide1 == X_ConnectorSide.Right) connectorSide1 = X_ConnectorSide.Left;
            else if (connectorSide1 == X_ConnectorSide.Top) connectorSide1 = X_ConnectorSide.Bottom;
            else if (connectorSide1 == X_ConnectorSide.Bottom) connectorSide1 = X_ConnectorSide.Top;

            if (connectorSide2 == X_ConnectorSide.Left) connectorSide2 = X_ConnectorSide.Right;
            else if (connectorSide2 == X_ConnectorSide.Right) connectorSide2 = X_ConnectorSide.Left;
            else if (connectorSide2 == X_ConnectorSide.Top) connectorSide2 = X_ConnectorSide.Bottom;
            else if (connectorSide2 == X_ConnectorSide.Bottom) connectorSide2 = X_ConnectorSide.Top;

            return ConnectAndMove(from, from.GetConnectorPoint(connectorSide1), to, to.GetConnectorPoint(connectorSide2));
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms. The method acts out of the PERSPECTIVE OF THE ROOMS!</para>
        /// <para>The connector is in the middle</para>
        /// 
        /// <br>-------------------- </br>
        /// <br>left |||XXX||| right </br>
        /// <br>-------------------- </br>
        ///  
        /// <para>OR</para>
        /// 
        /// <br>  top   </br>
        /// <br> |||||  </br>
        /// <br> |XXX|  </br>
        /// <br> |||||  </br>
        /// <br> bottom </br>
        /// 
        /// <para>If room1 is on the LEFT side of the connector, then the connectorPoint1 must be on the RIGHT side of room1.</para>
        /// </summary>
        /// <param name="room1">Y_Room with connectorPoint1</param>
        /// <param name="connectorPoint1">The connector point on Y_Room 1</param>
        /// <param name="room2">Y_Room with connectorPoint2</param>
        /// <param name="connectorPoint2">The connector point on Y_Room 2</param>
        /// <returns></returns>
        public IWalkable ConnectAndMove(
            IWalkable room1,
            X_ConnectorPoint roomConnectorPoint1,
            IWalkable room2,
            X_ConnectorPoint roomConnectorPoint2
        )
        {
            // check possibilities
            // horizontal: left and right or right and left
            // vertical: top and bottom or bottom and top
            bool hCheck1 = roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Left && roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Right;
            bool hCheck2 = roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Right && roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Left;
            bool vCheck1 = roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Bottom && roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Top;
            bool vCheck2 = roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Top && roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Bottom;

            // horizontal connectors only with left and right or right and left
            // vertical connectors only with top and bottom or bottom and top
            if (!(
                ((hCheck1 || hCheck2) && _direction == X_DoorDirection.Horizontal) ||
                ((vCheck1 || vCheck2) && _direction == X_DoorDirection.Vertical))
                )
            {
                Logger.Error("Invalid connector combination");
            }

            /* left to right */
            if (hCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Right].First().Point;
                MoveTo(roomConnectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Left].First().Point + room2.Rect.Location - roomConnectorPoint2.Point);
            }
            /* right to left */
            else if (hCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Left].First().Point;
                MoveTo(roomConnectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Right].First().Point + room2.Rect.Location - roomConnectorPoint2.Point);
            }
            /* bottom to top */
            else if (vCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Top].First().Point;
                MoveTo(roomConnectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Bottom].First().Point + room2.Rect.Location - roomConnectorPoint2.Point);
            }
            /* top to bottom */
            else if (vCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Bottom].First().Point;
                MoveTo(roomConnectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Top].First().Point + room2.Rect.Location - roomConnectorPoint2.Point);
            }

            // the following flip of connectorPoint2 and connectorPoint1 is NOT a bug
            DoorRooms.Add(roomConnectorPoint2.ConnectorSide, new List<IWalkable> { room1 });
            DoorRooms.Add(roomConnectorPoint1.ConnectorSide, new List<IWalkable> { room2 });
            room1.DoorRooms.Add(roomConnectorPoint1.ConnectorSide, new List<IWalkable> { this });
            room2.DoorRooms.Add(roomConnectorPoint2.ConnectorSide, new List<IWalkable> { this });

            return this;
        }

        public IWalkable Connect(
            IWalkable room1,
            X_ConnectorPoint roomConnectorPoint1,
            IWalkable room2,
            X_ConnectorPoint roomConnectorPoint2,
            X_DoorDirection direction
        )
        {
            if(direction != X_DoorDirection.Corner)
            {
                DoorRooms.Add(roomConnectorPoint2.ConnectorSide, new List<IWalkable> { room1 });
                DoorRooms.Add(roomConnectorPoint1.ConnectorSide, new List<IWalkable> { room2 });
                room1.DoorRooms.Add(roomConnectorPoint1.ConnectorSide, new List<IWalkable> { this });
                room2.DoorRooms.Add(roomConnectorPoint2.ConnectorSide, new List<IWalkable> { this });

                var deltaPC = Rect.Location - Doors[roomConnectorPoint2.ConnectorSide].First().Point;
                MoveTo(roomConnectorPoint1.Point + deltaPC);
            }
            else
            {
                X_ConnectorSide connectorSide1;
                if (roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Top) connectorSide1 = X_ConnectorSide.Bottom;
                else if (roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Bottom) connectorSide1 = X_ConnectorSide.Top;
                else if (roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Left) connectorSide1 = X_ConnectorSide.Right;
                else /*if (roomConnectorPoint1.ConnectorSide == X_ConnectorSide.Right)*/ connectorSide1 = X_ConnectorSide.Left;

                X_ConnectorSide connectorSide2;
                if (roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Top) connectorSide2 = X_ConnectorSide.Bottom;
                else if (roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Bottom) connectorSide2 = X_ConnectorSide.Top;
                else if (roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Left) connectorSide2 = X_ConnectorSide.Right;
                else /*if (roomConnectorPoint2.ConnectorSide == X_ConnectorSide.Right)*/ connectorSide2 = X_ConnectorSide.Left;

                DoorRooms.Add(connectorSide1, new List<IWalkable> { room1 });
                DoorRooms.Add(connectorSide2, new List<IWalkable> { room2 });
                room1.DoorRooms.Add(roomConnectorPoint1.ConnectorSide, new List<IWalkable> { this });
                room2.DoorRooms.Add(roomConnectorPoint2.ConnectorSide, new List<IWalkable> { this });

                var deltaPC = Rect.Location - Doors[connectorSide1].First().Point;
                MoveTo(roomConnectorPoint1.Point + deltaPC);
            }

            return this;
        }
            

        public void SplitConnectedCollisionModels()
        {
            foreach (var room in DoorRooms)
            {
                if (room.Key == X_ConnectorSide.Left || room.Key == X_ConnectorSide.Right)
                {
                    foreach (var r in room.Value)
                    {
                        // need vertical split
                        var res = r.Collision.SplitCollisionVerticallyAt(Doors[room.Key].First().Point, NumTilesDoorWidth);
                        foreach(var rects in res)
                        {
                            List<Rectangle> cRect;
                            if (!_doorCollisionRectangles.TryGetValue(rects.Key, out cRect))
                            {
                                _doorCollisionRectangles.Add(rects.Key, rects.Value);
                            }
                            else _doorCollisionRectangles[rects.Key].AddRange(rects.Value);
                        }
                        if(r.WhatAreYou() == X_LevelElements.Room)
                        {
                            ((Y_CMRoom)r).ResetRects.AddRange(res[X_DoorState.Closed]);
                        }
                    }
                }
                else
                {
                    foreach (var r in room.Value)
                    {
                        // need horizontal split                    
                        var res = r.Collision.SplitCollisionHorizontallyAt(Doors[room.Key].First().Point, NumTilesDoorWidth);
                        foreach (var rects in res)
                        {
                            List<Rectangle> cRect;
                            if (!_doorCollisionRectangles.TryGetValue(rects.Key, out cRect))
                            {
                                _doorCollisionRectangles.Add(rects.Key, rects.Value);
                            }
                            else _doorCollisionRectangles[rects.Key].AddRange(rects.Value);
                        }
                        if (r.WhatAreYou() == X_LevelElements.Room)
                        {
                            ((Y_CMRoom)r).ResetRects.AddRange(res[X_DoorState.Closed]);
                        }
                    }
                }
            }
            foreach(var state in _doorCollisionRectangles)
            {
                state.Value.AddRange(Collision.GetCollisionRectangles());
            }

            if(_doorCollisionRectangles.ContainsKey(X_DoorState.Closed))
                Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Closed]);
        }

        public bool LockDoor()
        {
            if (!(State == X_DoorState.Open || State == X_DoorState.Closed)) return false;
            if(State == X_DoorState.Open)
                State = X_DoorState.LockedOpen;
            if(State == X_DoorState.Closed)
                State = X_DoorState.LockedClosed;

            return true;
        }

        public void Update(GameTime gameTime)
        {
            bool keyPressed = Input.IsKeyTriggered(Keybinds.ToggleConnectors);
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            switch (State)
            {
                case X_DoorState.Closed:
                    if (keyPressed)
                    {
                        State = X_DoorState.Opening;
                    }
                    break;
                case X_DoorState.Opening:
                    if (!doorAnimation(dt, true))
                    {
                        State = X_DoorState.Open;
                        OpenDoor();
                        foreach (var room in DoorRooms.Values) room.First().Illuminate();
                    }
                    break;
                case X_DoorState.Open:
                    if (keyPressed)
                    {
                        State = X_DoorState.Closing;
                    }
                    break;
                case X_DoorState.Closing:
                    if (!_closingTheDoor)
                    {
                        CloseDoor();
                        _closingTheDoor = true;
                    }
                    if (!doorAnimation(dt, false))
                    {
                        _closingTheDoor = false;
                        State = X_DoorState.Closed;
                        foreach (var room in DoorRooms.Values) room.First().Illuminate();
                    }
                    break;
            }
        }

        public void OpenDoor()
        {
            if (_doorCollisionRectangles.ContainsKey(X_DoorState.Open))
                Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Open]);
        }

        public void CloseDoor()
        {
            if (_doorCollisionRectangles.ContainsKey(X_DoorState.Closed))
                Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Closed]);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.Orange, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            foreach(var door in Doors)
            {
                foreach(var d in door.Value)
                {
                    d.DrawOutline(gameTime, globalOffset, spriteBatch);
                }
            }
        }

        private bool doorAnimation(float dt, bool opening)
        {
            _animationTime += dt;
            if (_animationTime < _doorOpeningTime)
            {
                float percent = 1.0f / _doorOpeningTime * _animationTime;
                if(opening)
                    _currentDoorOpenOffset = (int)Math.Round(_tileSize*percent);
                else
                    _currentDoorOpenOffset = (int)Math.Round(_tileSize * (1.0f - percent));
            }
            if (_animationTime >= _doorOpeningTime)
            {
                if(opening)
                    _currentDoorOpenOffset = _tileSize;
                else
                    _currentDoorOpenOffset = 0;
                _animationTime = 0;
                return false;
            }
            return true;
        }

        private void draw(
            List<X_AutoTiler.X_AutoTileTexture> textures, 
            Vector2 position, 
            SpriteBatch spriteBatch,
            bool partial)
        {
            foreach(var t in textures)
            {
                int height = t.Texture().Height;
                int width = t.Texture().Width;
                Vector2 pos = position + t.Location().ToVector2() * _tileSize;

                if (partial)
                {
                    if (_direction == X_DoorDirection.Horizontal)
                    {
                        if (pos.X > _tileSize && pos.X < Rect.Width - _tileSize) height = _currentDoorOpenOffset;
                    }
                    else
                    {
                        height = _currentDoorOpenOffset;
                    }
                }

                spriteBatch.Draw(
                    t.Texture(), pos,
                    new Rectangle(0, 0, width, height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Vector2 position = Rect.Location.ToVector2();
            Vector2 movePosition = position;
            movePosition.Y += _currentDoorOpenOffset;
            if (State == X_DoorState.Closed || State == X_DoorState.LockedClosed)
            {
                //draw(_tileTextures[X_DoorTextureLayer.Wall], position, spriteBatch, false);
                //draw(_tileTextures[X_DoorTextureLayer.Door], position, spriteBatch, false);
            }
            else if(State == X_DoorState.Opening || State == X_DoorState.Closing)
            {
                draw(_tileTextures[X_DoorTextureLayer.Floor],position, spriteBatch, true);
                draw(_tileTextures[X_DoorTextureLayer.Door], movePosition, spriteBatch, false);
                if(_currentDoorOpenOffset%2 == 0)
                    draw(_tileTextures[X_DoorTextureLayer.Mechanism1], position, spriteBatch, false);
                else
                    draw(_tileTextures[X_DoorTextureLayer.Mechanism2], position, spriteBatch, false);
                draw(_tileTextures[X_DoorTextureLayer.Wall], position, spriteBatch, false);
            }
            else
            {
                draw(_tileTextures[X_DoorTextureLayer.Floor],position, spriteBatch, false);
                draw(_tileTextures[X_DoorTextureLayer.Wall], position, spriteBatch, false);
            }
        }

        public void MoveTo(Point position)
        {
            var p = position - Rect.Location;
            Rect = new Rectangle(p.X, p.Y, Rect.Width, Rect.Height);
            foreach(var door in Doors)
            {
                foreach(var d in door.Value)
                {
                    d.MoveBy(p);
                }
            }
            Collision.MoveBy(p);

            foreach(var rect in _doorCollisionRectangles)
            {
                foreach(var r in rect.Value)
                    r.Offset(p);
            }
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Door;
        }
    }
}
