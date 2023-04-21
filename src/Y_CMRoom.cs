using Assimp;
using LDtk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YGR
{
    internal sealed class Door
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public class Y_CMRoom : IWalkable
    {
        public float Scale { get; private set; }
        public string Name { get; set; }
        public X_CollisionModel_Room Collision { get; }
        public X_RoomGraph Graph { get; set; }
        public Rectangle Rect { get; set; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        public int TextureTileSize { get; }
        public string ResourceFolder { get; }
        public Color RegionColor { get; }

        private Texture2D _floor;
        private Texture2D _window;
        private bool[] _shade;
        private List<X_Light> _lights;

        public Y_CMRoom(
            string name,
            int tileWidth,
            int tileHeight,
            string resourceFolder,
            GraphicsDevice graphicsDevice
        )
        {
            ResourceFolder = Util.PathOsNormalization(resourceFolder);
            var files = Directory.GetFiles(ResourceFolder);
            string dataFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("data") && Path.GetFileName(x).EndsWith(".json")).First());

            string collisionsFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("Collision") && Path.GetFileName(x).EndsWith(".csv")).First());
            string[] lines = File.ReadAllLines(ResourceFolder + collisionsFileName);
            int[][] collisions = new int[lines.Length][];
            int counter = 0;
            foreach (var line in lines)
            {
                collisions[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

            collisions = paddOutline(collisions);
            Collision = new X_CollisionModel_Room(collisions, tileWidth, tileHeight);
            Graph = new X_RoomGraph(this, collisions, tileWidth, tileHeight);
            Rect = new Rectangle(0, 0, collisions[0].Length * Collision.TileWidth, collisions.Length * Collision.TileHeight);

            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();

            List<string> layers;
            using (StreamReader stream = new StreamReader(ResourceFolder + dataFileName))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                IList<Door> doors = JsonConvert.DeserializeObject<List<Door>>(array.entities.Door.ToString());
                layers = JsonConvert.DeserializeObject<List<string>>(array.layers.ToString());

                foreach(var door in doors)
                {
                    int x = (int)((float)door.x / door.width * Collision.TileWidth) + Rect.X;
                    int y = (int)((float)door.y / door.height * Collision.TileHeight) + Rect.Y;
                    var side = determineSide(x, y);
                    IList<X_ConnectorPoint> list;
                    if(!Doors.TryGetValue(side, out list)){
                        if(side == X_ConnectorSide.Top || side == X_ConnectorSide.Bottom)
                            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                        else
                            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                    }
                    else list.Add(new X_ConnectorPoint(side, new Point(door.x, door.y)));
                }
            }

            Name = name;
            Color[] target = null;
            for (int i=0; i<layers.Count; ++i)
            {
                using (FileStream fileStream = new FileStream(ResourceFolder + layers[i], FileMode.Open))
                {
                    if(i==0){
                        _floor = Texture2D.FromStream(graphicsDevice, fileStream);
                        target = new Color[_floor.Width * _floor.Height];
                        _floor.GetData<Color>(target);
                    }
                    else
                    {
                        var t = Texture2D.FromStream(graphicsDevice, fileStream);
                        Color[] source = new Color[_floor.Width * _floor.Height];
                        Texture2D.FromStream(graphicsDevice, fileStream).GetData<Color>(source);

                        for(int h=0; h<_floor.Height; ++h)
                        {
                            for (int w = 0; w < _floor.Width; ++w)
                            {
                                var c = source[h * _floor.Width + w];
                                if(c.A != 0)
                                    target[h * _floor.Width + w] = source[h * _floor.Width + w];
                            }
                        }
                    }
                }
            }
            _floor.SetData<Color>(target);

            //using (FileStream fileStream = new FileStream(ResourceFolder + "Custom_grounds.png", FileMode.Open))
            //{
            //    _window = Texture2D.FromStream(graphicsDevice, fileStream);
            //}

            TextureTileSize = _floor.Height / collisions.Length;

            //int len = _window.Width * _window.Height;
            //Color[] groundData = new Color[len];
            //_window.GetData<Color>(groundData);
            //Color[] floorData = new Color[len];
            //_floor.GetData<Color>(floorData);
            //Color[] newData = new Color[len];

            //int minh = int.MaxValue;
            //int minw = int.MaxValue;
            //int maxh = int.MinValue;
            //int maxw = int.MinValue;
            //for (int h = 0; h < _window.Height; ++h)
            //{
            //    for (int w = 0; w < _window.Width; ++w)
            //    {
            //        if(groundData[h*_window.Width +w].A == 0)
            //        {
            //            if (minh > h) minh = h;
            //            if (maxh < h) maxh = h;
            //            if (minw > w) minw = w;
            //            if (maxw < w) maxw = w;
            //        }
            //    }
            //}

            //minh = minh + TextureTileSize / 2;
            //minw = minw + TextureTileSize / 2;
            //maxh = maxh - TextureTileSize / 2;
            //maxw = maxw - TextureTileSize / 2;

            //RegionColor = floorData[minh * _window.Width + minw];

            //for (int h = 0; h < _window.Height; ++h)
            //{
            //    for (int w = 0; w < _window.Width; ++w)
            //    {
            //        if (h >= minh && w >= minw && h < maxh && w < maxw)
            //        {
            //            newData[h * _window.Width + w] = floorData[h * _window.Width + w];
            //        }
            //        else newData[h * _window.Width + w] = RegionColor;
            //    }
            //}

            //_floor.SetData<Color>(newData);

            Scale = (float)tileHeight * collisions.Length / _floor.Height;
            _shade = null;
            _lights = new List<X_Light>() { new X_Light(new Vector3(Rect.X + -5 * TextureTileSize, Rect.Y + 5 * TextureTileSize, 5 * TextureTileSize), Rect, Scale) };
        }

        public void Illuminate()
        {
            _shade = Manager_Light.Illuminate(_lights, this);

            Color[] data = new Color[_floor.Width * _floor.Height];
            _floor.GetData<Color>(data);

            for (int i = 0; i < _shade.Length; ++i)
            {
                if (!_shade[i])
                {
                    var col = data[i];
                    Color nCol = Color.White;
                    nCol.R = (byte)((1 - 0.4f) * col.R + 0.4f * Color.Black.R);
                    nCol.G = (byte)((1 - 0.4f) * col.G + 0.4f * Color.Black.G);
                    nCol.B = (byte)((1 - 0.4f) * col.B + 0.4f * Color.Black.B);
                    data[i] = nCol;
                }
            }
            _floor.SetData<Color>(data);
        }

        public ref Texture2D GetFloor()
        {
            return ref _floor;
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!Doors.ContainsKey(side)) return null;
            return Doors[side].First();
        }

        private int[][] paddOutline(int[][] collision)
        {
            int x = 0;
            int y = 0;
            while (x < collision.Length)
            {
                y = 0;
                while (y < collision[0].Length)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y++;
                }
                x++;
            }

            x = collision.Length - 1;
            while (x >= 0)
            {
                y = collision[0].Length - 1;
                while (y >= 0)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y--;
                }
                x--;
            }

            x = 0;
            while (x < collision.Length)
            {
                y = collision[0].Length - 1;
                while (y >= 0)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y--;
                }
                x++;
            }

            x = collision.Length - 1;
            while (x >= 0)
            {
                y = 0;
                while (y < collision[0].Length)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y++;
                }
                x--;
            }

            return collision;
        }

        private X_ConnectorSide determineSide(int x0, int y0)
        {
            /**
             *  x1,y1 -- x2,y2
             *    |        |
             *    |        |
             *    |        |
             *  x4,y4 -- x3,y3
             */

            float x1 = Rect.X;
            float y1 = Rect.Y;
            float x2 = Rect.X + Rect.Width;
            float y2 = Rect.Y;
            float x3 = Rect.X + Rect.Width;
            float y3 = Rect.Y + Rect.Height;
            float x4 = Rect.X;
            float y4 = Rect.Y + Rect.Height;

            float[] dists = {
                distance(x0, y0, x1, y1, x2, y2),
                distance(x0, y0, x2, y2, x3, y3),
                distance(x0, y0, x3, y3, x4, y4),
                distance(x0, y0, x4, y4, x1, y1) };

            float min = float.MaxValue;
            int ind = 0;
            for(int i=0; i<dists.Length; ++i)
            {
                if (dists[i] < min)
                {
                    min = dists[i];
                    ind = i;
                }
            }

            if (ind == 0) return X_ConnectorSide.Top;
            if (ind == 1) return X_ConnectorSide.Right;
            if (ind == 2) return X_ConnectorSide.Bottom;
            else return X_ConnectorSide.Left;
        }

        private float distance(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            return (float)(Math.Abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1)) / Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
        }

        public void MoveTo(Point position)
        {
            Point p = position - Rect.Location;
            Collision.MoveBy(p);
            foreach (var side in Doors)
            {
                for (int i=0; i<side.Value.Count(); ++i)
                {
                    side.Value[i].MoveBy(p);
                    //side.Value[i] = new Tuple<int, int>(position.X + side.Value[i].Item1, position.Y + side.Value[i].Item2);
                }
            }
            
            // needs to be done this way because properties return by value and not by ref
            Rect = new Rectangle(position.X, position.Y, Rect.Width, Rect.Height);
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side)
        {
            return Doors[side].First();
        }

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {
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
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.Blue, spriteBatch);
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _floor, Rect.Location.ToVector2(),
                new Rectangle(0, 0, _floor.Width, _floor.Height),
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// This method is the standard ILevelElement WhatAreYou
        /// </summary>
        /// <returns>Returns the fitting X_LevelElements enum entry</returns>
        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Room;
        }
    }
}
