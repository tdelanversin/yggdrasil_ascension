using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public enum SpriteDirection
    {
        Idle,
        Left,
        Right,
        Up,
        Down,
    }

    public class AnimatedSprite
    {
        public Texture2D Texture { get; }
        public SpriteEffects Effect { get; private set; }
        public Vector2 SpriteDimension { get; }
        public Vector2 SourceRectangle { get; private set; }
        public SpriteDirection Direction { get; private set; }
        public Dictionary<SpriteDirection, int[]> Animations { get; }
        public bool UseHorizontalFlip { get; }
        public float AnimationDuration { get; }
        public int AnimationIndex { get; private set; }

        int[] CurrentAnimation;
        int DirectionalIndex;
        float AnimationTimer;
        float AnimationTreshold;

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<SpriteDirection, int[]> animations, bool useHorizontalFlip = false)
        {
            if (Animations.Keys.Count < 1)
            {
                throw new System.Exception("Animation dictionary cannot be empty");
            }
            Texture = texture;
            SpriteDimension = spriteDimension;
            UseHorizontalFlip = useHorizontalFlip;
            AnimationTimer = 0;
            AnimationDuration = 1000;
        }

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<SpriteDirection, int[]> animations, float animationDuration, bool useHorizontalFlip = false)
        : this(texture, spriteDimension, animations, useHorizontalFlip)
        {
            AnimationDuration = animationDuration;
        }

        private void ResetAnimation()
        {
            AnimationTimer = 0;
            AnimationTreshold = AnimationDuration / CurrentAnimation.Length;
            DirectionalIndex = 0;
            AnimationIndex = CurrentAnimation[DirectionalIndex];
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            AnimationTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (AnimationTimer > AnimationTreshold)
            {
                DirectionalIndex = (DirectionalIndex + 1) % CurrentAnimation.Length;
                AnimationIndex = CurrentAnimation[DirectionalIndex];
                AnimationTimer = 0;
            }
        }

        private SpriteDirection GetFallbackDirection(SpriteDirection direction)
        {
            switch (direction)
            {
                case SpriteDirection.Idle:
                    if (Animations.ContainsKey(SpriteDirection.Right)) { return SpriteDirection.Right; }
                    if (Animations.ContainsKey(SpriteDirection.Left)) { return SpriteDirection.Left; }
                    break;
                case SpriteDirection.Left:
                    if (Animations.ContainsKey(SpriteDirection.Idle)) { return SpriteDirection.Idle; }
                    break;
                case SpriteDirection.Right:
                    if (Animations.ContainsKey(SpriteDirection.Idle)) { return SpriteDirection.Idle; }
                    break;
                case SpriteDirection.Up:
                    if (Animations.ContainsKey(SpriteDirection.Right)) { return SpriteDirection.Right; }
                    if (Animations.ContainsKey(SpriteDirection.Left)) { return SpriteDirection.Left; }
                    break;
                case SpriteDirection.Down:
                    if (Animations.ContainsKey(SpriteDirection.Left)) { return SpriteDirection.Left; }
                    if (Animations.ContainsKey(SpriteDirection.Right)) { return SpriteDirection.Right; }
                    break;
                default:
                    break;
            }
            // Last resort
            return Animations.Keys.First();
        }

        private void UpdateDirection(SpriteDirection direction)
        {
            if (!Animations.ContainsKey(direction))
            {
                direction = GetFallbackDirection(direction);
            }

            Direction = direction;
            CurrentAnimation = Animations[Direction];
        }

        public void Update(GameTime gameTime, SpriteDirection direction)
        {
            if (direction != Direction)
            {
                UpdateDirection(direction);
                ResetAnimation();
            }
            else
            {
                UpdateAnimation(gameTime);
            }
            SourceRectangle = new Vector2(AnimationIndex * SpriteDimension.X, 0);
        }
    }
}