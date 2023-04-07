using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Y_Level : IGameElement
    {
        public Rectangle Rect { get; set; }
        public IDictionary<string, IWalkable> Rooms { get; private set; }

        public int TileWidth { get; }
        public int TileHeight { get; }

        public Y_Level(
            string name,
            int tileWidth,
            int tileHeight,
            string resourceFolder)
        {
            if (resourceFolder.Substring(0, 1) == "/")
                resourceFolder = "." + resourceFolder;
            else if (resourceFolder.Substring(0, 2) != "./")
                resourceFolder = "./" + resourceFolder;
            if (resourceFolder.Substring(resourceFolder.Length - 2, 1) != "/")
                resourceFolder += "/";


            TileWidth = tileWidth;
            TileHeight = tileHeight;

            Rooms = new Dictionary<string, IWalkable>
            {
                { "door", new Y_Door(X_DoorDirection.Horizontal, 13, TileWidth, TileHeight, 0) }
                //{ "center", new Y_CMRoom("r0", TileWidth, TileHeight, resourceFolder + "R0") },
                //{ "bottom", new Y_CMRoom("r2", TileWidth, TileHeight, resourceFolder + "R2") },
                //{ "top", new Y_CMRoom("r1", TileWidth, TileHeight, resourceFolder + "R1") },
                //{ "left", new Y_CMRoom("r3", TileWidth, TileHeight, resourceFolder + "R3") }
            };

            //Rooms["center"].MoveTo(new Point(0, -Rooms["center"].Rect.Height - 40));
            //Rooms["top"].MoveTo(new Point(0, Rooms["center"].Rect.Y - Rooms["top"].Rect.Height - 40));
            //Rooms["left"].MoveTo(new Point(-Rooms["left"].Rect.Width - 40, Rooms["bottom"].Rect.Height / 3));


        }

        public void Update(GameTime gameTime)
        {

        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach(var room in Rooms)
            {
                room.Value.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach(var room in Rooms)
            {
                room.Value.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Level;
        }
    }
}
