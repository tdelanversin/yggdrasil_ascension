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
        public static Texture2D Player_Ghost { get; private set; }
        public static IList<Texture2D> AimIndicator { get; private set; }

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
            AimIndicator = new List<Texture2D> {
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_red"),
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_blue"),
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_green"),
                contentManager.Load<Texture2D>("SpritesOther/target_indicator_yellow"),
            };

            Enemy_Basic = Player_Simple;
            Enemy_Gigachad = contentManager.Load<Texture2D>("SpritesCharacters/gigachad");

            SpinningHeart = contentManager.Load<Texture2D>("SpritesOther/SpinningHeart");
            SpinningPlus = contentManager.Load<Texture2D>("SpritesOther/SpinningPlus");
        }

        public static AnimatedSprite NewAnimatedSprite_SpinningHeart(){
            return new AnimatedSprite(
                texture: SpinningHeart,
                spriteDimension: new Vector2(638, 987),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
                },
                animationDuration: 1000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_SpinningPlus(){
            return new AnimatedSprite(
                texture: SpinningPlus,
                spriteDimension: new Vector2(1811, 1938),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5 } },
                },
                animationDuration: 1000
            );
        }
    }
}
