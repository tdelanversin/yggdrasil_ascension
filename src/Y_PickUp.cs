using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace YGR
{
    public enum Y_PowerUps
    {
        // have an empty one as default
        None,

        // Powerups
        Revive,
        Life,

        // Weapons
        WeaponPistol,
        WeaponShotgun,
        WeaponKeyboard,
        WeaponFunky,
        WeaponEnemySlowPistol,
        WeaponWide,
        WeaponGiga,

        // CharacterChoosers
        ChooserNerd,
        ChooserNinja,
        ChooserMailman,
        ChooserProfessor,
        WeaponPinkHammer,

        LevelUpNerd,
        LevelUpProfessor,
        LevelUpMailman,

        // Dead Enemy
        Gravestone
    }

    public class PickUp : IGameElement
    {
        public float LocalScale { get; set; }

        public Rectangle Rect { get; set; }

        public bool Active { get; set; }

        public static float _gunScale = 2f;

        public Y_PowerUps Type { get; }

        public Color Color;

        public Func<IPlayer, PickUp, bool> Action { get; }
        public int ElementLevel { get { return 1; } set { } }

        private AnimatedSprite _sprite;
        private AnimatedSprite _carriedSprite;
        private float _spriteDrawScale;
        private bool _floaty;

        private Texture2D _texture;

        private IVictim _lastOwner;

        private static bool switchGun(IShooter gun, IPlayer player, PickUp self)
        {
            // This one has to be set at switching time
            // in the Update method of the PowerUp if _lastOwner != null then it will check if the two Rects
            // of the PowerUp and the _lastOwner still intersect. If now, then _lastOwner will be set to null
            // => prevent infinite chains of re-pickups of the old weapon
            if (self._lastOwner == player)
                return false;

            if (player is Player_Ghost)
                return false;

            var oldGun = player.Gun;
            player.Gun = gun;
            player.Room.PickUps.Remove(self);
            oldGun.DropAsPickUp(player, player.Room, self.Rect.Center);

            Manager_Sound.Sound_GunCocking.Play();

            return true;
        }

        public PickUp(
            Y_PowerUps type, 
            Point location, 
            int width, 
            int height, 
            float scale, 
            AnimatedSprite sprite, 
            AnimatedSprite carriedSprite,
            IVictim lastOwner, 
            bool floaty,
            Func<IPlayer, PickUp, bool> action)
        {
            LocalScale = scale * Y_Level.GlobalScale;
            Type = type;
            Color = Color.White;

            Action = action;
            Active = true;
            _sprite = sprite;
            _carriedSprite = carriedSprite;
            _floaty = floaty;

            int heightNew = (int)(height * LocalScale);
            int widthNew = (int)((heightNew * sprite.SpriteDimension.X / sprite.SpriteDimension.Y));

            Rect = new Rectangle(location.X - widthNew / 2, location.Y - heightNew / 2, widthNew, heightNew);
            _spriteDrawScale = LocalScale * Util.GetSpriteScale(Rect, _sprite.SpriteDimension);


            _lastOwner = lastOwner;
        }

        public PickUp(Y_PowerUps type, Point location, int width, int height, float scale, Texture2D texture, IVictim lastOwner, bool floaty, Func<IPlayer, PickUp, bool> action)
        {
            LocalScale = scale * Y_Level.GlobalScale;
            Type = type;
            Color = Color.White;

            int heightNew = (int)(height * scale);
            int widthNew = (int)((heightNew * texture.Width / texture.Height));

            Rect = new Rectangle(location.X - widthNew / 2, location.Y - heightNew / 2, widthNew, heightNew);
            Action = action;
            Active = true;
            _texture = texture;
            _spriteDrawScale = LocalScale * Util.GetSpriteScale(Rect, texture.Bounds.Size.ToVector2());
            _floaty = floaty;

            _lastOwner = lastOwner;
        }

        public PickUp(Y_PowerUps type, Point location, int width, int height, float scale, Texture2D texture, IVictim lastOwner, bool floaty, Color color, Func<IPlayer, PickUp, bool> action)
        : this(type, location, width, height, scale, texture, lastOwner, floaty, action)
        {
            Color = color;
        }

        public static PickUp Factory(Y_PowerUps type, Point location, int width, int height, float scale, IVictim lastOwner = null)
        {
            switch (type)
            {
                case Y_PowerUps.ChooserNerd:
                    return new PickUp(type, location, width, IPlayer.PlayerBaseHeight, scale * 1.0f, 
                        Manager_Sprites.NewAnimatedSprite_NerdyGirl(), 
                        null, 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player is not Player_NerdyGirl)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Nerd);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserMailman:
                    return new PickUp(type, location, width, IPlayer.PlayerBaseHeight, scale * 1.0f, 
                        Manager_Sprites.NewAnimatedSprite_Mailman(), 
                        null, 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player is not Player_Mailman)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Mailman);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserNinja:
                    return new PickUp(type, location, width, IPlayer.PlayerBaseHeight, scale * 1.0f, 
                        Manager_Sprites.NewAnimatedSprite_Ninja(), 
                        null, 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player is not Player_Ninja)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Ninja);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserProfessor:
                    return new PickUp(type, location, width, IPlayer.PlayerBaseHeight, scale * 1.0f, 
                        Manager_Sprites.NewAnimatedSprite_Professor(), 
                        null, 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player is not Player_Professor)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Professor);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });

                case Y_PowerUps.WeaponPistol:
                    return new PickUp(type, location, width, height, _gunScale, Manager_Sprites.Weapon_Pistol, lastOwner, true,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_Basic))
                                return false;

                            return switchGun(new Gun_Basic(player), player, self);
                        });
                case Y_PowerUps.WeaponEnemySlowPistol:
                    return new PickUp(type, location, width, height, _gunScale, Manager_Sprites.Weapon_Pistol, lastOwner, true,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_BasicEnemy))
                                return false;

                            return switchGun(new Gun_BasicEnemy(player), player, self);
                        });
                case Y_PowerUps.WeaponWide:
                    return new PickUp(type, location, width, height, _gunScale, Manager_Sprites.Weapon_Pistol, lastOwner, true,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_Wide))
                                return false;

                            return switchGun(new Gun_Wide(player), player, self);
                        });
                case Y_PowerUps.WeaponShotgun:
                    return new PickUp(type, location, width, height, _gunScale, Manager_Sprites.Weapon_Shotgun, lastOwner, true,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_ShotGun))
                                return false;

                            return switchGun(new Gun_ShotGun(player), player, self);
                        });
                case Y_PowerUps.WeaponFunky:
                    return new PickUp(type, location, width, height, _gunScale, Manager_Sprites.Weapon_RedGun, lastOwner, true,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_Funky))
                                return false;

                            return switchGun(new Gun_Funky(player), player, self);
                        });
                case Y_PowerUps.WeaponPinkHammer:
                    return new PickUp(type, location, width, height, _gunScale, Manager_Sprites.Weapon_Hammer, lastOwner, true, Color.LightPink,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Weapon_PinkHammer))
                                return false;

                            return switchGun(new Weapon_PinkHammer(player), player, self);
                        });
                case Y_PowerUps.Life:
                    return new PickUp(type, location, width, height, 1.5f, 
                        Manager_Sprites.NewAnimatedSprite_SpinningHeart(),
                        null, 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player.WhatAreYou() != X_LevelElements.Victim)
                                return false;
                            if (player.LifePoints >= player.LifePointsMax)
                                return false;

                            player.Heal();
                            player.Room.PickUps.Remove(self);
                            Manager_Sound.Sound_CashIn.Play();
                            return true;
                        });
                case Y_PowerUps.LevelUpNerd:
                    return new PickUp(type, location, width, height, 1.5f, 
                        Manager_Sprites.NewAnimatedSprite_LevelUp_DarkFlash(),
                        Manager_Sprites.NewAnimatedSprite_NerdyGirl(),
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player.WhatAreYou() != X_LevelElements.Victim)
                                return false;
                            if (!(player is Player_NerdyGirl))
                                return false;
                            if (player.ElementLevel > 3)
                                return false;

                            player.Room.PickUps.Remove(self);
                            Manager_Sound.Sound_CashIn.Play();
                            player.ElementLevel = player.ElementLevel + 1;
                            return true;
                        });
                case Y_PowerUps.LevelUpMailman:
                    return new PickUp(type, location, width, height, 1.5f, 
                        Manager_Sprites.NewAnimatedSprite_LevelUp_DarkFlash(),
                        Manager_Sprites.NewAnimatedSprite_Mailman(), 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player.WhatAreYou() != X_LevelElements.Victim)
                                return false;
                            if (!(player is Player_Mailman))
                                return false;
                            if (player.ElementLevel > 3)
                                return false;

                            player.Room.PickUps.Remove(self);
                            Manager_Sound.Sound_CashIn.Play();
                            player.ElementLevel = player.ElementLevel + 1;
                            return true;
                        });
                case Y_PowerUps.LevelUpProfessor:
                    return new PickUp(type, location, width, height, 1.5f, 
                        Manager_Sprites.NewAnimatedSprite_LevelUp_DarkFlash(),
                        Manager_Sprites.NewAnimatedSprite_Ninja(), 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (player.WhatAreYou() != X_LevelElements.Victim)
                                return false;
                            if (!(player is Player_Ninja))
                                return false;
                            if (player.ElementLevel > 3)
                                return false;

                            player.Room.PickUps.Remove(self);
                            Manager_Sound.Sound_CashIn.Play();
                            player.ElementLevel = player.ElementLevel + 1;
                            return true;
                        });
                case Y_PowerUps.Gravestone:
                    return new PickUp(type, location, width, height, 1.5f, Manager_Sprites.Gravestone, lastOwner, false,
                        (player, self) =>
                        {
                            // un-pick-up-able
                            return false;
                        });
                default: // case Y_PowerUps.Revive:
                    return new PickUp(type, location, width, height, 1.5f, 
                        Manager_Sprites.NewAnimatedSprite_SpinningPlus(),
                        null, 
                        lastOwner,
                        false,
                        (player, self) =>
                        {
                            if (!(!player.IsAlive() && player is not Player_Ghost))
                                return false;


                            player.Room.PickUps.Remove(self);
                            player.Revive();
                            Manager_Sound.Sound_CashIn.Play();
                            return true;
                        });
            }
        }

        public void DrawSpinningTexture(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // We only got a texture, so let's make our own "highly advanced" rotating animation
            float spin = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
            int spinWidth = (int)Math.Min(Rect.Width, Math.Abs(spin) * Rect.Width * 1.25);

            // And render a "shadow" to make it more visible
            spriteBatch.Draw(
                    texture: _texture,
                    destinationRectangle: new Rectangle(Rect.X + (Rect.Width - spinWidth) / 2 + 1, Rect.Y + 1, spinWidth, Rect.Height),
                    sourceRectangle: null,
                    color: Color.Black,
                    rotation: 0,
                    origin: Vector2.Zero,
                    effects: spin >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    layerDepth: 0);

            // Now draw the actual sprite
            spriteBatch.Draw(
                    texture: _texture,
                    destinationRectangle: new Rectangle(Rect.X + (Rect.Width - spinWidth) / 2, Rect.Y, spinWidth, Rect.Height),
                    sourceRectangle: null,
                    color: Color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    effects: spin >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    layerDepth: 0);
        }

        public void DrawFloatingTexture(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            int floatyOffset = 0;
            if (_floaty)
            {
                // Spinning looks meh, so instead float up and down a bit
                floatyOffset = (int)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 4) * Y_Level.InGameTileSize * 0.25f);
            }

            // And render a "shadow" to make it more visible
            spriteBatch.Draw(
                    texture: _texture,
                    destinationRectangle: new Rectangle(Rect.X + 2, Rect.Y + floatyOffset + 2, Rect.Width, Rect.Height),
                    sourceRectangle: null,
                    color: Color.Black,
                    rotation: 0,
                    origin: Vector2.Zero,
                    effects: SpriteEffects.None,
                    layerDepth: 0);

            // Now draw the actual sprite
            spriteBatch.Draw(
                    texture: _texture,
                    destinationRectangle: new Rectangle(Rect.X, Rect.Y + floatyOffset, Rect.Width, Rect.Height),
                    sourceRectangle: null,
                    color: Color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // add some culling to the power ups
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty) return;

            if (_sprite != null)
            { // We have an AnimatedSprite, yay!
                spriteBatch.Draw(
                        _sprite.Texture, Rect.Location.ToVector2(),
                        _sprite.SourceRectangle,
                        Color.White, 0, Vector2.Zero, _spriteDrawScale, SpriteEffects.None, 0);

                if (_carriedSprite != null)
                {
                    var target = _carriedSprite.AnimationSourceRects[AnimationState.WalkRight][0];
                    var scale = (float)Rect.Height / (float)target.Height;
                    scale *= 0.5f;
                    var offset = new Vector2(
                        0,
                        Rect.Height - target.Height * scale
                    );

                    spriteBatch.Draw(
                            _carriedSprite.Texture, Rect.Location.ToVector2() + offset,
                            target,
                            Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                }
            }
            else
            { // We only got a texture, well, let's do something fun with it at least
                DrawFloatingTexture(gameTime, globalOffset, spriteBatch);
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 4, Color.Purple, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            // no need to update stuff that isn't inside the camera view either
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty) return;

            if (_sprite != null)
            {
                _sprite.Update(gameTime, AnimationState.Idle);
            }

            if (_lastOwner != null)
            {
                if (!_lastOwner.Rect.Intersects(Rect))
                {
                    _lastOwner = null;
                }
            }
        }

        public void MoveBy(Point offset)
        {
            Rect = new Rectangle(Rect.X + offset.X, Rect.Y + offset.Y, Rect.Width, Rect.Height);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.PowerUp;
        }
    }
}
