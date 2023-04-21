using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Assimp.Unmanaged;
using Newtonsoft.Json;
using System.Reflection.Emit;
using Nvidia.TextureTools;
using System.Reflection;
using CppNet;

namespace YGR
{
    public enum X_DoorDirection
    {
        Horizontal = 0,
        Vertical
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

    internal enum X_DoorTextureLayer
    {
        Floor,
        Wall,
        Door,
        Mechanism
    }

    internal class X_AutoTiler
    {

        internal struct Textel
        {
            public int x;
            public int y;
        }

        internal class TileData
        {
            public string[,][] mask;
            public List<Textel> coordinates;
        }

        internal class Data
        {
            public string textureName;
            public int size;
            public Dictionary<string, TileData> tiles;
        }

        internal class TileInfo
        {
            public int tileSize;
            public Dictionary<string, List<Color[]>> tiles;
            public Dictionary<string, List<Texture2D>> tilesTexture;
            public Dictionary<string, X_TileType[,][]> masks;
        }

        private static Random random;

        private static Dictionary<string, TileInfo> tileInfo;

        private static X_DoorTextureLayer mapTexture(string textureType)
        {
            if (textureType.ToLower().Contains("wall") || textureType.ToLower().Contains("floor")) return X_DoorTextureLayer.Floor;
            if (textureType.ToLower().Contains("roof")) return X_DoorTextureLayer.Wall;
            return X_DoorTextureLayer.Door;
        }

        private static X_TileType[,][] map(string[,][] mask, Func<string, X_TileType> mapJsonName)
        {
            X_TileType[,][] res = new X_TileType[3, 3][];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    res[i, j] = new X_TileType[mask[i, j].Length];
                    for(int k=0; k< mask[i, j].Length; ++k)
                    {
                        res[i, j][k] = mapJsonName(mask[i, j][k]); 
                    }
                }
            }
            return res;
        }

        public static void Initialize(string resourceFolder, string jsonFileName, GraphicsDevice graphicsDevice, Func<string, X_TileType> mapJsonName)
        {
            X_AutoTiler.Data data;
            using (StreamReader stream = new StreamReader(resourceFolder + jsonFileName))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                data = JsonConvert.DeserializeObject<X_AutoTiler.Data>(array.ToString());
            }

            random = new Random();

            Texture2D texture;
            using (FileStream fileStream = new FileStream(resourceFolder + data.textureName, FileMode.Open))
                texture = Texture2D.FromStream(graphicsDevice, fileStream);

            if (tileInfo == null) tileInfo = new Dictionary<string, TileInfo>();

            string staticKey = resourceFolder + jsonFileName;
            if (tileInfo.ContainsKey(staticKey)) return;

            TileInfo info = new TileInfo();
            tileInfo.Add(resourceFolder + jsonFileName, info);

            info.tileSize = data.size;

            info.tiles = new Dictionary<string, List<Color[]>>();
            info.tilesTexture = new Dictionary<string, List<Texture2D>>();

            info.masks = new Dictionary<string, X_TileType[,][]>();
            //textures = new Dictionary<string, X_DoorTextureLayer>();
            foreach (var t in data.tiles)
            {
                info.tiles.Add(t.Key, new List<Color[]>());
                info.tilesTexture.Add(t.Key, new List<Texture2D>());
                foreach (var e in t.Value.coordinates)
                {
                    Color[] temp = new Color[data.size * data.size];
                    texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                    info.tiles[t.Key].Add(temp);

                    Texture2D ttemp = new Texture2D(graphicsDevice, data.size, data.size);
                    ttemp.SetData<Color>(temp);
                    info.tilesTexture[t.Key].Add(ttemp);
                }

                info.masks.Add(t.Key, map(t.Value.mask, mapJsonName));
            }
        }

        private static bool evalTile(X_TileType p, X_TileType[] candidates)
        {
            bool match = false;
            foreach(var c in candidates)
            {
                if (c == X_TileType.DontCare) return true;
                match = match || c == p;
            }
            return match;
        }

        private static List<Tuple<int, int>> match(X_TileType[][] pattern, X_TileType[,][] tile)
        {
            List<Tuple<int, int>> res = new List<Tuple<int, int>>();
            for (int y = 1; y < pattern.Length - 1; ++y)
            {
                for (int x = 1; x < pattern[0].Length-1; ++x)
                {
                    for (int j = -1; j <= 1; ++j)
                    {
                        for (int i = -1; i <= 1; ++i)
                        {
                            if(!evalTile(pattern[y + j][x + i], tile[j+1,i+1])) goto no_match;
                        }
                    }
                    res.Add(new Tuple<int, int>(x-1, y-1));
                no_match: continue;
                }
            }
            return res;
        }

        private static void output(List<Tuple<int, int>> coords, string fileName, int width, int height)
        {
            int[,] p = new int[height, width];
            foreach (var c in coords) p[c.Item2, c.Item1] = 1;

            string s = "";
            for (int x = 0; x < p.GetLength(0); ++x)
            {
                for (int y = 0; y < p.GetLength(1); ++y)
                {
                    s += "\t" + p[x,y];
                }
                s += "\n";
            }

            File.WriteAllText(fileName, s);
        }

        private static void output(X_TileType[][] pattern, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.GetLength(0); ++x)
            {
                s += string.Join("\t", pattern[x]);
                s += "\n";
            }

            File.WriteAllText(name, s);
        }

        public static void Resolve(
            string resourceFolder, string jsonFileName,
            GraphicsDevice graphicsDevice, 
            int[][] pattern, 
            out Dictionary<X_DoorTextureLayer, List<Tuple<Point, Texture2D>>> texture, 
            out Dictionary<X_DoorTextureLayer, List<Tuple<Point, Color[]>>> color)
        {
            X_TileType[][] padded = new X_TileType[pattern.Length + 2][];
            for (int i = 0; i < pattern.Length + 2; ++i)
            {
                padded[i] = new X_TileType[pattern[0].Length + 2];
                if (i == 0 || i == padded.Length - 1)
                {
                    for (int j = 0; j < padded[0].Length; ++j)
                    {
                        padded[i][j] = X_TileType.Outside;
                    }
                }
                else
                {
                    padded[i][0] = X_TileType.Outside;
                    padded[i][pattern[0].Length + 1] = X_TileType.Outside;
                }
            }
            for (int i = 0; i < pattern.Length; ++i)
            {
                for (int j = 0; j < pattern[0].Length; ++j)
                {
                    padded[i + 1][j + 1] = (X_TileType)pattern[i][j];
                }
            }

            //texture = new Dictionary<X_DoorTextureLayer, Texture2D>();
            texture = new Dictionary<X_DoorTextureLayer, List<Tuple<Point, Texture2D>>>();
            color = new Dictionary<X_DoorTextureLayer, List<Tuple<Point, Color[]>>>();
            //foreach (var e in textures)
            //{
            //    if(!texture.ContainsKey(e.Value))
            //        texture.Add(e.Value, new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length));
            //}

            //Color[] trans = Enumerable.Repeat<Color>(Color.Transparent, tileSize * tileSize * pattern[0].Length * pattern.Length).ToArray();
            //foreach(var tex in texture)
            //{
            //    tex.Value.SetData<Color>(trans);
            //}

            string staticKey = resourceFolder + jsonFileName;
            if (!tileInfo.ContainsKey(staticKey)) Logger.Error("No tile info for the resource " + staticKey);

            TileInfo info = tileInfo[staticKey];

            int len = info.tileSize * info.tileSize;
            foreach (var m in info.masks)
            {
                var res = match(padded, m.Value);
                foreach (var t in res)
                {
                    Rectangle rect = new Rectangle(t.Item1 * info.tileSize, t.Item2 * info.tileSize, info.tileSize, info.tileSize);
                    List<Tuple<Point, Texture2D>> list;
                    var key = mapTexture(m.Key);
                    int ind = getRandomTile(info.tiles[m.Key]);
                    if (texture.TryGetValue(key, out list))
                    {
                        list.Add(new Tuple<Point, Texture2D>(new Point(t.Item1, t.Item2), info.tilesTexture[m.Key][ind]));
                    }
                    else
                    {
                        list = new List<Tuple<Point, Texture2D>>() { new Tuple<Point, Texture2D>(new Point(t.Item1, t.Item2), info.tilesTexture[m.Key][ind]) };
                        texture.Add(key, list);
                    }

                    List<Tuple<Point, Color[]>> list2;
                    if (color.TryGetValue(key, out list2))
                    {
                        list2.Add(new Tuple<Point, Color[]>(new Point(t.Item1, t.Item2), info.tiles[m.Key][ind]));
                    }
                    else
                    {
                        list2 = new List<Tuple<Point, Color[]>>() { new Tuple<Point, Color[]>(new Point(t.Item1, t.Item2), info.tiles[m.Key][ind]) };
                        color.Add(key, list2);
                    }
                }
            }
        }

        private static int getRandomTile(List<Color[]> colors)
        {
            return random.Next(0, colors.Count());
        }
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

        private X_DoorState _state;

        private X_DoorDirection _direction;
        private Texture2D _floor;
        private Texture2D _wall;
        private Texture2D _door;
        const int _numTilesDoorWidth = 5;
        private int _halfHeight;

        private int _currentDoorOpenOffset = 0;
        private float _doorOpeningTime = 2000.0f;
        private float _animationTime = 0.0f;
        int _tileSize;
        int _tileOffset;
        //private Y_Door _door;


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
            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);
            collision = flipToPosition(collision, direction, tileOffset);
            collision = getDoorPoints(collision, tileWidth, tileHeight);

            _direction = direction;
            _tileOffset = tileOffset;
            Collision = new X_CollisionModel_Room(collision, tileWidth, tileHeight);
            Rect = new Rectangle(0, 0, tileWidth * collision[0].Length, tileHeight * collision.Length);
            Scale = 1.0f;
            _doorCollisionRectangles = new Dictionary<X_DoorState, List<Rectangle>>();
            ResourceFolder = Util.PathOsNormalization(resourceFolder);

            X_AutoTiler.Initialize(ResourceFolder, "data.json", graphicsDevice, Y_Door.MapJsonName);
            Dictionary<X_DoorTextureLayer, List<Tuple<Point, Texture2D>>> tileTextures;
            Dictionary<X_DoorTextureLayer, List<Tuple<Point, Color[]>>> tileColors;
            X_AutoTiler.Resolve(ResourceFolder, tileJsonFile, graphicsDevice, Collision.GetCollisionTemplate(), out tileTextures, out tileColors);

            Scale = (float)tileHeight / tileTextures.First().Value.First().Item2.Height;
            TextureTileSize = tileTextures.First().Value.First().Item2.Height;
            _tileSize = (int)(TextureTileSize * Scale);
            _state = X_DoorState.Closed;

            int width = Collision.GetCollisionTemplate()[0].Length;
            int height = Collision.GetCollisionTemplate().Length;
            Color[] trans = Enumerable.Repeat<Color>(Color.Transparent, _tileSize * _tileSize * width * height).ToArray();
            _wall = new Texture2D(graphicsDevice, _tileSize * width, _tileSize * height); //textures[X_DoorTextureLayer.Wall];
            _floor = new Texture2D(graphicsDevice, _tileSize * width, _tileSize * height); // textures[X_DoorTextureLayer.Floor];
            _door = new Texture2D(graphicsDevice, _tileSize * width, _tileSize * height); // textures[X_DoorTextureLayer.Door];

            _wall.SetData<Color>(trans);
            _floor.SetData<Color>(trans);
            _door.SetData<Color>(trans);

            foreach(var tile in tileColors)
            {
                foreach (var t in tile.Value)
                {
                    Rectangle rect = new Rectangle(t.Item1.X * _tileSize, t.Item1.Y * _tileSize, _tileSize, _tileSize);

                    if (tile.Key == X_DoorTextureLayer.Wall) _wall.SetData<Color>(0, rect, t.Item2, 0, _tileSize * _tileSize);
                    else if (tile.Key == X_DoorTextureLayer.Floor) _floor.SetData<Color>(0, rect, t.Item2, 0, _tileSize * _tileSize);
                    else if (tile.Key == X_DoorTextureLayer.Door) _door.SetData<Color>(0, rect, t.Item2, 0, _tileSize * _tileSize);
                }
            }
        }

        public static X_TileType MapJsonName(string tileType)
        {
            if (tileType.ToLower().Contains("floor")) return X_TileType.Floor;
            if (tileType.ToLower().Contains("wall")) return X_TileType.Wall;
            if (tileType.ToLower().Contains("roof")) return X_TileType.Roof;
            if (tileType.ToLower().Contains("out")) return X_TileType.Outside;
            return X_TileType.DontCare;
        }

        private void output(int[][] pattern, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.GetLength(0); ++x)
            {
                s += string.Join("\t", pattern[x]) + "\n";
            }

            File.WriteAllText(name, s);
        }

        public ref Texture2D GetFloor()
        {
            return ref _floor;
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
            else
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

            return newCol;
        }

        private int[][] createDoorTemplate(int numTilesLength, int tileOffset)
        {
            int width = numTilesLength;
            int doorWidth = _numTilesDoorWidth;
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
                for(int y=doorWidth; y < collision[0].Length; ++y) collision[x][y] = -1;
            }

            for (int x = half1-doorWidth; x < width-1; ++x)
            {
                collision[x][height - 1] = 1;
            }

            for (int x = half1; x<width-1; ++x)
            {
                collision[x][Math.Abs(tileOffset)] = 1;
                for (int y = 0; y <= Math.Abs(tileOffset)-1; ++y) collision[x][y] = -1;
            }
            for (int y = 0; y <= Math.Abs(tileOffset) - 1; ++y) {
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

            collision[width-2][height-1-doorWidth/2] = 2;
            collision[1][doorWidth/2] = 3;

            return collision;
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!Doors.ContainsKey(side)) return null;
            return Doors[side].First();
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms</para>
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
        /// <para>The method acts out of the PERSPECTIVE OF THE CONNECTOR!</para>
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
            X_ConnectorSide side1,
            IWalkable from,
            X_ConnectorSide side2,
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

            if (side1 == X_ConnectorSide.Left) side1 = X_ConnectorSide.Right;
            else if (side1 == X_ConnectorSide.Right) side1 = X_ConnectorSide.Left;
            else if (side1 == X_ConnectorSide.Top) side1 = X_ConnectorSide.Bottom;
            else if (side1 == X_ConnectorSide.Bottom) side1 = X_ConnectorSide.Top;

            if (side2 == X_ConnectorSide.Left) side2 = X_ConnectorSide.Right;
            else if (side2 == X_ConnectorSide.Right) side2 = X_ConnectorSide.Left;
            else if (side2 == X_ConnectorSide.Top) side2 = X_ConnectorSide.Bottom;
            else if (side2 == X_ConnectorSide.Bottom) side2 = X_ConnectorSide.Top;

            return connect(from, from.GetConnectorPoint(side1), to, to.GetConnectorPoint(side2));
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms</para>
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
        /// <para>The method acts out of the PERSPECTIVE OF THE ROOMS!</para>
        /// 
        /// <para>If room1 is on the LEFT side of the connector, then the connectorPoint1 must be on the RIGHT side of room1.</para>
        /// </summary>
        /// <param name="room1">Y_Room with connectorPoint1</param>
        /// <param name="connectorPoint1">The connector point on Y_Room 1</param>
        /// <param name="room2">Y_Room with connectorPoint2</param>
        /// <param name="connectorPoint2">The connector point on Y_Room 2</param>
        /// <returns></returns>
        private IWalkable connect(
            IWalkable room1,
            X_ConnectorPoint connectorPoint1,
            IWalkable room2,
            X_ConnectorPoint connectorPoint2
        )
        {
            // check possibilities
            // horizontal: left and right or right and left
            // vertical: top and bottom or bottom and top
            bool hCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Left && connectorPoint2.ConnectorSide == X_ConnectorSide.Right;
            bool hCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Right && connectorPoint2.ConnectorSide == X_ConnectorSide.Left;
            bool vCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Bottom && connectorPoint2.ConnectorSide == X_ConnectorSide.Top;
            bool vCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Top && connectorPoint2.ConnectorSide == X_ConnectorSide.Bottom;

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
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Left].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* right to left */
            else if (hCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Left].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Right].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* bottom to top */
            else if (vCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Top].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Bottom].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* top to bottom */
            else if (vCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Bottom].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Top].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }

            // the following flip of connectorPoint2 and connectorPoint1 is NOT a bug
            DoorRooms.Add(connectorPoint2.ConnectorSide, new List<IWalkable> { room1 });
            DoorRooms.Add(connectorPoint1.ConnectorSide, new List<IWalkable> { room2 });
            room1.DoorRooms.Add(connectorPoint2.ConnectorSide, new List<IWalkable> { this });
            room2.DoorRooms.Add(connectorPoint1.ConnectorSide, new List<IWalkable> { this });

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
                        var res = r.Collision.SplitCollisionVerticallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                        foreach(var rects in res)
                        {
                            List<Rectangle> cRect;
                            if (!_doorCollisionRectangles.TryGetValue(rects.Key, out cRect))
                            {
                                _doorCollisionRectangles.Add(rects.Key, rects.Value);
                            }
                            else _doorCollisionRectangles[rects.Key].AddRange(rects.Value);
                        }
                    }
                }
                else
                {
                    foreach (var r in room.Value)
                    {
                        // need horizontal split                    
                        var res = r.Collision.SplitCollisionHorizontallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                        foreach (var rects in res)
                        {
                            List<Rectangle> cRect;
                            if (!_doorCollisionRectangles.TryGetValue(rects.Key, out cRect))
                            {
                                _doorCollisionRectangles.Add(rects.Key, rects.Value);
                            }
                            else _doorCollisionRectangles[rects.Key].AddRange(rects.Value);
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
            if (!(_state == X_DoorState.Open || _state == X_DoorState.Closed)) return false;
            if(_state == X_DoorState.Open)
                _state = X_DoorState.LockedOpen;
            if(_state == X_DoorState.Closed)
                _state = X_DoorState.LockedClosed;

            return true;
        }

        public void Update(GameTime gameTime)
        {
            bool keyPressed = Input.IsKeyTriggered(Keybinds.ToggleConnectors);
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            switch (_state)
            {
                case X_DoorState.Closed:
                    if (keyPressed)
                    {
                        _state = X_DoorState.Opening;
                    }
                    break;
                case X_DoorState.Opening:
                    if (!doorAnimation(dt, true))
                    {
                        _state = X_DoorState.Open;
                        if(_doorCollisionRectangles.ContainsKey(X_DoorState.Open))
                            Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Open]);
                    }
                    break;
                case X_DoorState.Open:
                    if (keyPressed)
                    {
                        _state = X_DoorState.Closing;
                        if (_doorCollisionRectangles.ContainsKey(X_DoorState.Closed))
                            Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Closed]);
                    }
                    break;
                case X_DoorState.Closing:
                    if (!doorAnimation(dt, false))
                    {
                        _state = X_DoorState.Closed;
                    }
                    break;
            }
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

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (_state != X_DoorState.Closed && _state != X_DoorState.LockedClosed)
            {
                if (_direction == X_DoorDirection.Vertical)
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _floor.Width, _floor.Height - _tileSize + _currentDoorOpenOffset),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _floor.Width, _floor.Height),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2() + new Vector2(_floor.Width * Scale - _tileSize, 0),
                        new Rectangle(_floor.Width - TextureTileSize, 0, TextureTileSize, _floor.Height),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
            }
            else
            {
                if (_direction == X_DoorDirection.Horizontal)
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _tileSize, _floor.Height),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);


                }
                else
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _floor.Width, _tileSize),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
            }

            if (_state != X_DoorState.Open && _state != X_DoorState.LockedOpen)
            {
                Vector2 pos = Rect.Location.ToVector2();
                pos.Y += _currentDoorOpenOffset;

                if(_direction == X_DoorDirection.Vertical)
                {
                    if (_tileOffset == 0)
                    {
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(0, 0, _floor.Width, _floor.Height),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                    else if (_tileOffset < 0)
                    {
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(0, 0, _floor.Width - _tileOffset*_tileSize, (_halfHeight - 1) * _tileSize - _currentDoorOpenOffset),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                        pos.Y += (_halfHeight - _numTilesDoorWidth - 1) * _tileSize;
                        pos.X += _floor.Width - (_numTilesDoorWidth - 1) * _tileSize;
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(_floor.Width - (_numTilesDoorWidth-1)*_tileSize, (_halfHeight - 1 - _numTilesDoorWidth) * _tileSize, _numTilesDoorWidth*_tileSize, _floor.Height),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                    else
                    {
                        pos.Y += (_halfHeight - 2) * _tileSize;
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(0, (_halfHeight - 2) * _tileSize, _numTilesDoorWidth * _tileSize, _floor.Height - (_halfHeight - 2) * _tileSize),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                }
                else
                {
                    if(_tileOffset == 0)
                    {
                        spriteBatch.Draw(
                             _door, pos,
                             new Rectangle(0, 0, _floor.Width, _floor.Height - _tileSize - _currentDoorOpenOffset),
                             Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                    else if(_tileOffset < 0)
                    {
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(0, 0, (_halfHeight - 2 + _numTilesDoorWidth - 2) * _tileSize, _floor.Height - _tileSize - _currentDoorOpenOffset),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                        pos.X += (_halfHeight - 2 + _numTilesDoorWidth - 2) * _tileSize;
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(
                                (_halfHeight - 2 + _numTilesDoorWidth - 2) * _tileSize,
                                0,
                                _floor.Width - (_halfHeight - 2 + _numTilesDoorWidth - 2) * _tileSize,
                                _numTilesDoorWidth * _tileSize - _tileSize - _currentDoorOpenOffset),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(0, 0, (_halfHeight - 2) * _tileSize, _numTilesDoorWidth * _tileSize - _tileSize - _currentDoorOpenOffset),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                        pos.X += (_halfHeight - 2) * _tileSize;
                        spriteBatch.Draw(
                            _door, pos,
                            new Rectangle(
                                (_halfHeight - 2) * _tileSize,
                                0,
                                _floor.Width - ((_halfHeight - 2) * _tileSize),
                                _floor.Height - _tileSize - _currentDoorOpenOffset),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                }
            }
            spriteBatch.Draw(
                _wall, Rect.Location.ToVector2(),
                new Rectangle(0, 0, _wall.Width, _wall.Height),
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
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
