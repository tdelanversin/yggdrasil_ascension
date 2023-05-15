using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace YGR
{
    public class Background : IGameElement
    {
        public Rectangle Rect { get; set; }
        public int ElementLevel { get; set; }

        Texture2D SpriteYggdrasil;
        Texture2D SpriteSky;
        Texture2D SpriteTitleText;
        Vector2 DrawOffset;
        Rectangle DrawRect;

        float SkyAnimationDuration = 90000;
        float SkyAnimationTimer = 0;

        static Rectangle TextBounds;

        A_Yggdrasil Game;
        Y_Level Level;

        public Background(A_Yggdrasil game, Y_Level level) { Game = game; Level = level; }

        public void LoadContent()
        {
            SpriteYggdrasil = Manager_Sprites.BackgroundYggdrasil;
            SpriteTitleText = Manager_Sprites.BackgroundTitleText;
            SpriteSky = Manager_Sprites.BackgroundSky;

            // This Rect defines the bounds for what we want to show as a menu background
            float scale = 3.8f;
            int width = (int)(3840 * scale);
            int height = (int)(2160 * scale);
            Point startLocation = new Point((int)(width / 1.96f), (int)(height / 1.12f));
            Rect = new Rectangle(-startLocation.X, -startLocation.Y, width, height);
            var r = new Rectangle(816, 68, 2463, 738); // Rectangle inside the sprite
            TextBounds = new Rectangle(Rect.X + (int)(r.X * scale), Rect.Y + (int)(r.Y * scale), (int)(r.Width * scale), (int)(r.Height * scale));

            // Since the actual background image is now larger, we have a seperate Rect for it
            int drawWidth = (int)(SpriteYggdrasil.Width * scale);
            int drawHeight = (int)(SpriteYggdrasil.Height * scale);
            DrawOffset = new Vector2(
                -(SpriteYggdrasil.Width - 3840) * scale / 2,
                -(SpriteYggdrasil.Height - 2160) * scale / 2
            );
            DrawRect = new Rectangle(-startLocation.X + (int)DrawOffset.X, -startLocation.Y + (int)DrawOffset.Y, drawWidth, drawHeight);

            // Update the camera ASAP
            Camera.SetFocusMenu(Rect, animate: false);
        }

        public void DrawYggdrasil(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(SpriteYggdrasil, DrawRect, Color.White);
        }

        public void DrawSky(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // spriteBatch.Draw(SpriteSky, DrawRect, Color.White);
            float f = 1f - SkyAnimationTimer / SkyAnimationDuration;
            int w1 = SpriteSky.Width;
            int w2 = DrawRect.Width;
            int x1 = (int)(f * w1);
            int x2 = (int)(f * w2);
            Rectangle sr1 = new Rectangle(x1, 0, w1 - x1, SpriteSky.Height);
            Rectangle sr2 = new Rectangle(0, 0, x1, SpriteSky.Height);
            Rectangle dr1 = new Rectangle(DrawRect.X, DrawRect.Y, w2 - x2, DrawRect.Height);
            Rectangle dr2 = new Rectangle(DrawRect.X + w2 - x2, DrawRect.Y, x2, DrawRect.Height);
            // dr1.Offset(DrawRect.Location);
            // dr2.Offset(DrawRect.Location);
            spriteBatch.Draw(SpriteSky, dr1, sr1, Color.White);
            spriteBatch.Draw(SpriteSky, dr2, sr2, Color.White);
        }

        public void DrawTitleText(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var (_, room) in Y_Level.Rooms)
            {
                if (room.IsVisible() && TextBounds.Intersects(room.Rect))
                {
                    return;
                }
            }

            Color c = Color.White;

            var opacity = 1f;

            if (Camera.InTransitionToMenu)
            {
                opacity *= (float)Camera.EaseInOut(Camera.AnimationFraction, 6d);
            }
            else if (Camera.InTransitionFromMenu)
            {
                opacity *= (float)Camera.EaseInOut(1f - Camera.AnimationFraction, 6d);
            }
            else if (Camera.Mode != CameraMode.Menu)
            {
                return;
            }

            spriteBatch.Draw(SpriteTitleText, DrawRect, c * opacity);
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
            Factory_Debug.DrawRectangle(TextBounds, 32, Color.BlueViolet, spriteBatch);
            Factory_Debug.DrawRectangle(Rect, 32, Color.BlueViolet, spriteBatch);
        }

        public X_LevelElements WhatAreYou()
        {
            throw new System.NotImplementedException();
        }
    }
}