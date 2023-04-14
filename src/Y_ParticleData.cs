using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public  struct Y_ParticleData
    {
        private static Texture2D _defaultTexture;
        public static Texture2D texture;
        public float lifespan = 2f;
        public float opacitystart = 1f;
        public float opacityend = 0f;
        public Color colorStart = Color.Yellow;
        public Color colorEnd = Color.Red;

        public static void LoadParticles(ContentManager contentManager)
        {
            texture = contentManager.Load<Texture2D>("small_dust_cloud");

        }

        public Y_ParticleData()
        {

        }
 
    }
}
