using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpFont.Cache;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Y_Level : IGameElement
    {
        public float Scale { get; }
        public Rectangle Rect { get; set; }
        public IDictionary<string, IWalkable> Rooms { get; private set; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }

        private string _name;

        public Y_Level(
            string name,
            int tileSize,
            string resourceFolder,
            GraphicsDevice graphicsDevice
        )
        {
            _name = name;

            if (resourceFolder.Substring(0, 1) == "/")
                resourceFolder = "." + resourceFolder;
            else if (resourceFolder.Substring(0, 2) != "./")
                resourceFolder = "./" + resourceFolder;
            if (resourceFolder.Substring(resourceFolder.Length - 2, 1) != "/")
                resourceFolder += "/";

            TileWidth = tileSize;
            TileHeight = tileSize;
            Scale = 1.0f;

            Rooms = new Dictionary<string, IWalkable>
            {
                //{ "center", new Y_CMRoom("r2", TileWidth, TileHeight, resourceFolder + "R2", graphicsDevice) },
                { "middle", new Y_CMRoom("r0", TileWidth, TileHeight, resourceFolder + "R0", graphicsDevice) },
                //{ "top", new Y_CMRoom("r1", TileWidth, TileHeight, resourceFolder + "R1", graphicsDevice) },
                //{ "left", new Y_CMRoom("r3-L", TileWidth, TileHeight, resourceFolder + "R3", graphicsDevice) },
                //{ "right", new Y_CMRoom("r3-R", TileWidth, TileHeight, resourceFolder + "R3", graphicsDevice) },
                //{ "bottom", new Y_CMRoom("r3-B", TileWidth, TileHeight, resourceFolder + "R3", graphicsDevice) },
                //{ "door-center-to-middle", new Y_Door(X_DoorDirection.Vertical, 17, TileWidth, TileHeight, -7, graphicsDevice) },
                //{ "door-center-to-left", new Y_Door(X_DoorDirection.Horizontal, 17, TileWidth, TileHeight, 9, graphicsDevice) },
                //{ "door-center-to-right", new Y_Door(X_DoorDirection.Horizontal, 17, TileWidth, TileHeight, -3, graphicsDevice) },
                //{ "door-center-to-bottom", new Y_Door(X_DoorDirection.Vertical, 25, TileWidth, TileHeight, -3, graphicsDevice) },
                //{ "door-middle-to-top", new Y_Door(X_DoorDirection.Vertical, 17, TileWidth, TileHeight, 3, graphicsDevice) },
            };

            //Rooms["left"].Illuminate(light);

            foreach(var room in Rooms.Keys)
            {
                X_Light light = new X_Light(
                    new Vector3(Rooms[room].Rect.Width / 2, Rooms[room].Rect.Height / 2, Rooms[room].Rect.Width),
                    new Vector3(Rooms[room].Rect.Width / 2, Rooms[room].Rect.Height / 2, 0),
                    1, 1,
                    new Vector3(1.0f, 1.0f, 1.0f)
                );
                Rooms[room].Illuminate(light);
            }

            //((Y_Door)Rooms["door-center-to-middle"]).Connect(X_ConnectorSide.Bottom, Rooms["center"], X_ConnectorSide.Top, Rooms["middle"]);
            //((Y_Door)Rooms["door-center-to-left"]).Connect(X_ConnectorSide.Right, Rooms["center"], X_ConnectorSide.Left, Rooms["left"]);
            //((Y_Door)Rooms["door-center-to-right"]).Connect(X_ConnectorSide.Left, Rooms["center"], X_ConnectorSide.Right, Rooms["right"]);
            //((Y_Door)Rooms["door-center-to-bottom"]).Connect(X_ConnectorSide.Top, Rooms["center"], X_ConnectorSide.Bottom, Rooms["bottom"]);
            //((Y_Door)Rooms["door-middle-to-top"]).Connect(X_ConnectorSide.Bottom, Rooms["middle"], X_ConnectorSide.Top, Rooms["top"]);

            // finalize: split collision models
            foreach (var room in Rooms)
            {
                if(room.Value.WhatAreYou() == X_LevelElements.Door)
                {
                    ((Y_Door)room.Value).SplitConnectedCollisionModels();
                }
            }

            Projectiles = new List<IProjectile>();
            Victims = new List<IVictim>();
        }

        public IWalkable GetRoom(IGameElement elem, IWalkable currentRoom)
        {
            var location = elem.Rect.Location + new Point(Rect.Width / 2, Rect.Height / 2);
            if (currentRoom != null && currentRoom.Rect.Contains(location))
            {
                return currentRoom;
            }

            foreach (var r in Rooms)
            {
                if (r.Value.Rect.Contains(location))
                {
                    currentRoom = r.Value;
                }
            }

            return currentRoom;
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
