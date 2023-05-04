using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YGR
{
    public static class Manager_Sprites
    {

        // Players
        public static Texture2D Player_Ninja { get; private set; }
        public static Texture2D Player_Simple { get; private set; }
        public static Texture2D Player_NerdyGirl { get; private set; }
        public static Texture2D Player_Ghost { get; private set; }
        public static IList<Texture2D> AimIndicator { get; private set; }

        // Projectiles
        public static Texture2D Projectile_Simple { get; private set; }


        // Power Ups
        public static Texture2D SpinningHeart { get; private set; }
        public static Texture2D SpinningPlus { get; private set; }

        // Enemies
        public static Texture2D Enemy_Basic { get; private set; }
        public static Texture2D Enemy_Gigachad { get; private set; }

        public static void LoadContent(ContentManager contentManager)
        {
            Player_Ninja = contentManager.Load<Texture2D>("SpritesCharacters/charaset");
            Player_Simple = contentManager.Load<Texture2D>("SpritesCharacters/tester_60");
            Player_Ghost = contentManager.Load<Texture2D>("SpritesCharacters/ghosty");
            Player_NerdyGirl = contentManager.Load<Texture2D>("SpritesCharacters/NerdyGirl");

            AimIndicator = new List<Texture2D> {
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_red"),
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_blue"),
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_green"),
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_yellow"),
            };

            Projectile_Simple = contentManager.Load<Texture2D>("SpritesOther/projectiles");

            Enemy_Basic = Player_Simple;
            Enemy_Gigachad = contentManager.Load<Texture2D>("SpritesCharacters/gigachad");

            SpinningHeart = contentManager.Load<Texture2D>("SpritesOther/SpinningHeart");
            SpinningPlus = contentManager.Load<Texture2D>("SpritesOther/SpinningPlus");
        }

        public static AnimatedSprite NewAnimatedSprite_SpinningHeart()
        {
            return new AnimatedSprite(
                texture: SpinningHeart,
                spriteDimension: new Vector2(638, 987),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
                },
                animationDuration: 1000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_SpinningPlus()
        {
            return new AnimatedSprite(
                texture: SpinningPlus,
                spriteDimension: new Vector2(1811, 1938),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5 } },
                },
                animationDuration: 1000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_TestCharacter()
        {
            return new AnimatedSprite(
                    texture: Player_Simple,
                    spriteDimension: new Vector2(44, 62),
                    animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.WalkLeft, new int[] { 1, 2 } },
                    { AnimationState.IdleLeft, new int[] { 1 } },
                    { AnimationState.WalkRight, new int[] { 3, 4 } },
                    { AnimationState.IdleRight, new int[] { 3 } },
                    }
                );
        }

        public static AnimatedSprite NewAnimatedSprite_Gigachad()
        {
            return new AnimatedSprite(
                    texture: Enemy_Gigachad,
                    spriteDimension: new Vector2(250, 250),
                    animations: new Dictionary<AnimationState, int[]> {
                        { AnimationState.WalkLeft, new int[] { 1 } },
                        { AnimationState.IdleLeft, new int[] { 1 } },
                        { AnimationState.WalkRight, new int[] { 0 } },
                        { AnimationState.IdleRight, new int[] { 0 } },
                    }
                );
        }

        public static AnimatedSprite NewAnimatedSprite_Ghost()
        {
            return new AnimatedSprite(
                texture: Player_Ghost,
                spriteDimension: new Vector2(Player_Ghost.Width / 8, Player_Ghost.Height),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.WalkRight, new int[] { 0, 1, 2, 3, 2, 1 } },
                    { AnimationState.IdleRight, new int[] { 0, 1, 2, 3, 2, 1 } },
                    { AnimationState.WalkLeft, new int[] { 7, 6, 5, 4, 5, 6 } },
                    { AnimationState.IdleLeft, new int[] { 7, 6, 5, 4, 5, 6 } },
                }
            );
        }

        public static AnimatedSprite NewAnimatedSprite_NerdyGirl()
        {
            return new AnimatedSprite(
                texture: Player_NerdyGirl,
                spriteDimension: new Vector2(1142, 1527),
                animations: new Dictionary<AnimationState, int[,]> {
                    { AnimationState.WalkLeft, new int[,] { {0,0}, {0,1}, {0,2}, {0,3} } },
                    { AnimationState.IdleLeft, new int[,] { {0,4}, {0,5}, {0,6}, {0,7} } },
                    { AnimationState.IdleRight, new int[,] { {1,0}, {1,1}, {1,2}, {1,3} } },
                    { AnimationState.WalkRight, new int[,] { {1,4}, {1,5}, {1,6}, {1,7} } },
                },
                animationDuration: 1000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Ninja()
        {
            return new AnimatedSprite(
                texture: Player_Ninja,
                spriteDimension: new Vector2(48, 64),
                animationSourceRects: new Dictionary<AnimationState, Rectangle[]> {
                    {
                        AnimationState.WalkDown, new Rectangle[]
                        {
                            new Rectangle(0, 128, 48, 64),
                            new Rectangle(48, 128, 48, 64),
                            new Rectangle(96, 128, 48, 64)
                        }
                    },
                    {
                        AnimationState.WalkUp, new Rectangle[]
                        {
                            new Rectangle(0, 0, 48, 64),
                            new Rectangle(48, 0, 48, 64),
                            new Rectangle(96, 0, 48, 64)
                        }
                    },
                    {
                        AnimationState.WalkRight, new Rectangle[]
                        {
                            new Rectangle(0, 64, 48, 64),
                            new Rectangle(48, 64, 48, 64),
                            new Rectangle(96, 64, 48, 64)
                        }
                    },
                    {
                        AnimationState.WalkLeft, new Rectangle[]
                        {
                            new Rectangle(0, 192, 48, 64),
                            new Rectangle(48, 192, 48, 64),
                            new Rectangle(96, 192, 48, 64)
                        }
                    },
                        {
                        AnimationState.Idle, new Rectangle[]
                        {
                            new Rectangle(0, 128, 48, 64),
                            new Rectangle(0, 128, 48, 64),
                            new Rectangle(0, 128, 48, 64)
                        }
                    }
                },
                animationDuration: 750
            );
        }
    }
}
