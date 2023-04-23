using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YGR
{
    public static class X_AutoTiler
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

        public class X_AutoTileTexture
        {
            private Point _location;
            private Texture2D _texture;

            public X_AutoTileTexture(Point location, Texture2D texture)
            {
                _location = location;
                _texture = texture;
            }

            public ref Point Location() => ref _location;

            public ref Texture2D Texture() => ref _texture;
            public void Texture(Texture2D texture) => _texture = texture;
        }

        public class X_AutoTileColor
        {
            private Point _location;
            private Color[] _color;

            public X_AutoTileColor(Point location, Color[] color)
            {
                _location = location;
                _color = color;
            }

            public ref Point Location() => ref _location;

            public ref Color[] Color() => ref _color;
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
            public Dictionary<string, X_TileType[,][]> masks;
        }

        private static Random random;

        private static Dictionary<string, TileInfo> tileInfo;

        private static X_TileType[,][] map(string[,][] mask, Func<string, X_TileType> mapJsonName)
        {
            X_TileType[,][] res = new X_TileType[3, 3][];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    res[i, j] = new X_TileType[mask[i, j].Length];
                    for (int k = 0; k < mask[i, j].Length; ++k)
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
            //info.tilesTexture = new Dictionary<string, List<Texture2D>>();

            info.masks = new Dictionary<string, X_TileType[,][]>();
            //textures = new Dictionary<string, X_DoorTextureLayer>();
            foreach (var t in data.tiles)
            {
                info.tiles.Add(t.Key, new List<Color[]>());
                //info.tilesTexture.Add(t.Key, new List<Texture2D>());
                foreach (var e in t.Value.coordinates)
                {
                    Color[] temp = new Color[data.size * data.size];
                    texture.GetData<Color>(0, new Rectangle(e.x * data.size, e.y * data.size, data.size, data.size), temp, 0, data.size * data.size);
                    info.tiles[t.Key].Add(temp);

                    //Texture2D ttemp = new Texture2D(graphicsDevice, data.size, data.size);
                    //ttemp.SetData<Color>(temp);
                    //info.tilesTexture[t.Key].Add(ttemp);
                }

                info.masks.Add(t.Key, map(t.Value.mask, mapJsonName));
            }
        }

        private static bool evalTile(X_TileType p, X_TileType[] candidates)
        {
            bool match = false;
            foreach (var c in candidates)
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
                for (int x = 1; x < pattern[0].Length - 1; ++x)
                {
                    for (int j = -1; j <= 1; ++j)
                    {
                        for (int i = -1; i <= 1; ++i)
                        {
                            if (!evalTile(pattern[y + j][x + i], tile[j + 1, i + 1])) goto no_match;
                        }
                    }
                    res.Add(new Tuple<int, int>(x - 1, y - 1));
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
                    s += "\t" + p[x, y];
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

        public static void Resolve<Types>(
            string resourceFolder, string jsonFileName,
            GraphicsDevice graphicsDevice,
            int[][] pattern,
            Func<string, Types> MapTexture,
            out Dictionary<Types, List<X_AutoTileTexture>> texture,
            out Dictionary<Types, List<X_AutoTileColor>> color)
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

            texture = new Dictionary<Types, List<X_AutoTileTexture>>();
            color = new Dictionary<Types, List<X_AutoTileColor>>();

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
                    int ind = getRandomTile(info.tiles[m.Key]);

                    var key = MapTexture(m.Key);
                    X_AutoTileColor nc = new X_AutoTileColor(new Point(t.Item1, t.Item2), info.tiles[m.Key][ind].Clone() as Color[]);
                    List<X_AutoTileColor> list2;
                    if (color.TryGetValue(key, out list2))
                    {
                        list2.Add(nc);
                    }
                    else
                    {
                        list2 = new List<X_AutoTileColor>() { nc };
                        color.Add(key, list2);
                    }

                    var tex = new Texture2D(graphicsDevice, info.tileSize, info.tileSize);
                    tex.SetData<Color>(nc.Color());
                    X_AutoTileTexture np = new X_AutoTileTexture(new Point(t.Item1, t.Item2), tex);
                    List<X_AutoTileTexture> list;
                    if (texture.TryGetValue(key, out list))
                    {
                        list.Add(np);
                    }
                    else
                    {
                        list = new List<X_AutoTileTexture>() { np };
                        texture.Add(key, list);
                    }
                }
            }
        }

        private static int getRandomTile(List<Color[]> colors)
        {
            return random.Next(0, colors.Count());
        }
    }
}
