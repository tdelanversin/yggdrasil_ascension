using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Collections.Generic;

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
        public static Random random;


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
                typeof(Gun_Basic),
                // typeof(Gun_Wide), // Make game child friendly for now
                // typeof(Gun_Funky),
                typeof(Gun_ShotGun),
            };
            return (IShooter)Activator.CreateInstance(
                gunTypes[random.Next(gunTypes.Count)]
            );
        }

        public static string PathOsNormalization(string path)
        {
            var sepC = Path.DirectorySeparatorChar;

            // switch separators if necessary
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = path.Replace('/', sepC);
            }
            else
            {
                path = path.Replace('\\', sepC);
            }

            if (path != Path.GetFullPath(path))
            {
                // make path relative
                var sep = sepC.ToString();
                if (path.Substring(0, 1) == sep)
                    path = "." + path;
                else if (path.Substring(0, 2) != ("." + sep))
                    path = "." + sep + path;
            }

            if (!Path.EndsInDirectorySeparator(path))
            {
                path += sepC;
            }

            path = path.Replace((sepC + "." + sepC).ToString(), sepC.ToString());

            return path;
        }

        private static string GetThisFilePath([CallerFilePath] string path = null)
        {
            return path;
        }

        public static string GetSrcDirectory()
        {
            var path = GetThisFilePath();
            var srcDirectory = Path.GetDirectoryName(path);
            return srcDirectory;
        }

        public static string GetAbsResourceFolderPath(string resourceFolder)
        {
            if (resourceFolder.Substring(0, 1) == ".") resourceFolder = resourceFolder.Substring(1, resourceFolder.Length - 1);
            if (resourceFolder.Substring(0, 1) == Path.DirectorySeparatorChar.ToString()) resourceFolder = resourceFolder.Substring(1, resourceFolder.Length - 1);

            var srcPath = Path.Join(Util.GetSrcDirectory(), resourceFolder);

            return srcPath;
        }

        public static string CreateGenericIdentifier()
        {
            var time = DateAndTime.Now;
            var random = new Random();

            return time.Year.ToString() +
                    time.Month.ToString().PadLeft(2, '0') +
                    time.Day.ToString().PadLeft(2, '0') +
                    time.Hour.ToString().PadLeft(2, '0') +
                    time.Minute.ToString().PadLeft(2, '0') +
                    time.Second.ToString().PadLeft(2, '0') +
                    time.Millisecond.ToString() +
                    "_" +
                    random.Next().ToString();
        }
    }
}
