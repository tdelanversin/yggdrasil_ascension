using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
#nullable enable

namespace YGR
{
    public interface IParticle
    {
        public float TTL { get; set; }
        public bool IsDead { get; set; }
        public void Update(GameTime gt);
        public void Draw(GameTime gt, SpriteBatch spriteBatch);
    }

    public class Particle_Base : IParticle
    {
        public float TTL { get; set; }
        public bool IsDead { get; set; }
        protected Vector2 _position;
        protected AnimatedSprite _sprite;
        protected float _rotation;
        protected Color _color;
        protected float _scale;
        public Particle_Base(AnimatedSprite sprite, Vector2 position)
        {
            _sprite = sprite;
            _position = position;
            _color = Color.White;

            _scale = 1;
            _rotation = (float) (Util.random.NextDouble() * MathHelper.TwoPi);
            TTL = _sprite.AnimationDuration;
        }

        public virtual void Update(GameTime gt)
        {
            _sprite.Update(gt, AnimationState.Idle);
            TTL -= gt.ElapsedGameTime.Milliseconds;
            if (TTL <= 0)
                IsDead = true;
        }

        public virtual void Draw(GameTime gt, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: _sprite.Texture, 
                position: _position,
                sourceRectangle: _sprite.SourceRectangle, 
                color: _color,
                rotation: _rotation,
                origin: _sprite.SourceRectangle.Size.ToVector2() / 2f,
                scale: _scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }

    public class Particle_Slime : Particle_Base
    {
        protected Vector2 _velocity;
        public Particle_Slime(
            AnimatedSprite sprite, 
            Vector2 position,
            Color color,
            Vector2 velocity,
            float scale = 1f
        ) : base(sprite, position) 
        {
            _color = color;
            _velocity = velocity;
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
            _position += _velocity * gt.ElapsedGameTime.Milliseconds / 1000f;
        }
    }

    public class Particle_Slime_Moving : Particle_Base
    {
        protected Vector2 _finalPosition;
        public Particle_Slime_Moving(
            AnimatedSprite sprite, 
            Vector2 position,
            Color color,
            Vector2 finalPosition,
            float scale = 1f
        ) : base(sprite, position) 
        {
            _color = color;
            _finalPosition = finalPosition;
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
            _position = Vector2.Lerp(_position, _finalPosition, 0.1f);
        }

    }

    public class Particle_Slime_Splatter : Particle_Base
    {
        protected Vector2 _finalPosition;
        protected float _finalRotation;
        public Particle_Slime_Splatter(
            AnimatedSprite sprite, 
            Vector2 position,
            Color color,
            Vector2 finalPosition,
            float finalRotation,
            float scale = 1f
        ) : base(sprite, position) 
        {
            _color = color;
            _finalPosition = finalPosition;
            _finalRotation = _rotation + finalRotation;
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
            _position = Vector2.Lerp(_position, _finalPosition, 0.1f);
            _rotation = MathHelper.Lerp(_rotation, _finalRotation, 0.1f);
        }
    }
}