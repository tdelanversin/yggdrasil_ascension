using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;

namespace YGR
{
    public class Y_TestRoom : IWalkable
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }

        private int[][] _collisionTemplate;
        private Rectangle[] _collisionRectangles;
        private int[,] _collisionModel;
        private string _location;
        private Color[] _collisionRectColors;

        public Y_TestRoom(
            string name,
            string resourceFile,
            int tileWidth = 16,
            int tileHeight = 16,
            string location = "Rooms"
        )
        {
            Position = Vector2.Zero;
            Name = name;
            TileWidth = tileWidth;
            TileHeight = tileHeight;

            if (location[location.Length - 1] != '/') _location = location + '/'; else _location = location;
            if (_location.Substring(0, 2) != "./") _location = "./" + _location;

            Projectiles = new List<IProjectile>();
            Victims = new List<IVictim>();

            load();
        }

        private void load()
        {
            string[] lines = File.ReadAllLines(_location + "Collisions.csv");
            _collisionTemplate = new int[lines.Length][];
            int counter = 0;
            foreach (var line in lines)
            {
                _collisionTemplate[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

            var components = fitRectangles(_collisionTemplate);

            // get all components and their tiles
            //Dictionary<int, List<Tuple<int, int>>> components = new Dictionary<int, List<Tuple<int, int>>>();
            //for (int x = 1; x < _collisionTemplate.Length; ++x)
            //{
            //    for (int y = 1; y < _collisionTemplate[x].Length; ++y)
            //    {
            //        if (_collisionTemplate[x][y] != 0)
            //        {
            //            int key = _collisionTemplate[x][y];
            //            List<Tuple<int, int>> list;
            //            if (!components.TryGetValue(key, out list))
            //                components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y) });
            //            else
            //                list.Add(new Tuple<int, int>(x, y));
            //        }
            //    }
            //}

            createCollisionModelRectangles(components);

            _collisionRectColors = Enumerable.Repeat<Color>(Color.Green, _collisionRectangles.Count()).ToArray();

        }

        private Dictionary<int, List<Tuple<int, int>>> fitRectangles(int[][] pattern)
        {
            Dictionary<int, List<Tuple<int, int>>> components = new Dictionary<int, List<Tuple<int, int>>>();

            int key = 2;
            // first find all horizontal ones
            for(int x=0; x<pattern.Length; ++x)
            {
                int y = 0;
                while(y < pattern[0].Length - 1)
                {
                    while (y < pattern[0].Length - 1 && pattern[x][y] >= 1 && pattern[x][y + 1] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!components.TryGetValue(key, out list))
                        {
                            components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x, y+1) });
                        }
                        else
                            list.Add(new Tuple<int, int>(x, y+1));

                        // extend es fahr as possible
                        // check if top and bottom is free. If not, leave 1 otherwise put the key
                        if(((x+1 < pattern.Length && pattern[x+1][y] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x-1][y] == 0) || x - 1 < 0))
                            pattern[x][y] = key;
                        if (((x + 1 < pattern.Length && pattern[x+1][y+1] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x - 1][y+1] == 0) || x - 1 < 0))
                            pattern[x][y + 1] = key;
                        y++;
                    }
                    // check if top and bottom is free. If not, put back 1, otherwise, leave it
                    //if (!(((x + 1 < pattern.Length && pattern[x][y] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x - 1][y] == 0) || x - 1 < 0)))
                    //    pattern[x][y] = 1;
                    key++;
                    y++;
                }
            }

            //output(pattern, "output1.csv");

            // then go vertical
            for (int y = 0; y < pattern[0].Length; ++y)
            {
                int x = 0;
                while (x < pattern.Length - 1)
                {
                    while (x < pattern.Length - 1 && pattern[x][y] >= 1 && pattern[x + 1][y] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!components.TryGetValue(key, out list))
                        {
                            components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x + 1, y) });
                        }
                        else
                            list.Add(new Tuple<int, int>(x + 1, y));

                        // extend es fahr as possible
                        pattern[x][y] = key;
                        pattern[x + 1][y] = key;
                        x++;
                    }
                    key++;
                    x++;
                }
            }

            //output(pattern, "output2.csv");

            for (int x = 0; x < pattern.Length; ++x)
            {
                for(int y=0; y < pattern[0].Length; ++y)
                {
                    if (pattern[x][y] == 1)
                    {
                        key++;
                        pattern[x][y] = key;

                        List<Tuple<int, int>> list;
                        if (!components.TryGetValue(key, out list))
                        {
                            components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y) });
                        }
                        else
                            list.Add(new Tuple<int, int>(x, y));
                    }
                }
            }

            //output(pattern, "output3.csv");
            return components;
        }

        private void createCollisionModelRectangles(Dictionary<int, List<Tuple<int, int>>> components)
        {
            _collisionModel = new int[_collisionTemplate.Length, _collisionTemplate[0].Length];
            var rects = new List<Rectangle>();

            int offset = 2;
            foreach (var component in components)
            {
                var list = component.Value;
                var topleft = list.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                int tlX = topleft.First().Item1;
                int tlY = topleft.First().Item2;
                int brX = topleft.Last().Item1;
                int brY = topleft.Last().Item2;
                int width, height;
                if(tlX == brX)
                {
                    height = 1;
                    width = (brY - tlY + 1);
                    // make these a little bit shorter and offset them a little bit
                    rects.Add(new Rectangle(
                        tlY * TileWidth + (int)Position.X + offset,
                        tlX * TileHeight + (int)Position.Y,
                        width * TileWidth - 2*offset,
                        height * TileHeight));
                }
                else
                {
                    width = 1;
                    height = (brX - tlX + 1);
                    // make these a little bit less high and offset them a little bit
                    rects.Add(new Rectangle(
                        tlY * TileWidth + (int)Position.X,
                        tlX * TileHeight + (int)Position.Y + offset,
                        width * TileWidth,
                        height * TileHeight - 2 * offset));
                }
            }

            _collisionRectangles = rects.ToArray();
        }

        //private void createCollisionModelRectangles2(Dictionary<int, List<Tuple<int, int>>> components)
        //{
        //    _collisionModel = new int[_collisionTemplate.Length, _collisionTemplate[0].Length];
        //    _collisionRectangles = new List<Rectangle>();

        //    // get outer hull of every component in rectangles
        //    foreach (var component in components)
        //    {
        //        int direction = 0;
        //        List<Tuple<int, int>> c = component.Value;
        //        while (c.Count() > 0)
        //        {
        //            int interval = 1;
        //            if (direction == 0)
        //            {
        //                // we go in x direction, sort by y so that all tiles with the same y are in one row
        //                c.Sort((x, y) => y.Item2.CompareTo(x.Item2));
        //                // go trough the list until the y changes
        //                int yCoord = c[0].Item2;
        //                while (interval < c.Count())
        //                {
        //                    if (interval > c.Count()-1 || c[interval].Item2 != yCoord) break;
        //                    interval++;
        //                }
        //                int xCoord = c[0].Item1 * TileWidth;
        //                yCoord *= TileHeight;
        //                int width = (c[interval-1].Item1 - c[0].Item1 + 1) * TileWidth;
        //                int height = TileHeight;
        //                _collisionRectangles.Add(new Rectangle(xCoord, yCoord, width, height));
        //            }
        //            else
        //            {
        //                // we go in y direction, sort by x so that all tiles with the same x are in one row
        //                c.Sort((x, y) => y.Item1.CompareTo(x.Item1));
        //                // go trough the list until the x changes
        //                int xCoord = c[0].Item1;
        //                while (interval < c.Count())
        //                {
        //                    if (interval > c.Count()-1 || c[interval].Item1 != xCoord) break;
        //                    interval++;
        //                }
        //                int yCoord = c[0].Item2 * TileHeight;
        //                xCoord *= TileWidth;
        //                int width = TileWidth;
        //                int height = (c[interval-1].Item2 - c[0].Item2 + 1) * TileHeight;
        //                _collisionRectangles.Add(new Rectangle(xCoord, yCoord, width, height));
        //            }

        //            // switch the direction
        //            direction = (direction + 1) % 2;
        //            // transfer indices to model
        //            for (int i = 0; i < interval; ++i)
        //                _collisionModel[c[i].Item1, c[i].Item2] = _collisionRectangles.Count() - 1;
        //            c.RemoveRange(0, interval);
        //        }
        //    }
        //}

        private void output(int[][] pattern, string name)
        {
            using (StreamWriter writer = new StreamWriter(_location + name))
            {
                for (int x = 0; x < pattern.GetLength(0); ++x)
                {
                    string s = string.Join("\t", pattern[x]);
                    writer.WriteLine(s);
                }
            }
        }

        public void transf(int[][] pattern)
        {
            int counter = 2;
            for(int x=1; x<pattern.Length; ++x)
            {
                for(int y=1; y<pattern[x].Length; ++y)
                {
                    if (pattern[x][y] == 1)
                    {
                        if (pattern[x][y - 1] != 0 || pattern[x - 1][y] != 0)
                        {
                            if(pattern[x][y - 1] < pattern[x - 1][y])
                            {
                                pattern[x][y] = pattern[x - 1][y];
                                elevate(pattern, x, y - 1, pattern[x - 1][y]);
                            }
                            else
                            {
                                pattern[x][y] = pattern[x][y - 1];
                                elevate(pattern, x - 1, y, pattern[x][y - 1]);
                            }
                        }
                        else
                        {
                            pattern[x][y] = counter;
                            counter++;
                        }
                    }
                }
            }
        }

        public void elevate(int[][] pattern, int x, int y, int newValue)
        {
            if (x < 0 || x >= pattern.Length || y < 0 || y >= pattern[0].Length) return;
            if (pattern[x][y] == 0 || pattern[x][y] == newValue) return;
            pattern[x][y] = newValue;

            elevate(pattern, x - 1, y, newValue);
            elevate(pattern, x + 1, y, newValue);
            elevate(pattern, x, y - 1, newValue);
            elevate(pattern, x, y + 1, newValue);
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
            for (int x = 0; x < _collisionModel.GetLength(0); ++x)
            {
                for (int y = 0; y < _collisionModel.GetLength(1); ++y)
                {
                    var color = Color.Gray;
                    var lineWidth = 1;
                    if (_collisionTemplate[x][y] == 0)
                    {
                        Factory_Debug.DrawRectangle(
                        y*TileWidth + (int)Position.X, x*TileHeight + (int)Position.Y, TileWidth, TileHeight,
                        lineWidth, color, spriteBatch);
                    }
                    else
                    {
                        Factory_Debug.DrawRectangle(
                        y * TileWidth + (int)Position.X, x * TileHeight + (int)Position.Y, TileWidth, TileHeight,
                        3, Color.Yellow, spriteBatch);
                    }
                }
            }

            for (int i = 0; i < _collisionRectangles.Count(); ++i)
            {
                Factory_Debug.DrawRectangle(
                        _collisionRectangles[i].X + (int)Position.X,
                        _collisionRectangles[i].Y + (int)Position.Y,
                        _collisionRectangles[i].Width,
                        _collisionRectangles[i].Height,
                        3, _collisionRectColors[i], spriteBatch);
            }
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
        /// This method sets the background color for the regular Monogame Draw method
        /// </summary>
        /// <param name="color">Some Monogame color</param>
        public void SetBackgroundColor(Color color)
        {
        }

        /// <summary>
        /// This method resets the background color for the regular Monogame Draw method to Color.White
        /// </summary>
        public void ResetBackgroundColor()
        {
        }

        public bool Intersects(Rectangle other)
        {
            // TODO: this one needs to deal with the tiles of the walls
            // then goto X_CollisionManager and do sofisticated tile collision detection with the relevant tiles
            return other.Intersects(GetRect());
        }

        public bool Intersects(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal)
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            float uHit;
            List<Manager_Collision.Set> collided = new List<Manager_Collision.Set>();
            for(int i=0; i<_collisionRectangles.Length; ++i)
            {
                bool result = Manager_Collision.DynamicRectVsRect(
                    ref movingRect, velocity, timeStepMS,
                    ref _collisionRectangles[i], out contactPoint, out contactNormal, out uHit);

                if (result)
                {
                    collided.Add(new Manager_Collision.Set(contactNormal, uHit));
                    _collisionRectColors[i] = Color.Yellow;
                }
                else
                {
                    _collisionRectColors[i] = Color.Red;
                }
            }

            foreach (var col in collided)
            {
                Vector2 v = new Vector2(Math.Abs(velocity.X), Math.Abs(velocity.Y)) * (1 - col.UHit);
                velocity += col.ContactNormal * v;
            }

            return collided.Count() > 0;
        }

        /// <summary>
        /// This method is the standard ILevelElement WhatAreYou
        /// </summary>
        /// <returns>Returns the fitting X_LevelElements enum entry</returns>
        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Collision;
        }

        public Rectangle GetRect()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, 10, 10);
        }

        public Vector2 Clamp(Rectangle rect, Vector2 pos, Vector2 dp, ref IWalkable who, ref Vector2 where)
        {
            return pos + dp;
        }
    }
}
