using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace YGR
{
    static class Manager_Sound
    {
        public static List<Song> SongsEncounter = new List<Song>();
        public static List<Song> SongsFreeRoam = new List<Song>();

        public static Song Song_Dramatic;
        public static Song Song_TheWhiteLion; // https://pixabay.com/music/main-title-the-white-lion-10379/
        public static Song Song_Orchestra;
        public static Song Song_Space;
        public static Song Song_Temple; // https://pixabay.com/music/ambient-the-temple-140782/
        public static Song Song_EndingWon; // https://pixabay.com/music/main-title-winning-elevation-111355/
        public static Song Song_EndingLost; // https://pixabay.com/music/main-title-cinematic-epic-trailer-background-music-123922/
        public static Song Song_Encounter02; // https://pixabay.com/music/main-title-chasing-victory-main-9448/

        public static SoundEffect Sound_Bonus;
        public static SoundEffect Sound_EnemyDeath;
        public static SoundEffect Sound_LevelCleared;
        public static SoundEffect Sound_MenuSelect;
        public static SoundEffect Sound_PlayerDeath;
        public static SoundEffect Sound_Wush;

        // Abilities
        public static SoundEffect Sound_Dash;
        public static SoundEffect Sound_Confusion;
        public static SoundEffect Sound_Blank;
        public static SoundEffect Sound_Gunslinger;
        public static SoundEffect Sound_Invincibility;

        // Power Ups
        public static SoundEffect Sound_CashIn; // https://freesound.org/people/kiddpark/sounds/201159/
        public static SoundEffect Sound_GunCocking;
        public static SoundEffect Sound_PosititveRandomPowerup;
        public static SoundEffect Sound_NegativeRandomPowerup;

        // Level sounds
        public static SoundEffect Sound_VikingHorn;
        public static SoundEffect Sound_RisingTension; // https://pixabay.com/sound-effects/riserhit-109796/

        // Other Weapons
        public static SoundEffect Sound_Fireball;
        public static SoundEffect Sound_Fireball2;
        public static SoundEffect Sound_Shotgun;
        public static SoundEffect Sound_Blaster;
        public static SoundEffect Sound_PlasmaPistol;
        public static SoundEffect Sound_OmniShotGun;
        public static SoundEffect Sound_Sword;
        public static SoundEffect Sound_SniperShot;
        public static SoundEffect Sound_Explosion;

        // Environmental
        public static SoundEffect Sound_StoneWall;
        public static SoundEffect Sound_PlatformActivate;
        public static SoundEffect Sound_BrutalPunch; // https://pixabay.com/sound-effects/hit-brutal-puncher-cinematic-trailer-sound-effects-124760/
        public static SoundEffect Sound_LowGravImpact; // https://pixabay.com/sound-effects/hit-low-gravity-absorber-cinematic-trailer-sound-effects-124761/
        public static SoundEffect Sound_Punch;

        // Class weapons
        public static SoundEffect Sound_Ninja1;
        public static SoundEffect Sound_Ninja2;
        public static SoundEffect Sound_Ninja3;
        public static SoundEffect Sound_Book1;
        public static SoundEffect Sound_Book2;
        public static SoundEffect Sound_Book3;
        public static SoundEffect Sound_Keyboard1;
        public static SoundEffect Sound_Keyboard2;
        public static SoundEffect Sound_Keyboard3;
        public static SoundEffect Sound_Keyboard4;
        public static SoundEffect Sound_Keyboard5;
        public static SoundEffect Sound_Letter1;
        public static SoundEffect Sound_Letter2;
        public static SoundEffect Sound_Letter3;
        public static SoundEffect Sound_Letter4;
        public static SoundEffect Sound_Slime4;
        public static SoundEffect Sound_Slime5;
        public static SoundEffect Sound_Slime6;
        public static SoundEffect Sound_Slime7;
        public static SoundEffect Sound_Slime8;
        public static SoundEffect Sound_Splash1;
        public static SoundEffect Sound_Splash2;
        public static SoundEffect Sound_Splash3;
        public static SoundEffect Sound_Splash4;

        public static float MusicVolume = 0.5f;
        public static float SoundVolume = 0.5f;

        // private static SoundEffect bogus_sound;
        public static Dictionary<IGameElement, SoundEffectInstance> playing_sound_effects;

        private static Dictionary<Func<bool>, Tuple<Stopwatch, SoundEffect>> _registry;

        public static void LoadContent(ContentManager contentManager)
        {
            Song_Dramatic = contentManager.Load<Song>("Sounds/song_dramatic");
            Song_Orchestra = contentManager.Load<Song>("Sounds/song_orchestra");
            Song_TheWhiteLion = contentManager.Load<Song>("Sounds/song_the-white-lion");
            Song_Space = contentManager.Load<Song>("Sounds/song_space");
            Song_EndingLost = contentManager.Load<Song>("Sounds/song_ending-lost");
            Song_EndingWon = contentManager.Load<Song>("Sounds/song_ending-won");
            Song_Encounter02 = contentManager.Load<Song>("Sounds/song_encounter-02");
            Song_Temple = contentManager.Load<Song>("Sounds/song_temple");

            SongsEncounter.Add(Song_Orchestra);
            SongsEncounter.Add(Song_Encounter02);

            SongsFreeRoam.Add(Song_Space);
            SongsFreeRoam.Add(Song_Temple);

            Sound_Bonus = contentManager.Load<SoundEffect>("Sounds/bonus_sound");
            Sound_Dash = contentManager.Load<SoundEffect>("Sounds/dash");
            Sound_EnemyDeath = contentManager.Load<SoundEffect>("Sounds/enemy_death_cry"); //TODO: change to slime change
            Sound_Explosion = contentManager.Load<SoundEffect>("Sounds/explosion");
            Sound_Fireball = contentManager.Load<SoundEffect>("Sounds/short_fireball");
            Sound_Fireball2 = contentManager.Load<SoundEffect>("Sounds/fireball");
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
            Sound_Blank = contentManager.Load<SoundEffect>("Sounds/cash-in"); //TODO: replace with blank sound
            Sound_Gunslinger = contentManager.Load<SoundEffect>("Sounds/cash-in"); //TODO: replace with gunslinger sound
            Sound_Invincibility = contentManager.Load<SoundEffect>("Sounds/cash-in"); //TODO: replace with invincibility sound
            Sound_Blaster = contentManager.Load<SoundEffect>("Sounds/blaster");
            Sound_PlasmaPistol = contentManager.Load<SoundEffect>("Sounds/plasmapistol");
            Sound_OmniShotGun = contentManager.Load<SoundEffect>("Sounds/omni_shotgun");
            Sound_Wush = contentManager.Load<SoundEffect>("Sounds/wush");
            Sound_PosititveRandomPowerup = contentManager.Load<SoundEffect>("Sounds/short-success-sound-glockenspiel-treasure-video-game-6346");
            Sound_NegativeRandomPowerup = contentManager.Load<SoundEffect>("Sounds/negative_beeps-6008");
            Sound_Sword = contentManager.Load<SoundEffect>("Sounds/sword");
            Sound_SniperShot = contentManager.Load<SoundEffect>("Sounds/sniper-rifle-firing");

            Sound_RisingTension = contentManager.Load<SoundEffect>("Sounds/rising-tension");

            Sound_BrutalPunch = contentManager.Load<SoundEffect>("Sounds/impact/brutal-punch");
            Sound_LowGravImpact = contentManager.Load<SoundEffect>("Sounds/impact/low-gravity-impact");
            Sound_Punch = contentManager.Load<SoundEffect>("Sounds/impact/punch");

            Sound_Ninja1 = contentManager.Load<SoundEffect>("Sounds/ninja_1");
            Sound_Ninja2 = contentManager.Load<SoundEffect>("Sounds/ninja_2");
            Sound_Ninja3 = contentManager.Load<SoundEffect>("Sounds/ninja_3");
            Sound_Book1 = contentManager.Load<SoundEffect>("Sounds/book_1");
            Sound_Book2 = contentManager.Load<SoundEffect>("Sounds/book_2");
            Sound_Book3 = contentManager.Load<SoundEffect>("Sounds/book_3");
            Sound_Keyboard1 = contentManager.Load<SoundEffect>("Sounds/keyboard_1");
            Sound_Keyboard2 = contentManager.Load<SoundEffect>("Sounds/keyboard_2");
            Sound_Keyboard3 = contentManager.Load<SoundEffect>("Sounds/keyboard_3");
            Sound_Keyboard4 = contentManager.Load<SoundEffect>("Sounds/keyboard_4");
            Sound_Keyboard5 = contentManager.Load<SoundEffect>("Sounds/keyboard_5");
            Sound_Letter1 = contentManager.Load<SoundEffect>("Sounds/letter_1");
            Sound_Letter2 = contentManager.Load<SoundEffect>("Sounds/letter_2");
            Sound_Letter3 = contentManager.Load<SoundEffect>("Sounds/letter_3");
            Sound_Letter4 = contentManager.Load<SoundEffect>("Sounds/letter_4");
            Sound_Slime4 = contentManager.Load<SoundEffect>("Sounds/slime_4");
            Sound_Slime5 = contentManager.Load<SoundEffect>("Sounds/slime_5");
            Sound_Slime6 = contentManager.Load<SoundEffect>("Sounds/slime_6");
            Sound_Slime7 = contentManager.Load<SoundEffect>("Sounds/slime_7");
            Sound_Slime8 = contentManager.Load<SoundEffect>("Sounds/slime_8");
            Sound_Splash1 = contentManager.Load<SoundEffect>("Sounds/splash_1");
            Sound_Splash2 = contentManager.Load<SoundEffect>("Sounds/splash_2");
            Sound_Splash3 = contentManager.Load<SoundEffect>("Sounds/splash_3");
            Sound_Splash4 = contentManager.Load<SoundEffect>("Sounds/splash_4");

            // Set up media player
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 1.0f;
            SoundEffect.MasterVolume = .5f;

            //byte[] buffer= new byte[10];
            //AudioChannels channels= new AudioChannels();
            //bogus_sound = new SoundEffect(buffer , 0, channels);

            playing_sound_effects = new Dictionary<IGameElement, SoundEffectInstance>();

            _registry = new Dictionary<Func<bool>, Tuple<Stopwatch, SoundEffect>>();
        }

        public static void PlaySoundWhile(Func<bool> condition, ref SoundEffect effect, float volume = 1.0f)
        {
            var watch = new Stopwatch();
            if (_registry.TryAdd(condition, new Tuple<Stopwatch, SoundEffect>(watch, effect)))
            {
                watch.Start();
                effect.Play(volume, 0, 0);
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
                else if (!eff.Key())
                {
                    _registry.Remove(eff.Key);
                }
            }
        }

        internal static void PlayMainMenuMusic()
        {
            MediaPlayer.Play(Manager_Sound.Song_TheWhiteLion);
            // (Re)set the volume, since we fade out the song on the ending screen
            MediaPlayer.Volume = 1.0f;
        }

        internal static void PlayFreeRoamMusic()
        {
            // Play a random free roam song
            MediaPlayer.Play(SongsFreeRoam[Util.random.Next(SongsEncounter.Count)]);
        }

        internal static void PlayBossMusic()
        {
            MediaPlayer.Play(Manager_Sound.Song_Dramatic);
        }

        internal static void PlayEncounterMusic()
        {
            // Play a random encounter song
            MediaPlayer.Play(SongsEncounter[Util.random.Next(SongsEncounter.Count)]);
        }

        internal static void PlaySongEndingWin()
        {
            MediaPlayer.Play(Manager_Sound.Song_EndingWon);
        }

        internal static void PlaySongEndingLose()
        {
            MediaPlayer.Play(Manager_Sound.Song_EndingLost);
        }

        internal static void StopMusic()
        {
            MediaPlayer.Stop();
        }
    }
}
