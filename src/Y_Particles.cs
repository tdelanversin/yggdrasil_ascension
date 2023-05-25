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
        public float Rotation { get; set; }
        public void Update(GameTime gt);
        public void Draw(GameTime gt, SpriteBatch spriteBatch);
    }

    public class Particle_Base : IParticle
    {
        public float TTL { get; set; }
        public bool IsDead { get; set; }
        public float Rotation { get; set; }
        protected Vector2 _position;
        protected AnimatedSprite _sprite;
        protected Color _color;
        protected float _scale;
        public Particle_Base(AnimatedSprite sprite, Vector2 position)
        {
            _sprite = sprite;
            _position = position;
            _color = Color.White;

            _scale = 1;
            Rotation = (float) (Util.random.NextDouble() * MathHelper.TwoPi);
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
                rotation: Rotation,
                origin: _sprite.SourceRectangle.Size.ToVector2() / 2f,
                scale: _scale,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }

    public class Particle_Move : Particle_Base
    {
        protected Vector2 _finalPosition;
        public Particle_Move(
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

    public class Particle_Move_Rotate : Particle_Base
    {
        protected Vector2 _finalPosition;
        protected float _finalRotation;
        public Particle_Move_Rotate(
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
            _finalRotation = Rotation + finalRotation;
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
            _position = Vector2.Lerp(_position, _finalPosition, 0.1f);
            Rotation = MathHelper.Lerp(Rotation, _finalRotation, 0.1f);
        }
    }

}