using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YGR
{
    public static class Manager_Sprites
    {

        // Players
        public static Texture2D Player_OldNinja { get; private set; }
        public static Texture2D Player_Ninja { get; private set; }
        public static Texture2D Player_Simple { get; private set; }
        public static Texture2D Player_NerdyGirl2 { get; private set; }
        public static Texture2D Player_NerdyGirl { get; private set; }
        public static Texture2D Player_Mailman { get; private set; }
        public static Texture2D Player_Professor { get; private set; }
        public static Texture2D Player_Ghost { get; private set; }
        public static Texture2D AimIndicator { get; private set; }

        // Projectiles
        public static Texture2D Projectile_Simple { get; private set; }
        public static Texture2D Projectile_Helix { get; private set; }
        public static Texture2D Projectile_Keyboard_Pink { get; private set; }
        public static Texture2D Projectile_Letter { get; private set; }
        public static Texture2D Projectile_Book { get; private set; }
        public static Texture2D Projectile_NinjaStar { get; private set; }
        public static Texture2D Projectile_Confusion { get; private set; }

        // Weapons
        public static Texture2D Weapon_Pistol { get; private set; }
        public static Texture2D Weapon_Keyboard { get; private set; }
        public static Texture2D Weapon_Keyboard_Pink { get; private set; }
        public static Texture2D Weapon_Letter { get; private set; }
        public static Texture2D Weapon_Book { get; private set; }
        public static Texture2D Weapon_Ninja_Star { get; private set; }
        public static Texture2D Weapon_RedDevil { get; private set; }
        public static Texture2D Weapon_Helix { get; private set; }
        public static Texture2D Weapon_Blunderbuss { get; private set; }
        public static Texture2D Weapon_Sniper { get; private set; }
        public static Texture2D Weapon_Shotgun { get; private set; }
        public static Texture2D Weapon_Hammer { get; private set; }

        // UI
        public static Texture2D HealthbarEmpty { get; private set; }
        public static Texture2D HealthbarInfill { get; private set; }
        public static Texture2D HealthbarInfillBoss { get; private set; }
        public static Texture2D HealthbarBackgroundBoss { get; private set; }
        public static Texture2D HealthbarForegroundBoss { get; private set; }
        public static Texture2D HealthbarSeperatorBoss { get; private set; }
        public static Texture2D Controls { get; private set; }
        public static Texture2D White { get; private set; }
        public static Texture2D CircleTimer {get; private set; }

        // Images
        public static Texture2D BackgroundYggdrasil { get; private set; }
        public static Texture2D BackgroundSky { get; private set; }
        public static Texture2D BackgroundTitleText { get; private set; }
        public static Texture2D ImageDefeat { get; private set; }
        public static Texture2D ImageVictory { get; private set; }

        // Level elements
        public static Texture2D ButtonOut { get; private set; }
        public static Texture2D ButtonHalf { get; private set; }
        public static Texture2D ButtonIn { get; private set; }
        public static Texture2D EscapeButtonOut { get; private set; }
        public static Texture2D EscapeButtonHalf { get; private set; }
        public static Texture2D EscapeButtonIn { get; private set; }
        public static Texture2D Gravestone { get; private set; }

        // Power Ups
        public static Texture2D SpinningHeart { get; private set; }
        public static Texture2D SpinningPlus { get; private set; }
        public static Texture2D SpinningQuestionMark { get; private set; }
        public static Texture2D LevelUp_Girly { get; private set; }
        public static Texture2D LevelUp_Mailman { get; private set; }
        public static Texture2D LevelUp_Ninja { get; private set; }
        public static Texture2D LevelUp_Prof { get; private set; }

        // Enemies
        public static Texture2D Enemy_Basic { get; private set; }
        public static Texture2D Enemy_Slime { get; private set; }
        public static Texture2D Enemy_SlimeSpiky { get; private set; }
        public static Texture2D Enemy_Gigachad { get; private set; }
        public static Texture2D Enemy_GigachadSlime { get; private set; }
        public static Texture2D Enemy_Boss { get; private set; }

        // Effects
        public static Texture2D Effect_Confusion { get; private set; }
        public static Texture2D Effect_Blank { get; private set; }
        public static Texture2D Effect_Invincibility { get; private set; }
        public static Texture2D Effect_Shield { get; private set; }
        public static Texture2D Effect_Gunslinger { get; private set; }

        // Level Ups
        public static Texture2D LevelUp_DarkFlash { get; private set; }
        public static Texture2D LevelUp_LightFlash { get; private set; }
        public static Texture2D LevelUp_Bullet { get; private set; }
        public static Texture2D LevelUp_DarkFlash_NoShade { get; private set; }
        public static Texture2D LevelUp_LightFlash_NoShade { get; private set; }
        public static Texture2D LevelUp_Bullet_NoShade { get; private set; }

        // Particles
        public static Texture2D Slime_Death_Particle { get; private set; }
        public static Texture2D Bullet_Impact_Particle { get; private set; }
        public static Texture2D Dust_Particle { get; private set; }

        public static void LoadContent(ContentManager contentManager)
        {
            Player_OldNinja = contentManager.Load<Texture2D>("SpritesCharacters/charaset");
            Player_Ninja = contentManager.Load<Texture2D>("SpritesCharacters/Ninja10x");
            Player_Simple = contentManager.Load<Texture2D>("SpritesCharacters/tester_60");
            Player_Ghost = contentManager.Load<Texture2D>("SpritesCharacters/ghosty");
            Player_NerdyGirl = contentManager.Load<Texture2D>("SpritesCharacters/NerdyGirl");
            Player_NerdyGirl2 = contentManager.Load<Texture2D>("SpritesCharacters/NerdyGirl2");
            Player_Mailman = contentManager.Load<Texture2D>("SpritesCharacters/Mailman");
            Player_Professor = contentManager.Load<Texture2D>("SpritesCharacters/Prof");

            AimIndicator = contentManager.Load<Texture2D>("SpritesOther/target_indicator");
            Projectile_Simple = contentManager.Load<Texture2D>("SpritesOther/projectiles");
            Projectile_Helix = contentManager.Load<Texture2D>("SpritesOther/Player_Projectile");
            Projectile_Keyboard_Pink = contentManager.Load<Texture2D>("SpriteProjectiles/Keyboard_Projectile");
            Projectile_Letter = contentManager.Load<Texture2D>("SpriteProjectiles/Letter_Projectile");
            Projectile_Book = contentManager.Load<Texture2D>("SpriteProjectiles/Book_Projectile");
            Projectile_NinjaStar = contentManager.Load<Texture2D>("SpriteProjectiles/Ninja_Star_Projectile");
            Projectile_Confusion = contentManager.Load<Texture2D>("SpriteProjectiles/Confusion_Projectile");

            Weapon_Pistol = contentManager.Load<Texture2D>("SpritesWeapons/Pistol");
            Weapon_Keyboard = contentManager.Load<Texture2D>("SpritesWeapons/Keyboard");
            Weapon_Keyboard_Pink = contentManager.Load<Texture2D>("SpritesWeapons/Keyboard_pink");
            Weapon_Letter = contentManager.Load<Texture2D>("SpritesWeapons/Letter");
            Weapon_Book = contentManager.Load<Texture2D>("SpritesWeapons/Book");
            Weapon_Ninja_Star = contentManager.Load<Texture2D>("SpritesWeapons/Ninja_star");
            Weapon_RedDevil = contentManager.Load<Texture2D>("SpritesWeapons/Red_Gun");
            Weapon_Helix = contentManager.Load<Texture2D>("SpritesWeapons/Helix");
            Weapon_Blunderbuss = contentManager.Load<Texture2D>("SpritesWeapons/Blunderbuss");
            Weapon_Sniper = contentManager.Load<Texture2D>("SpritesWeapons/Sniper");
            Weapon_Shotgun = contentManager.Load<Texture2D>("SpritesWeapons/Shotgun");
            Weapon_Hammer = contentManager.Load<Texture2D>("SpritesWeapons/hammer");

            HealthbarEmpty = contentManager.Load<Texture2D>("SpritesOther/healthbar_empty");
            HealthbarInfill = contentManager.Load<Texture2D>("SpritesOther/healthbar_infill");
            HealthbarInfillBoss = contentManager.Load<Texture2D>("SpritesOther/Boss_Healthbar_Infill");
            HealthbarBackgroundBoss = contentManager.Load<Texture2D>("SpritesOther/Boss_Healthbar_Background");
            HealthbarForegroundBoss = contentManager.Load<Texture2D>("SpritesOther/Boss_Healthbar_Foreground");
            HealthbarSeperatorBoss = contentManager.Load<Texture2D>("SpritesOther/Boss_Healthbar_Separator");
            Controls = contentManager.Load<Texture2D>("SpritesOther/Controls");
            White = contentManager.Load<Texture2D>("SpritesOther/white");
            CircleTimer = contentManager.Load<Texture2D>("SpritesOther/timer");

            BackgroundYggdrasil = contentManager.Load<Texture2D>("Images/title_tree");
            BackgroundSky = contentManager.Load<Texture2D>("Images/title_sky");
            BackgroundTitleText = contentManager.Load<Texture2D>("Images/title_text");

            ButtonOut = contentManager.Load<Texture2D>("SpritesOther/Button_Out_2");
            ButtonHalf = contentManager.Load<Texture2D>("SpritesOther/Button_Half_2");
            ButtonIn = contentManager.Load<Texture2D>("SpritesOther/Button_In_2");
            EscapeButtonOut = contentManager.Load<Texture2D>("SpritesOther/Escape_Button-Out");
            EscapeButtonHalf = contentManager.Load<Texture2D>("SpritesOther/Escape_Button-Half");
            EscapeButtonIn = contentManager.Load<Texture2D>("SpritesOther/Escape_Button-In");
            Gravestone = contentManager.Load<Texture2D>("SpritesOther/gravestone");

            Enemy_Basic = Player_Simple;
            Enemy_Slime = contentManager.Load<Texture2D>("SpritesCharacters/slime");
            Enemy_SlimeSpiky = contentManager.Load<Texture2D>("SpritesCharacters/spiky_sheet");
            Enemy_Gigachad = contentManager.Load<Texture2D>("SpritesCharacters/gigachad");
            Enemy_GigachadSlime = contentManager.Load<Texture2D>("SpritesCharacters/GigachadSlime");
            Enemy_Boss = contentManager.Load<Texture2D>("SpritesCharacters/slime_boss_sheet");

            SpinningHeart = contentManager.Load<Texture2D>("SpritesOther/SpinningHeart");
            SpinningPlus = contentManager.Load<Texture2D>("SpritesOther/SpinningPlus");
            SpinningQuestionMark = contentManager.Load<Texture2D>("SpritesOther/random-powerup");
            LevelUp_Girly = contentManager.Load<Texture2D>("SpritesOther/LevelUp/girly");
            LevelUp_Mailman = contentManager.Load<Texture2D>("SpritesOther/LevelUp/mailman");
            LevelUp_Ninja = contentManager.Load<Texture2D>("SpritesOther/LevelUp/ninja");
            LevelUp_Prof = contentManager.Load<Texture2D>("SpritesOther/LevelUp/prof");

            Effect_Confusion = contentManager.Load<Texture2D>("SpritesEffects/Confusion_Effect");
            Effect_Blank = contentManager.Load<Texture2D>("SpritesEffects/noise"); //TODO: Add blank effect
            Effect_Invincibility = contentManager.Load<Texture2D>("SpritesEffects/noise"); //TODO: Add invincibility effect
            Effect_Shield = contentManager.Load<Texture2D>("SpritesEffects/shield");
            Effect_Gunslinger = contentManager.Load<Texture2D>("SpritesEffects/noise"); //TODO: Add gunslinger effect

            LevelUp_DarkFlash = contentManager.Load<Texture2D>("SpritesOther/thunder_up_sheet_2");
            LevelUp_LightFlash = contentManager.Load<Texture2D>("SpritesOther/tunder_up_sheet");
            LevelUp_Bullet = contentManager.Load<Texture2D>("SpritesOther/bullet_sheet");

            LevelUp_DarkFlash_NoShade = contentManager.Load<Texture2D>("SpritesOther/thunder_up_sheet_2_no_shade");
            LevelUp_LightFlash_NoShade = contentManager.Load<Texture2D>("SpritesOther/tunder_up_sheet_no_shade");
            LevelUp_Bullet_NoShade = contentManager.Load<Texture2D>("SpritesOther/bullet_sheet_no_shade");

            Slime_Death_Particle = contentManager.Load<Texture2D>("SpritesOther/slime_death");
            Bullet_Impact_Particle = contentManager.Load<Texture2D>("SpritesEffects/Wall_Impact_Particle");
            Dust_Particle = contentManager.Load<Texture2D>("SpritesEffects/Dust_Particle");

            ImageDefeat = contentManager.Load<Texture2D>("Images/defeat");
            ImageVictory = contentManager.Load<Texture2D>("Images/victory");
        }

        public static AnimatedSprite NewAnimatedSprite_LevelUp_DarkFlash_NoShade()
        {
            return new AnimatedSprite(
                texture: LevelUp_DarkFlash_NoShade,
                spriteDimension: new Vector2(533, 1035),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
                },
                animationDuration: 750
            );
        } 
        public static AnimatedSprite NewAnimatedSprite_Small_Slime_Death_Particle()
        {
            return new AnimatedSprite(
                texture: Slime_Death_Particle,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 2, 3, 4, 5, 6, 7 } },
                },
                animationDuration: 750
            );
        } 
        public static AnimatedSprite NewAnimatedSprite_Big_Slime_Death_Particle()
        {
            return new AnimatedSprite(
                texture: Slime_Death_Particle,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7} },
                },
                animationDuration: 1000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Bullet_Impact_Particle()
        {
            return new AnimatedSprite(
                texture: Bullet_Impact_Particle,
                spriteDimension: new Vector2(27, 12),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3} },
                },
                animationDuration: 400
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Dust_Particle()
        {
            return new AnimatedSprite(
                texture: Dust_Particle,
                spriteDimension: new Vector2(28, 25),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5} },
                },
                animationDuration: 400
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Slime_Impact_Particle()
        {
            return new AnimatedSprite(
                texture: Slime_Death_Particle,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 5, 6, 7 } },
                },
                animationDuration: 375
            );
        }

        public static AnimatedSprite NewAnimatedSprite_LevelUp_LightFlash_NoShade()
        {
            return new AnimatedSprite(
                texture: LevelUp_LightFlash_NoShade,
                spriteDimension: new Vector2(533, 1035),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
                },
                animationDuration: 750
            );
        }

        public static AnimatedSprite NewAnimatedSprite_LevelUp_Bullet_NoShade()
        {
            return new AnimatedSprite(
                texture: LevelUp_Bullet_NoShade,
                spriteDimension: new Vector2(675, 955),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 } },
                },
                animationDuration: 750
            );
        }
        public static AnimatedSprite NewAnimatedSprite_LevelUp_DarkFlash()
        {
            return new AnimatedSprite(
                texture: LevelUp_DarkFlash,
                spriteDimension: new Vector2(533, 1035),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
                },
                animationDuration: 750
            );
        }

        public static AnimatedSprite NewAnimatedSprite_LevelUp_LightFlash()
        {
            return new AnimatedSprite(
                texture: LevelUp_LightFlash,
                spriteDimension: new Vector2(533, 1035),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 } },
                },
                animationDuration: 750
            );
        }

        public static AnimatedSprite NewAnimatedSprite_LevelUp_Bullet()
        {
            return new AnimatedSprite(
                texture: LevelUp_Bullet,
                spriteDimension: new Vector2(675, 955),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 } },
                },
                animationDuration: 750
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Confusion()
        {
            return new AnimatedSprite(
                texture: Effect_Confusion,
                spriteDimension: new Vector2(128, 128),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3} },
                },
                animationDuration: 500
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Blank() //TODO: add blank effect
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
        public static AnimatedSprite NewAnimatedSprite_SpinningQuestionMark()
        {
            return new AnimatedSprite(
                texture: SpinningQuestionMark,
                spriteDimension: new Vector2(1272, 2349),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[]  { 0, 1, 2, 3, 4, 5, 6, 7 } },
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

        public static AnimatedSprite NewAnimatedSprite_ProjectileHelix()
        {
            return new AnimatedSprite(
                texture: Projectile_Helix,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0 } },
                }
            );
        }

        public static AnimatedSprite NewAnimatedSprite_ProjectileConfusion()
        {
            return new AnimatedSprite(
                texture: Projectile_Confusion,
                spriteDimension: new Vector2(26, 19),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0 } },
                }
            );
        }

        public static AnimatedSprite NewAnimatedSprite_ProjectileKeyboardPink()
        {
            return new AnimatedSprite(
                texture: Projectile_Keyboard_Pink,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 } },
                },
                animationDuration: 2000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_ProjectileLetter()
        {
            return new AnimatedSprite(
                texture: Projectile_Letter,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 } },
                },
                animationDuration: 2000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_ProjectileBook()
        {
            return new AnimatedSprite(
                texture: Projectile_Book,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 } },
                },
                animationDuration: 2000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_ProjectileNinjaStar()
        {
            return new AnimatedSprite(
                texture: Projectile_NinjaStar,
                spriteDimension: new Vector2(32, 32),
                animations: new Dictionary<AnimationState, int[]> {
                    { AnimationState.Idle, new int[] { 0, 1, 2, 3} },
                },
                animationDuration: 200
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

        public static AnimatedSprite NewAnimatedSprite_EnemySlimeSpiky()
        {
            return new AnimatedSprite(
                    texture: Enemy_SlimeSpiky,
                    spriteDimension: new Vector2(125, 102),
                    // spriteDimension: new Vector2(64, 51),
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

        public static AnimatedSprite NewAnimatedSprite_GigachadSlime()
        {
            return new AnimatedSprite(
                    texture: Enemy_GigachadSlime,
                    spriteDimension: new Vector2(572, 637),
                    animations: new Dictionary<AnimationState, int[,]> {
                        { AnimationState.WalkLeft, new int[,] { {0,0}, {0,1}, {0,2}, {0,1} } },
                        { AnimationState.IdleLeft, new int[,] { {0,0}, {0,1}, {0,2}, {0,1} } },
                        { AnimationState.WalkRight, new int[,] { {1,2}, {1,1}, {1,0}, {1,1} } },
                        { AnimationState.IdleRight, new int[,] { {1,2}, {1,1}, {1,0}, {1,1} } }
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

        public static AnimatedSprite NewAnimatedSprite_NerdyGirl2()
        {
            return new AnimatedSprite(
                texture: Player_NerdyGirl2,
                spriteDimension: new Vector2(609, 451),
                animations: new Dictionary<AnimationState, int[,]> {
                    { AnimationState.WalkLeft, new int[,] { {0,0}, {0,1} } },
                    { AnimationState.IdleLeft, new int[,] { {0,2}, {0,3}, {0,4} } },
                    { AnimationState.IdleRight, new int[,] { {1,0}, {1,1}, {1,2} } },
                    { AnimationState.WalkRight, new int[,] { {1,3}, {1,4} } },
                },
                animationDuration: 1000
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

        public static AnimatedSprite NewAnimatedSprite_Professor()
        {
            return new AnimatedSprite(
                texture: Player_Professor,
                spriteDimension: new Vector2(201, 489),
                animations: new Dictionary<AnimationState, int[,]> {
                    { AnimationState.WalkLeft, new int[,] { {1,10}, {1,9}, {1,8}, {1,7}, {1,6}, {1,5}, {1,4}, {1,3} } },
                    { AnimationState.IdleLeft, new int[,] { {1,2}, {1,1}, {1,0}, {1,1} } },
                    { AnimationState.IdleRight, new int[,] { {0,8}, {0,9}, {0,10}, {0,9} } },
                    { AnimationState.WalkRight, new int[,] { {0,0}, {0,1}, {0,2}, {0,3}, {0,4}, {0,5}, {0,6}, {0,7} } },
                },
                animationDuration: 1000
            );
        }

        public static AnimatedSprite NewAnimatedSprite_Ninja()
        {
            return new AnimatedSprite(
                texture: Player_Ninja,
                spriteDimension: new Vector2(300, 410),
                animations: new Dictionary<AnimationState, int[,]> {
                    { AnimationState.WalkRight, new int[,] { {1,0}, {1,1}, {1,2}, {1,3}, {1,4}, {1,5} } },
                    { AnimationState.WalkLeft, new int[,] { {0,3}, {0,4}, {0,5}, {0,6}, {0,7}, {0,8} } },
                    { AnimationState.IdleRight, new int[,] { {1,6}, {1,7}, {1,8}, {1,7} } },
                    { AnimationState.IdleLeft, new int[,] { {0,0}, {0,1}, {0,2}, {0,1} } },
                },
                animationDuration: 500
            );
        }

        public static AnimatedSprite NewAnimatedSprite_OldNinja()
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
