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

        private class MenuTree : MenuItem
        {
            public MenuTree Parent;
            public List<MenuItem> Children;
            public int SelectedIndex;
            public MenuItem Selected { get { return Children[SelectedIndex]; } }

            public MenuTree(string name) : base(name, Menu.Descend)
            {
                Children = new List<MenuItem>(8);
            }

            public MenuTree(string name, MenuTree parent) : this(name)
            {
                Parent = parent;
            }

            public void AddChild(MenuItem item)
            {
                Children.Add(item);
                if (Children.Count == 1) { Selected.IsSelected = true; }
            }

            public void AddChildren(IEnumerable<MenuItem> items)
            {
                bool wasEmpty = Children.Count == 0;
                Children.AddRange(items);
                if (wasEmpty) { Selected.IsSelected = true; }
            }
        }

        private static void Descend()
        {
            if (CurrentSubmenu.Selected is MenuTree)
            {
                CurrentSubmenu = (MenuTree)CurrentSubmenu.Selected;
                RepositionMenuItems();
            }
        }

        private static void Ascend()
        {
            if (CurrentSubmenu.Parent != null)
            {
                CurrentSubmenu = CurrentSubmenu.Parent;
                RepositionMenuItems();
            }
            else
            {
                ReturnToGame();
            }
        }

        private static void ReturnToGame()
        {
            if (Game.State != GameState.Menu ||
                Y_Level.State == Y_Level.GamePlayState.GameOver)
            {
                return;
            }

            Game.DesiredState = GameState.InGame;
        }

        private class MenuItem
        {
            public Vector2 Position;
            public string Text;
            public string String;
            public bool IsSelected;
            public bool IsActive;
            public Action Handler;

            public MenuItem(string text, Action handler)
            {
                Text = text;
                String = Text;
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
                String = Text;

                if (!IsActive)
                {
                    color = Color.Gray;
                }
                else if (IsSelected)
                {
                    String = "> " + Text + "  ";
                    color = Color.Beige;
                }

                Util.DrawString(Fonts.Large, String, GetOffsetPosition(), color, spriteBatch);
            }

            public Vector2 GetOffsetPosition()
            {
                // Offset position by own size to center text
                return Position - Fonts.Large.MeasureString(String) / 2;
            }

            public Rectangle Bounds()
            {
                Vector2 topLeft = GetOffsetPosition();
                Vector2 size = Fonts.Large.MeasureString(String);
                return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y);
            }

            public virtual void Dispatch()
            {
                if (IsActive && Handler != null)
                    Handler();
            }

            internal virtual void Update() { }
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

        private static MenuTree MainMenu;
        private static MenuTree StatsMenu;
        private static MenuTree SettingsMenu;
        private static MenuTree CurrentSubmenu;

        internal static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            MainMenu = new MenuTree("Main Menu");
            CurrentSubmenu = MainMenu;

            SettingsMenu = new MenuTree("Settings", parent: MainMenu);
            SettingsMenu.AddChildren(
                new List<MenuItem>{
                    new SettingsItem("Fullscreen: ", Settings.Fullscreen, toggleFunc: Settings.ToggleFullscreen),
                    new SettingsItem("Show FPS: ", Settings.DrawFPS, toggleFunc: Settings.ToggleDrawFPS),
                    new SettingsItem("Dynamic Shades: ", Settings.DynamicShades, toggleFunc: Settings.ToggleShades),
                    new SettingsItem("Particle Effects: ", Settings.ParticleEffects, toggleFunc: Settings.ToggleParticleEffects),
                    new SettingsItem("Level Outlines: ", Settings.DebugOutlinesLevel, toggleFunc: Settings.ToggleDebugOutlinesLevel),
                    new SettingsItem("Entity Outlines: ", Settings.DebugOutlinesEntities, toggleFunc: Settings.ToggleDebugOutlinesEntities),
                    new SettingsItem("Debug Mode: ", Settings.DebugMode, toggleFunc: Settings.ToggleDebugMode),
                    new SettingsItem("Sound Effects: ", Settings.Sound, toggleFunc: Settings.ToggleSoundEffects),
                    new SettingsItem("Music: ", Settings.Music, toggleFunc: Settings.ToggleMusic),
                    new MenuItem("Back", Menu.Ascend),
                }
            );

            StatsMenu = new MenuTree("Statistics", parent: MainMenu);
            StatsMenu.IsActive = false;
            StatsMenu.AddChild(new MenuItem("Back", Menu.Ascend));

            CurrentSubmenu.AddChildren(
                new List<MenuItem>{
                    new MenuItem("Play", NewGame),
                    new MenuItem("Restart", NewGame, isActive: false),
                    StatsMenu,
                    SettingsMenu,
                    new MenuItem("Quit", Util.Quit)
                }
            );

            RepositionMenuItems();
        }

        internal static void RepositionMenuItems()
        {
            Bounds = Game._graphics.GraphicsDevice.Viewport.Bounds;
            float x = Bounds.Width / 2;
            float y = Bounds.Height / 2;
            for (int i = 0; i < CurrentSubmenu.Children.Count; i++)
            {
                CurrentSubmenu.Children[i].Position = new Vector2(x, y + i * 40);
            }
        }

        private static void NewGame()
        {
            // Turn 'Play' into 'Continue'
            CurrentSubmenu.Children[0] = new MenuItem("Continue", delegate () { Game.DesiredState = GameState.InGame; });

            // Enable the 'Restart' and 'Statistics' menu items
            CurrentSubmenu.Children[1].IsActive = true;
            StatsMenu.IsActive = true;

            // Don't leave the 'Restart' item selected
            SelectMenu(0);
            CurrentSubmenu.Children[0].IsSelected = true;

            RepositionMenuItems();

            Game.StartNewGame();
        }

        public static void GameOver()
        {
            // Disable 'Continue'
            CurrentSubmenu.Children[0].IsActive = false;

            // Select statistics
            SelectMenu(2);

            // Kind of a hack to set the game state here, but we have access to it, so let's do it
            Game.DesiredState = GameState.Menu;
        }

        public static void GameWon()
        {
            // Select statistics
            SelectMenu(2);
            Game.DesiredState = GameState.Menu;
        }

        private static void SelectMenu(int nextSelected)
        {
            if (CurrentSubmenu.Children.Count == 1) { return; }
            if (nextSelected == CurrentSubmenu.SelectedIndex) { return; }
            CurrentSubmenu.Children[CurrentSubmenu.SelectedIndex].IsSelected = false;

            // Wrap index around in both directions
            CurrentSubmenu.SelectedIndex = Util.ProperMod(nextSelected, CurrentSubmenu.Children.Count);
            CurrentSubmenu.Selected.IsSelected = true;
            if (CurrentSubmenu.Children[CurrentSubmenu.SelectedIndex].IsActive)
            {
                Manager_Sound.Sound_MenuSelect.Play(1, 0, 0);
            }
        }

        private static void SelectMenuNext()
        {
            do
            {
                SelectMenu(CurrentSubmenu.SelectedIndex + 1);
            } while (!CurrentSubmenu.Children[CurrentSubmenu.SelectedIndex].IsActive);
        }

        private static void SelectMenuPrev()
        {
            do
            {
                SelectMenu(CurrentSubmenu.SelectedIndex - 1);
            } while (!CurrentSubmenu.Children[CurrentSubmenu.SelectedIndex].IsActive);
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
            foreach (var item in CurrentSubmenu.Children)
            {
                item.Update();
            }

            if (Input.IsKeyTriggered(Keybinds.P1Down) || Input.IsKeyTriggered(Keys.Down) ||
                Input.IsButtonTriggeredAny(Buttons.DPadDown) || Input.IsButtonTriggeredAny(Buttons.LeftThumbstickDown))
            {
                SelectMenuNext();
            }

            if (Input.IsKeyTriggered(Keybinds.P1Up) || Input.IsKeyTriggered(Keys.Up) ||
                Input.IsButtonTriggeredAny(Buttons.DPadUp) || Input.IsButtonTriggeredAny(Buttons.LeftThumbstickUp))
            {
                SelectMenuPrev();
            }

            if (Input.IsKeyTriggered(Keybinds.Enter) || Input.IsKeyTriggered(Keys.Space) ||
                Input.IsButtonTriggeredAny(Buttons.A))
            {
                CurrentSubmenu.Selected.Dispatch();
            }

            if (Input.IsKeyTriggered(Keys.Escape) || Input.IsButtonTriggeredAny(Buttons.Back) ||
                Input.IsButtonTriggeredAny(Buttons.B))
            {
                Ascend();
            }

            // Select item based on mouse hover only if it was moved
            if (Input.HasMouseMoved() || Input.IsLeftMouseClick())
            {
                for (int i = 0; i < CurrentSubmenu.Children.Count; i++)
                {
                    if (CurrentSubmenu.Children[i].Bounds().Contains(Input.GetMousePosition()))
                    {
                        SelectMenu(i);
                        break;
                    }
                }
            }
            if (Input.IsLeftMouseClick() && CurrentSubmenu.Selected.Bounds().Contains(Input.GetMousePosition()))
            {
                CurrentSubmenu.Selected.Dispatch();
            }
        }

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Rectangle Bounds = Game._graphics.GraphicsDevice.Viewport.Bounds;
            foreach (var item in CurrentSubmenu.Children)
            {
                item.Draw(spriteBatch, Bounds);
            }
            if (CurrentSubmenu == StatsMenu)
            {
                UI.DrawPlayerStatistics(gameTime, spriteBatch);
                UI.DrawPlayerStatus(gameTime, spriteBatch);
            }
            else if (CurrentSubmenu == SettingsMenu || Game.State == GameState.PreGame)
            {
                DrawControllerState(spriteBatch);
            }
        }
    }
}
