using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public class Y_ConnectorBark : IGameElement
    {
        public float Scale { get; set; }

        public Rectangle Rect { get; set; }

        private Texture2D _barkTexture;
        private List<Vector2> _barkPositions;
        private float _barkScale;
        private Y_Door _door;

        public Y_ConnectorBark(Texture2D barkTexture, List<Vector2> barkPositions, float scale, float barkScale, Y_Door door)
        {
            _barkPositions = barkPositions;
            _barkTexture = barkTexture;
            Rect = new Rectangle(0, 0, 1, 1); // not needed for this one
            Scale = scale;
            _barkScale = barkScale;
            _door = door;
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if ((_door.IsDoorClosed() || _door.IsDoorLockedClosed()) && !_door.IsDoorOpeningOrClosing()) return;
            for(int i=0; i<_barkPositions.Count(); ++i)
            {
                spriteBatch.Draw(
                _barkTexture, _barkPositions[i],
                new Rectangle(0, 0, _barkTexture.Width, _barkTexture.Height),
                Color.White, 0, Vector2.Zero, Scale * _barkScale, SpriteEffects.None, 0);
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            //foreach (var p in _barkPositions)
            //{
            //    Factory_Debug.DrawRectangle((int)p.X, (int)p.Y, _barkTexture.Width, _barkTexture.Height, 2, Color.Aqua, spriteBatch);
            //}
        }

        public void Update(GameTime gameTime)
        {
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Bark;
        }
    }
}
