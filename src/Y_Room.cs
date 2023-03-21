using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public class Y_Room : IWalkable
    {
        // The global X position
        public int PosX { get; set; }
        // The global y position
        public int PosY { get; set; }
        // The total width of the texture
        public int Width { get { return _walls.Width; } }
        // The total height of the texture
        public int Height { get { return _walls.Height; } }
        // The list of the connectors connected to this room
        public IList<Y_Connector> Connectors { get; }
        // the name of the room
        public string Name { get; }
        // the background color as required by IDrawable
        private Color BackgroundColor { get; set; }

        // The list with all connector points located on the walls of this room
        private Dictionary<X_ConnectorSide, List<X_ConnectorPoint>> _connectorPoints;
        // The texture with the walls and the fixed obstacles inside the room
        private Texture2D _walls;
        // The polygon for the collision detection for the IWalkable
        private X_Polygon _outline;
        
        public Y_Room(
            string name,
            Texture2D walls,
            X_Polygon outline,
            Dictionary<X_ConnectorSide, int[,]> connectorPoints
        )
        {
            PosX = 0;
            PosY = 0;
            Name = name;
            BackgroundColor = Color.White;

            _outline = outline;
            _walls = walls;
            _connectorPoints = new Dictionary<X_ConnectorSide, List<X_ConnectorPoint>>();
            Connectors = new List<Y_Connector>();

            foreach (var side in connectorPoints)
            {
                int size = side.Value.GetLength(0);
                for (int i=0; i < size; ++i)
                {
                    if (!_connectorPoints.ContainsKey(side.Key)){
                        _connectorPoints.Add(side.Key, new List<X_ConnectorPoint>());
                    }
                    _connectorPoints[side.Key].Add(
                        new X_ConnectorPoint(
                            side.Key,
                            new Vector2(side.Value[i, 0], side.Value[i, 1])
                        )
                    );
                }
            }
        }

        /// <summary>
        /// Regular Monogame Update method
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        public void Update(GameTime gameTime)
        {

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
            _outline.DrawOutline(gameTime, new Vector2(PosX, PosY), spriteBatch);

            foreach(var side in _connectorPoints.Values)
            {
                foreach(var con in side)
                {
                    con.DrawOutline(gameTime, new Vector2(PosX, PosY), spriteBatch);
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
            spriteBatch.Draw(
                _walls,
                new Rectangle(PosX, PosY, _walls.Bounds.Size.X, _walls.Bounds.Size.Y),
                new Rectangle(0, 0, _walls.Bounds.Size.X, _walls.Bounds.Size.Y),
                BackgroundColor
            );
        }

        /// <summary>
        /// This method sets the background color for the regular Monogame Draw method
        /// </summary>
        /// <param name="color">Some Monogame color</param>
        public void SetBackgroundColor(Color color)
        {
            BackgroundColor = color;
        }

        /// <summary>
        /// This method resets the background color for the regular Monogame Draw method to Color.White
        /// </summary>
        public void ResetBackgroundColor()
        {
            BackgroundColor = Color.White;
        }

        /// <summary>
        /// This method adds a Y_Connector to the list of connectors for the current Y_Room
        /// </summary>
        /// <param name="connector"></param>
        public void AddConnector(Y_Connector connector)
        {
            Connectors.Add(connector);
        }

        /// <summary>
        /// This method returns a random connector point on the walls of the current room on the given X_ConnectorSide. 
        /// <para>X_ConnectorSide.Left means, the connector point will be on the left side wall of the room</para>
        /// </summary>
        /// <param name="side">X_ConnectorSide denoting the desired side of the connector point</param>
        /// <param name="random">Random generator for the randomness</param>
        /// <returns>Returns an X_ConnectorPoint on the given X_ConnectorSide</returns>
        public X_ConnectorPoint GetRandomConnectorPoint(X_ConnectorSide side, Random random)
        {
            if (!_connectorPoints.ContainsKey(side))
            {
                Logger.Error("Room doesn't have connector points on the requested side");
            }

            // TODO: add a seed or something...
            int index = random.Next(_connectorPoints[side].Count);

            return _connectorPoints[side].ElementAt(index);
        }

        /// <summary>
        /// This method implements the IWalkable collision detection
        /// </summary>
        /// <param name="rect">Outline Rectangle of the sprite</param>
        /// <param name="pos">Midpoint of the sprite (NOT the top left of the rect)</param>
        /// <param name="dp">Movement delta for the sprite</param>
        /// <param name="who">ref parameter that will point to an IWalkable if there was a collision</param>
        /// <param name="where">ref parameter that will contain the collision point if there was a collision</param>
        /// <returns></returns>
        public Vector2 Clamp(Rectangle rect, Vector2 pos, Vector2 dp, ref IWalkable who, ref Vector2 where)
        {
            bool collision = false;
            Vector2 offset = new Vector2(Math.Sign(dp.X) * rect.Width / 2, Math.Sign(dp.Y) * rect.Height / 2);
            where = _outline.Clamp(pos, dp + offset, new Vector2(-PosX, -PosY), ref collision);
            Vector2 newPos = where - offset;

            if (collision)
            {
                who = this;
            }
            else
            {
                who = null;
            }
            return newPos;
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
