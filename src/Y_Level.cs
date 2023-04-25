using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SharpFont.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YGR
{
    public class Y_Level : IGameElement
    {
        internal class LevelNode
        {
            public int Index;
            public bool Optional;
            public string Type;
            public float X;
            public float Y;
            public List<Dictionary<string, string[]>> Connections;
        }

        internal class Data
        {
            public float W;
            public float H;
            public Dictionary<string, List<LevelNode>> Level;
        }

        public float Scale { get; }
        public Rectangle Rect { get; set; }
        public IDictionary<int, IWalkable> Rooms { get; private set; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public IList<IVictim> Victims { get; }
        public Color OutsideColor { get; set; }

        private string _name;
        private string _levelResourceFolder;
        private string _doorResourceFolder;
        Dictionary<string, List<Y_CMRoom>> _availableRooms;
        Y_Level.Data _data;

        public Y_Level(
            string name,
            int tileSize,
            string levelResourceFolder,
            string doorResourceFolder,
            GraphicsDevice graphicsDevice
        )
        {
            _name = Util.PathOsNormalization(name);
            _levelResourceFolder = Util.PathOsNormalization(levelResourceFolder);
            _doorResourceFolder = Util.PathOsNormalization(doorResourceFolder);

            TileWidth = tileSize;
            TileHeight = tileSize;
            Scale = 1.0f;

            _availableRooms = new Dictionary<string, List<Y_CMRoom>>();

            var files = Directory.GetDirectories(Util.PathOsNormalization(_levelResourceFolder + _name));
            List<Y_CMRoom> bag = new List<Y_CMRoom>();

            var watch = new Stopwatch();
            watch.Start();
            foreach(var f in files)
            {
                var roomName = f.Split(Path.DirectorySeparatorChar).Last();
                //if (_availableRooms.ContainsKey(roomName))
                //    Logger.Error("Room with name " + roomName + " already added");
                var room = new Y_CMRoom(roomName, TileWidth, TileHeight, f, graphicsDevice);
                room.PreloadIlluminations();
                bag.Add(room);
                //var name = Path.GetDirectoryName(f);
            }//);

            foreach(var b in bag)
            {
                if (_availableRooms.ContainsKey(b.Name))
                    Logger.Error("Room with name " + b.Name + " already added");
                var key = b.Name.Split("_")[0];
                List<Y_CMRoom> rooms;
                if(!_availableRooms.TryGetValue(key, out rooms))
                {
                    _availableRooms.Add(key, new List<Y_CMRoom> { b });
                }
                else
                {
                    rooms.Add(b);
                }
            }

            Logger.Info("Loaded all rooms: " + watch.ElapsedMilliseconds.ToString());

            var dataFile = _name.Split(Path.DirectorySeparatorChar)[1];
            var dataFilePath = _levelResourceFolder + dataFile + "_data.json";
            using (StreamReader stream = new StreamReader(dataFilePath))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                _data = JsonConvert.DeserializeObject<Y_Level.Data>(array.ToString());
            }

            foreach(var dk in _data.Level.Keys)
            {
                _data.Level[dk] = _data.Level[dk].OrderBy(x => x.Index).ToList();
            }

            Rooms = new Dictionary<int, IWalkable>();
        }


        public void Create(GraphicsDevice graphicsDevice) 
        { 
            int connectorWidth = 17;
            var watch2 = new Stopwatch();
            var watch = new Stopwatch();
            watch.Start();
            watch2.Start();

            // put everything back
            foreach (var room in Rooms)
            {
                if(room.Value.WhatAreYou() == X_LevelElements.Room)
                {
                    var roomKey = room.Value.Name.Split("_")[0];
                    _availableRooms[roomKey].Add((Y_CMRoom)room.Value);
                }
            }

            Rooms = new Dictionary<int, IWalkable>();

            var random = new Random();
            // randomly select one level tree
            var key = _data.Level.Keys.ToArray()[random.Next(0, _data.Level.Keys.Count)]; // [random.Next(0, _data.Level.Keys.Count)];
            var tree = _data.Level[key];

            float offset = 1024;
            foreach (var node in tree)
            {
                var type = _availableRooms[node.Type];
                int index = random.Next(0, type.Count);
                var n = type[index];
                type.RemoveAt(index);
                float h = 1.0f;
                float w = (float)n.Rect.Width / (float)n.Rect.Height;
                Point p = new Point(
                    (int)(node.X * w * offset - n.Rect.Width/2), 
                    (int)(node.Y * h * offset - n.Rect.Height/2));
                p.X = p.X + (TileWidth - p.X % TileWidth);
                p.Y = p.Y + (TileHeight - p.Y % TileHeight);
                n.MoveTo(p);
                Rooms.Add(node.Index, n);
            }

            List<IWalkable> connectors = new List<IWalkable>();
            foreach(var room in Rooms)
            {
                int index = room.Key;
                int len = tree[index].Connections.Count();
                if (len == 0) continue;

                var connection = tree[index].Connections[random.Next(0, len)];

                int tileOffset = 1;
                int numTilesLength = 17;
                X_DoorDirection direction = X_DoorDirection.Horizontal;
                var fromRoom = room.Value;
                foreach(var con in connection)
                {
                    X_ConnectorSide fromSide = Y_Door.ParseFromSide(con.Key);
                    int ll = con.Value.Length;
                    string toSideStr = con.Value[random.Next(0, ll)];
                    Tuple<int, X_ConnectorSide> toSide = Y_Door.ParseToSide(toSideStr);

                    if (toSide == null) continue;

                    var fromConnectorPoint = fromRoom.GetConnectorPoint(fromSide);
                    var toRoom = Rooms[toSide.Item1];
                    var toConnectorPoint = toRoom.GetConnectorPoint(toSide.Item2);
                    Y_Door.GetDoorType(
                        fromConnectorPoint,
                        toConnectorPoint, 
                        TileHeight,
                        out direction, out numTilesLength, out tileOffset);

                    //if(fromRoom.Name == "Leaf_3" || toRoom.Name == "Leaf_3")
                    ////if (Math.Abs(numTilesLength) < 7 || direction == X_DoorDirection.Corner && Math.Abs(tileOffset) < 7)
                    //    Logger.Info("Problem");

                    if(direction == X_DoorDirection.Corner)
                    {
                        Logger.Info("blup");
                    }
                    var connector = new Y_Door(
                        direction,
                        numTilesLength,
                        TileWidth,
                        TileHeight,
                        tileOffset,
                        graphicsDevice,
                        "./Doors",
                        "data.json");

                    connectors.Add(connector.Connect(fromRoom, fromConnectorPoint, toRoom, toConnectorPoint, direction));
                }
            }

            int connectorIndex = Rooms.Count();
            foreach (var c in connectors)
            {
                Rooms.Add(connectorIndex, c);
                connectorIndex++;
            }









            //foreach (var room in _availableRooms)
            //{
            //    Rooms.Add(room.Key, room.Value);
            //    _availableRooms.Remove(room.Key);
            //}

            //{
            //    { "center", new Y_CMRoom("r2", TileWidth, TileHeight, resourceFolder + "Room_0", graphicsDevice) },
            //    { "middle", new Y_CMRoom("r0", TileWidth, TileHeight, resourceFolder + "Room_1", graphicsDevice) },
            //    { "top", new Y_CMRoom("r1", TileWidth, TileHeight, resourceFolder + "Room_2", graphicsDevice) },
            //    { "left", new Y_CMRoom("r3-L", TileWidth, TileHeight, resourceFolder + "Room_3", graphicsDevice) },
            //    { "right", new Y_CMRoom("r3-R", TileWidth, TileHeight, resourceFolder + "Room_4", graphicsDevice) },
            //    { "bottom1", new Y_CMRoom("r3-B1", TileWidth, TileHeight, resourceFolder + "Room_5", graphicsDevice) },
            //    { "bottom2", new Y_CMRoom("r3-B2", TileWidth, TileHeight, resourceFolder + "Room_5", graphicsDevice) },
            //    { "bottom3", new Y_CMRoom("r3-B3", TileWidth, TileHeight, resourceFolder + "Room_5", graphicsDevice) },
            //    { "bottom4", new Y_CMRoom("r3-B4", TileWidth, TileHeight, resourceFolder + "Room_5", graphicsDevice) },
            //    { "bottom5", new Y_CMRoom("r3-B5", TileWidth, TileHeight, resourceFolder + "Room_5", graphicsDevice) },
            //    { "bottom6", new Y_CMRoom("r3-B6", TileWidth, TileHeight, resourceFolder + "Room_5", graphicsDevice) },
            //};

            //Logger.Info("-----Initialized all rooms: " + watch.ElapsedMilliseconds.ToString());
            //int width = -17;
            //int offset2 = 9;
            //Rooms.Add(0,
            //    new Y_Door(
            //        X_DoorDirection.Corner,
            //        width,
            //        TileWidth,
            //        TileHeight,
            //        offset2,
            //        graphicsDevice,
            //        "./Doors",
            //        "data.json")
            //    );

            //Rooms.Add("door-center-to-left", new Y_Door(X_DoorDirection.Horizontal, connectorWidth, TileWidth, TileHeight, 1, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-right", new Y_Door(X_DoorDirection.Horizontal, connectorWidth, TileWidth, TileHeight, -3, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-bottom1", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 0, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-middle-to-top", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 0, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-bottom2", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 3, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-bottom3", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 3, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-bottom4", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 3, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-bottom5", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 3, graphicsDevice, "./Doors", "data.json"));
            //Rooms.Add("door-center-to-bottom6", new Y_Door(X_DoorDirection.Vertical, connectorWidth, TileWidth, TileHeight, 3, graphicsDevice, "./Doors", "data.json"));

            Logger.Info("-----Initialized all corridors: " + watch.ElapsedMilliseconds.ToString());

            //((Y_Door)Rooms["door-center-to-middle"]).Connect(X_ConnectorSide.Bottom, Rooms["center"], X_ConnectorSide.Top, Rooms["middle"]);
            //((Y_Door)Rooms["door-center-to-left"]).Connect(X_ConnectorSide.Right, Rooms["center"], X_ConnectorSide.Left, Rooms["left"]);
            //((Y_Door)Rooms["door-center-to-right"]).Connect(X_ConnectorSide.Left, Rooms["center"], X_ConnectorSide.Right, Rooms["right"]);
            //((Y_Door)Rooms["door-center-to-bottom1"]).Connect(X_ConnectorSide.Top, Rooms["center"], X_ConnectorSide.Bottom, Rooms["bottom1"]);
            //((Y_Door)Rooms["door-middle-to-top"]).Connect(X_ConnectorSide.Bottom, Rooms["middle"], X_ConnectorSide.Top, Rooms["top"]);

            //((Y_Door)Rooms["door-center-to-bottom2"]).Connect(X_ConnectorSide.Bottom, Rooms["top"], X_ConnectorSide.Top, Rooms["bottom2"]);
            //((Y_Door)Rooms["door-center-to-bottom3"]).Connect(X_ConnectorSide.Bottom, Rooms["bottom2"], X_ConnectorSide.Top, Rooms["bottom3"]);
            //((Y_Door)Rooms["door-center-to-bottom4"]).Connect(X_ConnectorSide.Bottom, Rooms["bottom3"], X_ConnectorSide.Top, Rooms["bottom4"]);
            //((Y_Door)Rooms["door-center-to-bottom5"]).Connect(X_ConnectorSide.Bottom, Rooms["bottom4"], X_ConnectorSide.Top, Rooms["bottom5"]);
            //((Y_Door)Rooms["door-center-to-bottom6"]).Connect(X_ConnectorSide.Bottom, Rooms["bottom5"], X_ConnectorSide.Top, Rooms["bottom6"]);

            Logger.Info("------Connected all levels: " + watch.ElapsedMilliseconds.ToString());

            //finalize: split collision models
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() == X_LevelElements.Door)
                {
                    ((Y_Door)room.Value).SplitConnectedCollisionModels();
                }
            }
            Logger.Info("------Split collision models: " + watch.ElapsedMilliseconds.ToString());

            Manager_Light.CreateModel(this);
            Logger.Info("------Initialized light manager: " + watch.ElapsedMilliseconds.ToString());

            //float scale = (float)TileWidth / 32.0f;

            //_lights = new List<X_Light>();
            //foreach (var room in Rooms.Values)
            //{
            //    if (room.WhatAreYou() == X_LevelElements.Door) continue;
            //    var rect = getLightRect(room, 2);
            //    //rect.Offset(0, -20 * tileSize);
            //    _lights.Add(
            //        new X_Light(
            //            //new Vector3((int)(3.5 * tileSize), (int)(4 * tileSize), 1 * tileSize),
            //            new Vector3(room.Rect.X + -5*tileSize, room.Rect.Y + 5*tileSize, 5 * tileSize),
            //            rect,
            //            scale
            //    ));
            //}

            //Rooms["center"].MoveTo(new Point(0, -(20 * tileSize)));

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

            //if (Settings.Lighting)
            //{
            foreach (var room in Rooms.Values)
            {
                room.Illuminate();
                Logger.Info("                                intermediate: " + watch2.ElapsedMilliseconds.ToString());
            }
            //}
            Logger.Info("-------Initialized level: " + watch2.ElapsedMilliseconds.ToString());

            //OutsideColor = ((Y_CMRoom)(Rooms.Values.First())).RegionColor;
        }

        //private Rectangle getLightRect(IWalkable room, int offsetWidth)
        //{
        //    return new Rectangle(
        //        room.Rect.X - offsetWidth * TileWidth,
        //        room.Rect.Y - offsetWidth * TileHeight,
        //        room.Rect.Width + 2 * offsetWidth * TileWidth,
        //        room.Rect.Height + 2 * offsetWidth * TileHeight
        //    );
        //}

        public IWalkable GetRoom(IGameElement elem, IWalkable currentRoom)
        {
            var location = elem.Rect.Location + new Point(elem.Rect.Width / 2, elem.Rect.Height / 2);
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
            foreach (var room in Rooms.Values)
            {
                room.Update(gameTime);
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var room in Rooms)
            {
                room.Value.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
            //foreach (var light in _lights)
            //{
            //    light.DrawOutline(gameTime, globalOffset, spriteBatch);
            //}
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var room in Rooms)
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
