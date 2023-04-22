using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace YGR
{
    static class Manager_Sound
    {
        private static Song song;
        private static SoundEffect explosion;
        private static SoundEffect fireball;
        private static SoundEffect dash;
        private static SoundEffect shotgun;

        public static void LoadContent(ContentManager contentManager)
        {
            song = contentManager.Load<Song>("example_intro_song");
            fireball = contentManager.Load<SoundEffect>("fireball");
            explosion = contentManager.Load<SoundEffect>("explosion");
            // dash = contentManager.Load<SoundEffect>("dash");
            shotgun = contentManager.Load<SoundEffect>("shotgun");
        }
        public static SoundEffect AddSound_Explosion() { return explosion; }
        public static SoundEffect AddSound_Fireball() { return fireball; }
        public static SoundEffect AddSound_Dash() { return dash; }
        public static SoundEffect AddSound_Shotgun() { return shotgun; }
        public static Song AddSong_Intro() { return song; }
    }
}
