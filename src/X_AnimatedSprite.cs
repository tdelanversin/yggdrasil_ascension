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

        public Dictionary<AnimationState, Rectangle[]> AnimationSourceRects { get; private set; }
        public float AnimationDuration { get; }

        int DirectionalIndex;
        float AnimationTimer;
        float AnimationTreshold;
        Vector2 LastMovement;
        const float DefaultAnimationDuration = 1000;

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<AnimationState, Rectangle[]> animationSourceRects, float animationDuration = DefaultAnimationDuration)
        {
            if (animationSourceRects.Keys.Count < 1)
            {
                throw new System.Exception("Animation dictionary cannot be empty");
            }
            AnimationSourceRects = animationSourceRects;
            Texture = texture;
            SpriteDimension = spriteDimension;
            AnimationTimer = 0;
            AnimationDuration = animationDuration;
            SourceRectangle = new Rectangle(0, 0, (int)SpriteDimension.X, (int)SpriteDimension.Y);
            LastMovement = new Vector2(1, 0);
        }

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<AnimationState, int[,]> animations, float animationDuration = DefaultAnimationDuration)
        : this(texture, spriteDimension, BuildAnimationSourceRects(animations, spriteDimension), animationDuration) { }

        public AnimatedSprite(Texture2D texture, Vector2 spriteDimension, Dictionary<AnimationState, int[]> animations, float animationDuration = DefaultAnimationDuration)
        : this(texture, spriteDimension, BuildAnimationSourceRects(animations, spriteDimension), animationDuration) { }

        static Dictionary<AnimationState, Rectangle[]> BuildAnimationSourceRects(Dictionary<AnimationState, int[,]> animations, Vector2 spriteDimension)
        {
            Dictionary<AnimationState, Rectangle[]> animationSourceRects = new Dictionary<AnimationState, Rectangle[]>();
            foreach (var animationSet in animations)
            {
                AnimationState state = animationSet.Key;
                int[,] indeces = animationSet.Value;
                animationSourceRects[state] = new Rectangle[indeces.Length / 2];
                for (int i = 0; i < indeces.Length / 2; i++)
                {
                    int x = indeces[i, 1];
                    int y = indeces[i, 0];
                    animationSourceRects[state][i] =
                        new Rectangle((int)(x * spriteDimension.X), (int)(y * spriteDimension.Y), (int)(spriteDimension.X), (int)(spriteDimension.Y));

                }
            }
            return animationSourceRects;
        }

        static Dictionary<AnimationState, Rectangle[]> BuildAnimationSourceRects(Dictionary<AnimationState, int[]> animations, Vector2 spriteDimension)
        {
            Dictionary<AnimationState, Rectangle[]> animationSourceRects = new Dictionary<AnimationState, Rectangle[]>();
            foreach (var animationSet in animations)
            {
                AnimationState state = animationSet.Key;
                int[] indeces = animationSet.Value;
                animationSourceRects[state] = new Rectangle[indeces.Length];
                for (int i = 0; i < indeces.Length; i++)
                {
                    int x = indeces[i];
                    animationSourceRects[state][i] =
                        new Rectangle((int)(x * spriteDimension.X), 0, (int)(spriteDimension.X), (int)(spriteDimension.Y));

                }
            }
            return animationSourceRects;
        }

        private void ResetAnimation()
        {
            AnimationTimer = 0;
            AnimationTreshold = AnimationDuration / AnimationSourceRects[Direction].Length;
            DirectionalIndex = 0;
            SourceRectangle = AnimationSourceRects[Direction][DirectionalIndex];
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            AnimationTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (AnimationTimer > AnimationTreshold)
            {
                DirectionalIndex = (DirectionalIndex + 1) % AnimationSourceRects[Direction].Length;
                AnimationTimer = 0;
                SourceRectangle = AnimationSourceRects[Direction][DirectionalIndex];
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
                    if (AnimationSourceRects.ContainsKey(AnimationState.IdleRight)) { return AnimationState.IdleRight; }
                    if (AnimationSourceRects.ContainsKey(AnimationState.IdleLeft)) { return AnimationState.IdleLeft; }
                    break;
                case AnimationState.WalkLeft:
                    if (AnimationSourceRects.ContainsKey(AnimationState.IdleLeft)) { return AnimationState.IdleLeft; }
                    break;
                case AnimationState.WalkRight:
                    if (AnimationSourceRects.ContainsKey(AnimationState.IdleRight)) { return AnimationState.IdleRight; }
                    break;
                case AnimationState.IdleLeft:
                    if (AnimationSourceRects.ContainsKey(AnimationState.Idle)) { return AnimationState.Idle; }
                    break;
                case AnimationState.IdleRight:
                    if (AnimationSourceRects.ContainsKey(AnimationState.Idle)) { return AnimationState.Idle; }
                    break;
                case AnimationState.WalkUp:
                    if (AnimationSourceRects.ContainsKey(AnimationState.WalkRight)) { return AnimationState.WalkRight; }
                    if (AnimationSourceRects.ContainsKey(AnimationState.WalkLeft)) { return AnimationState.WalkLeft; }
                    break;
                case AnimationState.WalkDown:
                    if (AnimationSourceRects.ContainsKey(AnimationState.WalkLeft)) { return AnimationState.WalkLeft; }
                    if (AnimationSourceRects.ContainsKey(AnimationState.WalkRight)) { return AnimationState.WalkRight; }
                    break;
                default:
                    break;
            }
            // Last resort
            return AnimationSourceRects.Keys.First();
        }

        private void UpdateDirection(AnimationState direction)
        {
            if (!AnimationSourceRects.ContainsKey(direction))
            {
                direction = GetFallbackDirection(direction);
            }

            Direction = direction;
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
            else if (movement.Y > 0 && AnimationSourceRects.ContainsKey(AnimationState.WalkDown))
            {
                Update(gameTime, AnimationState.WalkDown);
                LastMovement.Y = movement.Y;
            }
            else if (movement.Y < 0 && AnimationSourceRects.ContainsKey(AnimationState.WalkUp))
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