using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using SharpFont.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace YGR
{
    public class Y_Level : IGameElement
    {
        internal class LevelNode
        {
#pragma warning disable 0649
            public bool Optional;
            public string Type;
            public float X;
            public float Y;
            public List<Dictionary<string, string[]>> Connections;
#pragma warning restore 0649
        }

        internal class Data
        {
#pragma warning disable 0649
            public string LdtkSubfolderName;
            public Dictionary<string, Dictionary<string, string>> LdtkRoomTypes;
            public float W;
            public float H;
            public Dictionary<string, Dictionary<int, LevelNode>> Level;
#pragma warning restore 0649
        }

        public enum GamePlayState
        {
            Tutorial, // Make sure we don't skip the beginning
            Start, // Not completed starting room
            FreeRoam, // Not in an encounter, players can freely roam
            Encounter, // Players are in an encounter, room locked
            Escaped,
            End,
        }

        public enum GameTutorialState
        {
            Welcome,
            IntroduceAllCharacters,
            IntroduceCharactersNinja,
            IntroduceCharactersNerd,
            IntroduceCharactersMailman,
            IntroduceCharactersProfessor,
            IntroduceGhosts,
            IntroducePowerUps,
            IntroduceYggdrasil,
            IntroduceBoss,
            IntroduceSampleRoomWSpikySlime,
            IntroduceSpikySlime,
            IntroduceStartButton,
            EndTutorial
        }

        public Rectangle Rect { get; set; }
        public static IDictionary<int, IWalkable> Rooms { get; private set; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public IList<IVictim> Victims { get; }
        public Color OutsideColor { get; set; }
        public int ElementLevel { get; set; }

        private string _name;
        private string _levelResourceFolder;
        private string _doorResourceFolder;

        private Y_CMRoom _startRoom;
        private Y_CMRoom _goldRoom;

        private int _waitTimeBetweenEndOfFightAndLowerDoors = 125;
        private int _waitTimeBetweenEndOfFightAndLowerDoorsCounter = 0;

        public static int TextureTileSize { get; set; }
        public static int InGameTileSize { get; set; }
        public static float GlobalScale { get; set; }

        // Gameplay state objects
        public IWalkable ActiveRoom;
        public static GamePlayState State;

        public static GameTutorialState TutorialState;
        private int MaxTutorialStageDurationMS = 20000;
        private int TutorialStageDurationCounterMS = 0;
        private int TutorialButtonPressCoolDownFC = 60;
        private int TutorialButtonPressCoolDownCounter = 0;

        Vector2 Tutorial_PlayerSlot_Nerd;
        Vector2 Tutorial_PlayerSlot_Ninja;
        Vector2 Tutorial_PlayerSlot_Professor;
        Vector2 Tutorial_PlayerSlot_Mailman;
        Vector2 Tutorial_GhostSlot;
        IWalkable Tutorial_SpikyRoom;
        Enemy_Slime_Spiky Tutorial_Spiky;

        Dictionary<string, List<Tuple<X_RoomStump, Y_CMRoom>>> _availableRooms;
        Y_Level.Data _data;

        public Y_Level(
            string name,
            int tileSize,
            int textureTileSize,
            string levelResourceFolder,
            string doorResourceFolder,
            ContentManager content
        )
        {
            /**
             * Use these three sizes everywhere in the game unless a scale is supposed to be local!!
             *   TextureTileSize = size in pixels of one actual tile in the physical textures
             *   InGameTileSize = size of a tile on the screen (make bigger for zooming in, so-to-speak)
             *   GlobalScale = if the two above are not the same, the GlobalScale will blow up the sizes of the textures to match things like collision rectangles
             */
            TextureTileSize = textureTileSize;
            InGameTileSize = tileSize;
            GlobalScale = (float)InGameTileSize / (float)TextureTileSize;

            _name = Util.PathOsNormalization(name);
            _levelResourceFolder = Util.PathOsNormalization(levelResourceFolder);
            _doorResourceFolder = Util.PathOsNormalization(doorResourceFolder);

            TileWidth = tileSize;
            TileHeight = tileSize;

            var dataFile = _name.Split(Path.DirectorySeparatorChar)[1];
            var dataFilePath = _levelResourceFolder + "data.json";
            using (StreamReader stream = new StreamReader(dataFilePath))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                _data = JsonConvert.DeserializeObject<Y_Level.Data>(array.ToString());
            }

            var categoryFolders = Directory.GetDirectories(Util.PathOsNormalization(_levelResourceFolder + _name));
            List<Tuple<string, string>> files = new List<Tuple<string, string>>();
            foreach (var folder in categoryFolders)
            {
                var fs = Directory.GetDirectories(Util.PathOsNormalization(folder + Path.DirectorySeparatorChar + "Trunc" + Path.DirectorySeparatorChar + _data.LdtkSubfolderName));
                foreach (var f in fs)
                {
                    string category = f.Split(Path.DirectorySeparatorChar).Last().Split("_").First();

                    if (Directory.GetFiles(f).Length > 0)
                        files.Add(new Tuple<string, string>(category, f));
                }
            }

            ConcurrentBag<X_RoomStump> list = new ConcurrentBag<X_RoomStump>();
            var watch = new Stopwatch();
            watch.Start();
            foreach (var f in files)
            {
                var roomName = f.Item2.Split(Path.DirectorySeparatorChar).Last();
                var room = new X_RoomStump(f.Item1, roomName, TileWidth, TileHeight, f.Item2, _data.LdtkRoomTypes[f.Item1]);
                list.Add(room);
            }

            //Logger.Info("room load time " + watch.ElapsedMilliseconds.ToString());

            _availableRooms = new Dictionary<string, List<Tuple<X_RoomStump, Y_CMRoom>>>();
            foreach (var room in list)
            {
                //room.FinalizeItem(graphicsDevice);
                List<Tuple<X_RoomStump, Y_CMRoom>> rlist;
                if (!_availableRooms.TryGetValue(room.Category, out rlist))
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

        public void Preprocess(GraphicsDevice graphicsDevice)
        {
            var cat = _availableRooms.Select(x => x.Key).ToList();
            foreach (var rooms in _availableRooms)
            {
                foreach (var room in rooms.Value)
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    if (Y_CMRoom.PreprocessRoom(room.Item1, cat, _data.LdtkRoomTypes, graphicsDevice))
                    {
                        Logger.Info("Preprocessed level [" + watch.ElapsedMilliseconds + "ms]" + room.Item1.ResourceFolder);
                    }
                }
            }
        }

        public void Create(GraphicsDevice graphicsDevice)
        {
            // put everything back
            foreach (var room in Rooms)
            {
                room.Value.ResetRoom();
                if (room.Value.WhatAreYou() == X_LevelElements.Room)
                {
                    var r = (Y_CMRoom)room.Value;
                    _availableRooms[r.Category].Add(new Tuple<X_RoomStump, Y_CMRoom>(null, r));
                }
            }

            Rooms = new Dictionary<int, IWalkable>();

            //Rooms.Add(0, _availableRooms["Start"].First());

            var random = new Random();
            // randomly select one level tree
            var key = _data.Level.Keys.ToArray()[random.Next(0, _data.Level.Keys.Count)];
            //var key = _data.Level.Keys.ToArray()[7];
            var tree = _data.Level[key];

            var watch = new Stopwatch();
            watch.Start();
            float offset = 1024;

            int lIndex = Util.random.Next(0, 4);

            foreach (var node in tree)
            {
                var type = _availableRooms[node.Value.Type];
                int index = random.Next(0, type.Count);

                // some hack to make sure that the first room is always the chosen one
                var n = type[index];

                if (n.Item2 == null)
                {
                    n = new Tuple<X_RoomStump, Y_CMRoom>(null, new Y_CMRoom(n.Item1, this));
                    n.Item2.FinalizeItem(graphicsDevice);

                }
                type.RemoveAt(index);
                var room = n.Item2;
                float h = 1.0f;
                float w = (float)room.Rect.Width / (float)room.Rect.Height;
                Point p = new Point(
                    (int)(node.Value.X * w * offset - room.Rect.Width / 2),
                    (int)(node.Value.Y * h * offset - room.Rect.Height / 2));
                p.X = p.X + (TileWidth - p.X % TileWidth);
                p.Y = p.Y + (TileHeight - p.Y % TileHeight);

                Vector3[] lPos = new Vector3[]
                {
                    new Vector3(-7, -10, 10),
                    new Vector3(room.Rect.Width/Y_Level.TextureTileSize + 7, -10, 10),
                    new Vector3(-7, room.Rect.Height/Y_Level.TextureTileSize + 7, 10),
                    new Vector3(room.Rect.Width/Y_Level.TextureTileSize + 7, room.Rect.Height/Y_Level.TextureTileSize + 10, 10)
                };

                if (lIndex >= lPos.Length) Logger.Error("Max index and length of array must agree (just some random check)");

                ((Y_CMRoom)room).AddLight((int)lPos[lIndex].X, (int)lPos[lIndex].Y, (int)lPos[lIndex].Z);
                room.MoveTo(p);
                Rooms.Add(node.Key, room);

                if (node.Value.Type == "Start")
                {
                    _startRoom = room;
                }
                else if (node.Value.Type == "Gold")
                {
                    _goldRoom = room;
                }
            }

            Logger.Info("Loaded random rooms: " + watch.ElapsedMilliseconds.ToString());

            List<IWalkable> connectors = new List<IWalkable>();
            //_barks = new List<Y_ConnectorBark>();
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

                    Y_Door connector;
                    if (
                        ((Y_CMRoom)fromRoom).Category == "Leaf" && ((Y_CMRoom)toRoom).Category == "Leaf" ||
                        ((Y_CMRoom)fromRoom).Category == "Leaf" && ((Y_CMRoom)toRoom).Category == "Gold" ||
                        ((Y_CMRoom)fromRoom).Category == "Gold" && ((Y_CMRoom)toRoom).Category == "Leaf"
                    )
                    {
                        connector = new Y_Door(
                            direction,
                            numTilesLength,
                            TileWidth,
                            TileHeight,
                            tileOffset,
                            graphicsDevice,
                            "./Doors",
                            "data-green.json");
                    }
                    else
                    {
                        connector = new Y_Door(
                            direction,
                            numTilesLength,
                            TileWidth,
                            TileHeight,
                            tileOffset,
                            graphicsDevice,
                            "./Doors",
                            "data-brown.json");
                    }

                    connectors.Add(connector.Connect(fromRoom, fromConnectorPoint, toRoom, toConnectorPoint, direction));
                    //if (b != null) _barks.Add(b);
                }
            }
            Logger.Info("Created connectors: " + watch.ElapsedMilliseconds.ToString());

            //finalize: split collision models
            foreach (var room in connectors)
            {
                if (room.WhatAreYou() == X_LevelElements.Door)
                {
                    ((Y_Door)room).SplitConnectedCollisionModels();
                }
            }

            int connectorIndex = Rooms.Max(x => x.Key) + 1;
            foreach (var c in connectors)
            {
                Rooms.Add(connectorIndex, c);
                connectorIndex++;
            }

            Manager_Players.ClearPlayers();

            // Gameplay state
            State = GamePlayState.Tutorial;
            ActiveRoom = _startRoom;

            // Slowly transition on game start, to show players that we're inside Yggdrasil
            Camera.SetFocusRoom(_startRoom, animate: true, animationDuration: 3000);
            Camera.InTransitionFromMenu = true;

            Manager_Enemies.ClearEnemies();

            // Select 8 rooms out of all rooms minus the Gold and the Start room
            // and mark them for containing a spiky semi boss slime
            var spikeRooms = Rooms.Values
                .Where(x => x.WhatAreYou() == X_LevelElements.Room &&
                          !(x.Category == "Start") &&
                          !(x.Category == "Gold"))
                .ToList();

            var selected = spikeRooms.OrderBy(x => Util.random.Next()).Take(9);
            foreach (var sel in selected)
            {
                ((Y_CMRoom)sel).HasSpikeEnemy = true;
            }

            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() != X_LevelElements.Room) continue;

                var r = (Y_CMRoom)room.Value;

                r.SpawnEnemies();

                var players = r.GetPlayerSpawningPoints();
                int playerIndex = 0;
                foreach (var spr in players)
                {
                    Vector2 pos = new Vector2(spr.x, spr.y);
                    if (PlayerEntity.GetPointType(spr) == PlayerSpawningPointType.Spawner)
                    {
                        if (playerIndex == 4) break;
                        if (PlayerEntity.GetType(spr) == PlayerType.Nerd)
                            Manager_Players.AddPlayer(PlayerType.Nerd, (PlayerIndex)playerIndex, position: pos, this);
                        if (PlayerEntity.GetType(spr) == PlayerType.Ninja)
                            Manager_Players.AddPlayer(PlayerType.Ninja, (PlayerIndex)playerIndex, position: pos, this);
                        if (PlayerEntity.GetType(spr) == PlayerType.Ghost)
                        {
                            Manager_Players.AddPlayer(PlayerType.Ghost, (PlayerIndex)playerIndex, position: pos, this);
                            Tutorial_GhostSlot = pos;
                        }
                        playerIndex++;
                    }
                    else if (PlayerEntity.GetPointType(spr) == PlayerSpawningPointType.Chooser)
                    {
                        if (PlayerEntity.GetType(spr) == PlayerType.Nerd)
                        {
                            r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserNerd, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                            Tutorial_PlayerSlot_Nerd = pos;
                        }
                        if (PlayerEntity.GetType(spr) == PlayerType.Mailman)
                        {
                            r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserMailman, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                            Tutorial_PlayerSlot_Mailman = pos;
                        }
                        if (PlayerEntity.GetType(spr) == PlayerType.Ninja)
                        {
                            r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserNinja, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                            Tutorial_PlayerSlot_Ninja = pos;
                        }
                        if (PlayerEntity.GetType(spr) == PlayerType.Professor)
                        {
                            r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserProfessor, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                            Tutorial_PlayerSlot_Professor = pos;
                        }
                    }
                }

                // Make the last one controllable by keyboard
            }

            // iterate trough every enemy and give them the room they are in
            foreach(var enemy in Manager_Enemies.GetEnemies())
            {
                enemy.Room = GetRoom(enemy, null);
            }

            _startRoom.SetVisible(true);
            Manager_Sound.PlayFreeRoamMusic();

            if (Settings.DynamicShades)
            {
                foreach (var c in connectors)
                {
                    Manager_Light2.Illuminate(c);
                }
                Manager_Light2.Illuminate(_startRoom);
            }
            else
            {
                Manager_Light2.IlluminateSync(Rooms.Values.Where(c => c.WhatAreYou() == X_LevelElements.Room).ToList());
            }

            //string model = "";
            //int globalOffset = 0;
            //foreach (var room in Rooms)
            //{
            //    Manager_Light2.CreateModel(room.Value, true, ref model, ref globalOffset);
            //}
            //File.WriteAllText("./logs/model.obj", model);
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
                    break;
                }
            }

            return currentRoom;
        }

        public void SuppliedRoomFunctions()
        {
            if (Input.IsKeyTriggered(Keybinds.OpenAllDoors))
            {
                foreach (var room in Rooms)
                {
                    if (room.Value.WhatAreYou() == X_LevelElements.Room)
                    {
                        ((Y_CMRoom)room.Value).OpenAllUnlockedRoomDoors();
                    }
                }
            }
            else if (Input.IsKeyTriggered(Keybinds.CloseAllDoors))
            {
                foreach (var room in Rooms)
                {
                    if (room.Value.WhatAreYou() == X_LevelElements.Room)
                    {
                        ((Y_CMRoom)room.Value).CloseAllUnlockedRoomDoors();
                    }
                }
            }
            else if (Input.IsKeyTriggered(Keybinds.LockAllDoors))
            {
                foreach (var room in Rooms)
                {
                    if (room.Value.WhatAreYou() == X_LevelElements.Room)
                    {
                        ((Y_CMRoom)room.Value).LockAllDoors();
                    }
                }
            }
            else if (Input.IsKeyTriggered(Keybinds.UnlockAllDoors))
            {
                foreach (var room in Rooms)
                {
                    if (room.Value.WhatAreYou() == X_LevelElements.Room)
                    {
                        ((Y_CMRoom)room.Value).LockAllDoors(unlock: true);
                    }
                }
            }
        }

        private void TutorialNext()
        {
            switch (TutorialState)
            {
                case GameTutorialState.Welcome:
                    TutorialState = GameTutorialState.IntroduceAllCharacters;
                    break;
                case GameTutorialState.IntroduceAllCharacters:
                    TutorialState = GameTutorialState.IntroduceCharactersNinja;
                    break;
                case GameTutorialState.IntroduceCharactersNinja:
                    TutorialState = GameTutorialState.IntroduceCharactersNerd;
                    break;
                case GameTutorialState.IntroduceCharactersNerd:
                    TutorialState = GameTutorialState.IntroduceCharactersMailman;
                    break;
                case GameTutorialState.IntroduceCharactersMailman:
                    TutorialState = GameTutorialState.IntroduceCharactersProfessor;
                    break;
                case GameTutorialState.IntroduceCharactersProfessor:
                    TutorialState = GameTutorialState.IntroduceGhosts;
                    break;
                case GameTutorialState.IntroduceGhosts:
                    TutorialState = GameTutorialState.IntroducePowerUps;
                    break;
                case GameTutorialState.IntroducePowerUps:
                    TutorialState = GameTutorialState.IntroduceYggdrasil;
                    break;
                case GameTutorialState.IntroduceYggdrasil:
                    TutorialState = GameTutorialState.IntroduceBoss;
                    break;
                case GameTutorialState.IntroduceBoss:
                    TutorialState = GameTutorialState.IntroduceSampleRoomWSpikySlime;
                    break;
                case GameTutorialState.IntroduceSampleRoomWSpikySlime:
                    TutorialState = GameTutorialState.IntroduceSpikySlime;
                    break;
                case GameTutorialState.IntroduceSpikySlime:
                    TutorialState = GameTutorialState.IntroduceStartButton;
                    break;
                case GameTutorialState.IntroduceStartButton:
                    TutorialState = GameTutorialState.EndTutorial;
                    break;
                default:
                    TutorialState = GameTutorialState.Welcome;
                    break;
            }
        }

        public void ShowTutorialControls(int duration)
        {
            Notifications.New("\nPress any button to continue through the tutorial, [Start] / [Esc] to skip and start playing\n", Color.Wheat, duration, Fonts.Small);
        }

        public void UpdateTutorial(GameTime gameTime)
        {
            // find some random fat spiky slimy slime room to display
            if (TutorialState == GameTutorialState.Welcome)
            {
                var spiky = Manager_Enemies.GetEnemies().Where(x => x is Enemy_Slime_Spiky).OrderBy(x => Util.random.Next()).First();
                Tutorial_SpikyRoom = GetRoom(spiky, null);
                Tutorial_Spiky = (Enemy_Slime_Spiky)spiky;
            }

            float zoomCharacter = 3.0f;
            float zoomPowerUps = 1.6f;
            float zoomBigBoss = 1.0f;
            Color colorGoldRoom = Color.Wheat;
            Color colorLightRoom = Color.Wheat;
            Color colorLeafRoom = Color.Wheat;

            switch (TutorialState)
            {
                case GameTutorialState.Welcome:
                    /* Welcome the dear mortals */
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Welcome, dear Mortals, ", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("You have accepted the challenge of the Gods", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("and are now located at the root of the magical tree Yggdrasil.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceAllCharacters:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(_startRoom.Rect.Center.ToVector2() + new Vector2(0, Rooms[0].Rect.Height * 0.25f), 1.0f);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("You can choose between 4 distinct Characters.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("All Characters have one Gun and an Ability and can Dodge.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Use Gun with [" + Keybinds.GamePadShoot.ToString() + "]", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Use Ability with [" + Keybinds.GamePadAbility.ToString() + "]", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Use Dodge with [" + Keybinds.GamePadAction.ToString() + "]", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Aim with [RightThumbStick]", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersNinja:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Ninja, zoomCharacter);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is Ninja,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("he has been here for a long time", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("which is why he is still here!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersNerd:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Nerd, zoomCharacter);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is the Nerd,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("if she hits a confused target she does much more damage.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersMailman:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Mailman, zoomCharacter);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is the Mailman,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("he can activate a shield and protect himself and others", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("behind him for a while.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersProfessor:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Professor, zoomCharacter);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is the Professor,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("he can confuse enemies for a while.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceGhosts:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_GhostSlot, zoomCharacter);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Initially, every potential player is assigned a Ghost.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Move your Ghost into a Character", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to register and select that Character to play!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroducePowerUps:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        var pups = _startRoom.PickUps.Where(x => x.Type == Y_PowerUps.Life || x.Type == Y_PowerUps.Revive).ToList();
                        var allPos = pups.Select(x => x.Rect.Center.ToVector2()).ToArray();
                        var avgPos = new Vector2(allPos.Select(x => x.X).Average(), allPos.Select(x => x.Y).Average());

                        Camera.SetFocusManual(avgPos, zoomPowerUps);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("If you are low on life, you can try to find a Heart.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Only alive players can pick up Hearts!", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("If you die, you turn back into a Ghost.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Ghosts can pick up blue Revives to get another chance!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceYggdrasil:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        var pups = _startRoom.PickUps.Where(x => x.Type == Y_PowerUps.Life || x.Type == Y_PowerUps.Revive).ToList();
                        var allPos = pups.Select(x => x.Rect.Center.ToVector2()).ToArray();
                        var avgPos = new Vector2(allPos.Select(x => x.X).Average(), allPos.Select(x => x.Y).Average());

                        Camera.SetFocusMenu(A_Yggdrasil.Background_.Rect, animationDuration: 3000);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Your Mission:", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Get to the top of Yggdrasil!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceBoss:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        ((Y_CMRoom)_goldRoom).SetVisible(true);
                        Camera.SetFocusManual(_goldRoom.Rect.Center.ToVector2(), zoomBigBoss, animate: false);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n", colorGoldRoom, duration);
                        Notifications.New("There, you will find the Final Boss.", colorGoldRoom, duration, Fonts.Large);
                        Notifications.New("Defeat it to prove yourself worthy", colorGoldRoom, duration, Fonts.Large);
                        Notifications.New("to help the Gods when Ragnarok arrives!", colorGoldRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceSampleRoomWSpikySlime:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        ((Y_CMRoom)Tutorial_SpikyRoom).SetVisible(true);

                        foreach (var enemy in ((Y_CMRoom)Tutorial_SpikyRoom).GetEnemiesInside())
                            enemy.WakeUp();

                        Camera.SetFocusRoom(Tutorial_SpikyRoom, animate: false);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLeafRoom, duration);
                        Notifications.New("On your way you will fight trough various encounters.", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("An Encounter starts when all alive players are inside the same room!", colorLeafRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceSpikySlime:
                    if(TutorialStageDurationCounterMS > 0)
                    {
                        Camera.Position = Tutorial_Spiky.Rect.Center.ToVector2();
                    }
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_Spiky.Rect.Center.ToVector2(), zoomCharacter);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLeafRoom, duration);
                        Notifications.New("Some enemies contain Level-Ups or better weapons ", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("or may just drop a Heart when they die!", colorLeafRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceStartButton:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusRoom(_startRoom, animate: false);

                        int duration = MaxTutorialStageDurationMS;
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLeafRoom, duration);
                        Notifications.New("When you are ready, all stand on the", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("big button to begin the challenge!", colorLeafRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.EndTutorial:
                    /* Reset things here because we might be sent here directly
                        from the main game if tutorial is skipped */
                    Camera.SetFocusRoom(_startRoom, animate: false);
                    /* Reset camera and all the rooms used during the tutorial */
                    ((Y_CMRoom)_goldRoom).SetVisible(false);
                    ((Y_CMRoom)Tutorial_SpikyRoom).SetVisible(false);
                    ((Y_CMRoom)Tutorial_SpikyRoom).InGameReset();
                    Notifications.Clear();
                    /* Start the real game loop */
                    State = GamePlayState.Start;

                    // remove unwanted references
                    Tutorial_SpikyRoom = null;
                    Tutorial_Spiky = null;

                    // make absolutely sure that every stupid enemy has it's room set
                    foreach (var enemy in Manager_Enemies.GetEnemies())
                    {
                        enemy.Room = GetRoom(enemy, null);
                    }

                    break;
                default:
                    break;
            }

            TutorialStageDurationCounterMS += gameTime.ElapsedGameTime.Milliseconds;
            bool anything = Input.AnythingPressed() && TutorialButtonPressCoolDownCounter == 0;
            if (TutorialButtonPressCoolDownCounter > TutorialButtonPressCoolDownFC)
            {
                TutorialButtonPressCoolDownCounter = 0;
            }
            else if (TutorialButtonPressCoolDownCounter > 0)
            {
                TutorialButtonPressCoolDownCounter++;
            }

            if (TutorialStageDurationCounterMS > MaxTutorialStageDurationMS || anything)
            {
                if (anything)
                {
                    TutorialButtonPressCoolDownCounter++;
                }
                TutorialStageDurationCounterMS = 0;
                TutorialNext();
                Notifications.Clear();
            }
        }

        public void Update(GameTime gameTime)
        {
            // Skeakily insert the tutorial
            if (State == GamePlayState.Tutorial)
            {
                UpdateTutorial(gameTime);
                return;
            }

            foreach (var room in Rooms.Values)
            {
                // no need to update rooms not in visual range either
                if (Rectangle.Intersect(Camera.VisibleArea, room.Rect) == Rectangle.Empty) continue;
                room.Update(gameTime);
            }

            switch (State)
            {
                case GamePlayState.Start:
                    if (_startRoom.StartingPad().InteractionComplete)
                    {
                        State = GamePlayState.FreeRoam;
                        _startRoom.OpenAllUnlockedRoomDoors();
                        Camera.SetFocusPlayers();

                        /**
                         * Spawn the spiky slime dudes level ups and weapons
                         */
                        // spawn the powerups inside the spiky dudes
                        // we made sure that there are at least 8 of them
                        var arr = Manager_Enemies.GetEnemies().Where(x => x is Enemy_Slime_Spiky).ToArray();

                        // give each player two power ups somewhere inside a big fat spiky slime
                        var pws = new List<Y_PowerUps>();
                        foreach (var p in Manager_Players.Players)
                        {
                            if (p is Player_Mailman) pws.AddRange(new List<Y_PowerUps> { Y_PowerUps.LevelUpMailman, Y_PowerUps.LevelUpMailman });
                            else if (p is Player_NerdyGirl) pws.AddRange(new List<Y_PowerUps> { Y_PowerUps.LevelUpNerd, Y_PowerUps.LevelUpNerd });
                            else if (p is Player_Ninja) pws.AddRange(new List<Y_PowerUps> { Y_PowerUps.LevelUpNinja, Y_PowerUps.LevelUpNinja });
                            else if (p is Player_Professor) pws.AddRange(new List<Y_PowerUps> { Y_PowerUps.LevelUpProfessor, Y_PowerUps.LevelUpProfessor });
                        }

                        // make sure these are somewhere to be found, because, let's face it, it's the pinky hammer :-D
                        pws.Add(Y_PowerUps.WeaponPinkHammer);
                        pws.Add(Y_PowerUps.WeaponHelix);

                        // fill up the rest of the slots with some random stuff
                        // var pwsWeapons = new List<Y_PowerUps> {
                        //     Y_PowerUps.WeaponHelix
                        //     //Y_PowerUps.WeaponWide,
                        //     //Y_PowerUps.WeaponGiga
                        // };
                        // while (pws.Count() < arr.Length)
                        // {
                        //     pws.Add(pwsWeapons[Util.random.Next(0, pwsWeapons.Count())]);
                        // }

                        // mix everything thouroughly
                        var randpws = pws.OrderBy(x => Util.random.Next()).ToArray();
                        var randarr = arr.OrderBy(x => Util.random.Next()).ToArray();
                        for (int i = 0; i < randarr.Length && i < randpws.Length; ++i)
                        {
                            ((Enemy_Slime_Spiky)randarr[i]).SetPowerUp(randpws[i]);
                        }
                    }
                    break;

                case GamePlayState.FreeRoam:
                    // Just sample what room for any player right now. For an encounter
                    // to start, will check anyway if everyone is inside.
                    var alivePlayers = Manager_Players.Players.Where(x => x.IsAlive()).ToArray();
                    ActiveRoom = alivePlayers.First().Room;

                    if (ActiveRoom.WhatAreYou() != X_LevelElements.Room)
                    {
                        break;
                    }
                    var cmroom = (Y_CMRoom)ActiveRoom;

                    if (cmroom == _startRoom || cmroom.Cleared)
                    {
                        break;
                    }

                    // Make sure all players are inside
                    if (cmroom.GetPlayersInside().Count != alivePlayers.Count())
                    {
                        break;
                    }


                    // If an uncleared room does not contain any enemies, just mark as cleared an move on
                    if (cmroom.GetEnemiesInside().Count < 1 && !cmroom.Cleared)
                    {
                        cmroom.Cleared = true;
                        cmroom.OpenAllUnlockedRoomDoors();
                        break;
                    }

                    // At this point we have all players inside a room with
                    // enemies. Time to go in lock down and let the battle begin

                    // transfer all the ghosts to the spawning point of the current room
                    var allGhosts = Manager_Players.Players.Where(x => !x.IsAlive()).ToArray();
                    foreach (var ghost in allGhosts)
                    {
                        ghost.TeleportTo(cmroom.TeleporterTarget);
                    }

                    cmroom.CloseAllUnlockedRoomDoors();
                    cmroom.SetLocked(true);
                    Camera.SetFocusRoom(cmroom);
                    foreach (var enemy in cmroom.GetEnemiesInside())
                    {
                        enemy.WakeUp();
                    }
                    if (cmroom.Category == "Gold")
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

                    // Duration for Win/Lose message to be shown
                    int gameEndNotificationLength = 15000;

                    // Check if players died
                    if (encounterRoom.GetPlayersInside().FindAll(p => p.LifePoints > 0).Count < 1 && encounterRoom.PickUps.FindAll(x => x.Type == Y_PowerUps.Revive).Count < 1)
                    {
                        Notifications.New("\n\n\n\n", Color.Wheat, gameEndNotificationLength);
                        Notifications.New("Fighting to the bitter end, our heroes couldn't prove", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("themselves worthy of fighting alongside the gods...", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Manager_Sound.PlayFreeRoamMusic();
                        State = GamePlayState.End;
                        break;
                    }

                    if (encounterRoom.GetEnemiesInside().Count > 0)
                    {
                        break; // let players fight
                    }

                    //if (_waitTimeBetweenEndOfFightAndLowerDoorsCounter < _waitTimeBetweenEndOfFightAndLowerDoors)
                    //{
                    //    _waitTimeBetweenEndOfFightAndLowerDoorsCounter++;
                    //    break;
                    //}
                    //_waitTimeBetweenEndOfFightAndLowerDoorsCounter = 0;

                    encounterRoom.Cleared = true;
                    encounterRoom.OpenAllUnlockedRoomDoors();

                    if (!encounterRoom.AllDoorsOpen()) break; // wait for all doors to open

                    encounterRoom.SetLocked(false);
                    Camera.SetFocusPlayers();
                    Manager_Sound.PlayFreeRoamMusic();

                    if (encounterRoom.Category == "Gold")
                    {
                        Notifications.New("\n\n\n\n", Color.Wheat, gameEndNotificationLength);
                        Notifications.New("Overcoming the final challenge, glory awaits our heroes", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("when they fight alongside the gods in Ragnarok...", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Manager_Sound.PlayFreeRoamMusic();
                        encounterRoom.OpenAllUnlockedRoomDoors();
                        Camera.SetFocusPlayers();
                        State = GamePlayState.FreeRoam; // no end screen for now
                    }
                    else
                    {
                        State = GamePlayState.FreeRoam;
                        Notifications.New("Room " + ActiveRoom.Name + " cleared!");
                    }
                    break;

                case GamePlayState.Escaped:
                    // Check if players died
                    int escapeNotificationLength = 6000;

                    Notifications.New("\n\n\n\n", Color.Wheat, escapeNotificationLength);
                    Notifications.New("Maybe you can find some more things to help you defeat the Big Boss", Color.Wheat, escapeNotificationLength, Fonts.Large);
                    Manager_Sound.PlayFreeRoamMusic();
                    State = GamePlayState.FreeRoam;
                    break;
                case GamePlayState.End:
                    // Nothing yet
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
                // Culling
                if (Rectangle.Intersect(Camera.VisibleArea, room.Value.Rect) == Rectangle.Empty) continue;
                room.Value.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Level;
        }
    }
}
