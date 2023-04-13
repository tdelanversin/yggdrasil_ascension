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
        public Rectangle Rect { get; set; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }

        private Texture2D _floor;
        private Texture2D _window;

        public Y_CMRoom(
            string name,
            int tileWidth,
            int tileHeight,
            string resourceFolder,
            GraphicsDevice graphicsDevice
        )
        {
            if (resourceFolder.Substring(0, 1) == "/")
                resourceFolder = "." + resourceFolder;
            else if (resourceFolder.Substring(0, 2) != "./")
                resourceFolder = "./" + resourceFolder;
            if (resourceFolder.Substring(resourceFolder.Length - 2, 1) != "/")
                resourceFolder += "/";

            string[] lines = File.ReadAllLines(resourceFolder + "Collisions.csv");
            int[][] collisions = new int[lines.Length][];
            int counter = 0;
            foreach (var line in lines)
            {
                collisions[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

            collisions = paddOutline(collisions);
            Collision = new X_CollisionModel_Room(collisions, tileWidth, tileHeight);
            Rect = new Rectangle(0, 0, collisions[0].Length * Collision.TileWidth, collisions.Length * Collision.TileHeight);

            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            using (StreamReader stream = new StreamReader(resourceFolder + "data.json"))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                IList<Door> doors = JsonConvert.DeserializeObject<List<Door>>(array.entities.Door.ToString());

                foreach(var door in doors)
                {
                    int x = (int)((float)door.x / door.width * Collision.TileWidth) + Rect.X;
                    int y = (int)((float)door.y / door.height * Collision.TileHeight) + Rect.Y;
                    var side = determineSide(x, y);
                    IList<X_ConnectorPoint> list;
                    if(!Doors.TryGetValue(side, out list)){
                        if(side == X_ConnectorSide.Top || side == X_ConnectorSide.Bottom)
                            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y-tileHeight)) });
                        else
                            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                    }
                    else list.Add(new X_ConnectorPoint(side, new Point(door.x, door.y)));
                }
            }

            Name = name;

            using(FileStream fileStream = new FileStream(resourceFolder + "_composite.png", FileMode.Open)){
                _floor = Texture2D.FromStream(graphicsDevice, fileStream);
            }
            using (FileStream fileStream = new FileStream(resourceFolder + "Custom_grounds.png", FileMode.Open))
            {
                _window = Texture2D.FromStream(graphicsDevice, fileStream);
            }

            int len = _window.Width * _window.Height;
            Color[] groundData = new Color[len];
            _window.GetData<Color>(groundData);
            Color[] floorData = new Color[len];
            _floor.GetData<Color>(floorData);
            Color[] newData = new Color[len];
            for (int i = 0; i < len; ++i)
            {
                if (groundData[i].A == 0)
                {
                    newData[i] = floorData[i];
                }
                else
                {
                    newData[i] = Color.Transparent;
                }
            }
            _floor.SetData<Color>(newData);

            Scale = (float)tileHeight * collisions.Length / _floor.Height;
        }

        public void Illuminate(X_Light light)
        {
            Manager_Light.Illuminate(light, this);
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

            //foreach(var door in Doors)
            //{
            //int x = (int)((float)door.x / door.width * Collision.TileWidth) + Rect.X;
            //int y = (int)((float)door.y / door.height * Collision.TileHeight) + Rect.Y;
            //foreach(var side in Doors)
            //{
            //    foreach (var door in side.Value)
            //    {
            //        door.DrawOutline(gameTime, Vector2.Zero, spriteBatch);
            //        //Factory_Debug.DrawPoint(door.Item1, door.Item2, 11, color, spriteBatch);
            //    }
            //}
            //}
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
