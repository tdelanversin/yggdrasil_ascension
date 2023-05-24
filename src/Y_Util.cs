using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Reflection;
using System.IO.Compression;
using Newtonsoft.Json;

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

        /* Clamp the length of a vector to the specified length */
        public static Vector2 ClampMagnitude(Vector2 v, float max)
        {
            var r = v;
            var factor = v.Length() / max;
            if (factor > 1)
            {
                r = v / factor;
            }
            return r;
        }

        public static IShooter getRandomGun(IVictim owner)
        {
            // give them back their old gun instead of a random one
            if (owner is Player_Mailman) return (IShooter)(Activator.CreateInstance(typeof(Gun_Letter), owner));
            else if (owner is Player_NerdyGirl) return (IShooter)(Activator.CreateInstance(typeof(Gun_Keyboard), owner));
            else if (owner is Player_Professor) return (IShooter)(Activator.CreateInstance(typeof(Gun_Book), owner));
            else return (IShooter)(Activator.CreateInstance(typeof(Gun_NinjaStar), owner));
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

        public static void output(int[][] pattern, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.GetLength(0); ++x)
            {
                s += string.Join("\t", pattern[x]);
                s += "\n";
            }

            File.WriteAllText(name, s);
        }

        public static void output(bool[] pattern, int sideLen, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.Length; ++x)
            {
                if (x > 0 && x % sideLen == 0) s += "\n";
                s += "\t" + (pattern[x] ? "1" : "0");
            }

            File.WriteAllText(name, s);
        }

        public static float GetSpriteScale(Rectangle rect, Vector2 spriteDimension)
        {
            return Math.Min(
                rect.Width / spriteDimension.X,
                rect.Height / spriteDimension.Y
            );
        }

        public static Color[] ByteToColorArray(byte[] input)
        {
            Color[] output = new Color[input.Length / 4];
            int index = 0;
            for (int i = 0; i < input.Length - 4; i += 4)
            {
                output[index].R = input[i];
                output[index].G = input[i + 1];
                output[index].B = input[i + 2];
                output[index].A = input[i + 3];
                index++;
            }

            return output;
        }

        public static void SaveAsGZip(string filePath, byte[] data)
        {
            using FileStream compressedFileStream = File.Create(filePath);
            using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
            var stream = new MemoryStream(data);
            stream.CopyTo(compressor);
            stream.Close();

            //File.WriteAllBytes(compressedFileStream);
        }

        public static Color[] DeGZipFile(byte[] data)
        {
            var stream = new MemoryStream(data);
            using var decompressor = new GZipStream(stream, CompressionMode.Decompress);
            MemoryStream output = new MemoryStream();
            decompressor.CopyTo(output);
            return ByteToColorArray(output.ToArray());
        }

        public static List<T> LoadJSON<T>(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<T> items = JsonConvert.DeserializeObject<List<T>>(json);
                return items;
            }
        }

        public static void WriteJSON<T>(string path, List<T> data)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                // Serialize object directly into file stream
                serializer.Serialize(file, data);
            }
        }

        public static string OSExitString()
        {
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
            return os_exit_string;
        }

        /// <summary>
        /// Draw a string. But with shadows. So that it's readable.
        /// </summary>
        public static void DrawString(SpriteFont font, string text, Vector2 position, Color color, SpriteBatch spriteBatch)
        {
            Color shadow = Color.Black;
            shadow.A = color.A;
            spriteBatch.DrawString(font, text, position + 2 * Vector2.One, shadow);
            spriteBatch.DrawString(font, text, position, color);
        }
    }
}
