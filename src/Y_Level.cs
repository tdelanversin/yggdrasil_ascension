using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpFont.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public Color OutsideColor { get; set; }

        private string _name;

        private List<X_Light> _lights;

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

            int connectorWidth = 17;
            Rooms = new Dictionary<string, IWalkable>
            {
                { "center", new Y_CMRoom("r2", TileWidth, TileHeight, resourceFolder + "R2", graphicsDevice) },
                { "middle", new Y_CMRoom("r0", TileWidth, TileHeight, resourceFolder + "R0", graphicsDevice) },
                { "top", new Y_CMRoom("r1", TileWidth, TileHeight, resourceFolder + "R1", graphicsDevice) },
                { "left", new Y_CMRoom("r3-L", TileWidth, TileHeight, resourceFolder + "R3", graphicsDevice) },
                { "right", new Y_CMRoom("r3-R", TileWidth, TileHeight, resourceFolder + "R3", graphicsDevice) },
                { "bottom", new Y_CMRoom("r3-B", TileWidth, TileHeight, resourceFolder + "R3", graphicsDevice) },
                { "door-center-to-middle", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, -7, graphicsDevice) },
                { "door-center-to-left", new Y_Door(X_DoorDirection.Horizontal, connectorWidth, TileWidth, TileHeight, 9, graphicsDevice) },
                { "door-center-to-right", new Y_Door(X_DoorDirection.Horizontal, connectorWidth, TileWidth, TileHeight, -3, graphicsDevice) },
                { "door-center-to-bottom", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, -3, graphicsDevice) },
                { "door-middle-to-top", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 3, graphicsDevice) },
            };

            //Rooms["center"].MoveTo(new Point(-(4*tileSize), 0));

            ((Y_Door)Rooms["door-center-to-middle"]).Connect(X_ConnectorSide.Bottom, Rooms["center"], X_ConnectorSide.Top, Rooms["middle"]);
            ((Y_Door)Rooms["door-center-to-left"]).Connect(X_ConnectorSide.Right, Rooms["center"], X_ConnectorSide.Left, Rooms["left"]);
            ((Y_Door)Rooms["door-center-to-right"]).Connect(X_ConnectorSide.Left, Rooms["center"], X_ConnectorSide.Right, Rooms["right"]);
            ((Y_Door)Rooms["door-center-to-bottom"]).Connect(X_ConnectorSide.Top, Rooms["center"], X_ConnectorSide.Bottom, Rooms["bottom"]);
            ((Y_Door)Rooms["door-middle-to-top"]).Connect(X_ConnectorSide.Bottom, Rooms["middle"], X_ConnectorSide.Top, Rooms["top"]);

            // finalize: split collision models
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() == X_LevelElements.Door)
                {
                    ((Y_Door)room.Value).SplitConnectedCollisionModels();
                }
            }

            float scale = (float)TileWidth / 16.0f;
            _lights = new List<X_Light>();
            foreach (var room in Rooms.Values)
            {
                _lights.Add(
                    new X_Light(
                        //new Vector3((int)(3.5 * tileSize), (int)(4 * tileSize), 1 * tileSize),
                        new Vector3(room.Rect.X + tileSize, room.Rect.Y + tileSize, 5 * tileSize),
                        getLightRect(room, 2),
                        Color.Black,
                        0.4f,
                        scale
                ));
            }

            //var r = Rooms["center"];
            //_lights.Add(
            //        new X_Light(
            //            //new Vector3((int)(3.5 * tileSize), (int)(4 * tileSize), 1 * tileSize),
            //            new Vector3(r.Rect.X + tileSize, r.Rect.Y + tileSize, 5 * tileSize),
            //            getLightRect(r, 2),
            //            Color.Black,
            //            0.4f,
            //            scale
            //    ));

            //r = Rooms["left"];
            //_lights.Add(
            //        new X_Light(
            //            //new Vector3((int)(3.5 * tileSize), (int)(4 * tileSize), 1 * tileSize),
            //            new Vector3(r.Rect.X + tileSize, r.Rect.Y + tileSize, 5 * tileSize),
            //            getLightRect(r, 2),
            //            Color.Black,
            //            0.4f,
            //            scale
            //    ));

            //r = Rooms["middle"];
            //_lights.Add(
            //        new X_Light(
            //            //new Vector3((int)(3.5 * tileSize), (int)(4 * tileSize), 1 * tileSize),
            //            new Vector3(r.Rect.X + tileSize, r.Rect.Y + tileSize, 5 * tileSize),
            //            getLightRect(r, 2 ),
            //            Color.Black,
            //            0.4f,
            //            scale
            //    ));

            var model = Manager_Light.Elevate(this);
            foreach (var room in Rooms.Values)
            {
                Manager_Light.Illuminate(_lights, room, model);
            }

            OutsideColor = ((Y_CMRoom)(Rooms.Values.First())).RegionColor;
            Projectiles = new List<IProjectile>();
            Victims = new List<IVictim>();
        }

        private Rectangle getLightRect(IWalkable room, int offsetWidth)
        {
            return new Rectangle(
                room.Rect.X - offsetWidth * TileWidth,
                room.Rect.Y - offsetWidth * TileHeight,
                room.Rect.Width + 2 * offsetWidth * TileWidth,
                room.Rect.Height + 2 * offsetWidth * TileHeight
            );
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
            foreach(var light in _lights)
            {
                light.DrawOutline(gameTime, globalOffset, spriteBatch);
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
