using Microsoft.Xna.Framework;
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
        public string Name { get; set; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public X_CollisionModel_Room Collision { get; }
        public Rectangle Rect { get; set; }

        //IList<Door> _doors;

        Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> _doors;

        public Y_CMRoom(
            string name,
            int tileWidth,
            int tileHeight,
            string resourceFolder
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

            Collision = new X_CollisionModel_Room(collisions, tileWidth, tileHeight);
            Rect = new Rectangle(0, 0, collisions[0].Length * Collision.TileWidth, collisions.Length * Collision.TileHeight);

            _doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
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
                    if(!_doors.TryGetValue(side, out list)){
                        if(side == X_ConnectorSide.Top || side == X_ConnectorSide.Bottom)
                            _doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y-tileHeight)) });
                        else
                            _doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                    }
                    else list.Add(new X_ConnectorPoint(side, new Point(door.x, door.y)));
                }
            }

            Name = name;
            Projectiles = new List<IProjectile>();
            Victims = new List<IVictim>();
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!_doors.ContainsKey(side)) return null;
            return _doors[side].First();
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
            Collision.MoveTo(position);
            foreach (var side in _doors)
            {
                for (int i=0; i<side.Value.Count(); ++i)
                {
                    side.Value[i].MoveTo(position);
                    //side.Value[i] = new Tuple<int, int>(position.X + side.Value[i].Item1, position.Y + side.Value[i].Item2);
                }
            }
            
            // needs to be done this way because properties return by value and not by ref
            Rect = new Rectangle(position.X, position.Y, Rect.Width, Rect.Height);
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side)
        {
            return _doors[side].First();
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

            //foreach(var door in _doors)
            //{
            //int x = (int)((float)door.x / door.width * Collision.TileWidth) + Rect.X;
            //int y = (int)((float)door.y / door.height * Collision.TileHeight) + Rect.Y;
            foreach(var side in _doors)
            {
                foreach (var door in side.Value)
                {
                    door.DrawOutline(gameTime, Vector2.Zero, spriteBatch);
                    //Factory_Debug.DrawPoint(door.Item1, door.Item2, 11, color, spriteBatch);
                }
            }
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
