using Clearcove.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace YGR
{
    public class Room : ICollidable
    {

        private Texture2D _walls;
        private Polygon _outline;

        public Room(Texture2D walls, Polygon outline)
        {
            _outline = outline;
            _walls = walls;
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime, int ox, int oy, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _walls,
                new Rectangle(-ox, -oy, _walls.Bounds.Size.X, _walls.Bounds.Size.Y),
                new Rectangle(0, 0, _walls.Bounds.Size.X, _walls.Bounds.Size.Y),
                Color.White
            );

            _outline.Draw(gameTime, ox, oy, spriteBatch);
        }

        public Vector2 Clamp(Rectangle rect, Vector2 pos, Vector2 dp, ref ICollidable? who)
        {
            bool collision = false;
            Vector2 offset = new Vector2(Math.Sign(dp.X) * rect.Width / 2, Math.Sign(dp.Y) * rect.Height / 2);
            Vector2 newPos = _outline.Clamp(pos, dp + offset, ref collision) - offset;

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

        public Elements WhatAreYou()
        {
            return Elements.Room;
        }
    }
}
