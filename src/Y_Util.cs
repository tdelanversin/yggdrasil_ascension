using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/*
 * Useful static methods
 */

namespace YGR
{
    public static class Util
    {
        public const int RES_X = 1280;
        public const int RES_Y = 720;
        static Game Game;
        static GraphicsDeviceManager Gdm;
        static GameWindow Window;

        internal static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            Gdm = game._graphics;
            Window = game.Window;
        }

        public static void ToggleFullscreen()
        {
            if (Gdm.IsFullScreen)
            {
                Gdm.PreferredBackBufferWidth = RES_X;
                Gdm.PreferredBackBufferHeight = RES_Y;
                Gdm.IsFullScreen = false;
                Logger.Info("Turning fullscreen OFF. Resolution: " + RES_X.ToString() + "x" + RES_Y.ToString());
            }
            else
            {
                Gdm.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                Gdm.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                Gdm.IsFullScreen = true;
                Logger.Info("Turning fullscreen ON. Resolution: " + Gdm.PreferredBackBufferWidth.ToString() + "x" + Gdm.PreferredBackBufferHeight.ToString());
            }
            Gdm.ApplyChanges();
        }

        public static void Quit()
        {
            Game.Exit();
        }

        public static int ProperMod(int i, int m) {
            return (i % m + m) % m;
        }
    }
}
