using System;
using System.Collections.Generic;
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
        static Random random;

        internal static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            Gdm = game._graphics;
            Window = game.Window;
            random = new Random();
        }

        public static void Quit()
        {
            Game.Exit();
        }

        public static int ProperMod(int i, int m)
        {
            return (i % m + m) % m;
        }

        public static IShooter getRandomGun()
        {
            List<Type> gunTypes = new List<Type> {
                typeof(Y_StarterGun),
                typeof(Y_WideGun),
                typeof(Y_FunkyGun),
                typeof(Y_ShotGun),
            };
            return (IShooter)Activator.CreateInstance(
                gunTypes[random.Next(gunTypes.Count)]
            );
        }

    }
}
