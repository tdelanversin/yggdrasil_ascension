using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace YGR
{
    static class Manager_Sound
    {
        public static Song Song_Dramatic;
        public static Song Song_TheWhiteLion; // https://pixabay.com/music/main-title-the-white-lion-10379/
        public static Song Song_Orchestra;
        public static Song Song_Space;

        public static SoundEffect Sound_Bonus;
        public static SoundEffect Sound_Dash;
        public static SoundEffect Sound_EnemyDeath;
        public static SoundEffect Sound_Explosion;
        public static SoundEffect Sound_Fireball;
        public static SoundEffect Sound_GunCocking;
        public static SoundEffect Sound_LevelCleared;
        public static SoundEffect Sound_MenuSelect;
        public static SoundEffect Sound_PlatformActivate;
        public static SoundEffect Sound_PlayerDeath;
        public static SoundEffect Sound_Shotgun;
        public static SoundEffect Sound_VikingHorn;
        public static SoundEffect Sound_CashIn; // https://freesound.org/people/kiddpark/sounds/201159/
        public static SoundEffect Sound_StoneWall;
        public static SoundEffect Sound_Confusion;

        // private static SoundEffect bogus_sound;
        public static Dictionary<IGameElement, SoundEffectInstance> playing_sound_effects;

        private static Dictionary<Func<bool>, Tuple<Stopwatch, SoundEffect>> _registry;

        public static void LoadContent(ContentManager contentManager)
        {
            Song_Dramatic = contentManager.Load<Song>("Sounds/song_dramatic");
            Song_Orchestra = contentManager.Load<Song>("Sounds/song_orchestra");
            Song_TheWhiteLion = contentManager.Load<Song>("Sounds/song_the-white-lion");
            Song_Space = contentManager.Load<Song>("Sounds/song_space");

            Sound_Bonus = contentManager.Load<SoundEffect>("Sounds/bonus_sound");
            Sound_Dash = contentManager.Load<SoundEffect>("Sounds/dash");
            Sound_EnemyDeath = contentManager.Load<SoundEffect>("Sounds/enemy_death_cry");
            Sound_Explosion = contentManager.Load<SoundEffect>("Sounds/explosion");
            Sound_Fireball = contentManager.Load<SoundEffect>("Sounds/short_fireball");
            Sound_GunCocking = contentManager.Load<SoundEffect>("Sounds/gun-cocking-sound");
            Sound_LevelCleared = contentManager.Load<SoundEffect>("Sounds/level_completion");
            Sound_MenuSelect = contentManager.Load<SoundEffect>("Sounds/menu-select");
            Sound_PlatformActivate = contentManager.Load<SoundEffect>("Sounds/platform_activate");
            Sound_PlayerDeath = contentManager.Load<SoundEffect>("Sounds/player_death");
            Sound_Shotgun = contentManager.Load<SoundEffect>("Sounds/shotgun");
            Sound_VikingHorn = contentManager.Load<SoundEffect>("Sounds/viking_horn");
            Sound_CashIn = contentManager.Load<SoundEffect>("Sounds/cash-in");
            Sound_StoneWall = contentManager.Load<SoundEffect>("Sounds/stonewall");
            Sound_Confusion = contentManager.Load<SoundEffect>("Sounds/confusion");

            // Set up media player
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1.0f;

            //byte[] buffer= new byte[10];
            //AudioChannels channels= new AudioChannels();
            //bogus_sound = new SoundEffect(buffer , 0, channels);

            playing_sound_effects = new Dictionary<IGameElement, SoundEffectInstance>();

            _registry = new Dictionary<Func<bool>, Tuple<Stopwatch, SoundEffect>>();
        }

        public static void PlaySoundWhile(Func<bool> condition, ref SoundEffect effect)
        {
            var watch = new Stopwatch();
            if (_registry.TryAdd(condition, new Tuple<Stopwatch, SoundEffect>(watch, effect)))
            {
                watch.Start();
                effect.Play();
            }
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var eff in _registry)
            {
                if (eff.Key() && eff.Value.Item1.ElapsedMilliseconds > eff.Value.Item2.Duration.Milliseconds)
                {
                    eff.Value.Item2.Play();
                    eff.Value.Item1.Reset();
                }
                else if(!eff.Key())
                {
                    _registry.Remove(eff.Key);
                }
            }
        }

        internal static void PlayMainMenuMusic()
        {
            MediaPlayer.Play(Manager_Sound.Song_TheWhiteLion);
        }

        internal static void PlayFreeRoamMusic()
        {
            MediaPlayer.Play(Manager_Sound.Song_Space);
        }

        internal static void PlayBossMusic()
        {
            MediaPlayer.Play(Manager_Sound.Song_Dramatic);
        }

        internal static void PlayEncounterMusic()
        {
            MediaPlayer.Play(Manager_Sound.Song_Orchestra);
        }

        internal static void StopMusic()
        {
            MediaPlayer.Stop();
        }
    }
}
