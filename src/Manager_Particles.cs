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
using System.Reflection.Metadata;
using MonoGame.Extended.Sprites;

namespace YGR
{
    public static class Manager_Particles
    {
        private static ParticleEffect _particleEffect;
        private static Texture2D _particleTexture;
        public static List<ParticleEffect> _particleEffects { get;  private set; }
        public static void Initialize()
        {
            _particleEffects = new List<ParticleEffect>();
            _particleEffect = new ParticleEffect();
        }

        public static void LoadContent(ContentManager contentManager)
        {
            _particleTexture = contentManager.Load<Texture2D>("dust_particle");

        }
        public static void GenParticleEffect( )
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture);
            _particleEffect = new ParticleEffect(autoTrigger: true)
            {
                //Position = pos,//new Vector2(400, 240),
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(0.7),
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
                                    //new OpacityInterpolator
                                    //{
                                    //    StartValue =1f, 
                                    //    EndValue = -2f
                                    //}
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
                            //new RectangleContainerModifier {Width = 800, Height = 480},
                            //new LinearGravityModifier {Direction = -Microsoft.Xna.Framework.Vector2.UnitY, Strength = 3f},
                        }
                    }
                }
            };
            _particleEffects.Add(_particleEffect);
        }

        public static void Dispose()
        {
            _particleTexture.Dispose();
            _particleEffect.Dispose();
        }
        public static void Update(GameTime gameTime)
        {
            foreach (var pE in _particleEffects)
            {
                pE.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            }

        }

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var pE in _particleEffects)
            {
                spriteBatch.Draw(pE);
            }
        }
    }

}
