using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Content;

namespace YGR
{
    static class Manager_Sound
    {
        private static Song song;
        private static SoundEffect explosion;
        private static SoundEffect fireball;
        private static SoundEffect dash;
        private static SoundEffect shotgun;
        private static SoundEffect ennemy_death;
        private static SoundEffect bonus;
        private static SoundEffect level_cleared;
        private static SoundEffect player_death;
        public static Dictionary<int,SoundEffectInstance> playing_sound_effects;

        public static void LoadContent(ContentManager contentManager)
        {
            song = contentManager.Load<Song>("example_intro_song");
            fireball = contentManager.Load<SoundEffect>("better_fireball");
            explosion = contentManager.Load<SoundEffect>("explosion");
            dash = contentManager.Load<SoundEffect>("dash");
            shotgun = contentManager.Load<SoundEffect>("shotgun");
            ennemy_death = contentManager.Load<SoundEffect>("ennemy_death_cry");
            bonus = contentManager.Load<SoundEffect>("bonus_sound");
            level_cleared = contentManager.Load<SoundEffect>("level_completion");
            player_death = contentManager.Load<SoundEffect>("player_death");

            playing_sound_effects = new Dictionary<int, SoundEffectInstance>();
        }   
        public static SoundEffect AddSound_Explosion() { return explosion; }

        public static SoundEffect AddSound_Fireball() { return fireball; }

        public static SoundEffect AddSound_Dash() { return dash; }

        public static SoundEffect AddSound_Shotgun() { return shotgun; }

        public static Song AddSong_Intro() { return song; }

        public static SoundEffect AddSound_Ennemy_Death() {  return ennemy_death; }

        public static SoundEffect AddSound_Bonus() { return bonus; }

        public static SoundEffect AddSound_Level_Clear() {return level_cleared;}


        public static SoundEffect AddSound_Player_Death() { return player_death; }

    }
}
