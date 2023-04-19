using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Runtime.InteropServices;

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

            if (!path.Contains(Path.VolumeSeparatorChar))
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
            if (resourceFolder.Substring(0,1) == ".") resourceFolder = resourceFolder.Substring(1, resourceFolder.Length - 1);
            if (resourceFolder.Substring(0,1) == Path.DirectorySeparatorChar.ToString()) resourceFolder = resourceFolder.Substring(1, resourceFolder.Length - 1);

            var srcPath = Path.Join(Util.GetSrcDirectory(), resourceFolder);

            return srcPath;
        }
    }
}
