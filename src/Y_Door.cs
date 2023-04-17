using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Assimp.Unmanaged;

namespace YGR
{
    public enum X_DoorDirection
    {
        Horizontal = 0,
        Vertical
    }

    public enum X_TileType
    {
        Roof=2,
        Wall,
        Floor,
        Outside
    }

    public enum X_DoorState
    {
        Open = 1,
        Opening,
        Closing,
        Closed,
        LockedClosed,
        LockedOpen
    }

    public class Y_Door : IWalkable
    {
        public float Scale { get; private set; }
        public Rectangle Rect { get; set; }
        public string Name { get; }
        public X_CollisionModel_Room Collision { get; }
        public X_RoomGraph Graph { get; set; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }
        public int TextureTileSize { get; }

        private new Dictionary<X_DoorState, List<Rectangle>> _doorCollisionRectangles;

        private X_DoorState _state;

        private X_DoorDirection _direction;
        private Texture2D _floor;
        private Texture2D _roof;
        private Texture2D _door;
        const int _numTilesDoorWidth = 5;

        private int _currentDoorOpenOffset = 0;
        private float _doorOpeningTime = 2000.0f;
        private float _animationTime = 0.0f;
        int _tileSize;

        //private Y_Door _door;


        //Rectangle _outsideRect1;
        //Rectangle _outsideRect2;

        public Y_Door(
            X_DoorDirection direction, 
            int numTilesLength, 
            int tileWidth, 
            int tileHeight, 
            int tileOffset,
            GraphicsDevice graphicsDevice
            )
        {
            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);

            collision = flipToPosition(collision, direction, tileOffset);

            collision = getDoorPoints(collision, tileWidth, tileHeight);

            _direction = direction;
            
            Collision = new X_CollisionModel_Room(collision, tileWidth, tileHeight);

            Rect = new Rectangle(0, 0, tileWidth * collision[0].Length, tileHeight * collision.Length);

            Scale = 1.0f;

            _doorCollisionRectangles = new Dictionary<X_DoorState, List<Rectangle>>();

            Texture2D roof;
            Texture2D floor;
            Texture2D wall;

            using (FileStream fileStream = new FileStream("./Doors/roof.png", FileMode.Open))
                roof = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/floor.png", FileMode.Open))
                floor = Texture2D.FromStream(graphicsDevice, fileStream);
            using (FileStream fileStream = new FileStream("./Doors/wall.png", FileMode.Open))
                wall = Texture2D.FromStream(graphicsDevice, fileStream);

            TextureTileSize = wall.Width;

            Color[] roofA;
            Color[] floorA;
            Color[] wallA;

            Dictionary<X_TileType, Color[]> textels = new Dictionary<X_TileType, Color[]>();

            int len = roof.Width * roof.Height;
            /* 1 */
            roofA = new Color[len];
            roof.GetData<Color>(roofA);
            textels.Add(X_TileType.Roof, roofA);

            /* 2 */
            wallA = new Color[len];
            wall.GetData<Color>(wallA);
            textels.Add(X_TileType.Wall, wallA);

            /* 3 */
            floorA = new Color[len];
            floor.GetData<Color>(floorA);
            textels.Add(X_TileType.Floor, floorA);

            int width = floor.Width;
            int height = floor.Height;
            var pattern = Collision.GetCollisionTemplate();
            //createOutsideRects(pattern, tileWidth);
            _floor = new Texture2D(graphicsDevice, width * pattern[0].Length, height * pattern.Length);
            _roof = new Texture2D(graphicsDevice, width * pattern[0].Length, height * pattern.Length);
            _door = new Texture2D(graphicsDevice, width * pattern[0].Length, height * pattern.Length);
            Scale = (float)tileHeight / height;

            Color[] transparent = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
            for (int x = 0; x < pattern[0].Length; ++x)
            {
                for (int y = 0; y < pattern.Length; ++y)
                {
                    var index = pattern[y][x];
                    Color[] floorElem;
                    Color[] roofElem;
                    Color[] doorElem;
                    if ((X_TileType)index != X_TileType.Outside)
                    {
                        if((X_TileType)index == X_TileType.Roof)
                            roofElem = textels[(X_TileType)index];
                        else
                            roofElem = transparent;
                        if ((X_TileType)index == X_TileType.Floor || (X_TileType)index == X_TileType.Wall)
                            floorElem = textels[(X_TileType)index];
                        else
                            floorElem = transparent;
                    }
                    else
                    {
                        floorElem = transparent;
                        roofElem = transparent;
                    }
                    if ((X_TileType)index != X_TileType.Outside && (X_TileType)index != X_TileType.Roof)
                    {
                        if (x < 1 || y < 1 || x > pattern[0].Length - 2)
                            doorElem = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
                        else if(y > pattern.Length-2)
                            doorElem = transparent;
                        else
                            doorElem = textels[X_TileType.Roof];
                    }
                    else
                    {
                        doorElem = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
                    }
                    Rectangle rect = new Rectangle(x * width, y * height, width, height);

                    //var elem = Enumerable.Repeat<Color>(Color.Transparent, height * width).ToArray();
                    _floor.SetData(0, rect, floorElem, 0, floorElem.Length);
                    _door.SetData(0, rect, doorElem, 0, doorElem.Length);
                    _roof.SetData(0, rect, roofElem, 0, roofElem.Length);
                }
            }

            _tileSize = (int)(TextureTileSize * Scale);
            _state = X_DoorState.Closed;
        }

        private void output(int[][] pattern, string name)
        {
            string s = "";
            using (StreamReader reader = new StreamReader(name))
            {
                s = reader.ReadToEnd();
            }
            s += "\n\n";
            using (StreamWriter writer = new StreamWriter(name))
            {
                for (int x = 0; x < pattern.GetLength(0); ++x)
                {
                    s += string.Join("\t", pattern[x]) + "\n";
                }
                writer.WriteLine(s);
            }
        }

        public ref Texture2D GetFloor()
        {
            return ref _floor;
        }

        private IList<Tuple<int, int>> findMatch(int[,] match, int[][] pattern, int ox, int oy)
        {
            int lx = match.GetLength(0);
            int ly = match.GetLength(1);
            int px = pattern.Length - lx + 1;
            int py = pattern[0].Length - ly + 1;
            IList<Tuple<int, int>> matchPos = new List<Tuple<int, int>>();

            for (int x = 0; x < px; ++x)
            {
                for (int y = 0; y < py; ++y)
                {
                    for (int mx = 0; mx < lx; ++mx)
                    {
                        for (int my = 0; my < ly; ++my)
                        {
                            var p = pattern[x + mx][y + my];
                            var m = match[mx, my];
                            if (p != m && !(p==-1 && m==0)) goto no_match;
                        }
                    }
                    matchPos.Add(new Tuple<int, int>(x+ox, y+oy));
                    no_match: continue;
                }
            }

            return matchPos;
        }

        private int[][] getDoorPoints(int[][] collision, int tileWidth, int tileHeight)
        {
            Doors = new Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>>();
            DoorRooms = new Dictionary<X_ConnectorSide, IList<IWalkable>>();
            for (int x = 0; x < collision.Length; ++x)
            {
                for (int y = 0; y < collision[0].Length; ++y)
                {
                    var val = collision[x][y];
                    if (val == 2 || val == 4)
                    {
                        var side = (val == 2) ? X_ConnectorSide.Bottom : X_ConnectorSide.Left;
                        Doors.Add(side, new List<X_ConnectorPoint>{ new X_ConnectorPoint(side, new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2))});
                        collision[x][y] = 0;
                    }
                    if (val == 3 || val == 5)
                    {
                        var side = (val == 3) ? X_ConnectorSide.Top : X_ConnectorSide.Right;
                        Doors.Add(side, new List<X_ConnectorPoint> { new X_ConnectorPoint(side, new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2)) });
                        collision[x][y] = 0;
                    }
                }
            }

            return collision;
        }

        private int[][] flipToPosition(int[][] collision, X_DoorDirection direction, int tileOffset)
        {
            int[][] newCol = null;
            if (direction == X_DoorDirection.Vertical)
            {
                if(tileOffset < 0)
                {
                    // No need to do anything. This is the generation case
                    newCol = collision;
                }
                else
                {
                    // flip horizontally
                    newCol = new int[collision.Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision[0].Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            newCol[x][newCol[0].Length - 1 - y] = collision[x][y];
                        }
                    }
                }
            }
            else
            {
                if(tileOffset < 0)
                {
                    // rotate by 90 degrees clock-wise
                    newCol = new int[collision[0].Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision.Length];
                    }
                    for (int x = 0; x < newCol[0].Length; ++x)
                    {
                        for (int y = 0; y < newCol.Length; ++y)
                        {
                            var val = collision[x][y];
                            if (val == 2) val = 4;
                            else if (val == 3) val = 5;
                            newCol[y][newCol[0].Length - 1 - x] = val;
                        }
                    }
                }
                else
                {
                    // rotate by 90 degrees clock-wise
                    // then flip vertically
                    newCol = new int[collision[0].Length][];
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        newCol[x] = new int[collision.Length];
                    }
                    for (int x = 0; x < newCol.Length; ++x)
                    {
                        for (int y = 0; y < newCol[0].Length; ++y)
                        {
                            var val = collision[y][x];
                            if (val == 2) val = 4;
                            else if (val == 3) val = 5;
                            newCol[newCol.Length - 1 - x][newCol[0].Length - 1 - y] = val;
                        }
                    }
                }
            }

            return newCol;
        }

        private int[][] createDoorTemplate(int numTilesLength, int tileOffset)
        {
            int width = numTilesLength;
            int doorWidth = _numTilesDoorWidth;
            int height = doorWidth + Math.Abs(tileOffset);

            int[][] collision = new int[width][];
            for (int x = 0; x < width; ++x)
            {
                collision[x] = new int[height];
                for (int y = 0; y < height; ++y)
                {
                    collision[x][y] = 0;
                }
            }

            int half1 = (int)Math.Floor((float)width / 2.0f) + 2;

            for (int x = 1; x < half1; ++x)
            {
                collision[x][0] = 1;
            }

            for (int y = doorWidth; y < collision[0].Length; ++y)
            {
                collision[0][y] = -1;
                collision[1][y] = -1;
            }
            for (int x = 1; x < half1 - doorWidth; ++x)
            {
                collision[x][doorWidth - 1] = 1;
                for(int y=doorWidth; y < collision[0].Length; ++y) collision[x][y] = -1;
            }

            for (int x = half1-doorWidth; x < width-1; ++x)
            {
                collision[x][height - 1] = 1;
            }

            for (int x = half1; x<width-1; ++x)
            {
                collision[x][Math.Abs(tileOffset)] = 1;
                for (int y = 0; y <= Math.Abs(tileOffset)-1; ++y) collision[x][y] = -1;
            }
            for (int y = 0; y <= Math.Abs(tileOffset) - 1; ++y) {
                collision[width - 1][y] = -1;
                collision[width - 2][y] = -1;
            } 

            for (int y = 0; y < Math.Abs(tileOffset); ++y)
            {
                collision[half1 - 1][y + 1] = 1;
                collision[half1 - doorWidth][height - y - 2] = 1;
            }
            for (int y = 0; y < collision[0].Length; ++y)
            {
                if (collision[1][y] != 0) collision[0][y] = -1;
                if (collision[width - 2][y] != 0) collision[width - 1][y] = -1;
            }
            //for (int y = 0; y < collision[0].Length; ++y)
            //{
            //    collision[0][y] = -1;
            //    collision[width - 1][y] = -1;
            //}

            collision[width-2][height-1-doorWidth/2] = 2;
            collision[1][doorWidth/2] = 3;

            return collision;
        }

        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
        {
            if (!Doors.ContainsKey(side)) return null;
            return Doors[side].First();
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms</para>
        /// <para>The connector is in the middle</para>
        /// 
        /// <br>-------------------- </br>
        /// <br>left |||XXX||| right </br>
        /// <br>-------------------- </br>
        ///  
        /// <para>OR</para>
        /// 
        /// <br>  top   </br>
        /// <br> |||||  </br>
        /// <br> |XXX|  </br>
        /// <br> |||||  </br>
        /// <br> bottom </br>
        /// 
        /// <para>The method acts out of the PERSPECTIVE OF THE CONNECTOR!</para>
        /// 
        /// <para>side1 = X_ConnectorSide.Left means, that room1 is on the left side of the connector.</para>
        /// </summary>
        /// <param name="side1">The origin side: the connector will place the other side with respect to this side</param>
        /// <param name="from">The Y_Room on side1 (the origin side)</param>
        /// <param name="side2">The adjusted side: the connector will place this side with respect to the other side</param>
        /// <param name="to">The Y_Room on side 2 (the adjusted side)</param>
        /// <param name="random">Random generator for the random connector point selection</param>
        /// <returns></returns>
        public IWalkable Connect(
            X_ConnectorSide side1,
            IWalkable from,
            X_ConnectorSide side2,
            IWalkable to
        )
        {
            /*
             * If we use 
             *    connect(Left, room1, Right, room2)
             * we have to FLIP the sides for the next call because for the left side of the connector
             * we need a point on the right side of room 1 and for the right side of the connector, we need
             * a point on the left side of room 2!!!!!!!
             */

            if (side1 == X_ConnectorSide.Left) side1 = X_ConnectorSide.Right;
            else if (side1 == X_ConnectorSide.Right) side1 = X_ConnectorSide.Left;
            else if (side1 == X_ConnectorSide.Top) side1 = X_ConnectorSide.Bottom;
            else if (side1 == X_ConnectorSide.Bottom) side1 = X_ConnectorSide.Top;

            if (side2 == X_ConnectorSide.Left) side2 = X_ConnectorSide.Right;
            else if (side2 == X_ConnectorSide.Right) side2 = X_ConnectorSide.Left;
            else if (side2 == X_ConnectorSide.Top) side2 = X_ConnectorSide.Bottom;
            else if (side2 == X_ConnectorSide.Bottom) side2 = X_ConnectorSide.Top;

            return connect(from, from.GetConnectorPoint(side1), to, to.GetConnectorPoint(side2));
        }

        /// <summary>
        /// <para>This method will connect two Y_Rooms</para>
        /// <para>The connector is in the middle</para>
        /// 
        /// <br>-------------------- </br>
        /// <br>left |||XXX||| right </br>
        /// <br>-------------------- </br>
        ///  
        /// <para>OR</para>
        /// 
        /// <br>  top   </br>
        /// <br> |||||  </br>
        /// <br> |XXX|  </br>
        /// <br> |||||  </br>
        /// <br> bottom </br>
        /// 
        /// <para>The method acts out of the PERSPECTIVE OF THE ROOMS!</para>
        /// 
        /// <para>If room1 is on the LEFT side of the connector, then the connectorPoint1 must be on the RIGHT side of room1.</para>
        /// </summary>
        /// <param name="room1">Y_Room with connectorPoint1</param>
        /// <param name="connectorPoint1">The connector point on Y_Room 1</param>
        /// <param name="room2">Y_Room with connectorPoint2</param>
        /// <param name="connectorPoint2">The connector point on Y_Room 2</param>
        /// <returns></returns>
        private IWalkable connect(
            IWalkable room1,
            X_ConnectorPoint connectorPoint1,
            IWalkable room2,
            X_ConnectorPoint connectorPoint2
        )
        {
            // check possibilities
            // horizontal: left and right or right and left
            // vertical: top and bottom or bottom and top
            bool hCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Left && connectorPoint2.ConnectorSide == X_ConnectorSide.Right;
            bool hCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Right && connectorPoint2.ConnectorSide == X_ConnectorSide.Left;
            bool vCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Bottom && connectorPoint2.ConnectorSide == X_ConnectorSide.Top;
            bool vCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Top && connectorPoint2.ConnectorSide == X_ConnectorSide.Bottom;

            // horizontal connectors only with left and right or right and left
            // vertical connectors only with top and bottom or bottom and top
            if (!(
                ((hCheck1 || hCheck2) && _direction == X_DoorDirection.Horizontal) ||
                ((vCheck1 || vCheck2) && _direction == X_DoorDirection.Vertical))
                )
            {
                Logger.Error("Invalid connector combination");
            }

            /* left to right */
            if (hCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Right].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Left].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* right to left */
            else if (hCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Left].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Right].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* bottom to top */
            else if (vCheck1)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Top].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Bottom].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }
            /* top to bottom */
            else if (vCheck2)
            {
                var deltaPC = Rect.Location - Doors[X_ConnectorSide.Bottom].First().Point;
                MoveTo(connectorPoint1.Point + deltaPC);
                room2.MoveTo(Doors[X_ConnectorSide.Top].First().Point + room2.Rect.Location - connectorPoint2.Point);
            }

            // the following flip of connectorPoint2 and connectorPoint1 is NOT a bug
            DoorRooms.Add(connectorPoint2.ConnectorSide, new List<IWalkable> { room1 });
            DoorRooms.Add(connectorPoint1.ConnectorSide, new List<IWalkable> { room2 });
            room1.DoorRooms.Add(connectorPoint2.ConnectorSide, new List<IWalkable> { this });
            room2.DoorRooms.Add(connectorPoint1.ConnectorSide, new List<IWalkable> { this });

            return this;
        }

        public void SplitConnectedCollisionModels()
        {
            foreach (var room in DoorRooms)
            {
                if (room.Key == X_ConnectorSide.Left || room.Key == X_ConnectorSide.Right)
                {
                    foreach (var r in room.Value)
                    {
                        // need vertical split
                        var res = r.Collision.SplitCollisionVerticallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                        foreach(var rects in res)
                        {
                            List<Rectangle> cRect;
                            if (!_doorCollisionRectangles.TryGetValue(rects.Key, out cRect))
                            {
                                _doorCollisionRectangles.Add(rects.Key, rects.Value);
                            }
                            else _doorCollisionRectangles[rects.Key].AddRange(rects.Value);
                        }
                    }
                }
                else
                {
                    foreach (var r in room.Value)
                    {
                        // need horizontal split                    
                        var res = r.Collision.SplitCollisionHorizontallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                        foreach (var rects in res)
                        {
                            List<Rectangle> cRect;
                            if (!_doorCollisionRectangles.TryGetValue(rects.Key, out cRect))
                            {
                                _doorCollisionRectangles.Add(rects.Key, rects.Value);
                            }
                            else _doorCollisionRectangles[rects.Key].AddRange(rects.Value);
                        }
                    }
                }
            }
            foreach(var state in _doorCollisionRectangles)
            {
                state.Value.AddRange(Collision.GetCollisionRectangles());
            }

            Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Closed]);
        }

        public bool LockDoor()
        {
            if (!(_state == X_DoorState.Open || _state == X_DoorState.Closed)) return false;
            if(_state == X_DoorState.Open)
                _state = X_DoorState.LockedOpen;
            if(_state == X_DoorState.Closed)
                _state = X_DoorState.LockedClosed;

            return true;
        }

        public void Update(GameTime gameTime)
        {
            bool keyPressed = Input.IsKeyTriggered(Keybinds.ToggleConnectors);
            float dt = gameTime.ElapsedGameTime.Milliseconds;
            switch (_state)
            {
                case X_DoorState.Closed:
                    if (keyPressed)
                    {
                        _state = X_DoorState.Opening;
                    }
                    break;
                case X_DoorState.Opening:
                    if (!doorAnimation(dt, true))
                    {
                        _state = X_DoorState.Open;
                        Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Open]);
                    }
                    break;
                case X_DoorState.Open:
                    if (keyPressed)
                    {
                        _state = X_DoorState.Closing;
                        Collision.UpdateCollisionRectangles(_doorCollisionRectangles[X_DoorState.Closed]);
                    }
                    break;
                case X_DoorState.Closing:
                    if (!doorAnimation(dt, false))
                    {
                        _state = X_DoorState.Closed;
                    }
                    break;
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 3, Color.Orange, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            foreach(var door in Doors)
            {
                foreach(var d in door.Value)
                {
                    d.DrawOutline(gameTime, globalOffset, spriteBatch);
                }
            }

            //Factory_Debug.DrawRectangle(_outsideRect1.X, _outsideRect1.Y, _outsideRect1.Width, _outsideRect1.Height, 5, Color.Red, spriteBatch);
            //Factory_Debug.DrawRectangle(_outsideRect2.X, _outsideRect2.Y, _outsideRect2.Width, _outsideRect2.Height, 5, Color.Red, spriteBatch);

            //Factory_Debug.DrawPoint(_leftOrBottomConnector.Point.X, _leftOrBottomConnector.Point.Y, 11, Color.Red, spriteBatch);
            //Factory_Debug.DrawPoint(_rightOrTopConnector.Point.X, _rightOrTopConnector.Point.Y, 11, Color.Orange, spriteBatch);
        }

        //private void average(float a)
        //{
        //    int dx = (int)(Math.Ceiling(_dPos.X * a));
        //    _rect.X = newPosition(_rect.X, dx, _newRect.X);

        //    int dy = (int)(Math.Ceiling(_dPos.Y * a));
        //    _rect.Y = newPosition(_rect.Y, dy, _newRect.Y);
        //}

        private bool doorAnimation(float dt, bool opening)
        {
            _animationTime += dt;
            if (_animationTime < _doorOpeningTime)
            {
                float percent = 1.0f / _doorOpeningTime * _animationTime;
                if(opening)
                    _currentDoorOpenOffset = (int)Math.Round(_tileSize*percent);
                else
                    _currentDoorOpenOffset = (int)Math.Round(_tileSize * (1.0f - percent));
            }
            if (_animationTime >= _doorOpeningTime)
            {
                if(opening)
                    _currentDoorOpenOffset = _tileSize;
                else
                    _currentDoorOpenOffset = 0;
                _animationTime = 0;
                return false;
            }
            return true;
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            if (_state != X_DoorState.Closed && _state != X_DoorState.LockedClosed)
            {
                if(_direction == X_DoorDirection.Vertical)
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _floor.Width, _floor.Height - _tileSize + _currentDoorOpenOffset),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
                else
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _floor.Width, _floor.Height),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2() + new Vector2(_floor.Width * Scale - _tileSize, 0),
                        new Rectangle(_floor.Width - TextureTileSize, 0, TextureTileSize, _floor.Height),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
            }
            else
            {
                if(_direction == X_DoorDirection.Horizontal)
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _tileSize, _floor.Height),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);


                }
                else
                {
                    spriteBatch.Draw(
                        _floor, Rect.Location.ToVector2(),
                        new Rectangle(0, 0, _floor.Width, _tileSize),
                        Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                }
            }

            if (_state != X_DoorState.Open && _state != X_DoorState.LockedOpen)
            {
                Vector2 pos = Rect.Location.ToVector2();
                pos.Y += _currentDoorOpenOffset;
                spriteBatch.Draw(
                    _door, pos,
                    new Rectangle(0, 0, _floor.Width, _floor.Height),
                    Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
            }
            spriteBatch.Draw(
                _roof, Rect.Location.ToVector2(),
                new Rectangle(0, 0, _roof.Width, _roof.Height),
                Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        public void MoveTo(Point position)
        {
            var p = position - Rect.Location;
            Rect = new Rectangle(p.X, p.Y, Rect.Width, Rect.Height);
            foreach(var door in Doors)
            {
                foreach(var d in door.Value)
                {
                    d.MoveBy(p);
                }
            }
            Collision.MoveBy(p);

            foreach(var rect in _doorCollisionRectangles)
            {
                foreach(var r in rect.Value)
                    r.Offset(p);
            }
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Door;
        }
    }
}
