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

        float SkyAnimationDuration = 90000;
        float SkyAnimationTimer = 0;

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
            Camera.SetFocusMenu(Rect, animate: false);
        }

        public void DrawYggdrasil(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteYggdrasil, Rect, Color.White);
        }

        public void DrawSky(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // spriteBatch.Draw(SpriteSky, Rect, Color.White);
            float f = 1f - SkyAnimationTimer / SkyAnimationDuration;
            int w1 = SpriteSky.Width;
            int w2 = Rect.Width;
            int x1 = (int)(f * w1);
            int x2 = (int)(f * w2);
            Rectangle sr1 = new Rectangle(x1, 0, w1 - x1, SpriteSky.Height);
            Rectangle sr2 = new Rectangle(0, 0, x1, SpriteSky.Height);
            Rectangle dr1 = new Rectangle(Rect.X, Rect.Y, w2 - x2, Rect.Height);
            Rectangle dr2 = new Rectangle(Rect.X + w2 - x2, Rect.Y, x2, Rect.Height);
            // dr1.Offset(Rect.Location);
            // dr2.Offset(Rect.Location);
            spriteBatch.Draw(SpriteSky, dr1, sr1, Color.White);
            spriteBatch.Draw(SpriteSky, dr2, sr2, Color.White);
        }

        public void DrawTitleText(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {

            Color c = Color.White;

            if (Camera.InTransitionToMenu)
            {
                c *= (float)Camera.EaseInOut(Camera.AnimationFraction, 6d);
            }
            else if (Camera.InTransitionFromMenu)
            {
                c *= (float)Camera.EaseInOut(1f - Camera.AnimationFraction, 6d);
            }
            else if (Camera.Mode != CameraMode.Menu)
            {
                return;
            }

            spriteBatch.Draw(SpriteTitleText, Rect, c);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            DrawSky(gameTime, globalOffset, spriteBatch);
            DrawYggdrasil(gameTime, globalOffset, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            SkyAnimationTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (SkyAnimationTimer >= SkyAnimationDuration)
            {
                SkyAnimationTimer = 0;
            }
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