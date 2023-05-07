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
        private static ParticleEffect _particleEffect_dust;
        private static ParticleEffect _particleEffect_dust_cloud;
        private static ParticleEffect _particleEffect_dust_cloud_light;
        private static Texture2D _particleTexture_dust;
        private static Texture2D _particleTexture_dust_cloud;
        private static Texture2D _particleTexture_dust_cloud_light;
        private static ParticleEffect _particleEffect_fire;
        private static ParticleEffect _particleEffect_dash;
        private static ParticleEffect _particleEffect_impact;
        private static Texture2D _particleTexture_fire;
        private static Texture2D _particleTexture_dash;
        private static Texture2D _particleTexture_impact;


        public static List<ParticleEffect> _particleEffects { get;  private set; }
        public static void Initialize()
        {
            _particleEffects = new List<ParticleEffect>();
            _particleEffect_dust = new ParticleEffect();
            _particleEffect_dust_cloud_light = new ParticleEffect();
            _particleEffect_dust_cloud = new ParticleEffect();
            _particleEffect_fire = new ParticleEffect();
            _particleEffect_dash = new ParticleEffect();
            _particleEffect_impact = new ParticleEffect();
        }

        public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            _particleTexture_dust = contentManager.Load<Texture2D>("SpritesEffects/dust_particle");
            _particleTexture_dust_cloud = contentManager.Load<Texture2D>("SpritesEffects/dust_cloud");
            _particleTexture_dust_cloud_light = contentManager.Load<Texture2D>("SpritesEffects/big_dust_cloud");
            _particleTexture_fire = new Texture2D(graphicsDevice,1,1);
            _particleTexture_dash = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_dash.SetData(new[] { Color.Cyan });
            _particleTexture_impact = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_impact.SetData(new[] { Color.DarkRed });
            _particleTexture_dust_cloud_light = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_dust_cloud_light.SetData(new[] { Color.Black * 0.5f });
            //GenParticleEffectBase(new Vector2(0, 0));
            //GenParticleEffectGigaChad(new Vector2(0, 0));
            //GenParticleEffectProjectileTrails(new Vector2(0, 0));
            //GenParticleEffectDustCloudLight();
            //GenParticleEffectDash();
            //GenParticleEffectDustCloud();
            //GenParticleEffectImpact();
        }
        public static void GenParticleEffectBase(Vector2 pos )
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dust);
            _particleEffect_dust = new ParticleEffect(autoTrigger: false)
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
            _particleEffects.Add(_particleEffect_dust);
        }
        public static void GenParticleEffectDustCloud()
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dust_cloud);
            _particleEffect_dust_cloud = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(33330, 3330),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.15f),
                        Profile.Line(new Vector2(1,1), 5 ))
                        //Profile.BoxFill(15,15))
                    {
                        Parameters = new ParticleReleaseParameters{
                        
                            Speed = new Range<float>(5f, 5f),
                            Quantity = 5,
                            //Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1f,10f),
                            Opacity = 1f
                        },
                         Modifiers =  {
                            new AgeModifier()
                                {
                                    Interpolators = new List<Interpolator>()
                                    {
                                        new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(3,3) },


                                    }
                                },
                            new OpacityFastFadeModifier(),
                            

                        }

                    }
                }
            };
            _particleEffects.Add(_particleEffect_dust_cloud);
        }
        public static void GenParticleEffectDustCloudLight(Vector2 pos)
        {
           

            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dust_cloud_light);
            _particleEffect_dust_cloud_light = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(20060, 23005),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 25000, TimeSpan.FromSeconds(0.2),
                        //Profile.Spray(new Vector2(-0.95f,-1f), 1f ))
                        Profile.Circle(5, Profile.CircleRadiation.Out))
                        //Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(1f, 10f),
                            Quantity = 10,
                            Opacity = 1,//new Range<float>(0.1f, f),
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1.0f, 3.0f),
                            Mass = 1.25f,
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                                    new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                                }
                            },
                            new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }
                    }
                }
            };
            _particleEffects.Add(_particleEffect_dust_cloud_light);
        }
        public static void GenParticleEffectGigaChad(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dust);
            _particleEffect_dust = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(33330, 3330),
                Position = pos,
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
            _particleEffects.Add(_particleEffect_dust);
        }
        public static void GenParticleEffectProjectileTrails(Vector2 pos, Color c)
        {
            _particleTexture_fire.SetData(new[] {c});
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_fire);
            _particleEffect_fire = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(33330, 3330),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.9f),
                        //Profile.Point())
                        //Profile.BoxFill(15,15))
                        Profile.Spray(new Vector2(1,1), 3f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 100),
                            Quantity = 8,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(0.5f, 1f),
                            Opacity = 0.5f
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                                    new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                                }
                            },
                            new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_fire);
        }
        public static void GenParticleEffectProjectileTrails()
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_fire);
            _particleEffect_fire = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(33330, 3330),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.1f),
                        //Profile.Point())
                        //Profile.BoxFill(15,15))
                        Profile.Spray(new Vector2(1,1), 3f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(0f, 100),
                            Quantity = 8,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(0.5f, 1f),
                            Opacity = 0.5f
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                                    new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                                }
                            },
                            new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_fire);
        }

        public static void GenParticleEffectDash()
        {


            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dash);

            _particleEffect_dash = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(3330, 3330),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 100, TimeSpan.FromSeconds(1.5f),
                        //Profile.Point())
                        Profile.BoxFill(150,150))
                        //Profile.Line(new Vector2(1,1), 5f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(10f, 100),
                            Quantity = 30,
                            Rotation = new Range<float>(2f, 2f),
                            Scale = new Range<float>(5f, 5f),
                            Opacity = 1f
                        },
                        Modifiers =
                        {
                            //new AgeModifier
                            //{
                            //    Interpolators = new List<Interpolator>()
                            //    {
                            //        //new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                            //        //new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                            //    }
                            //},
                            //new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            //new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_dash);
        }

        public static void GenParticleEffectDash(Vector2 pos)
        {


            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dash);

            _particleEffect_dash = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(3330, 3330),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 100, TimeSpan.FromSeconds(1f),
                        //Profile.Point())
                        Profile.BoxFill(15,15))
                        //Profile.Line(new Vector2(1,1), 5f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(10f, 50),
                            Quantity = 10,
                            Rotation = new Range<float>(2f, 2f),
                            Scale = new Range<float>(2f, 3f),
                            Opacity = 1f
                        },
                        Modifiers =
                        {
                            //new AgeModifier
                            //{
                            //    Interpolators = new List<Interpolator>()
                            //    {
                            //        //new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                            //        //new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                            //    }
                            //},
                            //new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            //new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_dash);
        }
        public static void GenParticleEffectImpact()
        {


            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_impact);
            _particleEffect_impact = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(100, 100),
                //Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.8f),
                        //Profile.Point())
                        //Profile.BoxFill(15,15))
                        Profile.Line(new Vector2(1,1), 5f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(10f, 100),
                            Quantity = 30,
                            Rotation = new Range<float>(2f, 8f),
                            Scale = new Range<float>(1f, 2f),
                            Opacity = 1f
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    //new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                                    //new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                                }
                            },
                            //new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            //new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_impact);
        }
        public static void GenParticleEffectImpact(Vector2 pos)
        {


            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_impact);
            _particleEffect_impact = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(100, 100),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.8f),
                        //Profile.Point())
                        //Profile.BoxFill(15,15))
                        Profile.Line(new Vector2(1,1), 5f))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(10f, 100),
                            Quantity = 30,
                            Rotation = new Range<float>(2f, 8f),
                            Scale = new Range<float>(1f, 2f),
                            Opacity = 1f
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    //new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
                                    //new ScaleInterpolator { StartValue = new Vector2(1,1), EndValue = new Vector2(2,2) }
                                }
                            },
                            //new RotationModifier {RotationRate = -3f},
                            new OpacityFastFadeModifier(),
                            //new DragModifier { Density = 0.5f, DragCoefficient = 1f }
                        }


                    }
                }
            };
            _particleEffects.Add(_particleEffect_impact);
        }
        public static void Update(GameTime gameTime)
        {
            foreach (var pE in _particleEffects)
            {
                pE.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            foreach (var pE in _particleEffects)
            {
                pE.Emitters.ForEach(e => { e.AutoTrigger = false; }) ;
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
