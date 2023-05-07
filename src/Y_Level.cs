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
using System.Linq;

namespace YGR
{
    public class Y_Level : IGameElement
    {
        internal class LevelNode
        {
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
            public Dictionary<string, Dictionary<int, LevelNode>> Level;
        }

        public enum GamePlayState
        {
            Start, // Not completed starting room
            FreeRoam, // Not in an encounter, players can freely roam
            Encounter, // Players are in an encounter, room locked
            End,
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

        private int _waitTimeBetweenEndOfFightAndLowerDoors = 125;
        private int _waitTimeBetweenEndOfFightAndLowerDoorsCounter = 0;

        Texture2D _background;
        Rectangle _backgroundRect;

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
            string doorResourceFolder,
            ContentManager content
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
                var room = new X_RoomStump(f.Item1, roomName, TileWidth, TileHeight, textureTileSize, f.Item2, _data.LdtkRoomTypes[f.Item1]);
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

            _background = content.Load<Texture2D>("SpritesOther/title_image");
            int width = (int)(_background.Width * 3.5f);
            int height = (int)(_background.Height * 3.5f);
            Point startLocation = new Point((int)(width / 1.87f), (int)(height / 1.137f));
            _backgroundRect = new Rectangle(-startLocation.X, -startLocation.Y, width, height);

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
            //var key = _data.Level.Keys.ToArray()[3];
            var tree = _data.Level[key];

            var watch = new Stopwatch();
            watch.Start();
            float offset = 1024;
            foreach (var node in tree)
            {
                var type = _availableRooms[node.Value.Type];
                int index = random.Next(0, type.Count);

                // some hack to make sure that the first room is always the chosen one
                var n = type[index];

                if (n.Item2 == null)
                {
                    n = new Tuple<X_RoomStump, Y_CMRoom>(null, new Y_CMRoom(n.Item1));
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
                    if(
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

            int connectorIndex = Rooms.Max(x => x.Key)+1;
            foreach (var c in connectors)
            {
                Rooms.Add(connectorIndex, c);
                connectorIndex++;
            }

            Manager_Players.ClearPlayers();

            _interactables.Clear();

            // Place a tutorial field that guides the players
            // _interactables.Add(new Interactable_Tutorialfield(
            //     new Rectangle(16, 12, 11, 8), this, (Y_CMRoom)_startRoom)
            // );

            // Room opener field that can trigger the game start
            _interactables.Add(new Interactable_RoomOpener(
                new Rectangle(16, 2, 11, 7), this, (Y_CMRoom)_startRoom)
            );

            // Gameplay state
            State = GamePlayState.Start;
            ActiveRoom = _startRoom;
            Camera.SetFocusRoom(_startRoom, animate: false);

            Manager_Enemies.ClearEnemies();
            foreach (var room in Rooms)
            {
                if (room.Value.WhatAreYou() != X_LevelElements.Room) continue;

                var r = (Y_CMRoom)room.Value;

                var enemies = r.GetEnemySpawningPoints();
                foreach (var spr in enemies)
                {
                    Vector2 pos = new Vector2(spr.x, spr.y);

                    if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.SimpleEnemy)
                        Manager_Enemies.AddEnemy_SimpleEnemy(pos, this);
                    else if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.SlimeEnemy)
                        Manager_Enemies.AddEnemy_Slime(pos, this);
                    else if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.BossEnemy)
                        Manager_Enemies.AddEnemy_Gigachad(pos, this);
                }

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
                            Manager_Players.AddPlayer(PlayerType.Ghost, (PlayerIndex)playerIndex, position: pos, this);
                        playerIndex++;
                    }
                    else if (PlayerEntity.GetPointType(spr) == PlayerSpawningPointType.Chooser)
                    {
                        if (PlayerEntity.GetType(spr) == PlayerType.Nerd)
                            r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserNerd, pos.ToPoint(), spr.width, spr.height, Scale));
                        if (PlayerEntity.GetType(spr) == PlayerType.Ninja)
                            r.PickUps.Add(PickUp.Factory(Y_PowerUps.ChooserNinja, pos.ToPoint(), spr.width, spr.height, Scale));
                    }
                }

                // Make the last one controllable by keyboard
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
                    if (_interactables[0].InteractionComplete)
                    {
                        State = GamePlayState.FreeRoam;
                        _startRoom.OpenAllUnlockedRoomDoors();
                        Camera.SetFocusPlayers();
                    }
                    break;

                case GamePlayState.FreeRoam:
                    // Just sample the first room the first player is in right now
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

                    // Make sure all players are inside
                    if (cmroom.GetPlayersInside().Count != Manager_Players.Players.Count)
                    {
                        break;
                    }

                    if (cmroom.GetEnemiesInside().Count < 1)
                    {
                        // Room does not contain any enemies, so just mark as cleared an move on
                        cmroom.Cleared = true;
                        cmroom.OpenAllUnlockedRoomDoors();
                        break;
                    }

                    // At this point we have all players inside a room with
                    // enemies. Time to go in lock down and let the battle begin
                    cmroom.CloseAllUnlockedRoomDoors();
                    cmroom.SetLocked(true);
                    Camera.SetFocusRoom(cmroom);
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

                    // Check if players died
                    if (encounterRoom.GetPlayersInside().FindAll(p => p.LifePoints > 0).Count < 1)
                    {
                        Notifications.New("\n\n\n\n", Color.Wheat, 60000);
                        Notifications.New("Fighting to the bitter end, our heroes couldn't prove", Color.Wheat, 60000, Fonts.Large);
                        Notifications.New("themselves worthy of fighting alongside the gods...", Color.Wheat, 60000, Fonts.Large);
                        Manager_Sound.PlayFreeRoamMusic();
                        State = GamePlayState.End;
                        break;
                    }

                    if (encounterRoom.GetEnemiesInside().Count > 0)
                    {
                        break; // let players fight
                    }

                    Camera.SetFocusPlayers();

                    if (_waitTimeBetweenEndOfFightAndLowerDoorsCounter < _waitTimeBetweenEndOfFightAndLowerDoors)
                    {
                        _waitTimeBetweenEndOfFightAndLowerDoorsCounter++;
                        break;
                    }
                    _waitTimeBetweenEndOfFightAndLowerDoorsCounter = 0;

                    encounterRoom.Cleared = true;
                    encounterRoom.OpenAllUnlockedRoomDoors();
                    encounterRoom.SetLocked(false);

                    Manager_Sound.PlayFreeRoamMusic();

                    if (encounterRoom.Name.StartsWith("Gold"))
                    {
                        Notifications.New("\n\n\n\n", Color.Wheat, 60000);
                        Notifications.New("Overcoming the final challenge, glory awaits our heroes", Color.Wheat, 60000, Fonts.Large);
                        Notifications.New("when they fight alongside the gods in Ragnarok...", Color.Wheat, 60000, Fonts.Large);
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

                case GamePlayState.End:
                    // Nothing yet
                    break;

                default:
                    break;
            }
        }

        public void DrawPlayerSelectionUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont font = Fonts.Large;
            float spacing = 1.5f;
            string longest_str = "P1: [JOINED] Pick character"; // Used to center the text
            Vector2 size = font.MeasureString(longest_str);
            Vector2 pos = new Vector2((Camera.Bounds.Width - size.X) / 2, (Camera.Bounds.Height - size.Y * (1f + 3f * spacing)) / 2);
            pos.Y += 10; // Feels like CSS...

            foreach (IPlayer p in Manager_Players.Players)
            {
                string indexString = "P" + (int)p.PlayerIndex + ": ";
                string statusString = "";
                Color statusColor;
                if (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected)
                {
                    statusString += "[  --  ] Disconnected";
                    statusColor = Color.DimGray;
                }
                else if (!p.IsActive)
                {
                    statusString += "[  --  ] Move to join";
                    statusColor = Color.LightPink;
                }
                else if (p is Player_Ghost)
                {
                    statusString += "[JOINED] Pick character";
                    statusColor = Color.LightBlue;
                }
                else
                {
                    statusString += "[JOINED] Ready";
                    statusColor = Color.LimeGreen;
                }

                float indexStringWidth = font.MeasureString(indexString).X;
                // Draw text shadow
                spriteBatch.DrawString(font, indexString + statusString, pos + Vector2.One, Color.Black);

                // Draw text line itself
                Vector2 offset = new Vector2(font.MeasureString(indexString).X, 0);
                spriteBatch.DrawString(font, indexString, pos, Color.Lerp(p.Color, Color.Wheat, 0.5f));
                spriteBatch.DrawString(font, statusString, pos + offset, Color.Lerp(statusColor, Color.Wheat, 0.2f));
                pos.Y += size.Y * spacing;
            }
        }

        public void DrawPlayerStatusUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont font = Fonts.GetDecentlySizedFont();
            Vector2 pos = new Vector2((Camera.Bounds.Width) / 128, Camera.Bounds.Height / 16);
            float spacing = 1.25f;

            foreach (IPlayer p in Manager_Players.Players)
            {
                string indexString = "Player " + (int)p.PlayerIndex + ": ";
                string infoString = "";

                infoString += string.Format("\n HP: {0,-3}/{1,-3}", p.LifePoints, p.LifePointsMax);
                // infoString += string.Format("\n Class:  {0}", p.Name);
                // infoString += string.Format("\n Weapon: {0}", p.Gun.Name);
                infoString += string.Format("\n Class:");
                infoString += string.Format("\n Weapon:");

                Vector2 indexStringSize = font.MeasureString(indexString);
                Color playerColor = Color.Lerp(p.Color, Color.Wheat, 0.5f);

                // Draw a semi transparent background box
                int margin = 5;
                Vector2 totalSize = font.MeasureString(indexString + infoString);
                Rectangle rect = new Rectangle((int)pos.X - margin, (int)pos.Y - margin, (int)totalSize.X + 2 * margin, (int)totalSize.Y + 2 * margin);
                spriteBatch.Draw(Manager_Sprites.White, destinationRectangle: rect, null, playerColor * 0.4f, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw the player sprite
                AnimatedSprite charSprite = p.GetSprite();
                int height = (int)indexStringSize.Y;
                int width = (int)(charSprite.SpriteDimension.X / charSprite.SpriteDimension.Y * height);
                int x = rect.X + rect.Width - (int)(indexStringSize.Y * 1.5) - width / 2;
                int y = (int)(rect.Y + margin + indexStringSize.Y * 2);
                Rectangle charRect = new Rectangle(x, y, width, height);
                spriteBatch.Draw(charSprite.Texture, charRect, charSprite.SourceRectangle, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw the weapon sprite
                Texture2D weaponSprite = p.Gun.Sprite;
                width = (int)(weaponSprite.Width / weaponSprite.Height * height);
                y = (int)(rect.Y + margin + indexStringSize.Y * 3);
                Rectangle weaponRect = new Rectangle(x, y, width, height);
                if (p.Gun is not Gun_Ghost)
                    spriteBatch.Draw(weaponSprite, weaponRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw text itself
                Vector2 offset = new Vector2(0, font.MeasureString(infoString).Y);
                Util.DrawString(font, indexString, pos, playerColor, spriteBatch);
                Util.DrawString(font, infoString, pos, Color.Wheat, spriteBatch);
                pos.Y += totalSize.Y * spacing;
            }
        }

        public void DrawUI(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (State == Y_Level.GamePlayState.Start)
                DrawPlayerSelectionUI(gameTime, spriteBatch);

            DrawPlayerStatusUI(gameTime, spriteBatch);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var room in Rooms)
            {
                room.Value.DrawOutline(gameTime, globalOffset, spriteBatch);
            }

            foreach (var interactable in _interactables)
            {
                interactable.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_background, _backgroundRect, Color.White);
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
