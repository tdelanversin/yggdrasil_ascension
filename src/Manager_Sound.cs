using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace YGR
{
    static class Manager_Sound
    {
        public static SoundEffect Song_Dramatic;
        public static SoundEffectInstance SongInstance_Dramatic;
        public static SoundEffect Sound_Explosion;
        public static SoundEffect Sound_Fireball;
        public static SoundEffect Sound_Dash;
        public static SoundEffect Sound_Shotgun;
        public static SoundEffect Sound_GunCocking;
        public static SoundEffect Sound_EnemyDeath;
        public static SoundEffect Sound_Bonus;
        public static SoundEffect Sound_PlatformActivate;
        public static SoundEffect Sound_LevelCleared;
        public static SoundEffect Sound_PlayerDeath;
        public static SoundEffect Sound_MenuSelect;

        // private static SoundEffect bogus_sound;
        public static Dictionary<IGameElement, SoundEffectInstance> playing_sound_effects;

        public static void LoadContent(ContentManager contentManager)
        {
            Song_Dramatic = contentManager.Load<SoundEffect>("Sounds/dramatic_music");
            Sound_Fireball = contentManager.Load<SoundEffect>("Sounds/better_fireball");
            Sound_Explosion = contentManager.Load<SoundEffect>("Sounds/explosion");
            Sound_Dash = contentManager.Load<SoundEffect>("Sounds/dash");
            Sound_Shotgun = contentManager.Load<SoundEffect>("Sounds/shotgun");
            Sound_EnemyDeath = contentManager.Load<SoundEffect>("Sounds/enemy_death_cry");
            Sound_Bonus = contentManager.Load<SoundEffect>("Sounds/bonus_sound");
            Sound_LevelCleared = contentManager.Load<SoundEffect>("Sounds/level_completion");
            Sound_PlayerDeath = contentManager.Load<SoundEffect>("Sounds/player_death");
            Sound_MenuSelect = contentManager.Load<SoundEffect>("Sounds/menu-select");
            Sound_GunCocking = contentManager.Load<SoundEffect>("Sounds/gun-cocking-sound");
            Sound_PlatformActivate = contentManager.Load<SoundEffect>("Sounds/platform_activate");

            SongInstance_Dramatic = Song_Dramatic.CreateInstance();
            SongInstance_Dramatic.Volume = 0.6f;

            //byte[] buffer= new byte[10];
            //AudioChannels channels= new AudioChannels();
            //bogus_sound = new SoundEffect(buffer , 0, channels);

            playing_sound_effects = new Dictionary<IGameElement, SoundEffectInstance>();
        }
    }
}
