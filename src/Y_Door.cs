using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Assimp;

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

        private int[][] _collision;
        private Point _leftOrBottomConnector;
        private Point _rightOrTopConnector;

        const int _numTilesDoorWidth = 5;

        public Y_Door(X_DoorDirection direction, int numTilesLength, int tileWidth, int tileHeight, int tileOffset)
        {
            int[][] collision = createDoorTemplate(numTilesLength, tileOffset);

            collision = flipToPosition(collision, direction, tileOffset);

            _collision = getDoorPoints(collision, tileWidth, tileHeight);

            Collision = new X_CollisionModel_Room(_collision, tileWidth, tileHeight);
        }

        private int[][] getDoorPoints(int[][] collision, int tileWidth, int tileHeight)
        {
            for (int x = 0; x < collision.Length; ++x)
            {
                for (int y = 0; y < collision[0].Length; ++y)
                {
                    if (collision[x][y] == 2)
                    {
                        _leftOrBottomConnector = new Point(y * tileWidth+tileWidth/2, x * tileHeight+tileHeight/2);
                        collision[x][y] = 0;
                    }
                    if (collision[x][y] == 3)
                    {
                        _rightOrTopConnector = new Point(y * tileWidth + tileWidth / 2, x * tileHeight + tileHeight / 2);
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
                            newCol[y][newCol[0].Length - 1 - x] = collision[x][y];
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
                            newCol[newCol.Length - 1 - x][newCol[0].Length - 1 - y] = collision[y][x];
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
            for (int x = 0; x < half1; ++x)
            {
                collision[x][0] = 1;
            }

            for (int x = 0; x < half1 - doorWidth; ++x)
            {
                collision[x][doorWidth - 1] = 1;
            }

            for (int x = half1-doorWidth; x < width; ++x)
            {
                collision[x][height - 1] = 1;
            }

            for (int x = half1; x<width; ++x)
            {
                collision[x][Math.Abs(tileOffset)] = 1;
            }

            for (int y = 0; y < Math.Abs(tileOffset); ++y)
            {
                collision[half1 - 1][y + 1] = 1;
                collision[half1 - doorWidth][height - y - 2] = 1;
            }

            collision[width-1][height-1-doorWidth/2] = 2;
            collision[0][doorWidth/2] = 3;

            return collision;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            Factory_Debug.DrawPoint(_leftOrBottomConnector.X, _leftOrBottomConnector.Y, 11, Color.Red, spriteBatch);
            Factory_Debug.DrawPoint(_rightOrTopConnector.X, _rightOrTopConnector.Y, 11, Color.Orange, spriteBatch);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

        }

        public void MoveTo(Point position)
        {

        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Door;
        }
    }
}
