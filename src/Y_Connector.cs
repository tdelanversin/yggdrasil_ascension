//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System.Collections.Generic;
//using System;

//namespace YGR
//{
//    public enum Y_ConnectorDirection
//    {
//        Horizontal = 0,
//        Vertical
//    }

//    public enum Y_ConnectorState
//    {
//        Opened = 0,
//        Closed
//    }

//    public class Y_Connector : IWalkable
//    {
//        /* The X position of the connector in global coordinates */
//        public Vector2 Position { get; set; }
//        /* The Y position of the connector in global coordinates */
//        public Y_ConnectorDirection Direction { get; }
//        /* The left or the bottom point that has to match the room connection point */
//        public Vector2 ConnectorLeftOrBottom { get; }
//        /* The right or top point that has to match the room connection point */
//        public Vector2 ConnectorRightOrTop { get; }
//        /* The left or the bottom platform that sticks into the room*/
//        public Rectangle LeftOrBottomPlatform { get; }
//        /* The right or the top platform that sticks into the room*/
//        public Rectangle RightOrTopPlatform { get; }
//        /* The state of the connector (opened/closed)*/
//        public Y_ConnectorState State { get; set; }
//        /* The spacing distance the connector introduces between two rooms (must match the texture) */
//        public int SpacingDistance { get; }
//        /* The name of the connector */
//        public string Name { get; }
//        /* The background color used by the Draw method (default should be Color.White)*/
//        private Color BackgroundColor { get; set; }
//        /* The Y_Room attached on the left or the bottom side */
//        public Y_Room LeftOrBottomRoom { get; private set; }
//        /* The Y_Room attached on the right or the top side */
//        public Y_Room RightOrTopRoom { get; private set; }
//        public X_CollisionModel_Room Collision { get; }

//        // outline of the connector (the same as the texture size)
//        // use this to check if a point is on the connector or not
//        public Rectangle Rect { get; set; }
//        // essentially the same as _rectangle but should be used for the sprite animations
//        Rectangle _window;
        
//        Texture2D _texture;
        
//        // collision polygons for both states of the connector
//        X_Polygon _outlineClosed;
//        X_Polygon _outlineOpened;

//        // incides for the animations
//        int _animationIndex;
//        Dictionary<string, int[]> _animations;

//        public IList<IProjectile> Projectiles { get; }
//        public IList<IVictim> Victims { get; }

//        public Y_Connector(
//            string name,
//            Y_ConnectorDirection direction,
//            Texture2D texture,
//            Rectangle window,
//            Vector2 connectorLeftOrBottom,
//            Vector2 connectorRightOrTop,
//            X_Polygon outlineClosed,
//            X_Polygon outlineOpened,
//            Rectangle leftOrBottomPlatform,
//            Rectangle rightOrTopPlatform,
//            int spacingDistance,
//            Dictionary<string, int[]> animations,
//            Y_ConnectorState initialState = Y_ConnectorState.Closed
//        )
//        {
//            Direction = direction;
//            ConnectorLeftOrBottom = connectorLeftOrBottom;
//            ConnectorRightOrTop = connectorRightOrTop;
//            LeftOrBottomPlatform = leftOrBottomPlatform;
//            RightOrTopPlatform = rightOrTopPlatform;
//            SpacingDistance = spacingDistance;
//            State = initialState;
//            Name = name;
//            BackgroundColor = Color.White;

//            _outlineClosed = outlineClosed;
//            _outlineOpened = outlineOpened;
//            _texture = texture;
//            _animations = animations;
//            _window = window;

//            Rect = _window;

//            if(State == Y_ConnectorState.Closed)
//            {
//                _animationIndex = 0;
//            }
//            else
//            {
//                _animationIndex = 4;
//            }

//            Projectiles = new List<IProjectile>();
//            Victims = new List<IVictim>();
//        }

//        /// <summary>
//        /// Regular Monogame Update method
//        /// </summary>
//        /// <param name="gameTime">Monogame GameTime</param>
//        public void Update(GameTime gameTime)
//        {
//            if (Keyboard.HasBeenPressed(Keybinds.ToggleConnectors))
//            {
//                if (State == Y_ConnectorState.Closed)
//                {
//                    State = Y_ConnectorState.Opened;
//                    _animationIndex = 4;
//                }
//                else
//                {
//                    State = Y_ConnectorState.Closed;
//                    _animationIndex = 0;
//                }
//            }
//        }

//        public X_ConnectorPoint GetConnectorPoint(X_ConnectorSide side, string name = "")
//        {
//            return null;
//        }

//        public void MoveTo(Point position)
//        {

//        }

//        /// <summary>
//        /// This Method will draw all internal structures for debugging purposes
//        /// Note: potentially heavy impact on performace
//        /// </summary>
//        /// <param name="gameTime">Monogame GameTime</param>
//        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
//        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
//        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
//        {
//            globalOffset = new Vector2(Position.X, Position.Y);

//            if (State == Y_ConnectorState.Opened)
//            {
//                _outlineOpened.DrawOutline(gameTime, globalOffset, spriteBatch);
//            }
//            else
//            {
//                _outlineClosed.DrawOutline(gameTime, globalOffset, spriteBatch);
//            }

//            int size = 15;
//            Factory_Debug.DrawPoint((int)(ConnectorRightOrTop.X + globalOffset.X), (int)(ConnectorRightOrTop.Y + globalOffset.Y), size, Color.Green, spriteBatch);
//            Factory_Debug.DrawPoint((int)(ConnectorLeftOrBottom.X + globalOffset.X), (int)(ConnectorLeftOrBottom.Y + globalOffset.Y), size, Color.Yellow, spriteBatch);

//            Factory_Debug.DrawRectangle(LeftOrBottomPlatform.X + (int)globalOffset.X, LeftOrBottomPlatform.Y + (int)globalOffset.Y, LeftOrBottomPlatform.Width, LeftOrBottomPlatform.Height, 5, Color.Blue, spriteBatch);
//            Factory_Debug.DrawRectangle(RightOrTopPlatform.X + (int)globalOffset.X, RightOrTopPlatform.Y + (int)globalOffset.Y, RightOrTopPlatform.Width, RightOrTopPlatform.Height, 5, Color.Green, spriteBatch);
//        }

//        /// <summary>
//        /// Regular Draw method for all drawable objects
//        /// </summary>
//        /// <param name="gameTime">Monogame GameTime</param>
//        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
//        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
//        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
//        {
//            spriteBatch.Draw(
//                _texture,
//                new Rectangle((int)Position.X, (int)Position.Y, _window.Width, _window.Height),
//                new Rectangle(_animationIndex * _window.Width, 0, _window.Width, _window.Height),
//                BackgroundColor
//            );
//        }

//        /// <summary>
//        /// This method sets the background color for the Monogame Draw method
//        /// </summary>
//        /// <param name="color">New background color</param>
//        public void SetBackgroundColor(Color color)
//        {
//            BackgroundColor = color;
//        }

//        /// <summary>
//        /// This method resets the background color for the Monogame Draw method to Color.White
//        /// </summary>
//        public void ResetBackgroundColor()
//        {
//            BackgroundColor = Color.White;
//        }

//        /// <summary>
//        /// This method implements the IWalkable collision detection
//        /// </summary>
//        /// <param name="rect">Outline Rectangle of the sprite</param>
//        /// <param name="pos">Midpoint of the sprite (NOT the top left of the rect)</param>
//        /// <param name="dp">Movement delta for the sprite</param>
//        /// <param name="who">ref parameter that will point to an IWalkable if there was a collision</param>
//        /// <param name="where">ref parameter that will contain the collision point if there was a collision</param>
//        /// <returns></returns>
//        public Vector2 Clamp(Rectangle rect, Vector2 pos, Vector2 dp, ref IWalkable who, ref Vector2 where)
//        {
//            bool collision = false;
//            Vector2 offset = new Vector2(Math.Sign(dp.X) * rect.Width / 2, Math.Sign(dp.Y) * rect.Height / 2);

//            if(State == Y_ConnectorState.Closed)
//            {
//                where = _outlineClosed.Clamp(pos, dp + offset, -Position, ref collision);
//            }
//            else
//            {
//                where = _outlineOpened.Clamp(pos, dp + offset, -Position, ref collision);
//            }
//            Vector2 newPos = where - offset;

//            if (collision)
//            {
//                who = this;
//            }
//            else
//            {
//                who = null;
//            }
//            return newPos;
//        }

//        /// <summary>
//        /// <para>This method will connect two Y_Rooms</para>
//        /// <para>The connector is in the middle</para>
//        /// 
//        /// <br>-------------------- </br>
//        /// <br>left |||XXX||| right </br>
//        /// <br>-------------------- </br>
//        ///  
//        /// <para>OR</para>
//        /// 
//        /// <br>  top   </br>
//        /// <br> |||||  </br>
//        /// <br> |XXX|  </br>
//        /// <br> |||||  </br>
//        /// <br> bottom </br>
//        /// 
//        /// <para>The method acts out of the PERSPECTIVE OF THE CONNECTOR!</para>
//        /// 
//        /// <para>side1 = X_ConnectorSide.Left means, that room1 is on the left side of the connector.</para>
//        /// </summary>
//        /// <param name="side1">The origin side: the connector will place the other side with respect to this side</param>
//        /// <param name="from">The Y_Room on side1 (the origin side)</param>
//        /// <param name="side2">The adjusted side: the connector will place this side with respect to the other side</param>
//        /// <param name="to">The Y_Room on side 2 (the adjusted side)</param>
//        /// <param name="random">Random generator for the random connector point selection</param>
//        /// <returns></returns>
//        public Y_Connector Connect(
//            X_ConnectorSide side1,
//            Y_Room from,
//            X_ConnectorSide side2,
//            Y_Room to,
//            Random random
//        )
//        {
//            /*
//             * If we use 
//             *    connect(Left, room1, Right, room2)
//             * we have to FLIP the sides for the next call because for the left side of the connector
//             * we need a point on the right side of room 1 and for the right side of the connector, we need
//             * a point on the left side of room 2!!!!!!!
//             */

//            if (side1 == X_ConnectorSide.Left) side1 = X_ConnectorSide.Right;
//            else if (side1 == X_ConnectorSide.Right) side1 = X_ConnectorSide.Left;
//            else if (side1 == X_ConnectorSide.Top) side1 = X_ConnectorSide.Bottom;
//            else if (side1 == X_ConnectorSide.Bottom) side1 = X_ConnectorSide.Top;

//            if (side2 == X_ConnectorSide.Left) side2 = X_ConnectorSide.Right;
//            else if (side2 == X_ConnectorSide.Right) side2 = X_ConnectorSide.Left;
//            else if (side2 == X_ConnectorSide.Top) side2 = X_ConnectorSide.Bottom;
//            else if (side2 == X_ConnectorSide.Bottom) side2 = X_ConnectorSide.Top;

//            return Connect(from, from.GetRandomConnectorPoint(side1, random), to, to.GetRandomConnectorPoint(side2, random));
//        }

//        /// <summary>
//        /// <para>This method will connect two Y_Rooms</para>
//        /// <para>The connector is in the middle</para>
//        /// 
//        /// <br>-------------------- </br>
//        /// <br>left |||XXX||| right </br>
//        /// <br>-------------------- </br>
//        ///  
//        /// <para>OR</para>
//        /// 
//        /// <br>  top   </br>
//        /// <br> |||||  </br>
//        /// <br> |XXX|  </br>
//        /// <br> |||||  </br>
//        /// <br> bottom </br>
//        /// 
//        /// <para>The method acts out of the PERSPECTIVE OF THE ROOMS!</para>
//        /// 
//        /// <para>If room1 is on the LEFT side of the connector, then the connectorPoint1 must be on the RIGHT side of room1.</para>
//        /// </summary>
//        /// <param name="room1">Y_Room with connectorPoint1</param>
//        /// <param name="connectorPoint1">The connector point on Y_Room 1</param>
//        /// <param name="room2">Y_Room with connectorPoint2</param>
//        /// <param name="connectorPoint2">The connector point on Y_Room 2</param>
//        /// <returns></returns>
//        public Y_Connector Connect(
//            Y_Room room1,
//            X_ConnectorPoint connectorPoint1,
//            Y_Room room2,
//            X_ConnectorPoint connectorPoint2
//        )
//        {
//            // check possibilities
//            // horizontal: left and right or right and left
//            // vertical: top and bottom or bottom and top
//            bool hCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Left && connectorPoint2.ConnectorSide == X_ConnectorSide.Right;
//            bool hCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Right && connectorPoint2.ConnectorSide == X_ConnectorSide.Left;
//            bool vCheck1 = connectorPoint1.ConnectorSide == X_ConnectorSide.Bottom && connectorPoint2.ConnectorSide == X_ConnectorSide.Top;
//            bool vCheck2 = connectorPoint1.ConnectorSide == X_ConnectorSide.Top && connectorPoint2.ConnectorSide == X_ConnectorSide.Bottom;

//            // horizontal connectors only with left and right or right and left
//            // vertical connectors only with top and bottom or bottom and top
//            if (!(
//                ((hCheck1 || hCheck2) && Direction == Y_ConnectorDirection.Horizontal) || 
//                ((vCheck1 || vCheck2) && Direction == Y_ConnectorDirection.Vertical))
//                )
//            {
//                Logger.Error("Invalid connector combination");
//            }

//            /* left to right */
//            if (hCheck1)
//            {
//                // compute global coordinates for connector (top-left corner of the texture)
//                Position = new Vector2(
//                    room1.Position.X + (int)connectorPoint1.Point.X - (int)ConnectorRightOrTop.X,
//                    room1.Position.Y + (int)connectorPoint1.Point.Y - (int)ConnectorRightOrTop.Y
//                );

//                // compute global coordinates for the other room (top-left corner of the texture)
//                room2.Position = new Vector2(
//                    room1.Position.X - room2.Width - SpacingDistance,
//                    Position.Y + (int)ConnectorRightOrTop.Y - (int)connectorPoint2.Point.Y
//                );

//                // add the connector to the two rooms
//                room1.AddConnector(this);
//                room2.AddConnector(this);

//                // add the two rooms to the connector
//                LeftOrBottomRoom = room2;
//                RightOrTopRoom = room1;
//            }
//            /* right to left */
//            else if (hCheck2)
//            {
//                // compute global coordinates for connector (top-left corner of the texture)
//                Position = new Vector2(
//                    room1.Position.X + (int)connectorPoint1.Point.X - (int)ConnectorLeftOrBottom.X,
//                    room1.Position.Y + (int)connectorPoint1.Point.Y - (int)ConnectorLeftOrBottom.Y
//                ); 

//                // compute global coordinates for the other room (top-left corner of the texture)
//                room2.Position = new Vector2(
//                    room1.Position.X + room1.Width + SpacingDistance,
//                    Position.Y + (int)ConnectorRightOrTop.Y - (int)connectorPoint2.Point.Y
//                );

//                // add the connector to the two rooms
//                room1.AddConnector(this);
//                room2.AddConnector(this);

//                // add the two rooms to the connector
//                LeftOrBottomRoom = room1;
//                RightOrTopRoom = room2;
//            }
//            /* bottom to top */
//            else if (vCheck1)
//            {
//                // compute global coordinates for connector (top-left corner of the texture)
//                Position = new Vector2(
//                    room1.Position.X + (int)connectorPoint1.Point.X - (int)ConnectorRightOrTop.X,
//                    room1.Position.Y + (int)connectorPoint1.Point.Y - (int)ConnectorRightOrTop.Y
//                );

//                // compute global coordinates for the other room (top-left corner of the texture)
//                room2.Position = new Vector2(
//                    Position.X + (int)ConnectorRightOrTop.X - (int)connectorPoint2.Point.X,
//                    room1.Position.Y + room1.Height + SpacingDistance
//                );

//                // add the connector to the two rooms
//                room1.AddConnector(this);
//                room2.AddConnector(this);

//                // add the two rooms to the connector
//                LeftOrBottomRoom = room2;
//                RightOrTopRoom = room1;
//            }
//            /* top to bottom */
//            else if (vCheck2)
//            {
//                // compute global coordinates for connector (top-left corner of the texture)
//                Position = new Vector2(
//                    room1.Position.X + (int)connectorPoint1.Point.X - (int)ConnectorLeftOrBottom.X,
//                    room1.Position.Y + (int)connectorPoint1.Point.Y - (int)ConnectorLeftOrBottom.Y
//                );

//                // compute global coordinates for the other room (top-left corner of the texture)
//                room2.Position = new Vector2(
//                    Position.X + (int)ConnectorRightOrTop.X - (int)connectorPoint2.Point.X,
//                    room1.Position.Y - room2.Height - SpacingDistance
//                );

//                // add the connector to the two rooms
//                room1.AddConnector(this);
//                room2.AddConnector(this);

//                // add the two rooms to the connector
//                LeftOrBottomRoom = room1;
//                RightOrTopRoom = room2;
//            }

//            return this;
//        }

//        /// <summary>
//        /// This method checks if the point is inside either of the entrance pads of the connector.
//        /// The entrance pads are larger rectangles on each side of the connector. They help with determining
//        /// which room the user will enter when it leaves the connector
//        /// </summary>
//        /// <param name="point">Position of the player in global coordinates</param>
//        /// <param name="side">reference parameter that will contain the X_ConnectorSide the player will leave the connector</param>
//        /// <returns>Returns true if the player is on either of the entrance pads of the connector</returns>
//        public bool IsOnPad(Vector2 point, ref X_ConnectorSide side)
//        {
//            int localX = (int)(point.X - Position.X);
//            int localY = (int)(point.Y - Position.Y);
//            if(LeftOrBottomPlatform.Contains(localX, localY))
//            {
//                if (Direction == Y_ConnectorDirection.Horizontal) side = X_ConnectorSide.Left;
//                else side = X_ConnectorSide.Bottom;
//                return true;
//            }

//            if(RightOrTopPlatform.Contains(localX, localY))
//            {
//                if (Direction == Y_ConnectorDirection.Horizontal) side = X_ConnectorSide.Right;
//                else side = X_ConnectorSide.Top;
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// This method checks if the player is inside the actual rectangle of the connector.
//        /// </summary>
//        /// <param name="point">Position of the player in global coordinates</param>
//        /// <returns>Returns true if the player is inside the actual rectangle of the connector</returns>
//        public bool IsInside(Vector2 point)
//        {
//            int localX = (int)(point.X - Position.X);
//            int localY = (int)(point.Y - Position.Y);
//            return Rect.Contains(localX, localY);
//        }

//        public bool Intersects(Rectangle other)
//        {
//            // TODO: this one needs to deal with the tiles of the walls
//            // then goto X_CollisionManager and do sofisticated tile collision detection with the relevant tiles
//            return other.Intersects(GetRect());
//        }

//        public bool Intersects(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal)
//        {
//            contactPoint = Point.Zero;
//            contactNormal = Vector2.Zero;
//            return false;
//        }

//        /// <summary>
//        /// This method returns the room on the X_ConnectorSide that is passed as parameter
//        /// </summary>
//        /// <param name="side">X_ConnectorSide denoting the desired side of the connector</param>
//        /// <returns>Returns the Y_Room located on the given X_ConnectorSide of the connector</returns>
//        public Y_Room GetRoom(X_ConnectorSide side)
//        {
//            if (side == X_ConnectorSide.Left || side == X_ConnectorSide.Bottom) return LeftOrBottomRoom;
//            else return RightOrTopRoom;
//        }

//        /// <summary>
//        /// This method is the standard ILevelElement WhatAreYou
//        /// </summary>
//        /// <returns>Returns the fitting X_LevelElements enum entry</returns>
//        public X_LevelElements WhatAreYou()
//        {
//            return X_LevelElements.Connector;
//        }

//        public Rectangle GetRect()
//        {
//            return new Rectangle((int)Position.X - _window.Width / 2, (int)Position.Y - _window.Height / 2, _window.Width, _window.Height);
//        }
//    }
//}
