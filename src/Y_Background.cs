using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public class Background : IGameElement
    {
        public float Scale { get; set; }
        public Rectangle Rect { get; set; }

        Texture2D SpriteYggdrasil;
        Texture2D SpriteSky;
        Texture2D SpriteTitleText;

        public Background() { }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            SpriteYggdrasil = Manager_Sprites.BackgroundYggdrasil;
            SpriteTitleText = Manager_Sprites.BackgroundTitleText;
            SpriteSky = Manager_Sprites.BackgroundSky;

            int width = (int)(SpriteYggdrasil.Width * 3.8f);
            int height = (int)(SpriteYggdrasil.Height * 3.8f);
            Point startLocation = new Point((int)(width / 1.96f), (int)(height / 1.12f));
            Rect = new Rectangle(-startLocation.X, -startLocation.Y, width, height);

            // Update the camera ASAP
            Camera.SetFocusRect(Rect, animate: false);
        }

        public void DrawYggdrasil(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteYggdrasil, Rect, Color.White);
        }

        public void DrawSky(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteSky, Rect, Color.White);
            // TODO: moving sky / clouds
        }

        public void DrawTitleText(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteTitleText, Rect, Color.White);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            DrawSky(gameTime, globalOffset, spriteBatch);
            DrawYggdrasil(gameTime, globalOffset, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            // TODO: moving sky / clouds
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            throw new System.NotImplementedException();
        }

        public X_LevelElements WhatAreYou()
        {
            throw new System.NotImplementedException();
        }
    }
}