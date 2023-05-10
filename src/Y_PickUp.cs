using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace YGR
{
    public enum Y_PowerUps
    {
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
    }

    public class PickUp : IGameElement
    {
        public float LocalScale { get; set; }

        public Rectangle Rect { get; set; }

        public bool Active { get; set; }

        public Y_PowerUps Type { get; }

        public Func<IPlayer, PickUp, bool> Action { get; }
        public int ElementLevel { get { return 1; } set { } }

        private AnimatedSprite _sprite;
        private float _spriteScale;

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

        public PickUp(Y_PowerUps type, Point location, int width, int height, float scale, AnimatedSprite sprite, IVictim lastOwner, Func<IPlayer, PickUp, bool> action)
        {
            LocalScale = scale * Y_Level.GlobalScale;
            Type = type;
            int heightNew = (int)(height * scale);
            int widthNew = (int)((heightNew * sprite.SpriteDimension.X / sprite.SpriteDimension.Y));
            //Rect = new Rectangle(
            //    (int)(location.X * scale - (widthNew - width) / 2.0), 
            //    (int)(location.Y * scale - (heightNew - height) - 10), 
            //    (int)(scale * widthNew), 
            //    (int)(scale * heightNew));

            Rect = new Rectangle(location.X - widthNew/2, location.Y-heightNew/2, widthNew, heightNew);
            Action = action;
            Active = true;
            _sprite = sprite;
            _spriteScale = LocalScale * Util.GetSpriteScale(Rect, _sprite.SpriteDimension);

            _lastOwner = lastOwner;
        }

        public PickUp(Y_PowerUps type, Point location, int width, int height, float scale, Texture2D texture, IVictim lastOwner, Func<IPlayer, PickUp, bool> action)
        {
            LocalScale = scale*Y_Level.GlobalScale;
            Type = type;
            int heightNew = (int)(height * scale);
            int widthNew = (int)((heightNew * texture.Width / texture.Height));
            //Rect = new Rectangle(
            //    (int)(location.X * scale - (widthNew - width) / 2.0),
            //    (int)(location.Y * scale - (heightNew - height) - 10),
            //    (int)(scale * widthNew),
            //    (int)(scale * heightNew));

            Rect = new Rectangle(location.X-widthNew/2, location.Y-heightNew/2, widthNew, heightNew);
            Action = action;
            Active = true;
            _texture = texture;
            _spriteScale = LocalScale * Util.GetSpriteScale(Rect, texture.Bounds.Size.ToVector2());

            _lastOwner = lastOwner;
        }

        public static PickUp Factory(Y_PowerUps type, Point location, int width, int height, float scale, IVictim lastOwner = null)
        {
            switch (type)
            {
                case Y_PowerUps.ChooserNerd:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.NewAnimatedSprite_NerdyGirl(), lastOwner,
                        (player, self) =>
                        {
                            if (player.Type != PlayerType.Nerd)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Nerd);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserMailman:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.NewAnimatedSprite_Mailman(), lastOwner,
                        (player, self) =>
                        {
                            if (player.Type != PlayerType.Mailman)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Mailman);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserNinja:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.NewAnimatedSprite_Ninja(), lastOwner,
                        (player, self) =>
                        {
                            if (player.Type != PlayerType.Ninja)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Ninja);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });

                case Y_PowerUps.WeaponPistol:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.Weapon_Pistol, lastOwner,
                        (player, self) => {
                            if (player.Gun.GetType() == typeof(Gun_Basic))
                                return false;

                            return switchGun(new Gun_Basic(player), player, self);
                        });
                case Y_PowerUps.WeaponEnemySlowPistol:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.Weapon_Pistol, lastOwner,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_BasicEnemy))
                                return false;

                            return switchGun(new Gun_BasicEnemy(player), player, self);
                        });
                case Y_PowerUps.WeaponWide:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.Weapon_Pistol, lastOwner,
                        (player, self) => {
                            if (player.Gun.GetType() == typeof(Gun_Wide))
                                return false;

                            return switchGun(new Gun_Wide(player), player, self);
                        });
                case Y_PowerUps.WeaponShotgun:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.Weapon_Shotgun, lastOwner,
                        (player, self) => {
                            if (player.Gun.GetType() == typeof(Gun_ShotGun))
                                return false;

                            return switchGun(new Gun_ShotGun(player), player, self);
                        });
                case Y_PowerUps.WeaponFunky:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.Weapon_RedGun, lastOwner,
                        (player, self) =>
                        {
                            if (player.Gun.GetType() == typeof(Gun_Funky))
                                return false;

                            return switchGun(new Gun_Funky(player), player, self);
                        });
                case Y_PowerUps.Life:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.NewAnimatedSprite_SpinningHeart(), lastOwner,
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
                default: // case Y_PowerUps.Revive:
                    return new PickUp(type, location, width, height, scale * 1.0f, Manager_Sprites.NewAnimatedSprite_SpinningPlus(), lastOwner,
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

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            if (_sprite != null)
            { // We have an AnimatedSprite, yay!
                spriteBatch.Draw(
                        _sprite.Texture, Rect.Location.ToVector2(),
                        _sprite.SourceRectangle,
                        Color.White, 0, Vector2.Zero, _spriteScale, SpriteEffects.None, 0);
            }
            else
            {
                // We only got a texture, so let's make our own "highly advanced" rotating animation
                float spin = (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds);
                int spinWidth = (int)Math.Min(Rect.Width, Math.Abs(spin) * Rect.Width * 1.25);
                spriteBatch.Draw(
                        texture: _texture,
                        destinationRectangle: new Rectangle(Rect.X + (Rect.Width - spinWidth) / 2, Rect.Y, spinWidth, Rect.Height),
                        sourceRectangle: null,
                        color: Color.White,
                        rotation: 0,
                        origin: Vector2.Zero,
                        effects: spin >= 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                        layerDepth: 0);
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 4, Color.Purple, spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            if (_sprite != null)
            {
                _sprite.Update(gameTime, AnimationState.Idle);
            }

            if(_lastOwner != null)
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
