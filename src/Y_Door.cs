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
        Door
    }

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

    internal class X_Data
    {
        public class Data
        {
            public string textureName;
            public int size;
            public Dictionary<string, TileData> tiles;
            //public List<Textel> floor;
            //public List<Textel> roofStraightVertical;
            //public List<Textel> roofStraightHorizontal;
            //public List<Textel> roofCornerOutsideBr;
            //public List<Textel> roofCornerOutsideBl;
            //public List<Textel> roofCornerOutsideTr;
            //public List<Textel> roofCornerOutsideTl;
            //public List<Textel> roofCornerInsideBr;
            //public List<Textel> roofCornerInsideBl;
            //public List<Textel> roofCornerInsideTr;
            //public List<Textel> roofCornerInsideTl;
            //public List<Textel> roofCover;
            //public List<Textel> wall;
        }

        private static Random random;
        private static int tileSize;
        private static Dictionary<string, List<Color[]>> tiles;
        private static Dictionary<string, X_TileType[,][]> masks;
        private static Dictionary<string, X_DoorTextureLayer> textures;

        //Func<X_TileType, bool>[,] downL = new Func<X_TileType, bool>[,]
        //    {
        //        { (t) => true, (t) => true, (t) => true },
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Floor || t == X_TileType.Wall },
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Floor },
        //    };
        ////var tDownL = X_Data.match(padded, downL);
        ////output(tDownL, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] downR = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => true, (t) => true },
        //        { (t) => t == X_TileType.Floor || t == X_TileType.Wall, (t) => t == X_TileType.Roof, (t) => true },
        //        { (t) => t == X_TileType.Floor || t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //};
        ////var tDownR = X_Data.match(padded, downR);
        ////output(tDownR, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] levelT = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => true, (t) => true },
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof },
        //        { (t) => true, (t) => t == X_TileType.Wall, (t) => t == X_TileType.Wall },
        //};
        ////var tLevelT = X_Data.match(padded, levelT);
        ////output(tLevelT, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] levelB = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => t == X_TileType.Floor, (t) => t == X_TileType.Floor || t == X_TileType.Roof },
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //        { (t) => true, (t) => true, (t) => true },
        //};
        ////var tLevelB = X_Data.match(padded, levelB);
        ////output(tLevelB, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] tli = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => t == X_TileType.Floor, (t) => true, (t) => true},
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //};
        ////var tTli = X_Data.match(padded, tli);

        //Func<X_TileType, bool>[,] tlo = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => true, (t) => true},
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Wall },
        //};
        ////var tTlo = X_Data.match(padded, tlo);
        ////output(tTl, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] tri = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => true, (t) => t == X_TileType.Floor },
        //        { (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //};
        ////var tTri = X_Data.match(padded, tri);
        ////output(tTr, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] tro = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => true, (t) => true },
        //        { (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //        { (t) => t == X_TileType.Wall, (t) => t == X_TileType.Roof, (t) => true },
        //};
        ////var tTro = X_Data.match(padded, tro);

        //Func<X_TileType, bool>[,] bl = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //        { (t) => true, (t) => true, (t) => true },
        //};
        ////var tBl = X_Data.match(padded, bl);
        ////output(tBl, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] br = new Func<X_TileType, bool>[,]
        //{
        //        { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //        { (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //        { (t) => true, (t) => true, (t) => true },
        //};

        //private static List<Color[]> floor;
        //private static List<Color[]> roofStraightVertical;
        //private static List<Color[]> roofStraightHorizontal;
        //private static List<Color[]> roofCornerOutsideBr;
        //private static List<Color[]> roofCornerOutsideBl;
        //private static List<Color[]> roofCornerOutsideTr;
        //private static List<Color[]> roofCornerOutsideTl;
        //private static List<Color[]> roofCornerInsideBr;
        //private static List<Color[]> roofCornerInsideBl;
        //private static List<Color[]> roofCornerInsideTr;
        //private static List<Color[]> roofCornerInsideTl;
        //private static List<Color[]> roofCover;
        //private static List<Color[]> wall;

        //private static Color[] flipX(Color[] tile, int size)
        //{
        //    Color[] n = new Color[tile.Length];
        //    for(int h=0; h<size; ++h)
        //    {
        //        for (int w = 0; w < size; ++w)
        //        {
        //            n[h * size + w] = tile[h * size + (size-1-w)];
        //        }
        //    }

        //    return n;
        //}

        //private static Color[] flipY(Color[] tile, int size)
        //{
        //    Color[] n = new Color[tile.Length];
        //    for (int h = 0; h < size; ++h)
        //    {
        //        for (int w = 0; w < size; ++w)
        //        {
        //            n[h * size + w] = tile[(size-1-h) * size + w];
        //        }
        //    }

        //    return n;
        //}

        //private static Color[] flipXY(Color[] tile, int size)
        //{
        //    Color[] n = new Color[tile.Length];
        //    for (int h = 0; h < size; ++h)
        //    {
        //        for (int w = 0; w < size; ++w)
        //        {
        //            n[h * size + w] = tile[(size-1-h) * size + (size-1-w)];
        //        }
        //    }

        //    return n;
        //}

        private static X_TileType map(string tileType)
        {
            if (tileType.ToLower().Contains("floor")) return X_TileType.Floor;
            if (tileType.ToLower().Contains("wall")) return X_TileType.Wall;
            if (tileType.ToLower().Contains("roof")) return X_TileType.Roof;
            if (tileType.ToLower().Contains("out")) return X_TileType.Outside;
            return X_TileType.DontCare;
        }

        private static X_DoorTextureLayer mapTexture(string textureType)
        {
            if (textureType.ToLower().Contains("floor")) return X_DoorTextureLayer.Floor;
            if (textureType.ToLower().Contains("wall") || textureType.ToLower().Contains("roof")) return X_DoorTextureLayer.Wall;
            return X_DoorTextureLayer.Door;
        }

        private static X_TileType[,][] map(string[,][] mask)
        {
            X_TileType[,][] res = new X_TileType[3, 3][];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    res[i, j] = new X_TileType[mask[i, j].Length];
                    for(int k=0; k< mask[i, j].Length; ++k)
                    {
                        res[i, j][k] = map(mask[i, j][k]); 
                    }
                }
            }
            return res;
        }

        public static void Load(string resourceFolder, Data data, GraphicsDevice graphicsDevice)
        {
            random = new Random();

            Texture2D texture;
            using (FileStream fileStream = new FileStream(resourceFolder + data.textureName, FileMode.Open))
                texture = Texture2D.FromStream(graphicsDevice, fileStream);

            tileSize = data.size;

            tiles = new Dictionary<string, List<Color[]>>();
            masks = new Dictionary<string, X_TileType[,][]>();
            textures = new Dictionary<string, X_DoorTextureLayer>();
            foreach (var t in data.tiles)
            {
                tiles.Add(t.Key, new List<Color[]>());
                foreach(var e in t.Value.coordinates)
                {
                    Color[] temp = new Color[data.size * data.size];
                    texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                    tiles[t.Key].Add(temp);
                }

                masks.Add(t.Key, map(t.Value.mask));

                textures.Add(t.Key, mapTexture(t.Key));
                //foreach (var e in t.Value)
                //{

                //}
            }
        }/*
            roofStraight = new List<Color[]>();
            foreach (var e in data.roofStraight)
            {
                Color[] temp = new Color[data.size * data.size];
                texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                roofStraight.Add(temp);
            }

            roofCornerOutside = new List<Color[]>();
            foreach (var e in data.roofCornerOutside)
            {
                Color[] temp = new Color[data.size * data.size];
                texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                roofCornerOutside.Add(temp);
            }

            roofCornerInside = new List<Color[]>();
            foreach (var e in data.roofCornerInside)
            {
                Color[] temp = new Color[data.size * data.size];
                texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                roofCornerInside.Add(temp);
            }

            floor = new List<Color[]>();
            foreach (var e in data.floor)
            {
                Color[] temp = new Color[data.size * data.size];
                texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                floor.Add(temp);
            }

            roofCover = new List<Color[]>();
            foreach (var e in data.roofCover)
            {
                Color[] temp = new Color[data.size * data.size];
                texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                roofCover.Add(temp);
            }

            wall = new List<Color[]>();
            foreach (var e in data.wall)
            {
                Color[] temp = new Color[data.size * data.size];
                texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                wall.Add(temp);
            }
        }*/

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

        //private static List<Tuple<int, int>> match(X_TileType[][] pattern, Func<X_TileType, bool> tile)
        //{
        //    List<Tuple<int, int>> res = new List<Tuple<int, int>>();
        //    for (int x = 0; x < pattern[0].Length; ++x)
        //    {
        //        for (int y = 0; y < pattern.Length; ++y)
        //        {
        //            if (tile(pattern[y][x])) res.Add(new Tuple<int, int>(x-1, y-1));
        //        }
        //    }
        //    return res;
        //}

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

        public static void Resolve(GraphicsDevice graphicsDevice, int[][] pattern, out Dictionary<X_DoorTextureLayer, Texture2D> texture)
        {
            //Texture2D roofTexture;
            //Texture2D floorTexture;
            //Texture2D wallTexture;
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

            texture = new Dictionary<X_DoorTextureLayer, Texture2D>();
            foreach (var e in textures)
            {
                if(!texture.ContainsKey(e.Value))
                    texture.Add(e.Value, new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length));
            }

            //floorTexture = new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length);
            //wallTexture = new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length);
            //doorTexture = new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length);

            //var tFloor = X_Data.match(padded, (t) => t == X_TileType.Floor);

            ////output(tFloor, "./logs/data.csv", pattern[0].Length, pattern.Length);
            //var tWall = X_Data.match(padded, (t) => t == X_TileType.Wall);
            ////output(tWall, "./logs/data.csv", pattern[0].Length, pattern.Length);
            //var tDoor = X_Data.match(padded, (t) => t == X_TileType.Floor || t == X_TileType.Wall);
            ////output(tDoor, "./logs/data.csv", pattern[0].Length, pattern.Length);

            Color[] trans = Enumerable.Repeat<Color>(Color.Transparent, tileSize * tileSize * pattern[0].Length * pattern.Length).ToArray();
            foreach(var tex in texture)
            {
                tex.Value.SetData<Color>(trans);
            }

            int len = tileSize * tileSize;
            foreach (var m in masks)
            {
                var res = match(padded, m.Value);
                foreach (var t in res)
                {
                    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
                    texture[mapTexture(m.Key)].SetData(0, rect, getRandomTile(tiles[m.Key]), 0, len);
                }
            }
        }

        private static Color[] getRandomTile(List<Color[]> colors)
        {
            return colors.ElementAt(random.Next(0, colors.Count()));
        }


        //Func<X_TileType, bool>[,] tli = new Func<X_TileType, bool>[,]
        //{
        //    { (t) => t == X_TileType.Floor, (t) => true, (t) => true},
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //};
        //var tTli = X_Data.match(padded, tli);

        //Func<X_TileType, bool>[,] tlo = new Func<X_TileType, bool>[,]
        //{
        //    { (t) => true, (t) => true, (t) => true},
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Wall },
        //};
        //var tTlo = X_Data.match(padded, tlo);
        ////output(tTl, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] tri = new Func<X_TileType, bool>[,]
        //{
        //    { (t) => true, (t) => true, (t) => t == X_TileType.Floor },
        //    { (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //};
        //var tTri = X_Data.match(padded, tri);
        ////output(tTr, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] tro = new Func<X_TileType, bool>[,]
        //{
        //    { (t) => true, (t) => true, (t) => true },
        //    { (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //    { (t) => t == X_TileType.Wall, (t) => t == X_TileType.Roof, (t) => true },
        //};
        //var tTro = X_Data.match(padded, tro);

        //Func<X_TileType, bool>[,] bl = new Func<X_TileType, bool>[,]
        //{
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof},
        //    { (t) => true, (t) => true, (t) => true },
        //};
        //var tBl = X_Data.match(padded, bl);
        ////output(tBl, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //Func<X_TileType, bool>[,] br = new Func<X_TileType, bool>[,]
        //{
        //    { (t) => true, (t) => t == X_TileType.Roof, (t) => true },
        //    { (t) => t == X_TileType.Roof, (t) => t == X_TileType.Roof, (t) => true },
        //    { (t) => true, (t) => true, (t) => true },
        //};
        //var tBr = X_Data.match(padded, br);
        ////output(tBr, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //var tFloor = X_Data.match(padded, (t) => t == X_TileType.Floor);
        ////output(tFloor, "./logs/data.csv", pattern[0].Length, pattern.Length);
        //var tWall = X_Data.match(padded, (t) => t == X_TileType.Wall);
        ////output(tWall, "./logs/data.csv", pattern[0].Length, pattern.Length);
        //var tDoor = X_Data.match(padded, (t) => t == X_TileType.Floor || t == X_TileType.Wall);
        ////output(tDoor, "./logs/data.csv", pattern[0].Length, pattern.Length);

        //floorTexture = new Texture2D(graphicsDevice, tileSize *pattern[0].Length, tileSize * pattern.Length);
        //wallTexture = new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length);
        //doorTexture = new Texture2D(graphicsDevice, tileSize * pattern[0].Length, tileSize * pattern.Length);

        //int len = tileSize * tileSize;
        //foreach (var t in tFloor)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    floorTexture.SetData(0, rect, GetFloor(), 0, len);
        //}

        //foreach (var t in tWall)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetWall(), 0, len);
        //}

        //foreach (var t in tLevelB)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofStraight(), 0, len);
        //}

        //foreach (var t in tLevelT)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofStraight(), 0, len);
        //}

        //foreach (var t in tDownL)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofStraight(), 0, len);
        //}

        //foreach (var t in tDownR)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofStraight(), 0, len);
        //}

        //foreach (var t in tTli)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofCornerInside(), 0, len);
        //}

        //foreach (var t in tTlo)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofCornerOutside(), 0, len);
        //}

        //foreach (var t in tTro)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofCornerOutside(), 0, len);
        //}

        //foreach (var t in tTri)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofCornerInside(), 0, len);
        //}

        //foreach (var t in tBl)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofCover(), 0, len);
        //}

        //foreach (var t in tBr)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    wallTexture.SetData(0, rect, GetRoofCornerOutside(), 0, len);
        //}

        //foreach (var t in tDoor)
        //{
        //    Rectangle rect = new Rectangle(t.Item1 * tileSize, t.Item2 * tileSize, tileSize, tileSize);
        //    doorTexture.SetData(0, rect, GetRoofCover(), 0, len);
        //}
        //}

        //private static Color[] GetRandomTile(string name)
        //{
        //    return new Color[0]; // floor.ElementAt(random.Next(0, floor.Count()));
        //}

        //private static Color[] GetFloor()
        //{
        //    return new Color[0]; // floor.ElementAt(random.Next(0, floor.Count()));
        //}

        //private static Color[] GetRoofStraight()
        //{
        //    return new Color[0]; // roofStraight.ElementAt(random.Next(0, roofStraight.Count()));
        //}

        //private static Color[] GetRoofCornerOutside()
        //{
        //    return new Color[0]; //roofCornerOutside.ElementAt(random.Next(0, roofCornerOutside.Count()));
        //}

        //private static Color[] GetRoofCornerInside()
        //{
        //    return new Color[0]; //roofCornerInside.ElementAt(random.Next(0, roofCornerInside.Count()));
        //}

        //private static Color[] GetRoofCover()
        //{
        //    return new Color[0]; //roofCover.ElementAt(random.Next(0, roofCover.Count()));
        //}

        //private static Color[] GetWall()
        //{
        //    return new Color[0]; //wall.ElementAt(random.Next(0, wall.Count()));
        //}
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

        private int _currentDoorOpenOffset = 0;
        private float _doorOpeningTime = 2000.0f;
        private float _animationTime = 0.0f;
        int _tileSize;

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
            string resourceFolder = "./Doors/"
            )
        {
            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);

            collision = flipToPosition(collision, direction, tileOffset);

            collision = getDoorPoints(collision, tileWidth, tileHeight);

            _direction = direction;
            
            Collision = new X_CollisionModel_Room(collision, tileWidth, tileHeight);

            Rect = new Rectangle(0, 0, tileWidth * collision[0].Length, tileHeight * collision.Length);

            Scale = 1.0f;

            _doorCollisionRectangles = new Dictionary<X_DoorState, List<Rectangle>>();

            ResourceFolder = Util.PathOsNormalization(resourceFolder);

            //Texture2D roof;
            //Texture2D floor;
            //Texture2D wall;

            //using (FileStream fileStream = new FileStream("./Doors/roof.png", FileMode.Open))
            //    roof = Texture2D.FromStream(graphicsDevice, fileStream);
            //using (FileStream fileStream = new FileStream("./Doors/floor.png", FileMode.Open))
            //    floor = Texture2D.FromStream(graphicsDevice, fileStream);
            //using (FileStream fileStream = new FileStream("./Doors/wall.png", FileMode.Open))
            //    wall = Texture2D.FromStream(graphicsDevice, fileStream);

            //TextureTileSize = wall.Width;

            //Color[] roofA;
            //Color[] floorA;
            //Color[] wallA;

            Dictionary<X_TileType, Color[]> textels = new Dictionary<X_TileType, Color[]>();

            //List<string> layers;
            //Texture2D tiles;
            //string textureName;
            //int size;
            //List<Textel> roofStraight;
            //List<Textel> roofCorner;
            //List<Textel> roofCover;
            //List<Textel> wall;
            //List<Textel> floor;
            X_Data.Data data;
            using (StreamReader stream = new StreamReader(ResourceFolder + "data.json"))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                data = JsonConvert.DeserializeObject<X_Data.Data>(array.ToString());
                //var textureName = JsonConvert.DeserializeObject<string>(array.textureName.ToString());
                //var size = JsonConvert.DeserializeObject<int>(array.textureName);
                //var floor = JsonConvert.DeserializeObject<List<Textel>>(array.floor);
                //var roofStraight = JsonConvert.DeserializeObject<List<Textel>>(array.roofStraight);
                //var roofCorner = JsonConvert.DeserializeObject<List<Textel>>(array.roofCorner);
                //var roofCover = JsonConvert.DeserializeObject<List<Textel>>(array.roofCover);
                //var wall = JsonConvert.DeserializeObject<List<Textel>>(array.wall);

                //IList<Door> doors = JsonConvert.DeserializeObject<List<Door>>(array.entities.Door.ToString());
                //layers = JsonConvert.DeserializeObject<List<string>>(array.layers.ToString());

                //foreach (var door in doors)
                //{
                //    int x = (int)((float)door.x / door.width * Collision.TileWidth) + Rect.X;
                //    int y = (int)((float)door.y / door.height * Collision.TileHeight) + Rect.Y;
                //    var side = determineSide(x, y);
                //    IList<X_ConnectorPoint> list;
                //    if (!Doors.TryGetValue(side, out list))
                //    {
                //        if (side == X_ConnectorSide.Top || side == X_ConnectorSide.Bottom)
                //            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                //        else
                //            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                //    }
                //    else list.Add(new X_ConnectorPoint(side, new Point(door.x, door.y)));
                //}
            }

            //var m = data.GetType().GetMembers();
            //var m1 = m.Last();
            //var x = m1.MemberType;
            //var y = m1.MemberType;
            //var z = m1.Name;
            //var w = m1.ToString();
            //var t = m1.GetNestedType(typeof(List<Textel>));

            X_Data.Load(ResourceFolder, data, graphicsDevice);

            Dictionary<X_DoorTextureLayer, Texture2D> textures;
            X_Data.Resolve(graphicsDevice, Collision.GetCollisionTemplate(), out textures);
            _wall = textures[X_DoorTextureLayer.Wall];
            _floor = textures[X_DoorTextureLayer.Floor];
            _door = textures[X_DoorTextureLayer.Door];

            //int len = roof.Width * roof.Height;
            ///* 1 */
            //roofA = new Color[len];
            //roof.GetData<Color>(roofA);
            //textels.Add(X_TileType.Roof, roofA);

            ///* 2 */
            //wallA = new Color[len];
            //wall.GetData<Color>(wallA);
            //textels.Add(X_TileType.Wall, wallA);

            ///* 3 */
            //floorA = new Color[len];
            //floor.GetData<Color>(floorA);
            //textels.Add(X_TileType.Floor, floorA);

            //int width = floor.Width;
            //int height = floor.Height;
            //var pattern = Collision.GetCollisionTemplate();
            ////createOutsideRects(pattern, tileWidth);
            //_floor = new Texture2D(graphicsDevice, width * pattern[0].Length, height * pattern.Length);
            //_roof = new Texture2D(graphicsDevice, width * pattern[0].Length, height * pattern.Length);
            //_door = new Texture2D(graphicsDevice, width * pattern[0].Length, height * pattern.Length);
            Scale = (float)tileHeight / data.size;
            //output(pattern, "./logs/pattern2.csv");
            //Color[] transparent = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
            //for (int x = 0; x < pattern[0].Length; ++x)
            //{
            //    for (int y = 0; y < pattern.Length; ++y)
            //    {
            //        var index = pattern[y][x];
            //        Color[] floorElem;
            //        Color[] roofElem;
            //        Color[] doorElem;
            //        if ((X_TileType)index != X_TileType.Outside)
            //        {
            //            if((X_TileType)index == X_TileType.Roof)
            //                roofElem = textels[(X_TileType)index];
            //            else
            //                roofElem = transparent;
            //            if ((X_TileType)index == X_TileType.Floor || (X_TileType)index == X_TileType.Wall)
            //                floorElem = textels[(X_TileType)index];
            //            else
            //                floorElem = transparent;
            //        }
            //        else
            //        {
            //            floorElem = transparent;
            //            roofElem = transparent;
            //        }
            //        if ((X_TileType)index != X_TileType.Outside && (X_TileType)index != X_TileType.Roof)
            //        {
            //            if (x < 1 || y < 1 || x > pattern[0].Length - 2)
            //                doorElem = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
            //            else if(y > pattern.Length-2)
            //                doorElem = transparent;
            //            else
            //                doorElem = textels[X_TileType.Roof];
            //        }
            //        else
            //        {
            //            doorElem = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
            //        }
            //        Rectangle rect = new Rectangle(x * width, y * height, width, height);

            //        //var elem = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
            //        _floor.SetData(0, rect, floorElem, 0, floorElem.Length);
            //        _door.SetData(0, rect, doorElem, 0, doorElem.Length);
            //        _roof.SetData(0, rect, roofElem, 0, roofElem.Length);
            //    }
            //}

            //TextureTileSize = size;
            //_tileSize = (int)(TextureTileSize * Scale);
            _state = X_DoorState.Closed;
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
                spriteBatch.Draw(
                    _door, pos,
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            //spriteBatch.Draw(
            //    _wall, Rect.Location.ToVector2(),
            //    new Rectangle(0, 0, _wall.Width, _wall.Height),
            //    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);


            //spriteBatch.Draw(
            //    _floor, Rect.Location.ToVector2(),
            //    new Rectangle(0, 0, _floor.Width, _floor.Height),
            //    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            //spriteBatch.Draw(
            //    _wall, Rect.Location.ToVector2(),
            //    new Rectangle(0, 0, _wall.Width, _wall.Height),
            //    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            //spriteBatch.Draw(
            //    _door, Rect.Location.ToVector2(),
            //    new Rectangle(0, 0, _door.Width, _door.Height),
            //    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
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
