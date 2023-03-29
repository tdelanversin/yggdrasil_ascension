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
    public class Y_LdtkRoom : IWalkable
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
        private Rectangle[,] _collisionModel;
        private string _location;

        public Y_LdtkRoom(
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
            if(_location.Substring(0, 2) != "./") _location = "./" + _location;

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

            // create collision model
            int lenx = _collisionTemplate.Length;
            int leny = _collisionTemplate[0].Length;
            _collisionModel = new Rectangle[lenx, leny];
            for (int x = 0; x < lenx; ++x)
            {
                for (int y = 0; y < leny; ++y)
                {
                    _collisionModel[x, y] = new Rectangle(x * TileWidth, y * TileHeight, TileWidth, TileHeight);
                }
            }
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
            for(int x=0; x<_collisionModel.GetLength(0); ++x)
            {
                for(int y=0; y<_collisionModel.GetLength(1); ++y)
                {
                    var color = Color.Gray;
                    var lineWidth = 1;
                    if (_collisionTemplate[x][y] == 1)
                    {
                        lineWidth = 3;
                        color = Color.Red;
                    }
                    var rect = _collisionModel[x, y];
                    Factory_Debug.DrawRectangle(
                        rect.X + (int)Position.X, rect.Y + (int)Position.Y, rect.Width, rect.Height,
                        lineWidth, color, spriteBatch);
                }
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
