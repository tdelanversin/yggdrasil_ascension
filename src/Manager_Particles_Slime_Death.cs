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
using Vector2 = Microsoft.Xna.Framework.Vector2;
using MonoGame.Extended.Sprites;
using System.Collections.ObjectModel;

namespace YGR
{
    public static class Manager_Particles_Slime_Death
    {
        public class Small_Slime
        {
            public  static AnimatedSprite _particle_small_slime_death { get; private set; }
            public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
            {
                //_particleTexture_dust = contentManager.Load<Texture2D>("SpritesEffects/dust_particle");


                _particle_small_slime_death = Manager_Sprites.NewAnimatedSprite_Big_Slime_Death_Particle();
            }
            public void Update(GameTime gt)
            {
                if (!Settings.ParticleEffects)
                {
                    return;
                }
                if (_particle_small_slime_death != null)
                {
                    _particle_small_slime_death.Update(gt, AnimationState.Idle);
                }

            }

            public void Draw(GameTime gt, SpriteBatch spriteBatch)
            {
                if (!Settings.ParticleEffects)
                {
                    return;
                }
                spriteBatch.Draw(_particle_small_slime_death.Texture, _particle_small_slime_death.SourceRectangle, Color.White);
            }


        }

        public class Big_Slime
        {
            public static AnimatedSprite _particle_big_slime_death { get; private set; }
            public static void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
            {
                //_particleTexture_dust = contentManager.Load<Texture2D>("SpritesEffects/dust_particle");


                _particle_big_slime_death = Manager_Sprites.NewAnimatedSprite_Big_Slime_Death_Particle();

            }

            public void Update(GameTime gt)
            {
                if (!Settings.ParticleEffects)
                {
                    return;
                }
                if (_particle_big_slime_death != null)
                {
                    _particle_big_slime_death.Update(gt, AnimationState.Idle);
                }

            }

            public void Draw(GameTime gt, SpriteBatch spriteBatch)
            {
                if (!Settings.ParticleEffects)
                {
                    return;
                }
                spriteBatch.Draw(_particle_big_slime_death.Texture, _particle_big_slime_death.SourceRectangle, Color.White);
            }
        }
        public enum Effect
        {

            Big_Slime_Death,
            Small_Slime_Death
        }





    }

     
    
}
