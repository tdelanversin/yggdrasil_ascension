using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{

    public static class Settings
    {
        public const int RES_X = 1280;
        public const int RES_Y = 720;
        static Game Game;
        static GraphicsDeviceManager Gdm;
        static GameWindow Window;
        public static bool Lighting;
        public static bool Outlines;

        public static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            Gdm = game._graphics;
            Window = game.Window;

            // Defaults
            Lighting = true;
            Outlines = false;
        }

        internal static bool ToggleLighting()
        {
            Lighting = !Lighting;
            return Lighting;
        }

        internal static bool ToggleOutlines()
        {
            Outlines = !Outlines;
            return Outlines;
        }

        public static bool ToggleFullscreen()
        {
            bool ret;
            if (Gdm.IsFullScreen)
            {
                Gdm.PreferredBackBufferWidth = RES_X;
                Gdm.PreferredBackBufferHeight = RES_Y;
                Gdm.IsFullScreen = false;
                Logger.Info("Turning fullscreen OFF. Resolution: " + RES_X.ToString() + "x" + RES_Y.ToString());
                ret = false;
            }
            else
            {
                Gdm.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Gdm.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Gdm.IsFullScreen = true;
                Logger.Info("Turning fullscreen ON. Resolution: " + Gdm.PreferredBackBufferWidth.ToString() + "x" + Gdm.PreferredBackBufferHeight.ToString());
                ret = true;
            }
            Gdm.ApplyChanges();
            Menu.RepositionMenuItems();
            return ret;
        }
    }
}
