using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YGR
{
    public class Y_CMRoom : IWalkable
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public IList<IProjectile> Projectiles { get; }
        public IList<IVictim> Victims { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ICollisionModel Collision { get; }

        private Rectangle _rect;

        public Y_CMRoom(
            string name,
            ICollisionModel collision
        )
        {
            Position = Vector2.Zero;
            Name = name;
            Collision = collision;
            Projectiles = new List<IProjectile>();
            Victims = new List<IVictim>();

            _rect = new Rectangle(0, 0, Width, Height);
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
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        /// <summary>
        /// Regular Draw method for all drawable objects
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
        }

        /// <summary>
        /// This method sets the background color for the regular Monogame Draw method
        /// </summary>
        /// <param name="color">Some Monogame color</param>
        public void SetBackgroundColor(Color color)
        {
        }

        /// <summary>
        /// This method resets the background color for the regular Monogame Draw method to Color.White
        /// </summary>
        public void ResetBackgroundColor()
        {
        }

        /// <summary>
        /// This method is the standard ILevelElement WhatAreYou
        /// </summary>
        /// <returns>Returns the fitting X_LevelElements enum entry</returns>
        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.CollisionTester;
        }

        public Rectangle GetRect()
        {
            return _rect;
        }
    }
}
