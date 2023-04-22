using Assimp.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public class Y_Particle
    {
        private readonly Y_ParticleData _ParticleData;

        private Vector2 _position;

        private float _lifespanLeft;

        private float _lifespanAmount;

        private float _opacity;

        private Color _color;

        public bool isFinished = false;

        public Y_Particle(Vector2 pos, Y_ParticleData y_ParticleData)
        {
            _ParticleData = y_ParticleData;
            _position = pos;
            _lifespanLeft = y_ParticleData.lifespan;
            _lifespanAmount = 1f;
            _color = y_ParticleData.colorStart;
            _opacity = y_ParticleData.opacitystart;
        }

        public void Update(GameTime gt)
        {

            _lifespanLeft -= (float)gt.ElapsedGameTime.TotalSeconds;
            if (_lifespanLeft <= 0f)
            {
                isFinished = true;
                return;
            }

            _lifespanAmount = MathHelper.Clamp(_lifespanLeft / _ParticleData.lifespan, 0, 1);
            _color = Color.Lerp(_ParticleData.colorEnd, _ParticleData.colorStart, _lifespanAmount);
            _opacity = MathHelper.Clamp(MathHelper.Lerp(_ParticleData.opacityend, _ParticleData.opacitystart, _lifespanAmount), 0, 1);

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Y_ParticleData.texture, _position, null, _color * _opacity, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
        }
    }
}
