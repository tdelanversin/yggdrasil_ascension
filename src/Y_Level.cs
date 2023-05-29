using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Timers;
using Newtonsoft.Json;
using SharpFont.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Xml;

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
            EndScreen,
            GameOver,
            GigaChad_Prequel, // Give players a compliment
            GigaChad_MoveCamera, // Killed everything? Maybe get the hammer
            GigaChad_IntroduceGigaChad,
            GigaChad_IntroduceHammer,
            GigaChad_Power,
            GigaChad_MoveCameraBack,
            GigaChad_StartEncounter,
            GigaChad_Encounter,
            GigaChad_EncounterComplete // Killed GigaChat? Get some immediate practice
        }

        public enum GameTutorialState
        {
            Warning,
            Welcome,
            IntroduceAllCharacters,
            IntroduceControls,
            IntroduceMoving,
            IntroduceAiming,
            IntroduceShooting,
            IntroduceChangeGun,
            IntroduceDodging,
            IntroduceAbilities,
            IntroduceCharactersNinja,
            IntroduceCharactersNerd,
            IntroduceCharactersMailman,
            IntroduceCharactersMailman2,
            IntroduceCharactersProfessor,
            IntroduceGhosts,
            IntroduceGhosts2,
            IntroduceGhosts3,
            IntroducePowerUps,
            IntroduceYggdrasil,
            IntroduceBoss,
            IntroduceSampleRoomWSpikySlime,
            IntroduceSpikySlime,
            IntroduceStartButton,
            EndTutorial
        }

        public enum GameEndState
        {
            Lost,
            Won,
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

        // private int _waitTimeBetweenEndOfFightAndLowerDoors = 125;
        // private int _waitTimeBetweenEndOfFightAndLowerDoorsCounter = 0;

        public static int TextureTileSize { get; set; }
        public static int InGameTileSize { get; set; }
        public static float GlobalScale { get; set; }

        // Gameplay state objects
        public static IWalkable ActiveRoom;

        public static GamePlayState State;
        public static GameTutorialState TutorialState;
        public static GameEndState EndState;

        private int MoveShitAround_CounterS = 0;
        private int MoveShitAround_CounterMS = 0;
        List<Vector2> MoveShitAround_Positions = new List<Vector2>();
        Vector2 MoveShitAround_ZoomPoint;

        private bool GigaChad_CameraSwitch = true;
        private int GigaChadTimerMS = 0;
        private int GigaChadTimerS = 0;
        private int GigaChadTimerS_CountTo = 0;
        private int GigaChadTimer_CameraTransitS = 7;
        private int GigaChadTimer_CameraTransitToHammer = 2;
        private int GigaChadTimer_ShowTimeS = 30;
        private int GigaChadTimer_WaitToSkip = 2;

        private int MaxTutorialStageDurationMS = 1000;
        private int MaxTutorialStageDurationS = 30;
        private int TutorialStageDurationCounterMS = 0;
        private int TutorialStageDurationCounterS = 0;
        private int TutorialStageDurationCounterS_InitialSkipAfter = 10;
        private int TutorialButtonPressCoolDownFC = 120;
        private int TutorialButtonPressCoolDownCounter = 0;

        Vector2 Tutorial_PlayerSlot_Nerd;
        Vector2 Tutorial_PlayerSlot_Ninja;
        Vector2 Tutorial_PlayerSlot_Professor;
        Vector2 Tutorial_PlayerSlot_Mailman;
        Vector2 Tutorial_GhostSlot;
        Rectangle ControlsRect;
        IWalkable Tutorial_SpikyRoom;
        Vector2 Tutorial_ChangeGun;
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

            TutorialState = GameTutorialState.EndTutorial;

            Rooms = new Dictionary<int, IWalkable>();

            //Rooms.Add(0, _availableRooms["Start"].First());

            var random = new Random();
            // randomly select one level tree
            //var key = _data.Level.Keys.ToArray()[random.Next(0, _data.Level.Keys.Count)];
            var key = _data.Level.Keys.ToArray()[0];
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
                    ControlsRect = new Rectangle(
                        _startRoom.Rect.X + 16 * InGameTileSize,
                        _startRoom.Rect.Y + 16 * InGameTileSize + 10, // CSS style fixes by hand
                        12 * InGameTileSize,
                        7 * InGameTileSize
                    );
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

            // Select 8 rooms out of all rooms minus the Gold, Bonus and the Start room
            // and mark them for containing a spiky semi boss slime
            var spikeRooms = Rooms.Values
                .Where(x => x.WhatAreYou() == X_LevelElements.Room &&
                          !(x.Category == "Start") &&
                          !(x.Category == "Gold") && 
                          !(x.Category == "Bonus"))
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

                r.SetPowerUps();
                r.SpawnEnemies();

                var players = r.GetPlayerSpawningPoints();
                int playerIndex = 0;
                foreach (var spr in players)
                {
                    Vector2 pos = new Vector2(spr.x, spr.y);
                    //if(PlayerEntity.GetPointType(spr) == PlayerSpawningPointType.Chooser)
                    //if (playerIndex == 4) break;
                    if (PlayerEntity.GetType(spr) == PlayerType.Nerd)
                        Manager_Players.AddPlayer(PlayerType.Nerd, (PlayerIndex)playerIndex, position: pos, this);
                    if (PlayerEntity.GetType(spr) == PlayerType.Ninja)
                        Manager_Players.AddPlayer(PlayerType.Ninja, (PlayerIndex)playerIndex, position: pos, this);
                    if (PlayerEntity.GetType(spr) == PlayerType.Professor)
                        Manager_Players.AddPlayer(PlayerType.Professor, (PlayerIndex)playerIndex, position: pos, this);
                    if (PlayerEntity.GetType(spr) == PlayerType.Mailman)
                        Manager_Players.AddPlayer(PlayerType.Mailman, (PlayerIndex)playerIndex, position: pos, this);
                    if (PlayerEntity.GetType(spr) == PlayerType.Ghost)
                        MoveShitAround_Positions.Add(pos);
                    playerIndex++;
                    //else if (PlayerEntity.GetPointType(spr) == PlayerSpawningPointType.Chooser)
                    //{
                    //    if (PlayerEntity.GetType(spr) == PlayerType.Nerd)
                    //    {
                    //        r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserNerd, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                    //        Tutorial_PlayerSlot_Nerd = pos;
                    //    }
                    //    if (PlayerEntity.GetType(spr) == PlayerType.Mailman)
                    //    {
                    //        r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserMailman, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                    //        Tutorial_PlayerSlot_Mailman = pos;
                    //    }
                    //    if (PlayerEntity.GetType(spr) == PlayerType.Ninja)
                    //    {
                    //        r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserNinja, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                    //        Tutorial_PlayerSlot_Ninja = pos;
                    //    }
                    //    if (PlayerEntity.GetType(spr) == PlayerType.Professor)
                    //    {
                    //        r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserProfessor, pos.ToPoint(), spr.width, spr.height, GlobalScale));
                    //        Tutorial_PlayerSlot_Professor = pos;
                    //    }
                    //}
                }

                // Make the last one controllable by keyboard
            }

            // iterate trough every enemy and give them the room they are in
            foreach (var enemy in Manager_Enemies.GetEnemies())
            {
                enemy.Room = GetRoom(enemy, null);
            }

            _startRoom.SetVisible(true);
            Manager_Sound.PlayFreeRoamMusic();

            MoveShitAround_ZoomPoint = _startRoom.TeleporterTarget.ToVector2() + new Vector2(Y_Level.InGameTileSize/2.0f, Y_Level.InGameTileSize / 2.0f);

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

            GigaChadTimerS = 0;
            GigaChadTimerMS = 0;

            // this one is used to create a 3D model of the level to debug the whole thing
            //string model = "";
            //int globalOffset = 0;
            //foreach (var room in Rooms)
            //{
            //    Manager_Light2.CreateModel(room.Value, true, ref model, ref globalOffset);
            //}
            //File.WriteAllText("./logs/model.obj", model);

            State = GamePlayState.FreeRoam;
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
                case GameTutorialState.Warning:
                    TutorialState = GameTutorialState.IntroduceAllCharacters;
                    break;
                case GameTutorialState.Welcome:
                    TutorialState = GameTutorialState.IntroduceAllCharacters;
                    break;
                case GameTutorialState.IntroduceAllCharacters:
                    TutorialState = GameTutorialState.IntroduceControls;
                    break;
                case GameTutorialState.IntroduceControls:
                    TutorialState = GameTutorialState.IntroduceCharactersProfessor;
                    break;
                case GameTutorialState.IntroduceCharactersProfessor:
                    TutorialState = GameTutorialState.IntroduceCharactersNinja;
                    break;
                case GameTutorialState.IntroduceCharactersNinja:
                    TutorialState = GameTutorialState.IntroduceCharactersNerd;
                    break;
                case GameTutorialState.IntroduceCharactersNerd:
                    TutorialState = GameTutorialState.IntroduceCharactersMailman;
                    break;
                case GameTutorialState.IntroduceCharactersMailman:
                    TutorialState = GameTutorialState.IntroduceCharactersMailman2;
                    break;
                case GameTutorialState.IntroduceCharactersMailman2:
                    TutorialState = GameTutorialState.IntroduceGhosts;
                    break;
                case GameTutorialState.IntroduceGhosts:
                    TutorialState = GameTutorialState.IntroduceGhosts2;
                    break;
                case GameTutorialState.IntroduceGhosts2:
                    TutorialState = GameTutorialState.IntroduceGhosts3;
                    break;
                case GameTutorialState.IntroduceGhosts3:
                    // Skip the power ups state since we no longer have any in the start room
                    //  => sorry, was an unintentional bug ^^
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
            // find some random fat spiky slimy slime room to display (also do this while InGame for the Restart)
            if (TutorialState == GameTutorialState.Warning || TutorialState == GameTutorialState.EndTutorial)
            {
                var spiky = Manager_Enemies.GetEnemies().Where(x => x is Enemy_Slime_Spiky).OrderBy(x => Util.random.Next()).First();
                Tutorial_SpikyRoom = GetRoom(spiky, null);
                Tutorial_Spiky = (Enemy_Slime_Spiky)spiky;

                var gun = _startRoom.PickUps.Where(x => x.Type.ToString().Contains("Weapon")).FirstOrDefault();
                if (gun != null) Tutorial_ChangeGun = gun.Rect.Center.ToVector2();
                else Tutorial_ChangeGun = new Vector2(float.MaxValue, float.MaxValue);
            }

            float zoomCharacter = 3.0f;
            float zoomPowerUps = 1.6f;
            float zoomBigBoss = 1.0f;
            Color colorGoldRoom = Color.Wheat;
            Color colorLightRoom = Color.Wheat;
            Color colorLeafRoom = Color.Wheat;

            if (TutorialStageDurationCounterMS == 0)
            {
                Notifications.Clear();
            }

            int duration = MaxTutorialStageDurationS * MaxTutorialStageDurationMS;
            switch (TutorialState)
            {
                case GameTutorialState.Warning:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Press any button to skip steps of the tutorial,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Press [Start] / [Esc] to quit tutorial!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.Welcome:
                    /* Welcome the dear mortals */
                    if (TutorialStageDurationCounterMS == 0)
                    {
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

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n", colorLightRoom, duration);
                        Notifications.New("You can choose between 4 distinct Characters.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("All Characters have one Gun and an Ability and can Dodge.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceControls:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(ControlsRect.Center.ToVector2(), 2.0f);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n", colorLightRoom, duration);
                        Notifications.New("The controls are always shown here in the starting room.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceMoving:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(_startRoom.Rect.Center.ToVector2() + new Vector2(0, Rooms[0].Rect.Height * 0.25f), 1.0f);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Move the LEFT Joystick to move your character", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceAiming:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(_startRoom.Rect.Center.ToVector2() + new Vector2(0, Rooms[0].Rect.Height * 0.25f), 1.0f);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Move the RIGHT Joystick to aim your gun", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceShooting:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(_startRoom.Rect.Center.ToVector2() + new Vector2(0, Rooms[0].Rect.Height * 0.25f), 1.0f);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Press the RIGHT Trigger button to shoot", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("(Bottom, right button on the controller's back)", colorLightRoom, duration, Fonts.Small);
                    }
                    break;
                case GameTutorialState.IntroduceChangeGun:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        if (Tutorial_ChangeGun == new Vector2(float.MaxValue, float.MaxValue))
                        {
                            TutorialStageDurationCounterMS = MaxTutorialStageDurationMS;
                            TutorialStageDurationCounterS = MaxTutorialStageDurationS;
                        }
                        else
                        {
                            Camera.SetFocusManual(Tutorial_ChangeGun, zoomPowerUps);

                            ShowTutorialControls(duration);
                            Notifications.New("\n\n\n\n", colorLightRoom, duration);
                            Notifications.New("You can change your gun by walking through pick ups", colorLightRoom, duration, Fonts.Large);
                        }
                    }
                    break;
                case GameTutorialState.IntroduceDodging:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(_startRoom.Rect.Center.ToVector2() + new Vector2(0, Rooms[0].Rect.Height * 0.25f), 1.0f);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Press the LEFT Trigger button to dodge", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("(Bottom, left button on the controller's back)", colorLightRoom, duration, Fonts.Small);
                    }
                    break;
                case GameTutorialState.IntroduceAbilities:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(_startRoom.Rect.Center.ToVector2() + new Vector2(0, Rooms[0].Rect.Height * 0.25f), 1.0f);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Press the RIGHT Shoulder button to use an ability", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("(Top, right button on the controller's back)", colorLightRoom, duration, Fonts.Small);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersProfessor:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Professor, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is the Professor,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("he can use his special ability.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to confuse enemies.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersNinja:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Ninja, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is Ninja,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("If Ninja shoots a confused enemy, he does more damage!", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("His special ability allows him", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to become invincible for a short time.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersNerd:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Nerd, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is the Nerd.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("If she hits a confused target she does much more damage!", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Her special ability allows her", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to double her firing rate.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersMailman:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Mailman, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("This is the Mailman,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("He can use his special ability to activate a", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("protective shield.", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceCharactersMailman2:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Mailman, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("His shield discharges over time and needs 2x,", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("the discharging time to recharge!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceGhosts:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_GhostSlot, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Initially, every potential player is assigned a Ghost.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Move your Ghost into a Character", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to join the game and select that Character to play!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceGhosts2:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_GhostSlot, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("When you die, you can use your special ability", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to temporarily protect your friends from bullets!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceGhosts3:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_PlayerSlot_Professor, zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("Move your Ghost into a Character", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("to join the game and select that Character to play!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroducePowerUps:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        // only Y_PowerUps.Life and Y_PowerUps.Revive in the start room
                        var pups = _startRoom.PickUps.Where(x => x.Type == Y_PowerUps.Life || x.Type == Y_PowerUps.Revive).ToList();
                        var allPos = pups.Select(x => x.Rect.Center.ToVector2()).ToArray();
                        var avgPos = new Vector2(allPos.Select(x => x.X).Average(), allPos.Select(x => x.Y).Average());

                        Camera.SetFocusManual(avgPos, zoomPowerUps);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLightRoom, duration);
                        Notifications.New("If you are low on life, you can try to find a Heart.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Only alive players can pick up Hearts!", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("If you die, you turn back into a Ghost.", colorLightRoom, duration, Fonts.Large);
                        Notifications.New("Ghosts can pick up blue Revives to get another chance!", colorLightRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceYggdrasil:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        if (TutorialStageDurationCounterS == 0)
                            Camera.SetFocusMenu(animationDuration: 3000);

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

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLeafRoom, duration);
                        Notifications.New("On your way you will fight trough various encounters.", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("An Encounter starts when all alive players", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("move inside the same room containing enemies!", colorLeafRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceSpikySlime:
                    if (TutorialStageDurationCounterMS > 0)
                    {
                        Camera.Position = Tutorial_Spiky.Rect.Center.ToVector2();
                    }
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusManual(Tutorial_Spiky.Rect.Center.ToVector2(), zoomCharacter);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n\n\n\n\n\n\n\n\n\n", colorLeafRoom, duration);
                        Notifications.New("Some enemies contain Level-Ups or better weapons!", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("Others may just drop a Heart when they die!", colorLeafRoom, duration, Fonts.Large);
                    }
                    break;
                case GameTutorialState.IntroduceStartButton:
                    if (TutorialStageDurationCounterMS == 0)
                    {
                        Camera.SetFocusRoom(_startRoom, animate: false);

                        ShowTutorialControls(duration);
                        Notifications.New("\n\n\n\n", colorLeafRoom, duration);
                        Notifications.New("When you are ready, all STAND on the", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("big START button to begin the challenge!", colorLeafRoom, duration, Fonts.Large);
                        Notifications.New("GHOSTS that didn't join will vanish!", colorLeafRoom, duration, Fonts.Large);
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

            if (TutorialStageDurationCounterMS == 0)
            {
                // in the first stage: add comment non-skippable
                if (TutorialState == GameTutorialState.Warning && TutorialStageDurationCounterS < TutorialStageDurationCounterS_InitialSkipAfter)
                {
                    Notifications.New("[Continue in: " + (TutorialStageDurationCounterS_InitialSkipAfter - TutorialStageDurationCounterS) + "]", colorLightRoom, duration, Fonts.Medium);
                }
                else
                {
                    Notifications.New("[Press to continue - Remaining: " + (MaxTutorialStageDurationS - TutorialStageDurationCounterS) + "]", colorLightRoom, duration, Fonts.Medium);
                }
            }

            TutorialStageDurationCounterMS += gameTime.ElapsedGameTime.Milliseconds;
            bool anything = Input.AnythingPressed() &&
                            TutorialButtonPressCoolDownCounter == 0 &&
                            !(TutorialState == GameTutorialState.Warning && TutorialStageDurationCounterS < TutorialStageDurationCounterS_InitialSkipAfter);
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
                    TutorialStageDurationCounterS = MaxTutorialStageDurationS;
                }

                TutorialStageDurationCounterMS = 0;
                TutorialStageDurationCounterS++;
                if (TutorialStageDurationCounterS > MaxTutorialStageDurationS)
                {
                    TutorialStageDurationCounterS = 0;
                    TutorialNext();
                }
            }
        }

        public void UpdateMoveShitAround(GameTime gameTime)
        {
            if(MoveShitAround_CounterS == 0)
            {
                Camera.SetFocusRoom(Manager_Players.Players[0].Room);
            }
            if(MoveShitAround_CounterS == 2)
            {
                Camera.SetFocusPlayers(animationDuration: 2000);
            }
            else if(MoveShitAround_CounterS >= 4)
            {
                for(int i=0; i<4; ++i)
                {
                    Vector2 pos = Manager_Players.Players[i].Rect.Location.ToVector2();
                    Vector2 target = MoveShitAround_Positions[i];
                    ((Player_Basic)Manager_Players.Players[i]).UpdateVelocity(target - pos, gameTime);
                    ((Player_Basic)Manager_Players.Players[i]).UpdateCollision(gameTime);
                }
                if (MoveShitAround_CounterS == 5)
                {
                    Camera.SetFocusManual(MoveShitAround_ZoomPoint, 2.0f, animationDuration: 2000);
                }
            }

            if(MoveShitAround_CounterMS >= 1000)
            {
                MoveShitAround_CounterMS = 0;
                MoveShitAround_CounterS++;
            }
            else
            {
                MoveShitAround_CounterMS += gameTime.ElapsedGameTime.Milliseconds;
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
                //if (Rectangle.Intersect(Camera.VisibleArea, room.Rect) == Rectangle.Empty) continue;
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
                            if (p is Player_Mailman) pws.Add(Y_PowerUps.LevelUpMailman);
                            else if (p is Player_NerdyGirl) pws.Add(Y_PowerUps.LevelUpNerd);
                            else if (p is Player_Ninja) pws.Add(Y_PowerUps.LevelUpNinja);
                            else if (p is Player_Professor) pws.Add(Y_PowerUps.LevelUpProfessor);
                        }

                        if (Manager_Players.Players.Count <= 2)
                        {
                            foreach (var p in Manager_Players.Players)
                            {
                                if (p is Player_Mailman) pws.Add(Y_PowerUps.LevelUpMailman);
                                else if (p is Player_NerdyGirl) pws.Add(Y_PowerUps.LevelUpNerd);
                                else if (p is Player_Ninja) pws.Add(Y_PowerUps.LevelUpNinja);
                                else if (p is Player_Professor) pws.Add(Y_PowerUps.LevelUpProfessor);
                            }
                        }

                        // make sure these are somewhere to be found, because, let's face it, it's the pinky hammer :-D
                        pws.Add(Y_PowerUps.WeaponSniper);
                        pws.Add(Y_PowerUps.WeaponHelix);
                        pws.Add(Y_PowerUps.WeaponBlunderbuss);
                        pws.Add(Y_PowerUps.WeaponRedDevil);

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
                    
                    UpdateMoveShitAround(gameTime);

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
                    if (cmroom.GetPlayersInside().Where(p => p.WhatAreYou() != X_LevelElements.Ghost).ToList().Count != alivePlayers.Count())
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

                    if (cmroom.Category == "Gold" || cmroom.Category == "Bonus")
                    {
                        Manager_Sound.PlayBossMusic();
                    }
                    else
                    {
                        Manager_Sound.PlayEncounterMusic();
                    }

                    Notifications.New("Starting encouter");

                    if (cmroom.Category == "Bonus")
                    {
                        // if we entered GigaChad's room and GigaChad is still there...
                        State = GamePlayState.GigaChad_StartEncounter;
                    }
                    else
                    {
                        State = GamePlayState.Encounter;
                    }
                    break;

                case GamePlayState.Encounter:
                    // We can assume at this point that _currentRoom is actually
                    // a room, otherwise we wouldn't be here
                    var encounterRoom = (Y_CMRoom)ActiveRoom;

                    // Duration for Win/Lose message to be shown
                    int gameEndNotificationLength = 35000;

                    // Check if players died (in a more robust, room independent way)
                    if (Manager_Players.Players.FindAll(p => p.LifePoints > 0).Count < 1 && encounterRoom.PickUps.FindAll(x => x.Type == Y_PowerUps.Revive).Count < 1)
                    {
                        // kill all remaining enemies to make sure they don't keep shooting during the nice outro
                        // will have to restart the game anyways, so no problems there...
                        Manager_Enemies.ClearEnemies();

                        Notifications.New("\n\n\n\n", Color.Wheat, gameEndNotificationLength);
                        Notifications.New("Humans. You have tried to prove yourself to be worthy in a fight.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("But alas, there was a greater evil that slayed you.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("\nWill you train, every day, to earn that honor?", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("Will you commit to fighting and failing, until victory is the only outcome?", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("\nIf so, we will eagerly watch your every try, and one day,", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("invite you to fight with us, at the great battle of Ragnarok.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Manager_Sound.PlaySongEndingLose();
                        Camera.SetFocusMenu(animationDuration: gameEndNotificationLength / 5);
                        EndState = GameEndState.Lost;
                        State = GamePlayState.EndScreen;
                        break;
                    }

                    if (encounterRoom.GetEnemiesInside().Count > 0)
                    {
                        break; // let players fight
                    }

                    encounterRoom.Cleared = true;
                    encounterRoom.OpenAllUnlockedRoomDoors();

                    if (!encounterRoom.AllDoorsOpen()) break; // wait for all doors to open

                    encounterRoom.SetLocked(false);
                    var count = Manager_Enemies.CountRegularEnemies();
                    // check if we have regular enemies left or gigachad is already dead
                    // (we don't want the tiny camera move from room to players if we want to move to gigachad in this instance)
                    if(count > 0 || Manager_Enemies.GetGigaChads().Count() == 0)
                    {
                        Camera.SetFocusPlayers();
                    }
                    Manager_Sound.PlayFreeRoamMusic();

                    if (encounterRoom.Category == "Gold")
                    {
                        Notifications.New("\n\n\n\n", Color.Wheat, gameEndNotificationLength);
                        Notifications.New("Our heroes.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("\nYou have managed to climb Yggdrasil and prove yourselves.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("We applaud you for that.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("\nNow, it will be our honor, to have you fight along side of us", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("in the upcoming battle of Ragnarok.", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Notifications.New("\nAre you ready?", Color.Wheat, gameEndNotificationLength, Fonts.Large);
                        Manager_Sound.PlaySongEndingWin();
                        encounterRoom.OpenAllUnlockedRoomDoors();
                        Camera.SetFocusPlayers(); // First to player focus for the camera to remember
                        Camera.SetFocusMenu(animationDuration: gameEndNotificationLength / 5);
                        EndState = GameEndState.Won;
                        State = GamePlayState.EndScreen; // no end screen for now
                    }
                    else
                    {
                        Notifications.New("Room " + ActiveRoom.Name + " cleared!");

                        if(count == 0 && Manager_Enemies.GetGigaChads().Count() > 0)
                        {
                            // we killed all regular enemies and cleared the room => show gigachad
                            // imobilize all players...
                            Manager_Players.ImmobilizePlayers(true);
                            updateGigaChadState(gameTime);
                        }
                        else
                        {
                            // otherwise go back to regular free roam
                            State = GamePlayState.FreeRoam;
                        }
                        //var clearedRooms = Rooms.Values.Where(x => x.WhatAreYou() == X_LevelElements.Room && ((Y_CMRoom)x).Cleared);
                    }
                    break;
                case GamePlayState.GigaChad_StartEncounter:
                    // spawn the life and respawn power ups
                    var gigachadRoom2 = (Y_CMRoom)ActiveRoom;
                    gigachadRoom2.SetPowerUps();
                    var allHammers = gigachadRoom2.PickUps.Where(x => x.Type == Y_PowerUps.WeaponPinkHammer).ToList();
                    foreach (var weapoin in allHammers)
                        gigachadRoom2.PickUps.Remove(weapoin);
                    State = GamePlayState.GigaChad_Encounter;
                    break;
                case GamePlayState.GigaChad_Encounter:
                    if(Manager_Enemies.GetGigaChads().Count() == 0)
                    {
                        State = GamePlayState.GigaChad_EncounterComplete;
                    }
                    break;
                case GamePlayState.GigaChad_EncounterComplete:
                    // killed gigachad => spawn hammmers
                    var gigachadRoom = (Y_CMRoom)ActiveRoom;
                    List<PickUp> temp = new List<PickUp>();
                    foreach (var remaining in gigachadRoom.PickUps)
                        temp.Add(remaining);
                    gigachadRoom.SetPowerUps();
                    // remove everything that is not a hammer
                    var nonHammers = gigachadRoom.PickUps.Where(x => x.Type != Y_PowerUps.WeaponPinkHammer).ToList();
                    foreach (var nonHammer in nonHammers)
                        gigachadRoom.PickUps.Remove(nonHammer);
                    // add back stuff that wasn't used during the encounter
                    foreach (var remaining in temp)
                        gigachadRoom.PickUps.Add(remaining);

                    // reset every single, non-cleared room that is not the bonus room ^^
                    // respawn everything not just the regular rooms, the boss one too
                    // in this place of the code, all enemies but the boss are dead
                    // we respawn all enemies back and then remove the spikey slime ones power ups
                    // just for fun, we also respawn the big boss because it just doesn't matter
                    var allNonGigaChatRooms = Rooms.Values.Where(x => 
                                                        x.WhatAreYou() == X_LevelElements.Room &&
                                                        ((Y_CMRoom)x).Category != "Bonus" &&
                                                        ((Y_CMRoom)x).Category != "Start").Cast<Y_CMRoom>().ToList();
                    // kill all enemies and then respawn them, except in the gigachad room
                    Manager_Enemies.ClearEnemies();
                    foreach(var r in allNonGigaChatRooms)
                    {
                        r.InGameReset();
                        // but remove all the power ups inside the spikey fat slimy slimes that were in already cleared rooms
                        var spikies = Manager_Enemies.GetEnemies().Where(x => x.Room == r && x is Enemy_Slime_Spiky).Cast<Enemy_Slime_Spiky>().ToList();
                        foreach (var s in spikies)
                        {
                            s.ClearPowerUp();
                        }
                    }                    

                    // move back to the regular encounter stuff so that we have a synched game
                    State = GamePlayState.Encounter;
                    break;
                case GamePlayState.GigaChad_Prequel:
                    if (GigaChadTimerMS == 0)
                    {
                        Notifications.Clear();
                        Notifications.New("\n\n\n\n\n\n\n\n", Color.Wheat, GigaChadTimer_ShowTimeS * 1000);
                        Notifications.New("Congratulations!", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        Notifications.New("You beat all regular enemies!", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Medium);
                        Notifications.New("Challenge BigBoss!", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Medium);
                        Notifications.New("Or...", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        showGigaChadTimer(GigaChadTimer_ShowTimeS * 1000);
                    }
                    updateGigaChadState(gameTime);
                    break;
                case GamePlayState.GigaChad_MoveCamera: // basically show the players who gigachad is
                    if (GigaChadTimerMS == 0)
                    {
                        Notifications.Clear();
                        if (GigaChad_CameraSwitch)
                        {
                            // get the bonus room
                            var room = getBonusRoom();

                            // open the door to the bonus room and make it visible
                            _startRoom.OpenBottomDoor();

                            // remove all power ups because we want to show them effectfully later
                            room.PickUps.Clear();

                            // initiate the camera movement
                            var gigachad1 = Manager_Enemies.GetGigaChads().FirstOrDefault();
                            if (gigachad1 == null) Logger.Error("WE WANT GIGACHAD -.-!!");

                            // Play some really dramatic tension-y sound
                            Manager_Sound.StopMusic();
                            Manager_Sound.Sound_RisingTension.Play(); // Drama is at second 7, so the animation needs to be slow

                            Camera.SetFocusManual(gigachad1.Rect.Center.ToVector2(), 1.0f, animationDuration: GigaChadTimer_CameraTransitS * 1000);
                            GigaChad_CameraSwitch = false;
                        }
                    }
                    updateGigaChadState(gameTime);
                    break;
                case GamePlayState.GigaChad_IntroduceGigaChad:
                    var gigachad = Manager_Enemies.GetGigaChads().FirstOrDefault();
                    if (gigachad == null) Logger.Error("WE WANT GIGACHAD -.-!!");
                    Camera.SetFocusManual(gigachad.Rect.Center.ToVector2(), 1.0f, animate: false);

                    if (GigaChadTimerMS == 0)
                    {
                        // wake up GigaChad to make him move on camera queue
                        var room = getBonusRoom();
                        var enemies = room.GetEnemiesInside();
                        foreach (var enemie in enemies)
                            enemie.WakeUp();

                        if (Settings.GigaChadBabyMode)
                        {
                            // here we see GigaChad
                            Notifications.Clear();
                            Notifications.New("\n\n\n\n\n\n\n\n", Color.Wheat, GigaChadTimer_ShowTimeS * 1000);
                            Notifications.New("This is GigaChad!", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        }
                        showGigaChadTimer(GigaChadTimer_ShowTimeS * 1000);
                    }
                    updateGigaChadState(gameTime);
                    break;
                case GamePlayState.GigaChad_IntroduceHammer:
                    if (GigaChadTimerMS == 0)
                    {
                        var room = getBonusRoom();
                        
                        // spawn the hammers
                        room.SetPowerUps();

                        // get a random hammer and move the camera to it
                        var hammer = room.PickUps.Where(x => x.Type == Y_PowerUps.WeaponPinkHammer).FirstOrDefault();
                        if (hammer == null) Logger.Error("WE WANT A HAMMER -.-!!");
                        
                        Notifications.Clear();
                        Notifications.New("\n\n\n\n\n\n\n\n", Color.Wheat, GigaChadTimer_ShowTimeS * 1000);
                        Notifications.New("If you kill him", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        Notifications.New("you get Thor's Hammer!", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        Notifications.New("It's very powerful...", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        showGigaChadTimer(GigaChadTimer_ShowTimeS * 1000);

                        if (GigaChad_CameraSwitch)
                        {
                            Camera.SetFocusManual(hammer.Rect.Center.ToVector2(), 1.8f, animationDuration: GigaChadTimer_CameraTransitToHammer * 1000);
                            GigaChad_CameraSwitch = false;
                        }
                    }
                    updateGigaChadState(gameTime);
                    break;
                case GamePlayState.GigaChad_Power:
                    if (GigaChadTimerMS == 0)
                    {
                        var room = getBonusRoom();

                        // spawn the hammers
                        room.SetPowerUps();

                        // get a random hammer and move the camera to it
                        var life = room.PickUps.Where(x => x.Type == Y_PowerUps.Life).FirstOrDefault();
                        if (life == null) Logger.Error("WE ARE AFRAID OF GIGACHAD ^^!!");

                        Notifications.Clear();
                        Notifications.New("\n\n\n\n\n\n\n\n", Color.Wheat, GigaChadTimer_ShowTimeS * 1000);
                        Notifications.New("...though, so is GigaChad...", Color.Wheat, GigaChadTimer_ShowTimeS * 1000, Fonts.Large);
                        showGigaChadTimer(GigaChadTimer_ShowTimeS * 1000);

                        if (GigaChad_CameraSwitch)
                        {
                            Camera.SetFocusManual(life.Rect.Center.ToVector2(), 1.8f, animationDuration: GigaChadTimer_CameraTransitToHammer * 1000);
                            GigaChad_CameraSwitch = false;
                        }
                    }
                    updateGigaChadState(gameTime);
                    break;
                case GamePlayState.GigaChad_MoveCameraBack:
                    // maybe wait for a bit so that players can see the hammer
                    if (GigaChadTimerMS == 0)
                    {
                        // get the bonus room
                        var room = getBonusRoom();
                        // we are done with the sequence:
                        // reset the room then remove the hammers again because the players have to kill GigaChad first
                        // !! don't respawn anything here, this is just the show-off sequence !!
                        room.InGameReset();
                        room.PickUps.Clear();

                        if (GigaChad_CameraSwitch)
                        {
                            // move back to the players
                            Camera.SetFocusPlayers();
                            GigaChad_CameraSwitch = false;
                            Notifications.Clear();

                            //remobilize players
                            Manager_Players.ImmobilizePlayers(false);
                        }
                    }
                    updateGigaChadState(gameTime);
                    break;
                case GamePlayState.Escaped:
                    // Check if players died
                    int escapeNotificationLength = 6000;

                    Notifications.New("\n\n\n\n", Color.Wheat, escapeNotificationLength);
                    Notifications.New("Maybe you can find some more things to help you defeat the Big Boss", Color.Wheat, escapeNotificationLength, Fonts.Large);
                    Manager_Sound.PlayFreeRoamMusic();
                    State = GamePlayState.FreeRoam;
                    break;
                case GamePlayState.EndScreen:
                    // Wait for the ending screen and notifications to fade out
                    if (Notifications.GetNotifications().Count < 1)
                    {
                        // If the players won, then they can continue playing
                        if (EndState == GameEndState.Won)
                        {
                            State = GamePlayState.FreeRoam;
                            Menu.GameWon();
                            Manager_Sound.PlayMainMenuMusic();
                        }
                        else
                        {
                            State = GamePlayState.GameOver;
                            Menu.GameOver();
                            Manager_Sound.PlayMainMenuMusic();
                        }
                    }
                    break;
                case GamePlayState.GameOver:
                    // Needs to be handled in the main game class
                    break;
                default:
                    break;
            }
        }

        private void showGigaChadTimer(int duration)
        {
            // in the first stage: add comment non-skippable
            if (GigaChadTimerS >= GigaChadTimer_WaitToSkip)
            {
                Notifications.New("[Press to continue - Remaining: " + (GigaChadTimerS_CountTo - GigaChadTimerS) + "]", Color.Wheat, duration, Fonts.Medium);
            }
        }

        private void updateGigaChadState(GameTime gameTime)
        {
            if (GigaChadTimerMS >= 1000)
            {
                GigaChadTimerS++;
                GigaChadTimerMS = 0;
            }
            else
            {
                GigaChadTimerMS += gameTime.ElapsedGameTime.Milliseconds;
            }

            bool skipCondition = ((Input.AnythingPressed() || Input.AnyGamePadButtonPressed()) && GigaChadTimerS > GigaChadTimer_WaitToSkip);

            if (State == GamePlayState.Encounter)
            {
                // skip this one if we are not in baby mode
                if (Settings.GigaChadBabyMode)
                {
                    GigaChadTimerS_CountTo = GigaChadTimer_ShowTimeS;
                    State = GamePlayState.GigaChad_Prequel;
                }
                else
                {
                    GigaChadTimerS_CountTo = GigaChadTimer_CameraTransitS;
                    State = GamePlayState.GigaChad_MoveCamera;
                }
                GigaChad_CameraSwitch = true;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
            else if (State == GamePlayState.GigaChad_Prequel && (GigaChadTimerS >= GigaChadTimerS_CountTo || skipCondition))
            {
                GigaChadTimerS_CountTo = GigaChadTimer_CameraTransitS;
                State = GamePlayState.GigaChad_MoveCamera;
                GigaChad_CameraSwitch = true;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
            else if ((State == GamePlayState.GigaChad_MoveCamera && GigaChadTimerS >= GigaChadTimerS_CountTo))
            {
                if(Settings.GigaChadBabyMode) GigaChadTimerS_CountTo = GigaChadTimer_ShowTimeS;
                else GigaChadTimerS_CountTo = 2;
                State = GamePlayState.GigaChad_IntroduceGigaChad;
                GigaChad_CameraSwitch = true;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
            else if (State == GamePlayState.GigaChad_IntroduceGigaChad && (GigaChadTimerS >= GigaChadTimerS_CountTo || skipCondition))
            {
                GigaChadTimerS_CountTo = GigaChadTimer_ShowTimeS;
                if(Settings.GigaChadBabyMode) State = GamePlayState.GigaChad_IntroduceHammer;
                else State = GamePlayState.GigaChad_MoveCameraBack;
                GigaChad_CameraSwitch = true;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
            else if (State == GamePlayState.GigaChad_IntroduceHammer && (GigaChadTimerS >= GigaChadTimerS_CountTo || skipCondition))
            {
                State = GamePlayState.GigaChad_Power;
                GigaChad_CameraSwitch = true;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
            else if (State == GamePlayState.GigaChad_Power && (GigaChadTimerS >= GigaChadTimerS_CountTo || skipCondition))
            {
                State = GamePlayState.GigaChad_MoveCameraBack;
                GigaChad_CameraSwitch = true;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
            else if(State == GamePlayState.GigaChad_MoveCameraBack)
            {
                State = GamePlayState.FreeRoam;
                GigaChadTimerMS = 0;
                GigaChadTimerS = 0;
            }
        }

        private Y_CMRoom getBonusRoom()
        {
            var walkable = Rooms.Values.Where(x => x.Category == "Bonus").FirstOrDefault();
            if (walkable == null) Logger.Error("WE NEED A BONUS ROOM -.-!!");
            var room = (Y_CMRoom)walkable;
            return room;
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

            // Draw the controls inside the starting room pillar
            //spriteBatch.Draw(Manager_Sprites.Controls, ControlsRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Level;
        }
    }
}
