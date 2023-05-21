using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace YGR
{

    public sealed class SettingsObject
    {
        public DateTime Datetime;
        public int Version;
        public bool Fullscreen;
        public bool DynamicShades;
        public bool Outlines;
        public bool DrawFPS;
        public bool Sound;
        public bool Music;

        public SettingsObject()
        {
            Datetime = DateTime.Now;
        }
    }

    public static class Settings
    {
        public const int RES_X = 1600;
        public const int RES_Y = 900;
        static Game Game;
        static GraphicsDeviceManager Gdm;
        static GameWindow Window;
        public static bool Fullscreen;
        public static bool DynamicShades;
        public static bool ParticleEffects;
        public static bool DebugOutlinesLevel;
        public static bool DebugOutlinesEntities;
        public static bool DebugMode;
        public static bool DrawFPS;
        public static bool Sound;
        public static bool Music;

        public static string SettingsPath;

        public static void Initialize(A_Yggdrasil game)
        {
            Game = game;
            Gdm = game._graphics;
            Window = game.Window;

            // Defaults
            Fullscreen = true;
            DrawFPS = false; // Always on for now, shouldn't really bother anyone
            DynamicShades = false; // Off by default, not because they are slow (they are in fact very quick) but because Monogame sucks so much!!!
            DebugOutlinesLevel = false;
            Sound = true;
            Music = true;
            DebugMode = true;
            ParticleEffects = true; // TODO: disable if not fixed by jury release
#if DEBUG
            Fullscreen = false;
            DebugOutlinesLevel = false;
            DebugMode = true;
            DrawFPS = true;
#endif

            /*
            var LocalAppDataDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var GameDataDir = System.IO.Path.Combine(LocalAppDataDir, "YggdrasilAscension");
            var DirInfo = System.IO.Directory.CreateDirectory(GameDataDir);
            SettingsPath = Path.Combine(GameDataDir, "settings.json");

            if (File.Exists(SettingsPath))
            {
                SettingsObject settings = Util.LoadJSON<SettingsObject>(SettingsPath)[0];
                LoadSettings(settings);
            }
            else
            {
                SaveSettings();
            }
            */

            ApplySoundSettings();
        }

        private static void LoadSettings(SettingsObject so)
        {
            Fullscreen = so.Fullscreen;
            DynamicShades = so.DynamicShades;
            DebugOutlinesLevel = so.Outlines;
            DrawFPS = so.DrawFPS;
            Sound = so.Sound;
            Music = so.Music;
        }

        private static void SaveSettings()
        {
            // SettingsObject settings = new SettingsObject();
            // settings.Fullscreen = Fullscreen;
            // settings.DynamicShades = DynamicShades;
            // settings.Outlines = Outlines;
            // settings.DrawFPS = DrawFPS;
            // settings.Sound = Sound;
            // settings.Music = Music;

            // var settingsData = new List<SettingsObject> { settings };
            // Util.WriteJSON(SettingsPath, settingsData);
        }

        internal static void ApplySoundSettings()
        {
            if (!Sound)
            {
                ToggleSoundEffects();
            }

            if (!Music)
            {
                ToggleSoundEffects();
            }
        }

        internal static bool ToggleShades()
        {
            DynamicShades = !DynamicShades;
            SaveSettings();
            return DynamicShades;
        }

        internal static bool ToggleParticleEffects()
        {
            ParticleEffects = !ParticleEffects;
            SaveSettings();
            return ParticleEffects;
        }

        internal static bool ToggleDebugOutlinesLevel()
        {
            DebugOutlinesLevel = !DebugOutlinesLevel;
            SaveSettings();
            return DebugOutlinesLevel;
        }

        internal static bool ToggleDebugOutlinesEntities()
        {
            DebugOutlinesEntities = !DebugOutlinesEntities;
            SaveSettings();
            return DebugOutlinesEntities;
        }

        internal static bool ToggleDebugMode()
        {
            DebugMode = !DebugMode;
            SaveSettings();
            return DebugMode;
        }

        internal static bool ToggleDrawFPS()
        {
            DrawFPS = !DrawFPS;
            SaveSettings();
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
            SaveSettings();
            return Fullscreen;
        }

        public static bool ToggleSoundEffects()
        {
            Sound = !Sound;
            if (!Sound)
            {
                SoundEffect.MasterVolume = 0f;
            }
            else
            {
                SoundEffect.MasterVolume = 1f;
            }
            SaveSettings();
            return Sound;
        }

        public static bool ToggleMusic()
        {
            Music = !Music;
            if (!Music)
            {
                MediaPlayer.IsMuted = true;
            }
            else
            {
                MediaPlayer.IsMuted = false;
            }
            SaveSettings();
            return Music;
        }
    }
}
