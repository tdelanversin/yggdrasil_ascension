using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace YGR
{

    public static class Settings
    {
        public const int RES_X = 1600;
        public const int RES_Y = 900;
        static Game Game;
        static GraphicsDeviceManager Gdm;
        static GameWindow Window;
        public static bool Fullscreen;
        public static bool DynamicShades;
        public static bool Outlines;
        public static bool DrawFPS;
        public static bool Sound;

        public static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            Gdm = game._graphics;
            Window = game.Window;


            // Defaults
            DrawFPS = true; // Always on for now, shouldn't really bother anyone
            Sound = true;
#if DEBUG
            Fullscreen = false;
            DynamicShades = false;
            Outlines = false;
            Toggle_Volume();
#else
            Fullscreen = true;
            DynamicShades = false; // Off by default, not because they are slow (they are in fact very quick) but because Monogame sucks so much!!!
            Outlines = false;
#endif
        }

        internal static bool ToggleShades()
        {
            DynamicShades = !DynamicShades;
            return DynamicShades;
        }

        internal static bool ToggleOutlines()
        {
            Outlines = !Outlines;
            return Outlines;
        }

        internal static bool ToggleDrawFPS()
        {
            DrawFPS = !DrawFPS;
            return DrawFPS;
        }

        public static void ApplyScreenConfiguration()
        {
            if (Fullscreen)
            {
                Gdm.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Gdm.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Gdm.IsFullScreen = true;
            }
            else
            {
                Gdm.PreferredBackBufferWidth = RES_X;
                Gdm.PreferredBackBufferHeight = RES_Y;
                Gdm.IsFullScreen = false;
            }
            Gdm.ApplyChanges();
            Menu.RepositionMenuItems();
        }

        public static bool ToggleFullscreen()
        {
            if (Fullscreen)
            {
                Fullscreen = false;
                ApplyScreenConfiguration();
                Logger.Debug("Turning fullscreen OFF. Resolution: " + RES_X.ToString() + "x" + RES_Y.ToString());
            }
            else
            {
                Fullscreen = true;
                ApplyScreenConfiguration();
                Logger.Debug("Turning fullscreen ON. Resolution: " + Gdm.PreferredBackBufferWidth.ToString() + "x" + Gdm.PreferredBackBufferHeight.ToString());
            }
            return Fullscreen;
        }

        public static bool Toggle_Volume()
        {
            Sound = !Sound;
            if (!Sound)
            {
                SoundEffect.MasterVolume = 0f;
                MediaPlayer.IsMuted = true;
            }
            else
            {
                SoundEffect.MasterVolume = 1f;
                MediaPlayer.IsMuted = false;
            }
            return Sound;
        }
    }
}
