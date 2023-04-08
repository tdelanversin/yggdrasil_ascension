using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Assimp;
using System.Data;
using System.Linq;

namespace YGR
{
    public enum X_DoorDirection
    {
        Horizontal = 0,
        Vertical
    }

    public class Y_Door : IWalkable
    {
        public Rectangle Rect { get; set; }
        public string Name { get; }
        public X_CollisionModel_Room Collision { get; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public Dictionary<X_ConnectorSide, IList<X_ConnectorPoint>> Doors { get; set; }
        public Dictionary<X_ConnectorSide, IList<IWalkable>> DoorRooms { get; set; }

        private int[][] _collision;
        private X_DoorDirection _direction;

        const int _numTilesDoorWidth = 5;

        public Y_Door(X_DoorDirection direction, int numTilesLength, int tileWidth, int tileHeight, int tileOffset)
        {
            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);

            collision = flipToPosition(collision, direction, tileOffset);

            _collision = getDoorPoints(collision, tileWidth, tileHeight);

            _direction = direction;

            Collision = new X_CollisionModel_Room(_collision, tileWidth, tileHeight);

            Rect = new Rectangle(0, 0, tileWidth * _collision[0].Length, tileHeight * _collision.Length);
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

            for (int x = 1; x < half1 - doorWidth; ++x)
            {
                collision[x][doorWidth - 1] = 1;
            }

            for (int x = half1-doorWidth; x < width-1; ++x)
            {
                collision[x][height - 1] = 1;
            }

            for (int x = half1; x<width-1; ++x)
            {
                collision[x][Math.Abs(tileOffset)] = 1;
            }

            for (int y = 0; y < Math.Abs(tileOffset); ++y)
            {
                collision[half1 - 1][y + 1] = 1;
                collision[half1 - doorWidth][height - y - 2] = 1;
            }

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
            foreach(var room in DoorRooms)
            {
                if (room.Key == X_ConnectorSide.Left || room.Key == X_ConnectorSide.Right)
                {
                    foreach (var r in room.Value)
                    {
                        // need vertical split
                        r.Collision.SplitCollisionVerticallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                    }
                }
                else
                {
                    foreach (var r in room.Value)
                    {
                        // need horizontal split                    
                        r.Collision.SplitCollisionHorizontallyAt(Doors[room.Key].First().Point, _numTilesDoorWidth);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {

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

            //Factory_Debug.DrawPoint(_leftOrBottomConnector.Point.X, _leftOrBottomConnector.Point.Y, 11, Color.Red, spriteBatch);
            //Factory_Debug.DrawPoint(_rightOrTopConnector.Point.X, _rightOrTopConnector.Point.Y, 11, Color.Orange, spriteBatch);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

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
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Door;
        }
    }
}
