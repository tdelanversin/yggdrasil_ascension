using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public static class ParticleManager
    {
        private static readonly List<Y_Particle> _particles = new();
        
        public static void AddParticle(Y_Particle particle)
        {
            _particles.Add(particle);
            Logger.Info("Particle Generated");
        }

        public static void UpdateParticles(GameTime gt)
        {
            foreach (var particle in _particles)
            {
                particle.Update(gt);

            }
            _particles.RemoveAll(x => x.isFinished);
        }

        public static void Update(GameTime gt)
        {
            UpdateParticles(gt);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in _particles)
            {
                particle.Draw(spriteBatch);
            }
        }
    }
}
