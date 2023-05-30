using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

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
using Vector2 = Microsoft.Xna.Framework.Vector2;
using MonoGame.Extended.Sprites;

namespace YGR
{
    public static class Manager_Particles
    {
        public enum Effect
        {
            Base = 0,
            GigaChad,
            ProjectileTrails,
            DustCloudLight,
            Dash,
            Impact,
            Blank,
            Big_Slime_Death,
            Small_Slime_Death
        }

        private static ParticleEffect _particleEffect_dust;
        private static ParticleEffect _particleEffect_dust_cloud_light;
        private static Texture2D _particleTexture_dust;
        private static Texture2D _particleTexture_dust_cloud_light;
        private static ParticleEffect _particleEffect_fire;
        private static ParticleEffect _particleEffect_dash;
        private static ParticleEffect _particleEffect_impact;
        private static Texture2D _particleTexture_fire;
        private static Texture2D _particleTexture_dash;
        private static Texture2D _particleTexture_impact;
        private static ParticleEffect _particleEffect_Blank;
        private static Texture2D _particleTexture_ghost_ability;
        private static AnimatedSprite _particleTexture_big_slime_death;
        private static AnimatedSprite _particleTexture_small_slime_death;


        //public static Dictionary<string,ParticleEffect> _particleEffects { get;  private set; }
        //public static List<ParticleEffect> _particleEffectsTest { get; private set; }
        private static Dictionary<Effect, ParticleEffect> _particleEffects { get; set; }
        private static IList<IParticle> _spriteParticles { get; set; }

        public static ParticleEffect GetParticleEffect(Effect whichOne)
        {
            return _particleEffects[whichOne];
        }

        public static void Initialize()
        {
            //_particleEffectsTest = new List<ParticleEffect>();
            _particleEffects = new Dictionary<Effect, ParticleEffect>();
            //_particleEffects = new Dictionary<string,ParticleEffect>();

            _spriteParticles = new List<IParticle>();
        }

        public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            //_particleTexture_dust = contentManager.Load<Texture2D>("SpritesEffects/dust_particle");
            _particleTexture_fire = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_dash = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_dash.SetData(new[] { Color.Cyan });
            _particleTexture_impact = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_impact.SetData(new[] { Color.DarkRed });
            _particleTexture_dust_cloud_light = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_dust = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_dust.SetData(new[] { Color.Black * 0.5f });
            _particleTexture_dust_cloud_light.SetData(new[] { Color.Black * 0.5f });
            _particleTexture_ghost_ability = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture_ghost_ability.SetData(new[] { Color.White });

            _particleTexture_big_slime_death = Manager_Sprites.NewAnimatedSprite_Big_Slime_Death_Particle();
            _particleTexture_small_slime_death = Manager_Sprites.NewAnimatedSprite_Small_Slime_Death_Particle();

            Vector2 pos = new Vector2(10333, 22332);
            GenParticleEffectBase(pos);
            GenParticleEffectGigaChad(pos);
            GenParticleEffectProjectileTrails(pos, Color.WhiteSmoke);
            GenParticleEffectDustCloudLight(pos);
            GenParticleEffectDash(pos);
            GenParticleEffectImpact(pos);
            GenParticleEffectBlank(pos);
        }

        private static void GenParticleEffectBase(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dust);
            _particleEffect_dust = new ParticleEffect(autoTrigger: false)
            {
                Position = new Vector2(33330, 3330),
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
                            new LinearGravityModifier()

                        }
                    }
                }
            };
            _particleEffects.Add(Effect.Base, _particleEffect_dust);
        }
        private static void GenParticleEffectDustCloudLight(Vector2 pos)
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
            _particleEffects.Add(Effect.DustCloudLight, _particleEffect_dust_cloud_light);
        }

        private static void GenParticleEffectGigaChad(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dust);
            _particleEffect_dust = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(20060, 23005),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 25000, TimeSpan.FromSeconds(0.25),
                        //Profile.Spray(new Vector2(-0.95f,-1f), 1f ))
                        Profile.Circle(30, Profile.CircleRadiation.Out))
                        //Profile.Point())
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(1f, 10f),
                            Quantity = 15,
                            Opacity =0.7f,//new Range<float>(0.1f, f),
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(2.0f, 4.0f),
                            Mass = 1.25f,
                        },
                        Modifiers =
                        {
                            new AgeModifier
                            {
                                Interpolators = new List<Interpolator>()
                                {
                                    new OpacityInterpolator { StartValue = 0.9f, EndValue = 0.1f },
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
            _particleEffects.Add(Effect.GigaChad, _particleEffect_dust);
        }

        private static void GenParticleEffectProjectileTrails(Vector2 pos, Color c)
        {
            _particleTexture_fire.SetData(new[] { c });
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_fire);
            _particleEffect_fire = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(33330, 3330),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(0.2f),
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
            _particleEffects.Add(Effect.ProjectileTrails, _particleEffect_fire);
            //_particleEffectsTest.Add(_particleEffect_fire);
        }



        private static void GenParticleEffectDash(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_dash);

            _particleEffect_dash = new ParticleEffect(autoTrigger: false)
            {
                //Position = new Vector2(3330, 3330),
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 100, TimeSpan.FromSeconds(0.30f),
                        //Profile.Point())
                        // Profile.BoxFill(15,15))
                        Profile.Circle(30, Profile.CircleRadiation.In))
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
            _particleEffects.Add(Effect.Dash, _particleEffect_dash);
        }

        private static void GenParticleEffectBlank(Vector2 pos)
        {
            TextureRegion2D textureRegion = new TextureRegion2D(_particleTexture_ghost_ability);

            _particleEffect_Blank = new ParticleEffect(autoTrigger: false)
            {
                Position = pos,
                Emitters = new List<ParticleEmitter>
                {
                    new ParticleEmitter(textureRegion, 250, TimeSpan.FromSeconds(0.50f),
                        Profile.Circle(Ability_Blank.Radius, Profile.CircleRadiation.In))
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
                            // new OpacityFastFadeModifier(),
                        }
                    }
                }
            };
            _particleEffects.Add(Effect.Blank, _particleEffect_Blank);
        }

        private static void GenParticleEffectImpact(Vector2 pos)
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
                            Quantity = 10,
                            Rotation = new Range<float>(2f, 8f),
                            Scale = new Range<float>(2f, 3f),
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
            _particleEffects.Add(Effect.Impact, _particleEffect_impact);
        }

        private static Vector2 GetRandomFinalPosition(Rectangle Rect, float scale = 1f)
        {
            return (
                Rect.Center.ToVector2() 
                - Util.random.NextSingle() * Rect.Height * scale * Vector2.UnitY
                + (Util.random.NextSingle() * 2 - 1) * Rect.Width * scale * Vector2.UnitX
            );
        }

        private static float GetRandomFinalRotation()
        {
            return (Util.random.NextSingle() * 2 - 1) * MathHelper.PiOver2;
        }

        private static void SlimeDeath(Enemy_Slime slime)
        {
            for (int i = 0; i < 2; i++)
            {
                AnimatedSprite slimeParticle = Manager_Sprites.NewAnimatedSprite_Small_Slime_Death_Particle();
                IParticle particle = new Particle_Move_Rotate(
                    slimeParticle,
                    slime.Rect.Center.ToVector2(),
                    slime.Color,
                    finalPosition: GetRandomFinalPosition(slime.Rect),
                    finalRotation: GetRandomFinalRotation(),
                    scale: 6f
                );
                _spriteParticles.Add(particle);
            }
        }

        private static void SpikySlimeDeath(Enemy_Slime_Spiky slime)
        {
            for (int i = 0; i < 4; i++)
            {
                AnimatedSprite slimeSpikyParticle = Manager_Sprites.NewAnimatedSprite_Small_Slime_Death_Particle();
                IParticle particleSpiky = new Particle_Move_Rotate(
                    slimeSpikyParticle,
                    slime.Rect.Center.ToVector2(),
                    slime.Color,
                    finalPosition: GetRandomFinalPosition(slime.Rect, scale: .5f),
                    finalRotation: GetRandomFinalRotation(),
                    scale: 6f
                );
                _spriteParticles.Add(particleSpiky);
            }
        }

        private static void BossDeath(Enemy_Boss boss)
        {
            for (int i = 0; i < 40; i++)
            {
                AnimatedSprite slimeBossParticle = Manager_Sprites.NewAnimatedSprite_Big_Slime_Death_Particle();
                IParticle particleSpiky = new Particle_Move_Rotate(
                    slimeBossParticle,
                    boss.Rect.Center.ToVector2(),
                    boss.bossColor,
                    finalPosition: GetRandomFinalPosition(boss.Rect, scale: 1.2f),
                    finalRotation: GetRandomFinalRotation(),
                    scale: 8f
                );
                _spriteParticles.Add(particleSpiky);
            }
        }

        public static void MakeSlimeDeathParticle(IEnemy enemy)
        {
            if (!Settings.ParticleEffects)
                return;

            switch (enemy)
            {
                case Enemy_Slime slime:
                    SlimeDeath(slime);
                    break;
                case Enemy_Slime_Spiky slimeSpiky:
                    SpikySlimeDeath(slimeSpiky);
                    break;
                case Enemy_Boss slimeBoss:
                    BossDeath(slimeBoss);
                    break;
                default:
                    break;
            }
        }

        public static void MakeSlimeImpactParticle(IEnemy enemy, Vector2 pos, Vector2 normal)
        {
            if (!Settings.ParticleEffects)
                return;

            Color color = Color.White;
            switch (enemy)
            {
                case Enemy_Slime slime:
                    color = slime.Color;
                    break;
                case Enemy_Slime_Spiky slimeSpiky:
                    color = slimeSpiky.Color;
                    break;
                case Enemy_Boss slimeBoss:
                    color = slimeBoss.bossColor;
                    break;
                default:
                    break;
            }

            AnimatedSprite particleSprite = Manager_Sprites.NewAnimatedSprite_Slime_Impact_Particle();
            IParticle particle = new Particle_Move_Rotate(
                particleSprite,
                pos,
                color,
                finalPosition: pos + normal * 15f,
                finalRotation: GetRandomFinalRotation(),
                scale: 8f
            );
            _spriteParticles.Add(particle);
        }

        public static void MakeWallImpactParticle(Vector2 pos, Vector2 normal)
        {
            if (!Settings.ParticleEffects)
                return;

            int count = Util.random.Next(1, 3);
            for (int i = 0; i < count; i++)
            {
                AnimatedSprite particleSprite = Manager_Sprites.NewAnimatedSprite_Bullet_Impact_Particle();
                if (Util.random.NextDouble() < .5)
                    particleSprite = Manager_Sprites.NewAnimatedSprite_Dust_Particle();

                float normalRotation = (float)Math.Atan2(normal.Y, normal.X)  + (Util.random.NextSingle() * 2 - 1) * MathHelper.PiOver4;
                Vector2 normalRotated = new Vector2(
                    (float)Math.Cos(normalRotation),
                    (float)Math.Sin(normalRotation)
                );

                IParticle particle = new Particle_Move(
                    particleSprite,
                    pos,
                    Color.White,
                    finalPosition: pos + normalRotated * 40f,
                    scale: 4f
                );
                particle.Rotation = normalRotation;
                _spriteParticles.Add(particle);
            }
        }

        public static void MakeWalkParticle(Vector2 pos)
        {
            if (!Settings.ParticleEffects)
                return;

            AnimatedSprite particleSprite = Manager_Sprites.NewAnimatedSprite_Dust_Particle();

            IParticle particle = new Particle_Move_Rotate(
                particleSprite,
                pos,
                Color.White,
                finalPosition: pos + new Vector2(0, -15),
                finalRotation: GetRandomFinalRotation(),
                scale: 2f
            );
            _spriteParticles.Add(particle);
        }

        public static void Update(GameTime gameTime)
        {
            if (!Settings.ParticleEffects)
            {
                return;
            }
            foreach (var pE in _particleEffects.Values)
            {

                pE.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                pE.Emitters.ForEach(e => { e.AutoTrigger = false; });
            }

            foreach (var p in _spriteParticles)
            {
                p.Update(gameTime);
            }

            _spriteParticles = _spriteParticles.Where(p => !p.IsDead).ToList();
            // _spriteParticles.RemoveAll(p => p.IsDead);
        }

        //public static void Dispose()
        //{
        //    foreach (var pE in _particleEffects)
        //    {
        //        //pE.Dispose();
        //        //foreach (var emitter in pE.Emitters)
        //        //{
        //        //emitter.
        //        //}
        //    }
        //}

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!Settings.ParticleEffects)
            {
                return;
            }
            foreach (var pE in _particleEffects)
            {
                spriteBatch.Draw(pE.Value);
            }

            foreach (var p in _spriteParticles)
            {
                p.Draw(gameTime, spriteBatch);
            }
        }
    }
}
