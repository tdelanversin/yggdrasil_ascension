using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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

        // Gameplay state objects
        public IWalkable ActiveRoom;
        public GamePlayState State;
        private List<Interactable_Basic> _interactables = new List<Interactable_Basic> { };

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

            foreach (var f in files)
            {
                var roomName = f.Split(Path.DirectorySeparatorChar).Last();
                //if (_availableRooms.ContainsKey(roomName))
                //    Logger.Error("Room with name " + roomName + " already added");
                var room = new Y_CMRoom(roomName, TileWidth, TileHeight, f, graphicsDevice);
                room.PreloadIlluminations();
                bag.Add(room);
                //var name = Path.GetDirectoryName(f);
            }//);

            foreach (var b in bag)
            {
                if (_availableRooms.ContainsKey(b.Name))
                    Logger.Error("Room with name " + b.Name + " already added");
                var key = b.Name.Split("_")[0];
                if (key == "Start" || key == "Gold")
                {
                    b.State = X_RoomState.LockedOpen;
                }

                List<Y_CMRoom> rooms;
                if (!_availableRooms.TryGetValue(key, out rooms))
                {
                    _availableRooms.Add(key, new List<Y_CMRoom> { b });
                }
                else
                {
                    rooms.Add(b);
                }
            }

            var dataFile = _name.Split(Path.DirectorySeparatorChar)[1];
            var dataFilePath = _levelResourceFolder + dataFile + "_data.json";
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

            Rooms = new Dictionary<int, IWalkable>();
        }


        public void Create(GraphicsDevice graphicsDevice)
        {
            // Stop any songs that are playing
            Manager_Sound.StopMusic();

            Manager_Sound.Sound_VikingHorn.Play();

            // put everything back
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() == X_LevelElements.Room)
                {
                    var r = (Y_CMRoom)room.Value;
                    var roomKey = r.Name.Split("_")[0];
                    r.ResetRoom();
                    _availableRooms[roomKey].Add(r);
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
                    (int)(node.X * w * offset - n.Rect.Width / 2),
                    (int)(node.Y * h * offset - n.Rect.Height / 2));
                p.X = p.X + (TileWidth - p.X % TileWidth);
                p.Y = p.Y + (TileHeight - p.Y % TileHeight);
                n.MoveTo(p);
                Rooms.Add(node.Index, n);
            }

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

            int connectorIndex = Rooms.Count();
            foreach (var c in connectors)
            {
                Rooms.Add(connectorIndex, c);
                connectorIndex++;
            }

            Manager_Players.ClearPlayers();

            // Place all players, even if they're not going to play
            var spawningPoints = ((Y_CMRoom)Rooms[0]).GetPlayerSpawningPoints();
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
                new Rectangle(5, 5, 8, 8), this, (Y_CMRoom)Rooms[0]
            );
            _interactables.Add(playerField);

            // Room opener to start the game with all players standing in the field
            _interactables.Add(new Interactable_RoomOpener(
                new Rectangle(29, 5, 8, 8), this, (Y_CMRoom)Rooms[0], playerField)
            );

            // Gameplay state
            State = GamePlayState.Start;
            ActiveRoom = Rooms[0];
            Camera.focusOnRoom(Rooms[0]);

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

            //finalize: split collision models
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() == X_LevelElements.Door)
                {
                    ((Y_Door)room.Value).SplitConnectedCollisionModels();
                }
            }

            Manager_Light.CreateModel(this);

            foreach (var room in Rooms.Values)
            {
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

                    if (cmroom == Rooms[0] || cmroom.Cleared)
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
