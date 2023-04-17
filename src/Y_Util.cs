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
        static Game Game;
        static GraphicsDeviceManager Gdm;
        static GameWindow Window;

        internal static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            Gdm = game._graphics;
            Window = game.Window;
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
