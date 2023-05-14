//#define PARALLEL_DEBUG_ROOM

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YGR
{
    public class X_RoomStump
    {
        public string Category;
        public string Name;
        public int TileWidth;
        public int TileHeight;
        public string ResourceFolder;
        public Dictionary<string, string> LdtkRoomTypeProperties;

        public X_RoomStump(
            string category,
            string name,
            int tileWidth,
            int tileHeight,
            string resourceFolder,
            Dictionary<string, string> ldtkRoomTypeProperties
        )
        {
            Category = category;
            Name = name;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            ResourceFolder = resourceFolder;
            LdtkRoomTypeProperties = ldtkRoomTypeProperties;
        }
    }

    public enum X_RoomState
    {
        Visible = 0,
        Invisible,
        InEncounter
    }

    public class Y_CMRoom : IWalkable
    {
        internal class X_DoorMask
        {
            static public int RoiDepth { get { return 4; } }
            static public int RoiWidth { get { return Y_Door.NumTilesDoorWidth * 2 + 1; } }

            int X, Y, ReachX, ReachY;
            Rectangle Room;
            Rectangle RegionOfInterest;
            int TileSize;

            public X_DoorMask(Rectangle roi, Rectangle room, int tileSize)
            {
                RegionOfInterest = roi;
                X = roi.X;
                Y = roi.Y;
                ReachX = roi.Width + X;
                ReachY = roi.Height + Y;
                Room = room;
                TileSize = tileSize;
            }

            public bool Check(int i)
            {
                int x = i % Room.Width;
                int y = (i - x) / Room.Width;
                return x >= X && x < ReachX && y >= Y && y < ReachY;
            }

            public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
            {
                Factory_Debug.DrawRectangle(X + (int)globalOffset.X, Y + (int)globalOffset.Y, ReachX - X, ReachY - Y, 1, Color.BlueViolet, spriteBatch);
                Factory_Debug.DrawRectangle(
                    RegionOfInterest.X,
                    RegionOfInterest.Y,
                    RegionOfInterest.Width,
                    RegionOfInterest.Height,
                    3, Color.Aquamarine, spriteBatch);
            }

            public void MoveBy(Point p)
            {
                RegionOfInterest.Offset(p);
                Room.Offset(p);
            }

            public int[][] GetTemplate()
            {
                int[][] template = new int[Room.Height / TileSize][];
                for (int i = 0; i < template.Length; ++i)
                {
                    template[i] = Enumerable.Repeat<int>((int)X_TileType.DontCare, Room.Width / TileSize).ToArray();
                }

                for (int y = 0; y < template.Length; ++y)
                {
                    for (int x = 0; x < template[0].Length; ++x)
                    {
                        var p = new Point((int)((1.5 + x) * TileSize) + Room.Location.X, (int)((1.5 + y) * TileSize) + Room.Location.Y);
                        if (RegionOfInterest.Contains(p))
                        {
                            template[y][x] = (int)X_TileType.Floor;
                        }
                    }
                }
                //output(template, "./logs/illumination.csv");
                return template;
            }
        }

        public string Name { get; set; }
        public X_CollisionModel_Room Collision { get; }
        public int ElementLevel { get { return 1; } set { } }
        public X_RoomGraph Graph { get; set; }
        public Rectangle Rect { get; set; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        private Dictionary<X_ConnectorSide, IList<X_DoorMask>> _doorMasks;
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        private bool _doorsToggled;
        public string ResourceFolder { get; }
        public Color RegionColor { get; }
        public List<X_Light> Lights { get; set; }
        private X_RoomState State { get; set; }
        public string Category { get; }
        public List<Rectangle> ResetRects { get; }
        public Vector3 Offset { get; set; }
        public Color[] Shade { get; set; }
        public X_IlluminationResources IlluminationResources { get; set; }

        private List<Interactable_Basic> _interactables = new List<Interactable_Basic> { };

        private Dictionary<string, string> _ldtkRoomTypeProperties;

        public Point TeleporterTarget { get; private set; }

        private byte[] _floorData;
        private Texture2D _floor;
        private Color[] _floorColorData;
        private byte[] _roofData;
        private Texture2D _roof;
        int _width;
        int _height;
        bool _visited;
        Y_Level _level;
        public bool HasSpikeEnemy { get; set; }

        public Texture2D[] ShadeTexture { get; set; }
        public int ShadeIndex { get; set; }

        List<EnemyEntity> _enemies;
        List<PlayerEntity> _players;
        public List<PickUp> PickUps { get; }
        public List<PowerUp> _pUps;

        public bool Cleared;

        static public bool PreprocessRoom(
            X_RoomStump initiator,
            List<string> categories,
            Dictionary<string, Dictionary<string, string>> ldtkRoomTypes,
            GraphicsDevice graphicsDevice
        )
        {
            string name = initiator.Name;
            string category = initiator.Category;
            string resourceFolder = Util.PathOsNormalization(initiator.ResourceFolder);
            bool preprocessed = false;
            foreach (var c in categories)
            {
                var srcPath = Util.GetAbsResourceFolderPath(resourceFolder);
                if (File.Exists(srcPath + c + "_Floor.Color") && File.Exists(srcPath + c + "_Roof.Color"))
                {
                    continue;
                }

                preprocessed = true;

                var readFloor = File.ReadAllBytesAsync(resourceFolder + ldtkRoomTypes[c]["Floor"]);
                var readWall = File.ReadAllBytesAsync(resourceFolder + ldtkRoomTypes[c]["Wall"]);
                var readRoof = File.ReadAllBytesAsync(resourceFolder + ldtkRoomTypes[c]["Roof"]);

                Task<byte[]> readVegetation = null;
                if (File.Exists(resourceFolder + ldtkRoomTypes[c]["Vegetation"]))
                {
                    readVegetation = File.ReadAllBytesAsync(resourceFolder + ldtkRoomTypes[c]["Vegetation"]);
                }

                Task.WaitAll(readFloor, readWall, readRoof);
                if (readVegetation != null)
                {
                    Task.WaitAll(readVegetation);
                }
                var floorData = readFloor.Result;
                var wallData = readWall.Result;
                var roofData = readRoof.Result;
                byte[] vegetationData = null;
                if (readVegetation != null)
                {
                    vegetationData = readVegetation.Result;
                }

                MemoryStream floorStream = new MemoryStream(floorData);
                MemoryStream wallStream = new MemoryStream(wallData);
                MemoryStream roofStream = new MemoryStream(roofData);
                MemoryStream vegetationStream = null;
                if (vegetationData != null)
                {
                    vegetationStream = new MemoryStream(vegetationData);
                }

                Texture2D floor = Texture2D.FromStream(graphicsDevice, floorStream, DefaultColorProcessors.PremultiplyAlpha);
                Texture2D txWall = Texture2D.FromStream(graphicsDevice, wallStream, DefaultColorProcessors.PremultiplyAlpha);
                Texture2D roof = Texture2D.FromStream(graphicsDevice, roofStream, DefaultColorProcessors.PremultiplyAlpha);
                Texture2D vegetation = null;
                if (vegetationStream != null)
                {
                    vegetation = Texture2D.FromStream(graphicsDevice, vegetationStream, DefaultColorProcessors.PremultiplyAlpha);
                }

                Color[] target = new Color[floor.Width * floor.Height];
                Color[] source = new Color[txWall.Width * txWall.Height];
                Color[] roofC = new Color[txWall.Width * txWall.Height];
                Color[] vegC = null;
                if (vegetation != null)
                {
                    vegC = new Color[txWall.Width * txWall.Height];
                    vegetation.GetData<Color>(vegC);
                }
                byte[] toFloor = new byte[floor.Width * floor.Height * 4];
                byte[] toRoof = new byte[floor.Width * floor.Height * 4];

                roof.GetData<Color>(roofC);
                floor.GetData<Color>(target);
                txWall.GetData<Color>(source);


                int index = 0;
                for (int h = 0; h < floor.Height; ++h)
                {
                    for (int w = 0; w < floor.Width; ++w)
                    {
                        if (vegC != null && vegC[h * floor.Width + w].A != 0)
                        {
                            var col = vegC[h * floor.Width + w];
                            toFloor[index] = col.R;
                            toFloor[index + 1] = col.G;
                            toFloor[index + 2] = col.B;
                            toFloor[index + 3] = col.A;
                        }
                        else if (source[h * floor.Width + w].A != 0)
                        {
                            var col = source[h * floor.Width + w];
                            toFloor[index] = col.R;
                            toFloor[index + 1] = col.G;
                            toFloor[index + 2] = col.B;
                            toFloor[index + 3] = col.A;
                        }
                        else
                        {
                            toFloor[index] = target[h * floor.Width + w].R;
                            toFloor[index + 1] = target[h * floor.Width + w].G;
                            toFloor[index + 2] = target[h * floor.Width + w].B;
                            toFloor[index + 3] = target[h * floor.Width + w].A;
                        }

                        toRoof[index] = roofC[h * floor.Width + w].R;
                        toRoof[index + 1] = roofC[h * floor.Width + w].G;
                        toRoof[index + 2] = roofC[h * floor.Width + w].B;
                        toRoof[index + 3] = roofC[h * floor.Width + w].A;

                        index += 4;
                    }
                }

                Util.SaveAsGZip(resourceFolder + c + "_Floor.Color", toFloor);
                Util.SaveAsGZip(resourceFolder + c + "_Roof.Color", toRoof);

                if (Debugger.IsAttached && System.OperatingSystem.IsWindows())
                {
                    Util.SaveAsGZip(srcPath + c + "_Floor.Color", toFloor);
                    Util.SaveAsGZip(srcPath + c + "_Roof.Color", toRoof);
                }
            }
            return preprocessed;
        }

        public Y_CMRoom(
            X_RoomStump initiator,
            Y_Level level
        )
        {
            HasSpikeEnemy = false;
            Name = initiator.Name;
            Category = initiator.Category;
            ResourceFolder = Util.PathOsNormalization(initiator.ResourceFolder);
            var files = Directory.GetFiles(ResourceFolder);
            string dataFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("data") && Path.GetFileName(x).EndsWith(".json")).First());
            string collisionsFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("Collision") && Path.GetFileName(x).EndsWith(".csv")).First());
            _ldtkRoomTypeProperties = initiator.LdtkRoomTypeProperties;
            _level = level;

            var lines = File.ReadAllLinesAsync(ResourceFolder + collisionsFileName);
            var readFloor = File.ReadAllBytesAsync(ResourceFolder + Category + "_Floor.Color");
            var readRoof = File.ReadAllBytesAsync(ResourceFolder + Category + "_Roof.Color");
            var json = File.ReadAllTextAsync(ResourceFolder + dataFileName);

            lines.Wait();
            int[][] collisions = new int[lines.Result.Length][];
            int counter = 0;
            foreach (var line in lines.Result)
            {
                collisions[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

            collisions = paddOutline(collisions);
            Collision = new X_CollisionModel_Room(collisions, initiator.TileWidth, initiator.TileHeight);
            Graph = new X_RoomGraph(this, collisions, initiator.TileWidth, initiator.TileHeight);
            Rect = new Rectangle(0, 0, collisions[0].Length * Collision.TileWidth, collisions.Length * Collision.TileHeight);
            ResetRects = new List<Rectangle>();

            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            _doorMasks = new Dictionary<X_ConnectorSide, IList<X_DoorMask>>();

            List<string> layers;
            json.Wait();
            dynamic array = JsonConvert.DeserializeObject(json.Result);
            layers = JsonConvert.DeserializeObject<List<string>>(array.layers.ToString());

            var t1 = Task.Run(() =>
            {
                if (array.entities.Door != null)
                {
                    IList<GenericLdtkEntity> doors = JsonConvert.DeserializeObject<List<GenericLdtkEntity>>(array.entities.Door.ToString());

                    foreach (var door in doors)
                    {
                        int x = (int)((float)door.x / door.width * Collision.TileWidth) + Rect.X;
                        int y = (int)((float)door.y / door.height * Collision.TileHeight) + Rect.Y;
                        var side = determineSide(x, y);
                        IList<X_ConnectorPoint> list;
                        if (!Doors.TryGetValue(side, out list))
                        {
                            Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(x, y)) });
                        }
                    }
                }
            });

            _enemies = new List<EnemyEntity>();
            // assign power ups
            var t2 = Task.Run(() =>
            {
                if (array.entities.Enemy != null)
                {
                    var enemies = JsonConvert.DeserializeObject<List<EnemyEntity>>(array.entities.Enemy.ToString());
                    foreach (var s in enemies)
                    {
                        _enemies.Add(s);
                    }
                }
            });

            _players = new List<PlayerEntity>();
            var t4 = Task.Run(() =>
            {
                if (array.entities.Player != null)
                {
                    var playerSpawner = JsonConvert.DeserializeObject<List<PlayerEntity>>(array.entities.Player.ToString());
                    foreach (var s in playerSpawner)
                    {
                        _players.Add(s);
                    }
                }
            });

            _interactables = new List<Interactable_Basic>();
            if (array.entities.Button != null)
            {
                var buttonEntities = JsonConvert.DeserializeObject<List<ButtonEntity>>(array.entities.Button.ToString());
                // Place a tutorial field that guides the players
                // _interactables.Add(new Interactable_Tutorialfield(
                //     new Rectangle(16, 12, 11, 8), this, (Y_CMRoom)_startRoom)
                // );

                foreach (var button in buttonEntities)
                {
                    if (button.customFields["Type"] == ButtonEntity.EscapeButton)
                    {
                        // Room opener field that can trigger the game start
                        _interactables.Add(new Interactable_BossRoomEscaper(
                            // TODO: correct button size should ideally come from ldtk
                            new Rectangle(button.x, button.y, button.width, button.height), this)
                        );
                    }
                    else if (button.customFields["Type"] == ButtonEntity.StartButton)
                    {
                        // Room opener field that can trigger the game start
                        _interactables.Add(new Interactable_RoomOpener(
                            new Rectangle(button.x, button.y, button.width, button.height), this)
                        );
                    }
                }
            }

            if (array.entities.Teleportation_point != null)
            {
                var teleporter = JsonConvert.DeserializeObject<List<GenericLdtkEntity>>(array.entities.Teleportation_point.ToString());
                foreach (var t in teleporter)
                {
                    TeleporterTarget = new Point(t.x + 16, t.y + 16);
                    break;
                }
            }
            else
            {
                // make sure there is at least one teleporter
                TeleporterTarget = new Point(Rect.Width / 2, Rect.Height / 2);
            }

            PickUps = new List<PickUp>();
            _pUps = new List<PowerUp>();
            var t5 = Task.Run(() =>
            {
                if (array.entities.PowerUp != null)
                {
                    _pUps = JsonConvert.DeserializeObject<List<PowerUp>>(array.entities.PowerUp.ToString());
                }
                setPowerUps();
            });

            int tileSize = Y_Level.TextureTileSize;
            Task.WaitAll(t1);
            foreach (var door in Doors)
            {
                float sf = 1.5f;
                if (door.Key == X_ConnectorSide.Top)
                {
                    var p = Doors[X_ConnectorSide.Top].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X - (X_DoorMask.RoiWidth / 2.0f * tileSize)) / tileSize) * tileSize),
                        (int)((Math.Round(p.Y + sf * tileSize) / tileSize) * tileSize),
                        X_DoorMask.RoiWidth * tileSize, X_DoorMask.RoiDepth * tileSize);

                    _doorMasks.Add(door.Key, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, tileSize)
                        });
                }
                else if (door.Key == X_ConnectorSide.Left)
                {
                    var p = Doors[X_ConnectorSide.Left].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X + sf * tileSize) / tileSize) * tileSize),
                        (int)((Math.Round(p.Y - (X_DoorMask.RoiWidth / 2.0f * tileSize)) / tileSize) * tileSize),
                        X_DoorMask.RoiDepth * tileSize, X_DoorMask.RoiWidth * tileSize);

                    _doorMasks.Add(door.Key, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, tileSize)
                        });
                }
                else if (door.Key == X_ConnectorSide.Right)
                {
                    var p = Doors[X_ConnectorSide.Right].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X - (sf + X_DoorMask.RoiDepth) * tileSize) / tileSize) * tileSize),
                        (int)((Math.Round(p.Y - (X_DoorMask.RoiWidth / 2.0f * tileSize)) / tileSize) * tileSize),
                        X_DoorMask.RoiDepth * tileSize, X_DoorMask.RoiWidth * tileSize);

                    _doorMasks.Add(door.Key, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, tileSize)
                        });
                }
                else // if (door.Key == X_ConnectorSide.Bottom)
                {
                    var p = Doors[X_ConnectorSide.Bottom].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X - (X_DoorMask.RoiWidth / 2.0f * tileSize)) / tileSize) * tileSize),
                        (int)((Math.Round(p.Y - (sf + X_DoorMask.RoiDepth) * tileSize) / tileSize) * tileSize),
                        X_DoorMask.RoiWidth * tileSize, X_DoorMask.RoiDepth * tileSize);

                    _doorMasks.Add(X_ConnectorSide.Bottom, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, tileSize)
                        });
                }
            }

            State = X_RoomState.Invisible;

            Task.WaitAll(t2, t4, t5);

            Task.WaitAll(readFloor, readRoof);
            _floorData = readFloor.Result;
            _roofData = readRoof.Result;
            _width = Collision.GetCollisionTemplate()[0].Length * tileSize;
            _height = Collision.GetCollisionTemplate().Length * tileSize;
            _doorsToggled = false;
            _visited = false;
            IlluminationResources = new X_IlluminationResources();
        }

        public Interactable_Basic StartingPad()
        {
            if (_interactables.Count() > 0 && Category == "Start")
            {
                return _interactables[0];
            }
            else return null;
        }

        private void setPowerUps()
        {
            PickUps.Clear();
            foreach (var s in _pUps)
            {
                Point location = new Point(s.x + Rect.X, s.y + Rect.Y);
                if (s.customFields["Type"] == PowerUp.Life)
                    PickUps.Add(PickUp.Factory(Y_PowerUps.Life, location, s.width, s.height, Y_Level.GlobalScale));
                if (s.customFields["Type"] == PowerUp.Revive)
                    PickUps.Add(PickUp.Factory(Y_PowerUps.Revive, location, s.width, s.height, Y_Level.GlobalScale));

                // Weapons
                if (s.customFields["Type"] == PowerUp.WeaponPistol)
                    PickUps.Add(PickUp.Factory(Y_PowerUps.WeaponPistol, location, s.width, s.height, Y_Level.GlobalScale));
                if (s.customFields["Type"] == PowerUp.WeaponShotgun)
                    PickUps.Add(PickUp.Factory(Y_PowerUps.WeaponShotgun, location, s.width, s.height, Y_Level.GlobalScale));
                if (s.customFields["Type"] == PowerUp.WeaponFunky)
                    PickUps.Add(PickUp.Factory(Y_PowerUps.WeaponFunky, location, s.width, s.height, Y_Level.GlobalScale));
            }
        }

        public void AddLight(int offsetX, int offsetY, int offsetZ)
        {
            Lights = new List<X_Light>() {
            new X_Light(
                new Vector3(Rect.X + offsetX * Y_Level.TextureTileSize,
                Rect.Y + offsetY * Y_Level.TextureTileSize,
                offsetZ * Y_Level.TextureTileSize),
                Rect, Y_Level.GlobalScale)
            };
        }

        public int GetTargetShadeIndex()
        {
            return (ShadeIndex + 1) % 2;
        }

        public void SwitchTargetShadeIndex()
        {
            ShadeIndex = (ShadeIndex + 1) % 2;
        }

        public List<X_Light> GetAllRelevantLights()
        {
            List<X_Light> ret = new List<X_Light>();
            ret.AddRange(Lights);
            foreach (var door in DoorRooms)
            {
                // add all doors and make sure they are in the opened state
                var d = (Y_Door)door.Value.First();
                if (d.DoorIsOpen())
                {
                    ret.AddRange(d.Lights);
                }
            }
            if (ret.Count() == 0) ret.AddRange(Lights);
            //return ret.Distinct().ToList();
            return Lights;
        }

        public void ToggleDoors()
        {
            _doorsToggled = true;
        }

        public void FinalizeItem(GraphicsDevice graphicsDevice)
        {
            Color[] floor2 = Util.DeGZipFile(_floorData);
            Color[] roof2 = Util.DeGZipFile(_roofData);

            _floor = new Texture2D(graphicsDevice, _width, _height, false, SurfaceFormat.Color);
            _floor.SetData(floor2);
            _roof = new Texture2D(graphicsDevice, _width, _height);
            _roof.SetData(roof2);

            ShadeTexture = new Texture2D[] {
                new Texture2D(graphicsDevice, _floor.Width, _floor.Height, false, SurfaceFormat.Color, ShaderAccess.ReadWrite),
                new Texture2D(graphicsDevice, _floor.Width, _floor.Height, false, SurfaceFormat.Color, ShaderAccess.ReadWrite)
            };
            ShadeIndex = 0;

            _floorColorData = new Color[_floor.Width * _floor.Height];
            _floor.GetData<Color>(_floorColorData);

            //Color[] data = new Color[_floor.Width * _floor.Height];
            Color shadeColor = Color.Black;
            shadeColor.A = 0;
            Shade = Enumerable.Repeat<Color>(shadeColor, _floor.Width * _floor.Height).ToArray();
            ShadeTexture[0].SetData(Shade);
            ShadeTexture[1].SetData(Shade);
        }

        public void ResetRoom()
        {
            var rects = Collision.GetCollisionRectangles().ToList();
            rects.AddRange(ResetRects);
            Collision.UpdateCollisionRectangles(rects);
            foreach (var it in _interactables)
            {
                it.Reset();
            }
            DoorRooms.Clear();
            ResetRects.Clear();
            Cleared = false;
            State = X_RoomState.Invisible;
            MoveTo(new Point(0, 0));
            _visited = false;
            setPowerUps();
        }

        public void InGameReset()
        {

            _visited = false;
            setPowerUps();
            SpawnEnemies();
        }

        public void SpawnEnemies()
        {
            Manager_Enemies.ClearEnemies(this);
            var enemies = GetEnemySpawningPoints();
            if (!HasSpikeEnemy)
            {
                foreach (var spr in enemies)
                {
                    Vector2 pos = new Vector2(spr.x, spr.y);

                    if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.SimpleEnemy)
                        Manager_Enemies.AddEnemy_SimpleEnemy(pos, _level);
                    else if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.SlimeEnemy)
                        Manager_Enemies.AddEnemy_Slime(pos, _level);
                    else if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.BossEnemy)
                        Manager_Enemies.AddEnemy_Boss(pos, _level);
                }
            }
            else
            {
                // if we have a spiky room, place at most 6 regular slimy bastards
                var randenems = enemies
                    .Where(x => EnemyEntity.GetType(x) != Manager_Enemies.EnemyType.SpikyEnemy)
                    .OrderBy(x => Util.random.Next())
                    .Take(6)
                    .ToList();

                foreach (var spr in randenems)
                {
                    Vector2 pos = new Vector2(spr.x, spr.y);

                    if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.SimpleEnemy)
                        Manager_Enemies.AddEnemy_SimpleEnemy(pos, _level);
                    else if (EnemyEntity.GetType(spr) == Manager_Enemies.EnemyType.SlimeEnemy)
                        Manager_Enemies.AddEnemy_Slime(pos, _level);
                }

                // aaaaand place one giant spiky mega bastard
                var spiky = enemies
                    .Where(x => EnemyEntity.GetType(x) == Manager_Enemies.EnemyType.SpikyEnemy)
                    .OrderBy(x => Util.random.Next())
                    .Take(1)
                    .FirstOrDefault();

                if (spiky == null) {
                    Logger.Info("Missing spiky on selected room... Don't crash the game, because it may happen during the jury evaluation and it really doesn't matter that much ^^");
                }
                else
                {
                    Vector2 pos = new Vector2(spiky.x, spiky.y);
                    Manager_Enemies.AddEnemy_SlimeSpiky(pos, _level);
                }

            }
        }

        public void ApplyPowerUps(IVictim player)
        {
            if (player is not IPlayer)
            {
                return;
            }

            for (int i = 0; i < PickUps.Count(); ++i)
            {
                if (PickUps[i].Active && player.Rect.Intersects(PickUps[i].Rect))
                {
                    bool powerupExpired = PickUps[i].Action((IPlayer)player, PickUps[i]);
                }
            }
        }

        public bool AllDoorsOpen()
        {
            foreach(var door in DoorRooms)
            {
                if (!((Y_Door)door.Value.First()).IsDoorOpen()) return false;
            }
            return true;
        }

        public bool IsVisible()
        {
            return State == X_RoomState.Visible || State == X_RoomState.InEncounter;
        }

        public bool VisitedBeforeByPlayer()
        {
            return _visited;
        }

        public bool IsLocked()
        {
            return State == X_RoomState.InEncounter;
        }

        public void SetLocked(bool yes)
        {
            if (yes) State = X_RoomState.InEncounter;
            else State = X_RoomState.InEncounter;
        }

        public void SetVisited(bool yes)
        {
            _visited = yes;
        }

        public void SetVisible(bool yes)
        {
            if (yes) State = X_RoomState.Visible;
            else State = X_RoomState.Invisible;
        }

        public void LockAllDoors(bool unlock = false)
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (unlock) door.UnlockDoor();
                        else door.LockDoor();
                    }
                }
            }
        }

        public void LockAllClosedDoors(bool unlock = false)
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (door.IsDoorClosed())
                        {
                            if (unlock) door.UnlockDoor();
                            else door.LockDoor();
                        }
                    }
                }
            }
        }

        public void LockAllOpenedDoors(bool unlock = false)
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (door.IsDoorOpen())
                        {
                            if (unlock) door.UnlockDoor();
                            else door.LockDoor();
                        }
                    }
                }
            }
        }

        // One player has COVID -> Lockdown
        public void CloseAllUnlockedRoomDoors(bool lockWhenFinished = false)
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (door.CloseUnlockedDoor())
                        {
                            if (lockWhenFinished)
                            {
                                door.LockDoor();
                            }
                            ToggleDoors();
                        }
                    }
                }
            }
            Illuminate(this);
        }

        public void OpenAllUnlockedRoomDoors(bool lockWhenFinished = false)
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (door.OpenUnlockedDoor())
                        {
                            if (lockWhenFinished)
                            {
                                door.LockDoor();
                            }
                            ToggleDoors();
                            var otherRoom = door.GetOtherDoor(this);
                            ((Y_CMRoom)otherRoom.Item2).SetVisible(true);
                            ((Y_CMRoom)otherRoom.Item2).ToggleDoors();

                            Illuminate(otherRoom.Item2);
                        }
                    }
                    else if (walkable.WhatAreYou() == X_LevelElements.Room)
                    {
                        Logger.Error("Found room where there should have been a door");
                    }
                }
            }
            Illuminate(this);
            SetVisible(true);
        }

        public void Illuminate(IWalkable room)
        {
            if (!Settings.DynamicShades) return;
            Manager_Light2.Illuminate(room);
        }

        public static X_DoorTextureLayer MapTexture(string textureType)
        {
            if (textureType.ToLower().Contains("wall") || textureType.ToLower().Contains("floor")) return X_DoorTextureLayer.Floor;
            if (textureType.ToLower().Contains("roof")) return X_DoorTextureLayer.Wall;
            if (textureType.ToLower().Contains("mechanism1")) return X_DoorTextureLayer.Mechanism1;
            if (textureType.ToLower().Contains("mechanism2")) return X_DoorTextureLayer.Mechanism2;
            return X_DoorTextureLayer.Door;
        }

        public static X_TileType MapJsonName(string tileType)
        {
            if (tileType.ToLower().Contains("floor")) return X_TileType.Floor;
            if (tileType.ToLower().Contains("wall")) return X_TileType.Wall;
            if (tileType.ToLower().Contains("roof")) return X_TileType.Roof;
            if (tileType.ToLower().Contains("out")) return X_TileType.Outside;
            return X_TileType.DontCare;
        }

        private void drawFloor(SpriteBatch spriteBatch)
        {
            lock (this)
            {
                spriteBatch.Draw(
                _floor, Rect.Location.ToVector2(),
                new Rectangle(0, 0, _floor.Width, _floor.Height),
                Color.White, 0, Vector2.Zero, Y_Level.GlobalScale, SpriteEffects.None, 0);
            }
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!Doors.ContainsKey(side)) return null;
            return Doors[side].First();
        }

        private int[][] paddOutline(int[][] collision)
        {
            int x = 0;
            int y = 0;
            while (x < collision.Length)
            {
                y = 0;
                while (y < collision[0].Length)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y++;
                }
                x++;
            }

            x = collision.Length - 1;
            while (x >= 0)
            {
                y = collision[0].Length - 1;
                while (y >= 0)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y--;
                }
                x--;
            }

            x = 0;
            while (x < collision.Length)
            {
                y = collision[0].Length - 1;
                while (y >= 0)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y--;
                }
                x++;
            }

            x = collision.Length - 1;
            while (x >= 0)
            {
                y = 0;
                while (y < collision[0].Length)
                {
                    if (collision[x][y] == 1) break;
                    collision[x][y] = -1;
                    y++;
                }
                x--;
            }

            return collision;
        }

        private X_ConnectorSide determineSide(int x0, int y0)
        {
            /**
             *  x1,y1 -- x2,y2
             *    |        |
             *    |        |
             *    |        |
             *  x4,y4 -- x3,y3
             */

            float x1 = Rect.X;
            float y1 = Rect.Y;
            float x2 = Rect.X + Rect.Width;
            float y2 = Rect.Y;
            float x3 = Rect.X + Rect.Width;
            float y3 = Rect.Y + Rect.Height;
            float x4 = Rect.X;
            float y4 = Rect.Y + Rect.Height;

            float[] dists = {
                distance(x0, y0, x1, y1, x2, y2),
                distance(x0, y0, x2, y2, x3, y3),
                distance(x0, y0, x3, y3, x4, y4),
                distance(x0, y0, x4, y4, x1, y1) };

            float min = float.MaxValue;
            int ind = 0;
            for (int i = 0; i < dists.Length; ++i)
            {
                if (dists[i] < min)
                {
                    min = dists[i];
                    ind = i;
                }
            }

            if (ind == 0) return X_ConnectorSide.Top;
            if (ind == 1) return X_ConnectorSide.Right;
            if (ind == 2) return X_ConnectorSide.Bottom;
            else return X_ConnectorSide.Left;
        }

        private float distance(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            return (float)(Math.Abs((x2 - x1) * (y1 - y0) - (x1 - x0) * (y2 - y1)) / Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)));
        }

        public void MoveTo(Point position)
        {
            Point p = position - Rect.Location;
            Collision.MoveBy(p);
            foreach (var side in Doors)
            {
                for (int i = 0; i < side.Value.Count(); ++i)
                {
                    side.Value[i].MoveBy(p);
                    //side.Value[i] = new Tuple<int, int>(position.X + side.Value[i].Item1, position.Y + side.Value[i].Item2);
                }
            }

            foreach (var light in Lights)
            {
                light.MoveBy(p);
            }

            foreach (var door in _doorMasks)
            {
                door.Value.First().MoveBy(p);
            }

            foreach (var powerUp in PickUps)
            {
                powerUp.MoveBy(p);
            }

            //// needs to be done this way because properties return by value and not by ref
            Rect = new Rectangle(position.X, position.Y, Rect.Width, Rect.Height);
            Offset = new Vector3(Rect.Location.X / Y_Level.GlobalScale, Rect.Location.Y / Y_Level.GlobalScale, 0);
            foreach (var enemy in _enemies)
            {
                enemy.x += p.X;
                enemy.y += p.Y;
            }

            foreach (var player in _players)
            {
                player.x += p.X;
                player.y += p.Y;
            }

            TeleporterTarget = new Point(TeleporterTarget.X + p.X, TeleporterTarget.Y + p.Y);

            foreach (var interact in _interactables)
            {
                interact.MoveBy(p);
            }
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side)
        {
            return Doors[side].First();
        }

        public List<EnemyEntity> GetEnemySpawningPoints()
        {
            return _enemies;
        }

        public List<PickUp> GetPickUps()
        {
            return PickUps;
        }

        public List<PlayerEntity> GetPlayerSpawningPoints()
        {
            return _players;
        }

        public List<IPlayer> GetPlayersInside()
        {
            return Manager_Players.Players.FindAll(p => p.Room == this);
        }

        public List<IEnemy> GetEnemiesInside()
        {
            return Manager_Enemies.GetEnemies().ToList().FindAll(e => e.Room == this);
        }

        public void SuppliedRoomFunctions()
        {
            if (Input.IsKeyTriggered(Keybinds.OpenAllAdjacentDoors))
            {
                this.OpenAllUnlockedRoomDoors();
            }
            else if (Input.IsKeyTriggered(Keybinds.CloseAllAdjacentDoors))
            {
                this.CloseAllUnlockedRoomDoors();
            }
            else if (Input.IsKeyTriggered(Keybinds.LockAllAdjacentDoors))
            {
                this.LockAllDoors();
            }
            else if (Input.IsKeyTriggered(Keybinds.UnlockAllAdjacentDoors))
            {
                this.LockAllDoors(unlock: true);
            }
        }

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            switch (State)
            {
                case X_RoomState.Visible:
                    if (_doorsToggled)
                    {
                        _doorsToggled = false;
                    }
                    break;
                case X_RoomState.Invisible:
                    break;
                default:
                    break;
            }

            foreach (var powerUp in PickUps)
            {
                if (powerUp.Active)
                    powerUp.Update(gameTime);
            }

            foreach (var interactable in _interactables)
            {
                interactable.Update(gameTime);
            }
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
            // Culling
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty)
            {
                return;
            }

            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.Blue, spriteBatch);
            foreach (var light in Lights)
            {
                light.DrawOutline(gameTime, globalOffset, spriteBatch);
            }

            foreach (var door in _doorMasks)
            {
                door.Value.First().DrawOutline(gameTime, Rect.Location.ToVector2(), spriteBatch);
            }

            foreach (var door in Doors)
            {
                door.Value.First().DrawOutline(gameTime, Rect.Location.ToVector2(), spriteBatch);
            }

            foreach (var powerUp in PickUps)
            {
                if (powerUp.Active == true)
                {
                    powerUp.DrawOutline(gameTime, globalOffset, spriteBatch);
                }
            }

            foreach (var interactable in _interactables)
            {
                interactable.DrawOutline(gameTime, globalOffset, spriteBatch);
            }

            Factory_Debug.DrawRectangle(TeleporterTarget.X - 16, TeleporterTarget.Y - 16, 32, 32, 3, Color.Blue, spriteBatch);
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (State == X_RoomState.Invisible)
            {
            }
            else
            {
                spriteBatch.Draw(
                    _floor, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Y_Level.GlobalScale, SpriteEffects.None, 0);

                spriteBatch.Draw(
                    ShadeTexture[ShadeIndex], Rect.Location.ToVector2(),
                    new Rectangle(0, 0, ShadeTexture[ShadeIndex].Width, ShadeTexture[ShadeIndex].Height),
                    Color.White * Manager_Light2.ShadeFloat, 0, Vector2.Zero, Y_Level.GlobalScale, SpriteEffects.None, 0);

                spriteBatch.Draw(
                    _roof, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Y_Level.GlobalScale, SpriteEffects.None, 0);

                foreach (var interactable in _interactables)
                {
                    interactable.Draw(gameTime, globalOffset, spriteBatch);
                }

                foreach (var powerUp in PickUps)
                {
                    if (powerUp.Active == true)
                    {
                        powerUp.Draw(gameTime, globalOffset, spriteBatch);
                    }
                }
            }
        }

        /// <summary>
        /// This method is the standard ILevelElement WhatAreYou
        /// </summary>
        /// <returns>Returns the fitting X_LevelElements enum entry</returns>
        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Room;
        }
    }
}
