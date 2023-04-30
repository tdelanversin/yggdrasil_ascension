using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
            public string LdtkSubfolderName;
            public Dictionary<string, Dictionary<string, string>> LdtkRoomTypes;
            public float W;
            public float H;
            public Dictionary<string, List<LevelNode>> Level;
        }

        public enum GamePlayState
        {
            Start, // Not completed starting room
            FreeRoam, // Not in an encounter, players can freely roam
            Encounter, // Players are in an encounter, room locked
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

        private Y_CMRoom _startRoom;
        private Y_CMRoom _goldRoom;

        // Gameplay state objects
        public IWalkable ActiveRoom;
        public GamePlayState State;
        private List<Interactable_Basic> _interactables = new List<Interactable_Basic> { };

        Dictionary<string, List<Tuple<X_RoomStump, Y_CMRoom>>> _availableRooms;
        Y_Level.Data _data;

        public Y_Level(
            string name,
            int tileSize,
            int textureTileSize,
            string levelResourceFolder,
            string doorResourceFolder
        )
        {
            _name = Util.PathOsNormalization(name);
            _levelResourceFolder = Util.PathOsNormalization(levelResourceFolder);
            _doorResourceFolder = Util.PathOsNormalization(doorResourceFolder);

            TileWidth = tileSize;
            TileHeight = tileSize;
            Scale = 1.0f;

            var dataFile = _name.Split(Path.DirectorySeparatorChar)[1];
            var dataFilePath = _levelResourceFolder + "data.json";
            using (StreamReader stream = new StreamReader(dataFilePath))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                _data = JsonConvert.DeserializeObject<Y_Level.Data>(array.ToString());
            }

            foreach (var dk in _data.Level.Keys)
            {
                _data.Level[dk] = _data.Level[dk].OrderBy(x => x.Index).ToList();
            }

            var categoryFolders = Directory.GetDirectories(Util.PathOsNormalization(_levelResourceFolder + _name));
            List<Tuple<string, string>> files = new List<Tuple<string, string>>();
            foreach(var folder in categoryFolders)
            {
                string category = folder.Split(Path.DirectorySeparatorChar).Last();
                var fs = Directory.GetDirectories(Util.PathOsNormalization(folder + Path.DirectorySeparatorChar + category + Path.DirectorySeparatorChar + _data.LdtkSubfolderName));
                foreach(var f in fs)
                {
                    files.Add(new Tuple<string, string>(category, f));
                }
            }

            ConcurrentBag<X_RoomStump> list = new ConcurrentBag<X_RoomStump>();
            var watch = new Stopwatch();
            watch.Start();
            foreach (var f in files)
            {
                var roomName = f.Item2.Split(Path.DirectorySeparatorChar).Last();
                var room = new X_RoomStump(f.Item1, roomName, TileWidth, TileHeight, textureTileSize, f.Item2, _data.LdtkRoomTypes[f.Item1]);
                list.Add(room);
            }

            //Logger.Info("room load time " + watch.ElapsedMilliseconds.ToString());

            _availableRooms = new Dictionary<string, List<Tuple<X_RoomStump, Y_CMRoom>>>();
            foreach (var room in list)
            {
                //room.FinalizeItem(graphicsDevice);
                List<Tuple<X_RoomStump, Y_CMRoom>> rlist;
                if(!_availableRooms.TryGetValue(room.Category, out rlist))
                {
                    _availableRooms.Add(room.Category, new List<Tuple<X_RoomStump, Y_CMRoom>> { new Tuple<X_RoomStump, Y_CMRoom>(room, null) });
                }
                else
                {
                    rlist.Add(new Tuple<X_RoomStump, Y_CMRoom>(room, null));
                }
            }
            float elapsed = (float)watch.ElapsedMilliseconds;
            Logger.Info("room finalize time for " + list.Count() + " rooms: " + elapsed + " which is " + elapsed / list.Count() + " ms per room");

            Rooms = new Dictionary<int, IWalkable>();
        }


        public void Create(GraphicsDevice graphicsDevice)
        {
            //var room = new Y_CMRoom(roomName, TileWidth, TileHeight, f, graphicsDevice);
            // Stop any songs that are playing
            Manager_Sound.StopMusic();

            Manager_Sound.Sound_VikingHorn.Play();

            // put everything back
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() == X_LevelElements.Room)
                {
                    var r = (Y_CMRoom)room.Value;
                    r.ResetRoom();
                    _availableRooms[r.Category].Add(new Tuple<X_RoomStump, Y_CMRoom>(null, r));
                }
            }

            Rooms = new Dictionary<int, IWalkable>();

            //Rooms.Add(0, _availableRooms["Start"].First());
            
            var random = new Random();
            // randomly select one level tree
            var key = _data.Level.Keys.ToArray()[random.Next(0, _data.Level.Keys.Count)]; // [random.Next(0, _data.Level.Keys.Count)];
            var tree = _data.Level[key];

            var watch = new Stopwatch();
            watch.Start();
            float offset = 1024;
            foreach (var node in tree)
            {
                var type = _availableRooms[node.Type];
                int index = random.Next(0, type.Count);
                var n = type[index];
                if(n.Item2 == null)
                {
                    n = new Tuple<X_RoomStump,Y_CMRoom>(null, new Y_CMRoom(n.Item1));
                    n.Item2.FinalizeItem(graphicsDevice);
                }
                type.RemoveAt(index);
                var room = n.Item2;
                float h = 1.0f;
                float w = (float)room.Rect.Width / (float)room.Rect.Height;
                Point p = new Point(
                    (int)(node.X * w * offset - room.Rect.Width / 2),
                    (int)(node.Y * h * offset - room.Rect.Height / 2));
                p.X = p.X + (TileWidth - p.X % TileWidth);
                p.Y = p.Y + (TileHeight - p.Y % TileHeight);
                room.MoveTo(p);
                Rooms.Add(node.Index, room);

                if (node.Type == "Start") _startRoom = room;
                else if (node.Type == "Gold") _goldRoom = room;
            }
            Logger.Info("Loaded random rooms: " + watch.ElapsedMilliseconds.ToString());

            List<IWalkable> connectors = new List<IWalkable>();
            foreach (var room in Rooms)
            {
                int index = room.Key;
                int len = tree[index].Connections.Count();
                if (len == 0) continue;

                var connection = tree[index].Connections[random.Next(0, len)];

                int tileOffset = 1;
                int numTilesLength = 17;
                X_DoorDirection direction = X_DoorDirection.Horizontal;
                var fromRoom = room.Value;
                foreach (var con in connection)
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
            Logger.Info("Created connectors: " + watch.ElapsedMilliseconds.ToString());

            int connectorIndex = Rooms.Count();
            foreach (var c in connectors)
            {
                Rooms.Add(connectorIndex, c);
                connectorIndex++;
            }

            //finalize: split collision models
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() == X_LevelElements.Door)
                {
                    ((Y_Door)room.Value).SplitConnectedCollisionModels();
                }
            }

            _startRoom.State = X_RoomState.LockedOpen;
            _goldRoom.State = X_RoomState.LockedOpen;

            Manager_Players.ClearPlayers();

            // Place all players, even if they're not going to play
            var spawningPoints = ((Y_CMRoom)_startRoom).GetPlayerSpawningPoints();
            var sp = spawningPoints.First();
            for (int i = Manager_Players.Players.Count; i < 4; i++)
            {
                Manager_Players.AddPlayer_Random((PlayerIndex)i, position: spawningPoints[i].ToVector2(), this);
            }

            // Make the last one controllable by keyboard
            ((SimplePlayer)Manager_Players.Players[3]).ControlLayout = ControlLayout.KeyboardWASD;

            _interactables.Clear();

            // Useless box were all to be participating players should go in
            Interactable_PlayerField playerField = new Interactable_PlayerField(
                new Rectangle(5, 5, 8, 8), this, (Y_CMRoom)_startRoom
            );
            _interactables.Add(playerField);

            // Room opener to start the game with all players standing in the field
            _interactables.Add(new Interactable_RoomOpener(
                new Rectangle(29, 5, 8, 8), this, (Y_CMRoom)_startRoom, playerField)
            );

            // Gameplay state
            State = GamePlayState.Start;
            ActiveRoom = _startRoom;
            //Camera.focusOnRoom(_startRoom);
            Camera.focusManual();

            Camera.Players = Manager_Players.Players;
            Manager_Enemies.ClearEnemies();
            foreach (var room in Rooms)
            {
                if (room.Key == 0) continue;
                if (room.Value.WhatAreYou() != X_LevelElements.Room) continue;

                var r = (Y_CMRoom)room.Value;
                var regularSpawners = r.GetRegularSpawningPoints();
                var bossSpawners = r.GetBossSpawningPoints();

                if (regularSpawners != null)
                {
                    foreach (var spr in regularSpawners)
                    {
                        Manager_Enemies.AddEnemy_SimpleEnemy(spr.ToVector2(), this, Manager_Players.Players);
                    }
                }
                if (bossSpawners != null)
                {
                    foreach (var spr in bossSpawners)
                    {
                        Manager_Enemies.AddEnemy_Gigachad(spr.ToVector2(), this, Manager_Players.Players);
                    }
                }
            }

            foreach (var room in Rooms.Values)
            {
                Manager_Light2.CreateModel(room);
                room.Illuminate();
            }

            Manager_Sound.PlayFreeRoamMusic();
        }

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

            foreach (var interactable in _interactables)
            {
                interactable.Update(gameTime);
            }

            switch (State)
            {
                case GamePlayState.Start:
                    if (_interactables[1].InteractionComplete)
                    {
                        State = GamePlayState.FreeRoam;
                        Camera.focusOnPlayers();
                    }
                    break;
                case GamePlayState.FreeRoam:
                    ActiveRoom = Manager_Players.Players[0].Room;

                    if (ActiveRoom.WhatAreYou() != X_LevelElements.Room)
                    {
                        break;
                    }
                    var cmroom = (Y_CMRoom)ActiveRoom;

                    if (cmroom == _startRoom || cmroom.Cleared)
                    {
                        break;
                    }

                    if (cmroom.GetPlayersInside().Count != Manager_Players.Players.Count)
                    {
                        break;
                    }

                    cmroom.LockRoom();
                    Camera.focusOnRoom(cmroom);
                    foreach (var enemy in cmroom.GetEnemiesInside())
                    {
                        enemy.State = EnemyState.Idle;
                    }
                    if (cmroom.Name == "Gold_0")
                    {
                        Manager_Sound.PlayBossMusic();
                    }
                    else
                    {
                        Manager_Sound.PlayEncounterMusic();
                    }
                    Notifications.New("Starting encouter");

                    State = GamePlayState.Encounter;
                    break;
                case GamePlayState.Encounter:
                    // We can assume at this point that _currentRoom is actually
                    // a room, otherwise we wouldn't be here
                    var encounterRoom = (Y_CMRoom)ActiveRoom;

                    if (encounterRoom.GetEnemiesInside().Count > 0)
                    {
                        break; // let players fight
                    }
                    encounterRoom.Cleared = true;
                    encounterRoom.OpenDoorsAndAdjacentRooms();
                    Camera.focusOnPlayers();

                    Manager_Sound.PlayFreeRoamMusic();
                    Notifications.New("Room " + ActiveRoom.Name + " cleared!");

                    State = GamePlayState.FreeRoam;
                    break;
                default:
                    break;
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var room in Rooms)
            {
                room.Value.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var room in Rooms)
            {
                room.Value.Draw(gameTime, globalOffset, spriteBatch);
            }

            foreach (var interactable in _interactables)
            {
                interactable.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Level;
        }
    }
}
