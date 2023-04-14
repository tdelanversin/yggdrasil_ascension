using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace YGR
{

    public static class Menu
    {
        private class MenuItem
        {
            public Vector2 Position;
            public string Text;
            public bool IsSelected;
            public bool IsActive;
            Action Handler;

            public MenuItem(string text, Action handler)
            {
                Text = text;
                IsSelected = false;
                IsActive = true;
                Handler = handler;
            }

            public MenuItem(string text, Action handler, bool isActive) : this(text, handler)
            {
                IsActive = isActive;
            }

            public void Draw(SpriteBatch spriteBatch, Rectangle bounds)
            {
                var color = Color.BurlyWood;
                if (IsSelected)
                {
                    color = Color.Beige;
                }

                if (!IsActive) {
                    color = Color.Gray;
                }
                spriteBatch.DrawString(Fonts.Large, Text, GetOffsetPosition(), color);
            }

            public Vector2 GetOffsetPosition()
            {
                // Offset position by own size to center text
                return Position - Fonts.Large.MeasureString(Text) / 2;
            }

            public void Dispatch()
            {
                if (IsActive && Handler != null)
                    Handler();
            }
        }

        private static A_Yggdrasil Game;
        private static Rectangle Bounds;
        private static Texture2D TitleImage;
        private static int SelectedMenu;
        private static IList<MenuItem> SelectableItems;

        internal static void RepositionMenuItems()
        {
            Bounds = Game._graphics.GraphicsDevice.Viewport.Bounds;
            float x = Bounds.Width / 2;
            float y = Bounds.Height * 3 / 5;
            for (int i = 0; i < SelectableItems.Count; i++)
            {
                SelectableItems[i].Position = new Vector2(x, y + i * 40);
            }

        }
        internal static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            SelectedMenu = 0;

            // Need to compensate here for all the times I had to click "Exit to Windows" on Linux 🤦‍♂️
            string os_exit_string = Environment.OSVersion.ToString();
            if (os_exit_string.Contains("Unix"))
            {
                os_exit_string = "Exit to Linux Desktop";
            }
            else if (os_exit_string.Contains("Windows"))
            {
                os_exit_string = "Exit to Windows";
            }
            else
            {
                os_exit_string = "Exit to Desktop"; // MacOS whatever
            }

            SelectableItems = new List<MenuItem>{
                new MenuItem("Play", NewGame),
                new MenuItem("Restart", NewGame, isActive: false),
                new MenuItem("Toggle Fullscreen", Util.ToggleFullscreen),
                new MenuItem(os_exit_string, Util.Quit),
            };
            SelectableItems[SelectedMenu].IsSelected = true;
            RepositionMenuItems();
        }

        private static void NewGame()
        {
            // Turn 'Play' into 'Continue'
            SelectableItems[0] = new MenuItem("Continue", delegate () { Game.DesiredState = GameState.InGame; });

            // Enable the 'Restart' menu item
            SelectableItems[1].IsActive = true;

            // Don't leave the 'Restart' item selected
            SelectMenu(0);

            RepositionMenuItems();

            Game.StartNewGame();
        }

        private static void SelectMenu(int nextSelected)
        {
            SelectableItems[SelectedMenu].IsSelected = false;
            // Wrap index around in both directions
            SelectedMenu = (nextSelected % SelectableItems.Count + SelectableItems.Count) % SelectableItems.Count;
            SelectableItems[SelectedMenu].IsSelected = true;
        }

        public static void Update()
        {
            if (Keyboard.HasBeenPressed(Keybinds.P1Down))
            {
                SelectMenu(SelectedMenu + 1);
            }

            if (Keyboard.HasBeenPressed(Keybinds.P1Up))
            {
                SelectMenu(SelectedMenu - 1);
            }

            if (Keyboard.HasBeenPressed(Keybinds.Enter))
            {
                SelectableItems[SelectedMenu].Dispatch();
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Rectangle Bounds = Game._graphics.GraphicsDevice.Viewport.Bounds;
            spriteBatch.Draw(TitleImage, Bounds, Color.White);
            foreach (var item in SelectableItems)
            {
                item.Draw(spriteBatch, Bounds);
            }
        }

        internal static void LoadContent(ContentManager content)
        {
            TitleImage = content.Load<Texture2D>("title_image");
        }
    }
}
