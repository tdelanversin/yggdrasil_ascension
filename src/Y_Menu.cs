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
            public Action Handler;

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

                if (!IsActive)
                {
                    color = Color.Gray;
                }
                Util.DrawString(Fonts.Large, Text, GetOffsetPosition(), color, spriteBatch);
            }

            public Vector2 GetOffsetPosition()
            {
                // Offset position by own size to center text
                return Position - Fonts.Large.MeasureString(Text) / 2;
            }

            public Rectangle Bounds()
            {
                Vector2 topLeft = GetOffsetPosition();
                Vector2 size = Fonts.Large.MeasureString(Text);
                return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y);
            }

            public virtual void Dispatch()
            {
                if (IsActive && Handler != null)
                    Handler();
            }
        }

        private class SettingsItem : MenuItem
        {
            string BaseText;
            Func<bool> ToggleFunc;
            public SettingsItem(string baseText, bool status, Func<bool> toggleFunc) : base(baseText, null)
            {
                BaseText = baseText;
                this.ToggleFunc = toggleFunc;
                this.UpdateText(status);
            }

            private void UpdateText(bool status)
            {
                string statusText = status ? "On" : "Off";
                this.Text = BaseText + statusText;
            }

            public override void Dispatch()
            {
                if (IsActive && ToggleFunc != null)
                {
                    bool status = ToggleFunc();
                    UpdateText(status);
                }
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
            float y = Bounds.Height / 2;
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

            SelectableItems = new List<MenuItem> {
                new MenuItem("Play", NewGame),
                new MenuItem("Restart", NewGame, isActive: false),
                new SettingsItem("Fullscreen: ", Settings.Fullscreen, toggleFunc: Settings.ToggleFullscreen),
                new SettingsItem("Dynamic Shades: ", Settings.DynamicShades, toggleFunc: Settings.ToggleShades),
                new SettingsItem("Outlines: ", Settings.Outlines, toggleFunc: Settings.ToggleOutlines),
                new SettingsItem("Sound: ", Settings.Sound, toggleFunc: Settings.Toggle_Volume),
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
            SelectableItems[0].IsSelected = true;

            RepositionMenuItems();

            Game.StartNewGame();
        }

        private static void SelectMenu(int nextSelected)
        {
            if (nextSelected == SelectedMenu) { return; }
            SelectableItems[SelectedMenu].IsSelected = false;

            // Wrap index around in both directions
            SelectedMenu = Util.ProperMod(nextSelected, SelectableItems.Count);
            SelectableItems[SelectedMenu].IsSelected = true;
            if (SelectableItems[SelectedMenu].IsActive)
            {
                Manager_Sound.Sound_MenuSelect.Play(1, 0, 0);
            }
        }

        private static void SelectMenuNext()
        {
            do
            {
                SelectMenu(SelectedMenu + 1);
            } while (!SelectableItems[SelectedMenu].IsActive);
        }

        private static void SelectMenuPrev()
        {
            do
            {
                SelectMenu(SelectedMenu - 1);
            } while (!SelectableItems[SelectedMenu].IsActive);
        }

        private static void DrawControllerState(SpriteBatch spriteBatch)
        {
            Bounds = Game._graphics.GraphicsDevice.Viewport.Bounds;
            for (int i = 0; i < 4; i++)
            {
                float x = Bounds.Width * (i + 1) / 6;
                float y = Bounds.Height * 7 / 8;

                string string_a = "Controller " + i + ":  ";
                Util.DrawString(Fonts.Small, string_a, new Vector2(x, y), Color.Wheat, spriteBatch);

                Vector2 stringSize = Fonts.Small.MeasureString(string_a);
                GamePadState gamePadState = GamePad.GetState(i);

                string string_b = "Connected";
                Color color = Color.ForestGreen;
                if (!gamePadState.IsConnected)
                {
                    string_b = "N/A";
                    color = Color.Gray;
                }
                Util.DrawString(Fonts.Small, string_b, new Vector2(x + stringSize.X, y), color, spriteBatch);
            }
        }

        public static void Update()
        {
            if (Input.IsKeyTriggered(Keybinds.P1Down) || Input.IsKeyTriggered(Keys.Down) || Input.IsButtonTriggered(0, Buttons.DPadDown))
            {
                SelectMenuNext();
            }

            if (Input.IsKeyTriggered(Keybinds.P1Up) || Input.IsKeyTriggered(Keys.Up) || Input.IsButtonTriggered(0, Buttons.DPadUp))
            {
                SelectMenuPrev();
            }

            if (Input.IsKeyTriggered(Keybinds.Enter) || Input.IsKeyTriggered(Keys.Space) || Input.IsButtonTriggered(0, Buttons.A))
            {
                SelectableItems[SelectedMenu].Dispatch();
            }

            // Select item based on mouse hover only if it was moved
            if (Input.HasMouseMoved() || Input.IsLeftMouseClick())
            {
                for (int i = 0; i < SelectableItems.Count; i++)
                {
                    if (SelectableItems[i].Bounds().Contains(Input.GetMousePosition()))
                    {
                        SelectMenu(i);
                        break;
                    }
                }
            }
            if (Input.IsLeftMouseClick() && SelectableItems[SelectedMenu].Bounds().Contains(Input.GetMousePosition()))
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
            DrawControllerState(spriteBatch);
        }

        internal static void LoadContent(ContentManager content)
        {
            TitleImage = content.Load<Texture2D>("SpritesOther/title_image");
        }
    }
}
