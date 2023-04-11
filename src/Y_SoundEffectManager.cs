using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


namespace YGR

{
    class Y_SoundEffectManager
    {
        //public List<SoundEffect> SoundEffects;
        private static SoundEffect explosion;
        private static SoundEffect fireball;
        private static SoundEffect dash;
        private static SoundEffect shotgun;

        public static Y_SoundEffectManager instance= new Y_SoundEffectManager();

        public static Y_SoundEffectManager Instance { get { return instance; } }
     
        public static void LoadSounds(ContentManager contentManager)
        {
            fireball = contentManager.Load<SoundEffect>("fireball");
            explosion = contentManager.Load<SoundEffect>("explosion");
            dash = contentManager.Load<SoundEffect>("dash");
            shotgun = contentManager.Load<SoundEffect>("shotgun");

        }
        public SoundEffect GetExplosion() { return explosion; }
        public SoundEffect GetFireball() {  return fireball; }
        public SoundEffect GetDash() { return dash;}

        public SoundEffect GetShotgun() {  return shotgun;}
    }
}
