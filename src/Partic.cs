using System;
using System.Collections.Generic;
using System.Drawing;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using Microsoft.Xna.Framework.Content;
using Vector2=Microsoft.Xna.Framework.Vector2;

namespace YGR
{
    public static class Partic
    {
        private static ParticleEffect _particleEffect;
        private static Texture2D _particleTexture;


        internal static void GetParticles( Vector2 pos, ContentManager content)
        {
            _particleTexture = content.Load<Texture2D>("dust_particle");// new Texture2D(graphicsDevice, 1, 1);
            //_particleTexture.SetData(new[] { Microsoft.Xna.Framework.Color.White });

            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture);
            _particleEffect = new ParticleEffect(autoTrigger: false)
            {
                Position = pos,//new Vector2(400, 240),
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1),
                        Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 20f),
                            Quantity = 10,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(0.03f, 0.04f)
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators =
                                {
                                    //new ColorInterpolator
                                    //{
                                    //    StartValue = new HslColor(0.33f, 0.5f, 0.5f),
                                    //    EndValue = new HslColor(0.5f, 0.9f, 1.0f)
                                    //}
                                }
                            },
                            new VelocityColorModifier
                            {
                                StationaryColor = Microsoft.Xna.Framework.Color.Green.ToHsl(),
                                VelocityColor = Microsoft.Xna.Framework.Color.Blue.ToHsl(),
                                VelocityThreshold = 80f
                            },
                            //new RotationModifier {RotationRate = -2.1f},
                            new RectangleContainerModifier {Width = 800, Height = 480},
                            new LinearGravityModifier {Direction = -Microsoft.Xna.Framework.Vector2.UnitY, Strength = 30f},
                        }
                    }
                }
            };
        }

        public static void Dispose()
        {
            _particleTexture.Dispose();
            _particleEffect.Dispose();
        }
        public static void Update(GameTime gameTime)
        {
            _particleEffect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            

        }

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(_particleEffect);
            spriteBatch.End();

        }
        public static void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            //graphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(_particleEffect);
            spriteBatch.End();

        }
    }

}
