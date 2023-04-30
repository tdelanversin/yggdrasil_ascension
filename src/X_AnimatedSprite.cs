using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace YGR
{
    public enum AnimationState
    {
        WalkLeft,
        WalkRight,
        IdleLeft,
        IdleRight,
        Idle,
        WalkUp,
        WalkDown,
    }

    public class AnimatedSprite
    {
        public Texture2D Texture { get; }
        public Vector2 SpriteDimension { get; }
        public Rectangle SourceRectangle { get; private set; }
        public AnimationState Direction { get; private set; }
        public Dictionary<AnimationState, int[]> Animations { get; }
        public float AnimationDuration { get; }
        public int AnimationIndex { get; private set; }

        int[] CurrentAnimationSet;
        int DirectionalIndex;
        float AnimationTimer;
        float AnimationTreshold;
        Vector2 LastMovement;

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<AnimationState, int[]> animations)
        {
            if (animations.Keys.Count < 1)
            {
                throw new System.Exception("Animation dictionary cannot be empty");
            }
            Animations = animations;
            Texture = texture;
            SpriteDimension = spriteDimension;
            AnimationTimer = 0;
            AnimationDuration = 1000;
            SourceRectangle = new Rectangle(0, 0, (int)SpriteDimension.X, (int)SpriteDimension.Y);
            LastMovement = new Vector2(1, 0);
        }

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<AnimationState, int[]> animations, float animationDuration)
        : this(texture, spriteDimension, animations)
        {
            AnimationDuration = animationDuration;
        }

        private void ResetAnimation()
        {
            AnimationTimer = 0;
            AnimationTreshold = AnimationDuration / CurrentAnimationSet.Length;
            DirectionalIndex = 0;
            AnimationIndex = CurrentAnimationSet[DirectionalIndex];
            SourceRectangle = new Rectangle(AnimationIndex * (int)SpriteDimension.X, 0, (int)SpriteDimension.X, (int)SpriteDimension.Y);
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            AnimationTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (AnimationTimer > AnimationTreshold)
            {
                DirectionalIndex = (DirectionalIndex + 1) % CurrentAnimationSet.Length;
                AnimationIndex = CurrentAnimationSet[DirectionalIndex];
                AnimationTimer = 0;
                SourceRectangle = new Rectangle(AnimationIndex * (int)SpriteDimension.X, 0, (int)SpriteDimension.X, (int)SpriteDimension.Y);
            }
        }

        private AnimationState FlipDirection(AnimationState direction)
        {
            switch (direction)
            {
                case AnimationState.WalkLeft:
                    return AnimationState.WalkRight;
                case AnimationState.WalkRight:
                    return AnimationState.WalkLeft;
                case AnimationState.IdleLeft:
                    return AnimationState.IdleRight;
                default:
                    return AnimationState.IdleLeft;
            }
        }

        private AnimationState GetFallbackDirection(AnimationState direction)
        {
            switch (direction)
            {
                case AnimationState.Idle:
                    if (Animations.ContainsKey(AnimationState.IdleRight)) { return AnimationState.IdleRight; }
                    if (Animations.ContainsKey(AnimationState.IdleLeft)) { return AnimationState.IdleLeft; }
                    break;
                case AnimationState.WalkLeft:
                    if (Animations.ContainsKey(AnimationState.IdleLeft)) { return AnimationState.IdleLeft; }
                    break;
                case AnimationState.WalkRight:
                    if (Animations.ContainsKey(AnimationState.IdleRight)) { return AnimationState.IdleRight; }
                    break;
                case AnimationState.IdleLeft:
                    if (Animations.ContainsKey(AnimationState.Idle)) { return AnimationState.Idle; }
                    break;
                case AnimationState.IdleRight:
                    if (Animations.ContainsKey(AnimationState.Idle)) { return AnimationState.Idle; }
                    break;
                case AnimationState.WalkUp:
                    if (Animations.ContainsKey(AnimationState.WalkRight)) { return AnimationState.WalkRight; }
                    if (Animations.ContainsKey(AnimationState.WalkLeft)) { return AnimationState.WalkLeft; }
                    break;
                case AnimationState.WalkDown:
                    if (Animations.ContainsKey(AnimationState.WalkLeft)) { return AnimationState.WalkLeft; }
                    if (Animations.ContainsKey(AnimationState.WalkRight)) { return AnimationState.WalkRight; }
                    break;
                default:
                    break;
            }
            // Last resort
            return Animations.Keys.First();
        }

        private void UpdateDirection(AnimationState direction)
        {
            if (!Animations.ContainsKey(direction))
            {
                direction = GetFallbackDirection(direction);
            }

            Direction = direction;
            CurrentAnimationSet = Animations[Direction];
        }

        public void Update(GameTime gameTime, AnimationState direction)
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
        }

        public void Update(GameTime gameTime, Vector2 movement)
        {
            // Idling
            if (movement == Vector2.Zero)
            {
                // If character comes to a halt, try to keep facing the same direction
                if (LastMovement.X > 0)
                {
                    Update(gameTime, AnimationState.IdleRight);
                }
                else if (LastMovement.X < 0)
                {
                    Update(gameTime, AnimationState.IdleLeft);
                }
            } // Walking
            else if (movement.X > 0)
            {
                Update(gameTime, AnimationState.WalkRight);
                LastMovement.X = movement.X;
            }
            else if (movement.X < 0)
            {
                Update(gameTime, AnimationState.WalkLeft);
                LastMovement.X = movement.X;
            }
            else if (movement.Y > 0 && Animations.ContainsKey(AnimationState.WalkDown))
            {
                Update(gameTime, AnimationState.WalkDown);
                LastMovement.Y = movement.Y;
            }
            else if (movement.Y < 0 && Animations.ContainsKey(AnimationState.WalkUp))
            {
                Update(gameTime, AnimationState.WalkUp);
                LastMovement.Y = movement.Y;
            }
            else
            {
                // At this point we move up/down but have no sprite sets for that, so just use the walking sprite
                // in the same direction that we previously walked in
                if (LastMovement.X > 0)
                {
                    Update(gameTime, AnimationState.WalkRight);
                }
                else if (LastMovement.X < 0)
                {
                    Update(gameTime, AnimationState.WalkLeft);
                }
            }
        }
    }
}