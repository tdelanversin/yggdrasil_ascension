using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Modifiers.Containers;

using MonoGame.Extended.TextureAtlases;
using Microsoft.Xna.Framework.Content;
using Vector2=Microsoft.Xna.Framework.Vector2;
using MonoGame.Extended.Sprites;

namespace YGR
{
    public static class Manager_Particles
    {
        private static ParticleEffect _particleEffect;
        private static Texture2D _particleTexture;
        private static ParticleEffect _particleEffect_fire;
        private static Texture2D _particleTexture_fire;
        public static List<ParticleEffect> _particleEffects { get;  private set; }
        public static void Initialize()
        {
            _particleEffects = new List<ParticleEffect>();
            _particleEffect = new ParticleEffect();
            _particleEffect_fire = new ParticleEffect();
        }

        public static void LoadContent(ContentManager contentManager)
        {
            _particleTexture = contentManager.Load<Texture2D>("SpritesEffects/dust_particle");
            _particleTexture_fire = contentManager.Load<Texture2D>("SpritesEffects/dust_particle_red");
            GenParticleEffectBase(new Vector2(0, 0));
            GenParticleEffectGigaChad(new Vector2(0, 0));
            GenParticleEffectProjectileTrails(new Vector2(0, 0));
        }
        public static void GenParticleEffectBase(Vector2 pos )
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture);
            _particleEffect = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(33330,3330),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 400, TimeSpan.FromSeconds(0.6),
                        Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 20f),
                            Quantity = 20,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(0.03f, 0.04f),
                            Opacity = 0.2f
                        },
                        Modifiers =
                        {
                          
                            new VelocityColorModifier
                            {
                                StationaryColor = Microsoft.Xna.Framework.Color.Green.ToHsl(),
                                VelocityColor = Microsoft.Xna.Framework.Color.Blue.ToHsl(),
                                VelocityThreshold = 80f
                            },
                       
                        }
                    }
                }
            };
            _particleEffects.Add(_particleEffect);
        }

        public static void GenParticleEffectGigaChad(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture);
            _particleEffect = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(33330, 3330),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.5f),
                        Profile.Point())
                        //Profile.BoxFill(15,15))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 40f),
                            Quantity = 30,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(0.05f, 0.1f),
                            Opacity = 0.2f
                        },
                      
                        
                    }
                }
            };
            _particleEffects.Add(_particleEffect);
        }
        public static void GenParticleEffectProjectileTrails(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_fire);
            _particleEffect_fire = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(33330, 3330),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.4f),
                        //Profile.Point())
                        //Profile.BoxFill(15,15))
                        Profile.Spray(new Vector2(1,1), 6f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 20f),
                            Quantity = 8,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(0.05f, 0.1f),
                            Opacity = 0.5f
                        },
                        Modifiers =
            {
                new AgeModifier
                {
                    Interpolators =
                    {
                        //new ColorInterpolator
                        //{
                        //    StartValue = new HslColor(0f, 1f, 0.5f),
                        //    EndValue = new HslColor(0f, 1f, 0.5f),
                        //}
                    }
                },
                //new RotationModifier {RotationRate = -2.1f},
                //new RectangleContainerModifier {Width = 800, Height = 480},
                //new LinearGravityModifier {Direction = -Vector2.UnitY, Strength = 30f},
            }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_fire);
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

        public static void Draw(GameTime gameTime, Vector2 zero, SpriteBatch spriteBatch)
        {
            foreach (var pE in _particleEffects)
            {
                spriteBatch.Draw(pE);
            }
        }
    }
}
