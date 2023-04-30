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
using static YGR.X_AutoTiler;

namespace YGR
{
    public class X_RoomStump
    {
        public string Category;
        public string Name;
        public int TileWidth;
        public int TileHeight;
        public int TextureTileSize;
        public string ResourceFolder;
        public Dictionary<string, string> LdtkRoomTypeProperties;

        public X_RoomStump(
            string category,
            string name,
            int tileWidth,
            int tileHeight,
            int textureTileSize,
            string resourceFolder,
            Dictionary<string, string> ldtkRoomTypeProperties
        )
        {
            Category = category;
            Name = name;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            TextureTileSize = textureTileSize;
            ResourceFolder = resourceFolder;
            LdtkRoomTypeProperties = ldtkRoomTypeProperties;
        }
    }

    internal sealed class Door
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public sealed class Spawner
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public sealed class BossSpawner
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public sealed class PlayerSpawner
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public sealed class MultiPowerUp
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public sealed class Revive
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public sealed class Life
    {
        public string id;
        public string iid;
        public string layer;
        public int x;
        public int y;
        public int width;
        public int height;
        public int color;
    }

    public enum X_RoomState
    {
        Open = 1,
        Opening,
        Closing,
        Closed,
        LockedClosed,
        LockedOpen,
        Finished
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

        public float Scale { get; private set; }
        public string Name { get; set; }
        public X_CollisionModel_Room Collision { get; }
        public X_RoomGraph Graph { get; set; }
        public Rectangle Rect { get; set; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        private Dictionary<X_ConnectorSide, IList<X_DoorMask>> _doorMasks;
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        public int TextureTileSize { get; }
        public string ResourceFolder { get; }
        public Color RegionColor { get; }
        public List<X_Light> Lights { get; set; }
        private X_RoomState State { get; set; }
        public string Category { get; set; }
        public Manager_Light2.X_Vector3[] ShadeCoords { get; set; }
        public List<Rectangle> ResetRects { get; }

        private Dictionary<string, string> _ldtkRoomTypeProperties;

        private byte[] _floorData;
        private Texture2D _floor;
        private Color[] _floorColorData;
        private byte[] _roofData;
        private Texture2D _roof;
        private byte[] _wallData;
        bool[] _illumination = null;

        private int _currentDoorOpenOffset = 0;
        private float _doorOpeningTime = 2000.0f;
        private float _animationTime = 0.0f;

        //Dictionary<X_DoorTextureLayer, List<X_AutoTiler.X_AutoTileTexture>> _tileTextures;
        //Dictionary<X_DoorTextureLayer, List<X_AutoTiler.X_AutoTileColor>> _tileColors;

        X_AutoTiler.X_RoofDoor _roofDoor;

        List<Point> _spawner;
        List<Point> _bossSpawner;
        List<Point> _playerSpawner;

        private List<PowerUpItem> _powerUps;
        private List<MultiPowerUpItem> _multiPowerups;

        public bool Cleared;

#if PARALLEL_DEBUG_ROOM
        public static ConcurrentDictionary<string, int> dbag = new ConcurrentDictionary<string, int>();
#endif

        public Y_CMRoom(
            X_RoomStump initiator
        )
        {
            Name = initiator.Name;
            Category = initiator.Category;
            TextureTileSize = initiator.TextureTileSize;
            ResourceFolder = Util.PathOsNormalization(initiator.ResourceFolder);
            var files = Directory.GetFiles(ResourceFolder);
            string dataFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("data") && Path.GetFileName(x).EndsWith(".json")).First());
            string collisionsFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("Collision") && Path.GetFileName(x).EndsWith(".csv")).First());
            var lines = File.ReadAllLinesAsync(ResourceFolder + collisionsFileName);

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v+1);
#endif

            _ldtkRoomTypeProperties = initiator.LdtkRoomTypeProperties;

            var readFloor = File.ReadAllBytesAsync(ResourceFolder + _ldtkRoomTypeProperties["Floor"]);
            var readWall = File.ReadAllBytesAsync(ResourceFolder + _ldtkRoomTypeProperties["Wall"]);
            var readRoof = File.ReadAllBytesAsync(ResourceFolder + _ldtkRoomTypeProperties["Roof"]);

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            lines.Wait();
            int[][] collisions = new int[lines.Result.Length][];
            int counter = 0;
            foreach (var line in lines.Result)
            {
                collisions[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            collisions = paddOutline(collisions);
            Collision = new X_CollisionModel_Room(collisions, initiator.TileWidth, initiator.TileHeight);
            Graph = new X_RoomGraph(this, collisions, initiator.TileWidth, initiator.TileHeight);
            Rect = new Rectangle(0, 0, collisions[0].Length * Collision.TileWidth, collisions.Length * Collision.TileHeight);
            ResetRects = new List<Rectangle>();

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            _doorMasks = new Dictionary<X_ConnectorSide, IList<X_DoorMask>>();

            _roofDoor = X_AutoTiler.RoomCoverTexture(
                initiator.Name,
                Util.PathOsNormalization("./Levels/"), 
                "doors.json", Collision.GetCollisionTemplate());

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            Scale = (float)initiator.TileHeight / TextureTileSize;

            List<string> layers;
            //using (StreamReader stream = new StreamReader(ResourceFolder + dataFileName))
            //{

            var json = File.ReadAllTextAsync(ResourceFolder + dataFileName);
            json.Wait();
            dynamic array = JsonConvert.DeserializeObject(json.Result);
            layers = JsonConvert.DeserializeObject<List<string>>(array.layers.ToString());

            var t1 = Task.Run(() =>
            {
                if (array.entities.Door != null)
                {
                    IList<Door> doors = JsonConvert.DeserializeObject<List<Door>>(array.entities.Door.ToString());

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

            _spawner = new List<Point>();
            // assign power ups
            var t2 = Task.Run(() =>
            {
                if (array.entities.Spawner != null)
                {
                    var spawner = JsonConvert.DeserializeObject<List<Spawner>>(array.entities.Spawner.ToString());
                    foreach (var s in spawner)
                    {
                        _spawner.Add(new Point(s.x + Rect.X, s.y + Rect.Y));
                    }
                }
            });

            _bossSpawner = new List<Point>();
            var t3 = Task.Run(() =>
            {
                    if (array.entities.BossSpawner != null)
                {
                    var bossSpawner = JsonConvert.DeserializeObject<List<BossSpawner>>(array.entities.BossSpawner.ToString());
                    foreach (var s in bossSpawner)
                    {
                        _bossSpawner.Add(new Point(s.x + Rect.X, s.y + Rect.Y));
                    }
                }
            });

            _playerSpawner = new List<Point>();
            var t4 = Task.Run(() =>
            {
                if (array.entities.Player != null)
                {
                    var playerSpawner = JsonConvert.DeserializeObject<List<PlayerSpawner>>(array.entities.Player.ToString());
                    foreach (var s in playerSpawner)
                    {
                        _playerSpawner.Add(new Point(s.x + Rect.X, s.y + Rect.Y));
                    }
                }
            });

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            _powerUps = new List<PowerUpItem>();
            var t5 = Task.Run(() =>
            {
                if (array.entities.Life != null)
                {
                    var life = JsonConvert.DeserializeObject<List<Life>>(array.entities.Life.ToString());
                    foreach (var s in life)
                    {
                        Point location = new Point(s.x + Rect.X, s.y + Rect.Y);
                        _powerUps.Add(new PowerUpItem(Y_PowerUp.Factory(Y_PowerUps.Life, location, s.width, s.height, TextureTileSize, Scale, null)));
                    }
                }

                if (array.entities.Revive != null)
                {
                    var revive = JsonConvert.DeserializeObject<List<Revive>>(array.entities.Revive.ToString());
                    foreach (var s in revive)
                    {
                        Point location = new Point(s.x + Rect.X, s.y + Rect.Y);
                        _powerUps.Add(new PowerUpItem(Y_PowerUp.Factory(Y_PowerUps.Revive, location, s.width, s.height, TextureTileSize, Scale, null)));
                    }
                }
            });

            _multiPowerups = new List<MultiPowerUpItem>();
            var t6 = Task.Run(() =>
            {
                if (array.entities.MultiPowerUp != null)
                {
                    var multiPowerUp = JsonConvert.DeserializeObject<List<MultiPowerUp>>(array.entities.MultiPowerUp.ToString());
                    foreach (var s in multiPowerUp)
                    {
                        Point location = new Point(s.x + Rect.X, s.y + Rect.Y);
                        int width = s.width;
                        int height = s.height;
                        _multiPowerups.Add(new MultiPowerUpItem(Y_MultiPowerUp.Factory(Y_MultiPowerUps.Radio, location, width, height, TextureTileSize, Scale, null)));
                    }
                }
            });

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            Task.WaitAll(t1);
            foreach (var door in Doors)
            {
                float sf = 1.5f;
                int b = -2;
                if (door.Key == X_ConnectorSide.Top)
                {
                    var p = Doors[X_ConnectorSide.Top].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X - (X_DoorMask.RoiWidth / 2.0f * TextureTileSize)) / TextureTileSize) * TextureTileSize),
                        (int)((Math.Round(p.Y + sf * TextureTileSize) / TextureTileSize) * TextureTileSize),
                        X_DoorMask.RoiWidth * TextureTileSize, X_DoorMask.RoiDepth * TextureTileSize);

                    _doorMasks.Add(door.Key, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, TextureTileSize)
                        });
                }
                else if (door.Key == X_ConnectorSide.Left)
                {
                    var p = Doors[X_ConnectorSide.Left].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X + sf * TextureTileSize) / TextureTileSize) * TextureTileSize),
                        (int)((Math.Round(p.Y - (X_DoorMask.RoiWidth / 2.0f * TextureTileSize)) / TextureTileSize) * TextureTileSize),
                        X_DoorMask.RoiDepth * TextureTileSize, X_DoorMask.RoiWidth * TextureTileSize);

                    _doorMasks.Add(door.Key, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, TextureTileSize)
                        });
                }
                else if (door.Key == X_ConnectorSide.Right)
                {
                    var p = Doors[X_ConnectorSide.Right].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X - (sf + X_DoorMask.RoiDepth) * TextureTileSize) / TextureTileSize) * TextureTileSize),
                        (int)((Math.Round(p.Y - (X_DoorMask.RoiWidth / 2.0f * TextureTileSize)) / TextureTileSize) * TextureTileSize),
                        X_DoorMask.RoiDepth * TextureTileSize, X_DoorMask.RoiWidth * TextureTileSize);

                    _doorMasks.Add(door.Key, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, TextureTileSize)
                        });
                }
                else // if (door.Key == X_ConnectorSide.Bottom)
                {
                    var p = Doors[X_ConnectorSide.Bottom].First().Point;
                    var roi = new Rectangle(
                        (int)((Math.Round(p.X - (X_DoorMask.RoiWidth / 2.0f * TextureTileSize)) / TextureTileSize) * TextureTileSize),
                        (int)((Math.Round(p.Y - (sf + X_DoorMask.RoiDepth) * TextureTileSize) / TextureTileSize) * TextureTileSize),
                        X_DoorMask.RoiWidth * TextureTileSize, X_DoorMask.RoiDepth * TextureTileSize);

                    _doorMasks.Add(X_ConnectorSide.Bottom, new List<X_DoorMask> {
                            new X_DoorMask(roi, Rect, TextureTileSize)
                        });
                }
            }

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            State = X_RoomState.Closed;

            Lights = new List<X_Light>() {
            new X_Light(
                new Vector3(Rect.X + -5*TextureTileSize,
                Rect.Y - 10*TextureTileSize,
                10 * TextureTileSize),
                Rect, Scale)
            };

            Task.WaitAll(readFloor, readWall, readRoof);
            _floorData = readFloor.Result;
            _wallData = readWall.Result;
            _roofData = readRoof.Result;

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
#endif

            Task.WaitAll(t2, t3, t4, t5, t6);

            // check which power ups are inside multi power ups
            foreach (var mpu in _multiPowerups)
            {

                foreach (var pu in _powerUps)
                {
                    if (pu.Item.Rect.Intersects(mpu.Item.Rect))
                    {
                        pu.MultiPowerUp = mpu;
                    }
                }
            }

#if PARALLEL_DEBUG_ROOM
            dbag.AddOrUpdate(name, 0, (n, v) => v + 1);
            Debug.WriteLine(string.Join(", ", dbag.OrderBy(kvp => kvp.Value).Select(kvp => kvp.ToString())));
#endif
        }

        public void FinalizeItem(GraphicsDevice graphicsDevice)
        {
            var watch = new Stopwatch();
            watch.Start();
            MemoryStream floor = new MemoryStream(_floorData);
            MemoryStream wall = new MemoryStream(_wallData);
            MemoryStream roof = new MemoryStream(_roofData);

            _floor = Texture2D.FromStream(graphicsDevice, floor, DefaultColorProcessors.ZeroTransparentPixels);
            Texture2D txWall = Texture2D.FromStream(graphicsDevice, wall, DefaultColorProcessors.ZeroTransparentPixels);
            _roof = Texture2D.FromStream(graphicsDevice, roof, DefaultColorProcessors.ZeroTransparentPixels);

            Color[] target = new Color[_floor.Width * _floor.Height];
            Color[] source = new Color[txWall.Width * txWall.Height];

            _floor.GetData<Color>(target);
            txWall.GetData<Color>(source);

            for (int h = 0; h < _floor.Height; ++h)
            {
                for (int w = 0; w < _floor.Width; ++w)
                {
                    var c = source[h * _floor.Width + w];
                    if (c.A != 0)
                        target[h * _floor.Width + w] = source[h * _floor.Width + w];
                }
            }

            _floor.SetData<Color>(target);
            _roofDoor.Mechanism1 = new Texture2D(graphicsDevice, TextureTileSize, TextureTileSize);
            _roofDoor.Mechanism1.SetData<Color>(_roofDoor.Mechanism1Data);
            _roofDoor.Mechanism2 = new Texture2D(graphicsDevice, TextureTileSize, TextureTileSize);
            _roofDoor.Mechanism2.SetData<Color>(_roofDoor.Mechanism2Data);
            _roofDoor.Roof = new Texture2D(graphicsDevice, _roof.Width, _roof.Height);
            _roofDoor.Roof.SetData<Color>(_roofDoor.RoofData);

            // if the room is initially open, we need to illuminate it
            if (State == X_RoomState.Open || State == X_RoomState.LockedOpen)
            {
                Illuminate();
            }
        }

        public void ResetRoom()
        {
            var rects = Collision.GetCollisionRectangles().ToList();
            rects.AddRange(ResetRects);
            Collision.UpdateCollisionRectangles(rects);
            DoorRooms.Clear();
            ResetRects.Clear();
            Cleared = false;
            State = X_RoomState.Closed;
            MoveTo(new Point(0, 0));

            for (int i = 0; i < _powerUps.Count(); ++i)
            {
                _powerUps[i].Active = true;
            }

            for (int i = 0; i < _multiPowerups.Count(); ++i)
            {
                _multiPowerups[i].Active = true;
            }
        }

        public void ApplyPowerUps(IVictim player)
        {
            for (int i = 0; i < _powerUps.Count(); ++i)
            {
                if (_powerUps[i].Active && player.Rect.Intersects(_powerUps[i].Item.Rect))
                {
                    _powerUps[i].Active = false;
                    _powerUps[i].Item.Action(player);
                    Manager_Sound.Sound_CashIn.Play();

                    if (_powerUps[i].MultiPowerUp != null && _powerUps[i].MultiPowerUp.Active)
                    {
                        _powerUps[i].MultiPowerUp.Item.Action(_powerUps[i], _powerUps);
                        _powerUps[i].MultiPowerUp.Active = false;
                    }
                    return;
                }
            }
        }

        public void InitRoomOpen()
        {
            State = X_RoomState.Open;
        }

        public void InitRoomClosed()
        {
            State = X_RoomState.Closed;
        }

        public void InitRoomLocked(bool open = false)
        {
            if (open) State = X_RoomState.LockedOpen;
            else State = X_RoomState.LockedClosed;
        }

        public bool IsRoomOpen()
        {
            return State == X_RoomState.Open;
        }

        public bool IsRoomClosed()
        {
            return State == X_RoomState.Closed;
        }

        public bool IsRoomLocked()
        {
            return State == X_RoomState.LockedOpen || State == X_RoomState.LockedClosed;
        }

        public bool LockRoomOpen()
        {
            if(State == X_RoomState.Open || State == X_RoomState.Open)
            {
                State = X_RoomState.LockedOpen;
                return true;
            }
            return false;
        }

        public bool LockRoomClosed()
        {
            if (State == X_RoomState.Closed || State == X_RoomState.Open)
            {
                State = X_RoomState.LockedClosed;
                return true;
            }
            return false;
        }

        public bool UnlockRoom()
        {
            if (State == X_RoomState.LockedOpen)
            {
                State = X_RoomState.Open;
                return true;
            }
            if (State == X_RoomState.LockedClosed)
            {
                State = X_RoomState.Closed;
                return true;
            }
            return false;
        }

        public bool OpenUnlockedRoom()
        {
            if (State == X_RoomState.Closed)
            {
                State = X_RoomState.Opening;
                return true;
            }
            return false;
        }

        public bool CloseUnlockedRoom()
        {
            if(State == X_RoomState.Open)
            {
                State = X_RoomState.Closing;
                return true;
            }
            return false;
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
        public void CloseAllUnlockedRoomDoors(bool lockWhenFinished = false, bool doorsOnly = false)
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (door.CloseUnlockedDoor()){
                            if (lockWhenFinished)
                            {
                                door.LockDoor();
                            }
                            if (!doorsOnly)
                            {
                                foreach (var con in door.DoorRooms)
                                {
                                    foreach (var room in con.Value)
                                    {
                                        if (room == this)
                                        {
                                            continue; // We are already open
                                        }
                                        Y_CMRoom cmroom = (Y_CMRoom)room;
                                        cmroom.CloseUnlockedRoom();
                                        if (lockWhenFinished)
                                        {
                                            cmroom.LockRoomClosed();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                        if (door.OpenUnlockedDoor()){
                            if (lockWhenFinished)
                            {
                                door.LockDoor();
                            }
                            // Now the door hall is open, but the next room is not visible. so let's recurse...
                            foreach (var con in door.DoorRooms)
                            {
                                foreach (var room in con.Value)
                                {
                                    if (room == this)
                                    {
                                        continue; // We are already open
                                    }
                                    Y_CMRoom cmroom = (Y_CMRoom)room;
                                    cmroom.OpenUnlockedRoom();
                                    if (lockWhenFinished)
                                    {
                                        cmroom.LockRoomOpen();
                                    }
                                }
                            }
                        }
                    }
                    else if (walkable.WhatAreYou() == X_LevelElements.Room)
                    {
                        Logger.Error("Found room where there should have been a door");
                    }
                }
            }
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

        public void Illuminate()
        {
            if (!Settings.Lighting) return;

            if (_floorColorData == null)
            {
                _floorColorData = new Color[_floor.Width * _floor.Height];
                _floor.GetData<Color>(_floorColorData);                
            }
            
            Vector3 offset = new Vector3(Rect.Location.X / Scale, Rect.Location.Y / Scale, 0);
            _illumination = Manager_Light2.Illuminate(this, offset);


            Color[] data = new Color[_floor.Width * _floor.Height];
            Parallel.For(0, _illumination.Length, i =>
            //for (int i = 0; i < _illuminatedClosed.Length; ++i)
            {
                bool illuminated = _illumination[i];

                if (!illuminated)
                {
                    var col = _floorColorData[i];
                    Color nCol = Color.White;
                    nCol.R = (byte)((1 - 0.4f) * col.R + 0.4f * Color.Black.R);
                    nCol.G = (byte)((1 - 0.4f) * col.G + 0.4f * Color.Black.G);
                    nCol.B = (byte)((1 - 0.4f) * col.B + 0.4f * Color.Black.B);
                    data[i] = nCol;
                }
                else
                {
                    data[i] = _floorColorData[i];
                }
            });
            _floor.SetData<Color>(data);
        }

        private void drawFloor(SpriteBatch spriteBatch)
        {
            lock (this)
            {
                spriteBatch.Draw(
                _floor, Rect.Location.ToVector2(),
                new Rectangle(0, 0, _floor.Width, _floor.Height),
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
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

            foreach(var powerUp in _powerUps)
            {
                powerUp.Item.MoveBy(p);
            }

            foreach (var powerUp in _multiPowerups)
            {
                powerUp.Item.MoveBy(p);
            }

            // needs to be done this way because properties return by value and not by ref
            Rect = new Rectangle(position.X, position.Y, Rect.Width, Rect.Height);

            if (_spawner != null)
            {
                for (int i = 0; i < _spawner.Count; ++i)
                {
                    _spawner[i] += p;
                }
            }

            if (_bossSpawner != null)
            {
                for (int i = 0; i < _bossSpawner.Count; ++i)
                {
                    _bossSpawner[i] += p;
                }
            }

            if (_playerSpawner != null)
            {
                for (int i = 0; i < _playerSpawner.Count; ++i)
                {
                    _playerSpawner[i] += p;
                }
            }
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side)
        {
            return Doors[side].First();
        }

        private bool doorAnimation(float dt, bool opening)
        {
            _animationTime += dt;
            if (_animationTime < _doorOpeningTime)
            {
                float percent = 1.0f / _doorOpeningTime * _animationTime;
                if (opening)
                    _currentDoorOpenOffset = (int)Math.Round(TextureTileSize * percent);
                else
                    _currentDoorOpenOffset = (int)Math.Round(TextureTileSize * (1.0f - percent));
            }
            if (_animationTime >= _doorOpeningTime)
            {
                if (opening)
                    _currentDoorOpenOffset = TextureTileSize;
                else
                    _currentDoorOpenOffset = 0;
                _animationTime = 0;
                return false;
            }
            return true;
        }

        public List<Point> GetRegularSpawningPoints()
        {
            return _spawner;
        }

        public List<Point> GetBossSpawningPoints()
        {
            return _bossSpawner;
        }

        public List<Point> GetPlayerSpawningPoints()
        {
            return _playerSpawner;
        }

        public List<IVictim> GetPlayersInside()
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
            // Basic room clear logic: If players are in the room but no enemies, open up the doors
            // if (!Cleared && Name != "Start_0")
            // {
            //     bool playersInRoom = false;

            //     if (playersInRoom)
            //     {
            //         bool enemiesInRoom = false;
            //         foreach (var enemy in Manager_Enemies.GetEnemies())
            //         {
            //             if (enemy.Room == this)
            //             {
            //                 enemiesInRoom = true;
            //                 break;
            //             }
            //         }
            //         if (!enemiesInRoom)
            //         {
            //             /* Room cleared */
            //             Cleared = true;
            //             OpenDoorsAndAdjacentRooms();
            //             Manager_Sound.Sound_LevelCleared.Play(1, 0, 0);
            //             Logger.Info("Room " + Name + " cleared");
            //         }
            //     }
            // }



            //bool keyPressed = Input.IsKeyTriggered(Keybinds.ToggleConnectors);
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            switch (State)
            {
                case X_RoomState.Closed:
                    //if (keyPressed)
                    //{
                    //    State = X_RoomState.Opening;
                    //}
                    break;
                case X_RoomState.LockedOpen:
                    if (!doorAnimation(dt, true))
                    {
                        Illuminate();
                    }
                    break;
                case X_RoomState.Opening:
                    if (!doorAnimation(dt, true))
                    {
                        State = X_RoomState.Open;
                        Illuminate();
                    }
                    break;
                case X_RoomState.Open:
                    //if (keyPressed)
                    //{
                    //    State = X_RoomState.Closing;
                    //}
                    break;
                case X_RoomState.Closing:
                    if (!doorAnimation(dt, false))
                    {
                        State = X_RoomState.Closed;
                        Illuminate();
                    }
                    break;
                case X_RoomState.LockedClosed:
                    if (!doorAnimation(dt, false))
                    {
                        Illuminate();
                    }
                    break;
            }

            foreach(var powerUp in _powerUps)
            {
                if(powerUp.Active)
                    powerUp.Item.Update(gameTime);
            }

            foreach (var powerUp in _multiPowerups)
            {
                if (powerUp.Active)
                    powerUp.Item.Update(gameTime);
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

            foreach (var powerUp in _powerUps)
            {
                if (powerUp.Active == true)
                {
                    powerUp.Item.DrawOutline(gameTime, globalOffset, spriteBatch);
                }
            }

            foreach (var mpu in _multiPowerups)
            {
                if (mpu.Active == true)
                {
                    mpu.Item.DrawOutline(gameTime, globalOffset, spriteBatch);
                }
            }
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Culling
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty)
            {
                return;
            }

            if (State == X_RoomState.Closed || State == X_RoomState.LockedClosed)
            {
                //spriteBatch.Draw(
                //    _floor, Rect.Location.ToVector2(),
                //    new Rectangle(0, 0, _floor.Width, _floor.Height),
                //    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                //Vector2 position = Rect.Location.ToVector2();
                //draw(_tileTextures[X_DoorTextureLayer.Door], position, spriteBatch, false);

                //spriteBatch.Draw(
                //    _roof, Rect.Location.ToVector2(),
                //    new Rectangle(0, 0, _floor.Width, _floor.Height),
                //    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            else if (State == X_RoomState.Opening || State == X_RoomState.Closing)
            {
                spriteBatch.Draw(
                    _floor, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                Vector2 position = Rect.Location.ToVector2();
                Vector2 movePosition = position;
                movePosition.Y += _currentDoorOpenOffset;

                spriteBatch.Draw(
                    _roofDoor.Roof, movePosition,
                    new Rectangle(0, 0, _roofDoor.Roof.Width, _roofDoor.Roof.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                for(int i=0; i<_roofDoor.Mechanism1Positions.Count(); ++i)
                {
                    if (_currentDoorOpenOffset % 2 == 0)
                    {
                        spriteBatch.Draw(
                            _roofDoor.Mechanism1, position + _roofDoor.Mechanism1Positions[i],
                            new Rectangle(0, 0, TextureTileSize, TextureTileSize),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.Draw(
                            _roofDoor.Mechanism2, position + _roofDoor.Mechanism2Positions[i],
                            new Rectangle(0, 0, TextureTileSize, TextureTileSize),
                            Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    }
                }

                spriteBatch.Draw(
                    _roof, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(
                    _floor, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                spriteBatch.Draw(
                    _roof, Rect.Location.ToVector2(),
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);

                foreach (var powerUp in _powerUps)
                {
                    if(powerUp.Active == true)
                    {
                        powerUp.Item.Draw(gameTime, globalOffset, spriteBatch);
                    }
                }

                foreach (var powerUp in _multiPowerups)
                {
                    if (powerUp.Active == true)
                    {
                        powerUp.Item.Draw(gameTime, globalOffset, spriteBatch);
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
