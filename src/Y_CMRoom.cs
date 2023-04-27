using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace YGR
{
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

            private void output(int[][] pattern, string name)
            {
                string s = "";
                for (int x = 0; x < pattern.GetLength(0); ++x)
                {
                    s += string.Join("\t", pattern[x]) + "\n";
                }

                File.WriteAllText(name, s);
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
        public List<X_Light> Lights;
        public X_RoomState State { get; set; }

        public List<Rectangle> ResetRects { get; }

        private Texture2D _floor;
        private Texture2D _roof;
        private Color[] _floorData;
        private bool[] _illuminatedOpened;
        private bool[] _illuminatedClosed;

        private int _currentDoorOpenOffset = 0;
        private float _doorOpeningTime = 2000.0f;
        private float _animationTime = 0.0f;

        Dictionary<X_DoorTextureLayer, List<X_AutoTiler.X_AutoTileTexture>> _tileTextures;
        Dictionary<X_DoorTextureLayer, List<X_AutoTiler.X_AutoTileColor>> _tileColors;

        List<Point> _spawner;
        List<Point> _bossSpawner;
        List<Point> _playerSpawner;

        bool _cleared;

        public Y_CMRoom(
            string name,
            int tileWidth,
            int tileHeight,
            string resourceFolder,
            GraphicsDevice graphicsDevice
        )
        {
            ResourceFolder = Util.PathOsNormalization(resourceFolder);
            var files = Directory.GetFiles(ResourceFolder);
            string dataFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("data") && Path.GetFileName(x).EndsWith(".json")).First());

            string collisionsFileName = Path.GetFileName(files.Where(x => Path.GetFileName(x).Contains("Collision") && Path.GetFileName(x).EndsWith(".csv")).First());
            string[] lines = File.ReadAllLines(ResourceFolder + collisionsFileName);
            int[][] collisions = new int[lines.Length][];
            int counter = 0;
            foreach (var line in lines)
            {
                collisions[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

            collisions = paddOutline(collisions);
            Collision = new X_CollisionModel_Room(collisions, tileWidth, tileHeight);
            Graph = new X_RoomGraph(this, collisions, tileWidth, tileHeight);
            Rect = new Rectangle(0, 0, collisions[0].Length * Collision.TileWidth, collisions.Length * Collision.TileHeight);
            ResetRects = new List<Rectangle>();

            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            _doorMasks = new Dictionary<X_ConnectorSide, IList<X_DoorMask>>();

            X_AutoTiler.Resolve<X_DoorTextureLayer>(
                Util.PathOsNormalization("./Levels/"), "data.json",
                graphicsDevice,
                Collision.GetCollisionTemplate(),
                MapTexture,
                out _tileTextures, out _tileColors);

            List<string> layers;
            using (StreamReader stream = new StreamReader(ResourceFolder + dataFileName))
            {
                string json = stream.ReadToEnd();
                dynamic array = JsonConvert.DeserializeObject(json);
                layers = JsonConvert.DeserializeObject<List<string>>(array.layers.ToString());
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
                if (array.entities.Spawner != null)
                {
                    var spawner = JsonConvert.DeserializeObject<List<Spawner>>(array.entities.Spawner.ToString());
                    _spawner = new List<Point>();
                    foreach (var s in spawner)
                    {
                        _spawner.Add(new Point(s.x + Rect.X, s.y + Rect.Y));
                    }
                }
                if (array.entities.BossSpawner != null)
                {
                    var bossSpawner = JsonConvert.DeserializeObject<List<BossSpawner>>(array.entities.BossSpawner.ToString());
                    _bossSpawner = new List<Point>();
                    foreach (var s in bossSpawner)
                    {
                        _bossSpawner.Add(new Point(s.x + Rect.X, s.y + Rect.Y));
                    }
                }
                if (array.entities.Player != null)
                {
                    var playerSpawner = JsonConvert.DeserializeObject<List<PlayerSpawner>>(array.entities.Player.ToString());
                    _playerSpawner = new List<Point>();
                    foreach (var s in playerSpawner)
                    {
                        _playerSpawner.Add(new Point(s.x + Rect.X, s.y + Rect.Y));
                    }
                }
            }

            Name = name;
            Color[] target = null;
            for (int i = 0; i < layers.Count - 1; ++i)
            {
                using (FileStream fileStream = new FileStream(ResourceFolder + layers[i], FileMode.Open))
                {
                    if (i == 0)
                    {
                        _floor = Texture2D.FromStream(graphicsDevice, fileStream);
                        target = new Color[_floor.Width * _floor.Height];
                        _floor.GetData<Color>(target);
                    }
                    else
                    {
                        var t = Texture2D.FromStream(graphicsDevice, fileStream);
                        Color[] source = new Color[_floor.Width * _floor.Height];
                        Texture2D.FromStream(graphicsDevice, fileStream).GetData<Color>(source);

                        for (int h = 0; h < _floor.Height; ++h)
                        {
                            for (int w = 0; w < _floor.Width; ++w)
                            {
                                var c = source[h * _floor.Width + w];
                                if (c.A != 0)
                                    target[h * _floor.Width + w] = source[h * _floor.Width + w];
                            }
                        }
                    }
                }
            }
            _floor.SetData<Color>(target);

            using (FileStream fileStream = new FileStream(ResourceFolder + layers[layers.Count() - 1], FileMode.Open))
            {
                _roof = Texture2D.FromStream(graphicsDevice, fileStream);
            }

            TextureTileSize = _floor.Height / collisions.Length;

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

            State = X_RoomState.Closed;
            Scale = (float)tileHeight * collisions.Length / _floor.Height;
            _illuminatedOpened = null;
            _illuminatedClosed = null;

            Lights = new List<X_Light>() {
            new X_Light(
                new Vector3(Rect.X + -5*TextureTileSize,
                Rect.Y - 10*TextureTileSize,
                10 * TextureTileSize),
                Rect, Scale)
            };
        }
        private void output(int[][] pattern, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.GetLength(0); ++x)
            {
                s += string.Join("\t", pattern[x]) + "\n";
            }

            File.WriteAllText(name, s);
        }

        public void ResetRoom()
        {
            var rects = Collision.GetCollisionRectangles().ToList();
            rects.AddRange(ResetRects);
            Collision.UpdateCollisionRectangles(rects);
            DoorRooms.Clear();
            ResetRects.Clear();
        }

        public void OpenDoorsAndAdjacentRooms()
        {
            foreach (var side in DoorRooms)
            {
                foreach (var walkable in side.Value)
                {
                    if (walkable.WhatAreYou() == X_LevelElements.Door)
                    {
                        Y_Door door = (Y_Door)walkable;
                        if (door.State == X_DoorState.Closed)
                        {
                            door.State = X_DoorState.Opening;
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
                                if (cmroom.State == X_RoomState.Closed)
                                {
                                    cmroom.State = X_RoomState.Opening;
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

        public void PreloadIlluminations()
        {
            _illuminatedClosed = Manager_Light.LoadShadeFromFile(false, this, Lights);
            //_illuminatedOpened = Manager_Light.LoadShadeFromFile(true, this, Lights);
        }

        public void Illuminate()
        {
            if (!Settings.Lighting) return;

            if (_floorData == null)
            {
                Vector3 offset = new Vector3(Rect.Location.X / Scale, Rect.Location.Y / Scale, 0);

                if (_illuminatedClosed == null)
                {
                    //Logger.Info(" ...Recalculated closed illumination... ");
                    _illuminatedClosed = Manager_Light.Illuminate(Lights, Collision.GetCollisionTemplate(), TextureTileSize, offset, false);
                    Manager_Light.RemoveAllShadeFiles(this, false);
                    Manager_Light.SaveShadeToFile(_illuminatedClosed, false, this, Lights);
                }

                //if (_illuminatedOpened == null)
                //{
                //    //Logger.Info(" ...Recalculated opened illumination... ");
                //    _illuminatedOpened = Manager_Light.Illuminate(Lights, Collision.GetCollisionTemplate(), TextureTileSize, offset, true);
                //    Manager_Light.RemoveAllShadeFiles(this, true);
                //    Manager_Light.SaveShadeToFile(_illuminatedOpened, true, this, Lights);
                //}

                _illuminatedOpened = _illuminatedClosed.Clone() as bool[];
                foreach (var d in DoorRooms)
                {
                    var door = (Y_Door)d.Value.First(); //DoorRooms[mask.Key].First();
                    var room = door.GetOtherDoor(this);
                    var template = _doorMasks[room.Item1].First().GetTemplate();

                    var illumination = Manager_Light.Illuminate(room.Item2.Lights, template /*Collision.GetCollisionTemplate()*/, TextureTileSize, offset, true);//, Manager_Light.Caster.Light);

                    Parallel.For(0, illumination.Length, i =>
                    //for (int i = 0; i < illumination.Length; ++i)
                    {
                        if (illumination[i] && _doorMasks[room.Item1].First().Check(i))
                            _illuminatedOpened[i] = illumination[i];
                    });
                }

                _floorData = new Color[_floor.Width * _floor.Height];
                _floor.GetData<Color>(_floorData);
            }

            var rooms = DoorRooms.Select(x => (Y_Door)x.Value.First()).ToArray();
            var keys = DoorRooms.Select(x => x.Key).ToArray();

            Color[] data = new Color[_floor.Width * _floor.Height];


            Parallel.For(0, _illuminatedClosed.Length, i =>
            //for (int i = 0; i < _illuminatedClosed.Length; ++i)
            {
                bool illuminated = _illuminatedClosed[i];
                if (!illuminated)
                {
                    for (int r = 0; r < rooms.Length; ++r)
                    {
                        if (rooms[r].DoorIsOpen())
                        {
                            var check = _doorMasks[keys[r]].First().Check(i);
                            illuminated = illuminated || (check && _illuminatedOpened[i]);
                        }
                    }
                }

                if (!illuminated)
                {
                    var col = _floorData[i];
                    Color nCol = Color.White;
                    nCol.R = (byte)((1 - 0.4f) * col.R + 0.4f * Color.Black.R);
                    nCol.G = (byte)((1 - 0.4f) * col.G + 0.4f * Color.Black.G);
                    nCol.B = (byte)((1 - 0.4f) * col.B + 0.4f * Color.Black.B);
                    data[i] = nCol;
                }
                else
                {
                    data[i] = _floorData[i];
                }
            });
            _floor.SetData<Color>(data);
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

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {
            // Basic room clear logic: If players are in the room but no enemies, open up the doors
            if (!_cleared && Name != "Start_0")
            {
                bool playersInRoom = false;
                foreach (var player in Manager_Players.Players)
                {
                    if (player.Room == this)
                    {
                        playersInRoom = true;
                        break;
                    }
                }
                if (playersInRoom)
                {
                    bool enemiesInRoom = false;
                    foreach (var enemy in Manager_Enemies.GetEnemies())
                    {
                        if (enemy.Room == this)
                        {
                            enemiesInRoom = true;
                            break;
                        }
                    }
                    if (!enemiesInRoom)
                    {
                        _cleared = true;
                        OpenDoorsAndAdjacentRooms();
                        Logger.Info("Room " + Name + " cleared");
                    }
                }
            }



            bool keyPressed = Input.IsKeyTriggered(Keybinds.ToggleConnectors);
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            switch (State)
            {
                case X_RoomState.Closed:
                    if (keyPressed)
                    {
                        State = X_RoomState.Opening;
                    }
                    break;
                case X_RoomState.Opening:
                    if (!doorAnimation(dt, true))
                    {
                        State = X_RoomState.Open;
                        foreach (var room in DoorRooms.Values) room.First().Illuminate();
                    }
                    break;
                case X_RoomState.Open:
                    if (keyPressed)
                    {
                        State = X_RoomState.Closing;
                    }
                    break;
                case X_RoomState.Closing:
                    if (!doorAnimation(dt, false))
                    {
                        State = X_RoomState.Closed;
                        foreach (var room in DoorRooms.Values) room.First().Illuminate();
                    }
                    break;
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
        }

        private void draw(
            List<X_AutoTiler.X_AutoTileTexture> textures,
            Vector2 position,
            SpriteBatch spriteBatch,
            bool partial)
        {
            foreach (var t in textures)
            {
                int height = t.Texture().Height;
                int width = t.Texture().Width;
                Vector2 pos = position + t.Location().ToVector2() * TextureTileSize;

                spriteBatch.Draw(
                    t.Texture(), pos,
                    new Rectangle(0, 0, width, height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
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
                draw(_tileTextures[X_DoorTextureLayer.Door], movePosition, spriteBatch, false);
                if (_currentDoorOpenOffset % 2 == 0)
                    draw(_tileTextures[X_DoorTextureLayer.Mechanism1], position, spriteBatch, false);
                else
                    draw(_tileTextures[X_DoorTextureLayer.Mechanism2], position, spriteBatch, false);

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
