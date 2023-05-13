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
        public static Texture2D Player_Mailman { get; private set; }
        public static Texture2D Player_Ghost { get; private set; }
        public static Texture2D AimIndicator { get; private set; }

        // Projectiles
        public static Texture2D Projectile_Simple { get; private set; }

        // Weapons
        public static Texture2D Weapon_Pistol { get; private set; }
        public static Texture2D Weapon_Keyboard { get; private set; }
        public static Texture2D Weapon_RedGun { get; private set; }
        public static Texture2D Weapon_Shotgun { get; private set; }
        public static Texture2D Weapon_Hammer { get; private set; }

        // UI
        public static Texture2D HealthbarEmpty { get; private set; }
        public static Texture2D HealthbarInfill { get; private set; }
        public static Texture2D White { get; private set; }

        // Background
        public static Texture2D BackgroundYggdrasil { get; private set; }
        public static Texture2D BackgroundSky { get; private set; }
        public static Texture2D BackgroundTitleText { get; private set; }

        // Level elements
        public static Texture2D ButtonOut { get; private set; }
        public static Texture2D ButtonHalf { get; private set; }
        public static Texture2D ButtonIn { get; private set; }
        public static Texture2D EscapeButtonOut { get; private set; }
        public static Texture2D EscapeButtonHalf { get; private set; }
        public static Texture2D EscapeButtonIn { get; private set; }

        // Power Ups
        public static Texture2D SpinningHeart { get; private set; }
        public static Texture2D SpinningPlus { get; private set; }

        // Enemies
        public static Texture2D Enemy_Basic { get; private set; }
        public static Texture2D Enemy_Slime { get; private set; }
        public static Texture2D Enemy_Gigachad { get; private set; }
        public static Texture2D Enemy_Boss { get; private set; }

        // Effects
        public static Texture2D Effect_Confusion { get; private set; }
        public static Texture2D Effect_Shield { get; private set; }

        public static void LoadContent(ContentManager contentManager)
        {
            Player_Ninja = contentManager.Load<Texture2D>("SpritesCharacters/charaset");
            Player_Simple = contentManager.Load<Texture2D>("SpritesCharacters/tester_60");
            Player_Ghost = contentManager.Load<Texture2D>("SpritesCharacters/ghosty");
            Player_NerdyGirl = contentManager.Load<Texture2D>("SpritesCharacters/NerdyGirl");
            Player_Mailman = contentManager.Load<Texture2D>("SpritesCharacters/Mailman");

            AimIndicator = contentManager.Load<Texture2D>("SpritesOther/target_indicator");
            Projectile_Simple = contentManager.Load<Texture2D>("SpritesOther/projectiles");

            Weapon_Pistol = contentManager.Load<Texture2D>("SpritesWeapons/Pistol");
            Weapon_Keyboard = contentManager.Load<Texture2D>("SpritesWeapons/Keyboard");
            Weapon_RedGun = contentManager.Load<Texture2D>("SpritesWeapons/Red_Gun");
            Weapon_Shotgun = contentManager.Load<Texture2D>("SpritesWeapons/Shotgun");
            Weapon_Hammer = contentManager.Load<Texture2D>("SpritesWeapons/hammer");

            HealthbarEmpty = contentManager.Load<Texture2D>("SpritesOther/healthbar_empty");
            HealthbarInfill = contentManager.Load<Texture2D>("SpritesOther/healthbar_infill");
            White = contentManager.Load<Texture2D>("SpritesOther/white");

            BackgroundYggdrasil = contentManager.Load<Texture2D>("SpritesOther/background_yggdrasil");
            BackgroundSky = contentManager.Load<Texture2D>("SpritesOther/background_sky");
            BackgroundTitleText = contentManager.Load<Texture2D>("SpritesOther/background_text");

            ButtonOut = contentManager.Load<Texture2D>("SpritesOther/Button_Out_2");
            ButtonHalf = contentManager.Load<Texture2D>("SpritesOther/Button_Half_2");
            ButtonIn = contentManager.Load<Texture2D>("SpritesOther/Button_In_2");
            EscapeButtonOut = contentManager.Load<Texture2D>("SpritesOther/Escape_Button-Out");
            EscapeButtonHalf = contentManager.Load<Texture2D>("SpritesOther/Escape_Button-Half");
            EscapeButtonIn = contentManager.Load<Texture2D>("SpritesOther/Escape_Button-In");

            Enemy_Basic = Player_Simple;
            Enemy_Slime = contentManager.Load<Texture2D>("SpritesCharacters/slime");
            Enemy_Gigachad = contentManager.Load<Texture2D>("SpritesCharacters/gigachad");
            Enemy_Boss = contentManager.Load<Texture2D>("SpritesCharacters/slime_boss_sheet");

            SpinningHeart = contentManager.Load<Texture2D>("SpritesOther/SpinningHeart");
            SpinningPlus = contentManager.Load<Texture2D>("SpritesOther/SpinningPlus");

            Effect_Confusion = contentManager.Load<Texture2D>("SpritesEffects/noise");
            Effect_Shield = contentManager.Load<Texture2D>("SpritesEffects/shield");
        }

        public static AnimatedSprite NewAnimatedSprite_Confusion()
        {
            return new AnimatedSprite(
                texture: Effect_Confusion,
                spriteDimension: new Vector2(128, 128),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 } },
                },
                animationDuration: 500
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Shield()
        {
            return new AnimatedSprite(
                texture: Effect_Shield,
                spriteDimension: new Vector2(256, 160),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 } },
                },
                animationDuration: 750
            );
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

        public static AnimatedSprite NewAnimatedSprite_ProjectileHammer()
        {
            return new AnimatedSprite(
                texture: Weapon_Hammer,
                spriteDimension: new Vector2(Weapon_Hammer.Height, Weapon_Hammer.Height),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0 } },
                }
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

        public static AnimatedSprite NewAnimatedSprite_ProjectileSlimeOuter()
        {
            return new AnimatedSprite(
                texture: Projectile_Simple,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3 } },
                }
            );
        }

        public static AnimatedSprite NewAnimatedSprite_ProjectileSlimeInner()
        {
            return new AnimatedSprite(
                texture: Projectile_Simple,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 4, 5, 6, 7 } },
                }
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

        public static AnimatedSprite NewAnimatedSprite_EnemySlime()
        {
            return new AnimatedSprite(
                    texture: Enemy_Slime,
                    spriteDimension: new Vector2(55, 42),
                    animations: new Dictionary<AnimationState, int[]> {
                        { AnimationState.WalkLeft, new int[] { 0, 1, 2, 3, 4, 3, 2, 1 } },
                        { AnimationState.WalkRight, new int[] { 0, 1, 2, 3, 4, 3, 2, 1 } },
                        { AnimationState.IdleLeft, new int[] { 3, 4, 3 } },
                        { AnimationState.IdleRight, new int[] { 3, 4, 3 } },
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

        public static AnimatedSprite NewAnimatedSprite_EnemyBoss()
        {
            return new AnimatedSprite(
                    texture: Enemy_Boss,
                    spriteDimension: new Vector2(235, 205),
                    animations: new Dictionary<AnimationState, int[,]> {
                        { AnimationState.WalkLeft, new int[,] { {1,0}, {1,1}, {1,2} } },
                        { AnimationState.IdleLeft, new int[,] { {1,1}, {1,2} } },
                        { AnimationState.WalkRight, new int[,] { {1,0}, {1,1}, {1,2} } },
                        { AnimationState.IdleRight, new int[,] { {1,1}, {1,2} } },
                        { AnimationState.Jump, new int[,] { {1,2}, {1,1}, {1,0}, {1,1}, {1,2}, {2,0}, {2,1}, {2,2}, {2,3}, {2,4}, {2,5}, {2,6}, {2,7}, {2,8}}},
                        { AnimationState.Spawn, new int[,] { {0,0}, {0,0}, {0,0}, {0,0}, {0,0}, {0,1}, {0,2}, {0,3}, {0,4}, {1,0}, {1,1}, {1,2}}},
                        { AnimationState.Hide, new int[,] { {0,0} }}
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

        public static AnimatedSprite NewAnimatedSprite_Mailman()
        {
            return new AnimatedSprite(
                texture: Player_Mailman,
                spriteDimension: new Vector2(563, 911),
                animations: new Dictionary<AnimationState, int[,]> {
                    { AnimationState.WalkLeft, new int[,] { {1,7}, {1,6}, {1,7}, {1,5}, {1,4}, {1,3}, {1,4}, {1,5} } },
                    { AnimationState.IdleLeft, new int[,] { {1,2}, {1,1}, {1,0}, {1,1} } },
                    { AnimationState.IdleRight, new int[,] { {0,5}, {0,6}, {0,7}, {0,5} } },
                    { AnimationState.WalkRight, new int[,] { {0,0}, {0,1}, {0,0}, {0,2}, {0,3}, {0,4}, {0,3}, {0,2} } },
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
