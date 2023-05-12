using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YGR
{

    public static class UI
    {
        public static void DrawPlayerSelection(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (Camera.InAnimation || Camera.Mode != CameraMode.Room)
            {
                return;
            }

            SpriteFont font = Fonts.Large;
            float spacing = 1.5f;
            string longest_str = "P1: [JOINED] Pick character"; // Used to center the text
            Vector2 size = font.MeasureString(longest_str);
            Vector2 pos = new Vector2((Camera.Bounds.Width - size.X) / 2, (Camera.Bounds.Height - size.Y * (1f + 3f * spacing)) / 2);
            pos.Y +=Y_Level.InGameTileSize * 2.5f; // Feels like CSS...

            foreach (IPlayer p in Manager_Players.Players)
            {
                string indexString = "P" + (int)p.PlayerIndex + ": ";
                string statusString = "";
                Color statusColor;
                if (p.ControlLayout == ControlLayout.ControllerOnly && !GamePad.GetState(p.PlayerIndex).IsConnected)
                {
                    statusString += "[  --  ] Disconnected";
                    statusColor = Color.DimGray;
                }
                else if (!p.IsActive)
                {
                    statusString += "[  --  ] Move to join";
                    statusColor = Color.LightPink;
                }
                else if (p is Player_Ghost)
                {
                    statusString += "[JOINED] Pick character";
                    statusColor = Color.LightBlue;
                }
                else
                {
                    statusString += "[JOINED] Ready";
                    statusColor = Color.LimeGreen;
                }

                float indexStringWidth = font.MeasureString(indexString).X;
                // Draw text shadow
                spriteBatch.DrawString(font, indexString + statusString, pos + Vector2.One, Color.Black);

                // Draw text line itself
                Vector2 offset = new Vector2(font.MeasureString(indexString).X, 0);
                spriteBatch.DrawString(font, indexString, pos, Color.Lerp(p.Color, Color.Wheat, 0.5f));
                spriteBatch.DrawString(font, statusString, pos + offset, Color.Lerp(statusColor, Color.Wheat, 0.2f));
                pos.Y += size.Y * spacing;
            }
        }

        public static void DrawPlayerStatus(GameTime gameTime, SpriteBatch spriteBatch)
        {
            SpriteFont font = Fonts.GetDecentlySizedFont();
            Vector2 pos = new Vector2((Camera.Bounds.Width) / 128, Camera.Bounds.Height / 16);
            float spacing = 1.25f;

            foreach (IPlayer p in Manager_Players.Players)
            {
                string indexString = "Player " + (int)p.PlayerIndex + ": ";
                string infoString = "";

                infoString += string.Format("\n HP: {0,-3}/{1,-3}", p.LifePoints, p.LifePointsMax);
                // infoString += string.Format("\n Class:  {0}", p.Name);
                // infoString += string.Format("\n Weapon: {0}", p.Gun.Name);
                infoString += string.Format("\n Class:");
                infoString += string.Format("\n Weapon:");
                infoString += string.Format("\n Kills: {0:0}", p.Stats.Kills);

                Vector2 indexStringSize = font.MeasureString(indexString);
                Color playerColor = Color.Lerp(p.Color, Color.Wheat, 0.5f);

                // Draw a semi transparent background box
                int margin = 5;
                Vector2 totalSize = font.MeasureString(indexString + infoString);
                Rectangle rect = new Rectangle((int)pos.X - margin, (int)pos.Y - margin, (int)totalSize.X + 2 * margin, (int)totalSize.Y + 2 * margin);
                spriteBatch.Draw(Manager_Sprites.White, destinationRectangle: rect, null, playerColor * 0.4f, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw the player sprite
                AnimatedSprite charSprite = p.GetSprite();
                int height = (int)indexStringSize.Y;
                int width = (int)(charSprite.SpriteDimension.X / charSprite.SpriteDimension.Y * height);
                int x = rect.X + rect.Width - (int)(indexStringSize.Y * 1.5) - width / 2;
                int y = (int)(rect.Y + margin + indexStringSize.Y * 2);
                Rectangle charRect = new Rectangle(x, y, width, height);
                spriteBatch.Draw(charSprite.Texture, charRect, charSprite.SourceRectangle, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw the weapon sprite
                Texture2D weaponSprite = p.Gun.Sprite;
                width = (int)(weaponSprite.Width / weaponSprite.Height * height);
                y = (int)(rect.Y + margin + indexStringSize.Y * 3);
                Rectangle weaponRect = new Rectangle(x, y, width, height);
                if (p.Gun is not Gun_Ghost)
                    spriteBatch.Draw(weaponSprite, weaponRect, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw text itself
                Vector2 offset = new Vector2(0, font.MeasureString(infoString).Y);
                Util.DrawString(font, indexString, pos, playerColor, spriteBatch);
                Util.DrawString(font, infoString, pos, Color.Wheat, spriteBatch);
                pos.Y += totalSize.Y * spacing;
            }
        }

        public static void DrawPlayerStatistics(GameTime gameTime, SpriteBatch spriteBatch, bool printAll = false)
        {
            SpriteFont font = Fonts.GetDecentlySizedFont();
            Vector2 pos = new Vector2((Camera.Bounds.Width) / 5, Camera.Bounds.Height / 3);
            float spacing = 1.25f;

            for (int i = 0; i < Manager_Players.Players.Count; i++)
            {
                if (i == 2)
                {
                    pos = new Vector2((Camera.Bounds.Width) * 3 / 5, Camera.Bounds.Height / 3);
                }

                IPlayer p = Manager_Players.Players[i];
                string indexString = "Player " + (int)p.PlayerIndex + ": ";
                string infoString = "";

                var stats = p.Stats;

                if (printAll)
                {
                    // Use some reflection hacker-y... too lazy to hard code print everything
                    var type = stats.GetType();
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var field in fields)
                    {
                        var description = field.Name;
                        description = Regex.Replace(description, "(\\B[A-Z])", " $1"); // Split Camelcase variable names into nice strings
                        var value = field.GetValue(stats).GetType() == typeof(float) ? /* HAACK */ (float)field.GetValue(stats) / 32 : field.GetValue(stats);
                        infoString += string.Format("\n {0,-20} {1,-8:0}", description, value);
                    }
                }
                else
                {
                    // Never mind, let's do it by hand as well
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Kills", stats.Kills);
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Damage Dealt", stats.DamageDealt);
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Damage Taken", stats.DamageTaken);
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Times Fired", stats.TimesFired);
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Times Dashed", stats.TimesDashed);
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Distance Walked", stats.DistanceTravelled / 32);
                    infoString += string.Format("\n {0,-20} {1,-8:0}", "Bullets Dodged", stats.ProjectilesDodged);
                }

                Vector2 indexStringSize = font.MeasureString(indexString);
                Color playerColorLight = Color.Lerp(p.Color, Color.Wheat, 0.5f);
                Color playerColorDark = Color.DarkSlateGray;

                // Draw a semi transparent background box
                int margin = 5;
                Vector2 totalSize = font.MeasureString(indexString + infoString);
                Rectangle rect = new Rectangle((int)pos.X - margin, (int)pos.Y - margin, (int)totalSize.X + 2 * margin, (int)totalSize.Y + 2 * margin);
                spriteBatch.Draw(Manager_Sprites.White, destinationRectangle: rect, null, playerColorDark * 0.75f, 0, Vector2.Zero, SpriteEffects.None, 0);

                // Draw text itself
                Vector2 offset = new Vector2(0, font.MeasureString(infoString).Y);
                Util.DrawString(font, indexString, pos, playerColorLight, spriteBatch);
                Util.DrawString(font, infoString, pos, Color.Wheat, spriteBatch);
                pos.Y += totalSize.Y * spacing;
            }
        }
    }
}
