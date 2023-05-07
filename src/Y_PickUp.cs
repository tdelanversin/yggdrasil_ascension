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

        // CharacterChoosers
        ChooserNerd,
        ChooserNinja,
        ChooserMailman,
    }

    public class PickUp : IGameElement
    {
        public float Scale { get; set; }

        public Rectangle Rect { get; set; }

        public bool Active { get; set; }

        public Y_PowerUps Type { get; }

        public Func<IPlayer, bool> Action { get; }

        private AnimatedSprite _sprite;
        private float _spriteScale;

        private Texture2D _texture;

        public PickUp(Y_PowerUps type, Point location, int width, int height, float scale, AnimatedSprite sprite, Func<IPlayer, bool> action)
        {
            Scale = scale;
            Type = type;
            float heightNew = (int)(height * 1.75);
            float widthNew = (int)((heightNew * sprite.SpriteDimension.X / sprite.SpriteDimension.Y));
            Rect = new Rectangle((int)(location.X * scale - (widthNew - width) / 2.0), (int)(location.Y * scale - (heightNew - height) - 10), (int)(scale * widthNew), (int)(scale * heightNew));
            Action = action;
            Active = true;
            _sprite = sprite;
            _spriteScale = Scale * Util.GetSpriteScale(Rect, _sprite.SpriteDimension);
        }

        public PickUp(Y_PowerUps type, Point location, int width, int height, float scale, Texture2D texture, Func<IPlayer, bool> action)
        {
            Scale = scale;
            Type = type;
            float heightNew = (int)(height * 1.75);
            float widthNew = (int)((heightNew * texture.Width / texture.Height));
            Rect = new Rectangle((int)(location.X * scale - (widthNew - width) / 2.0), (int)(location.Y * scale - (heightNew - height) - 10), (int)(scale * widthNew), (int)(scale * heightNew));
            Action = action;
            Active = true;
            _texture = texture;
            _spriteScale = Scale * Util.GetSpriteScale(Rect, texture.Bounds.Size.ToVector2());
        }

        public static PickUp Factory(Y_PowerUps type, Point location, int width, int height, float scale)
        {
            switch (type)
            {
                case Y_PowerUps.ChooserNerd:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_NerdyGirl(),
                        (player) =>
                        {
                            if (player.Type != PlayerType.Nerd)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Nerd);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserMailman:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_Mailman(),
                        (player) =>
                        {
                            if (player.Type != PlayerType.Mailman)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Mailman);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.ChooserNinja:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_Ninja(),
                        (player) =>
                        {
                            if (player.Type != PlayerType.Ninja)
                            {
                                Manager_Players.SetPlayerType(player.PlayerIndex, PlayerType.Ninja);
                                Manager_Sound.Sound_GunCocking.Play();
                            }
                            return false;
                        });
                case Y_PowerUps.WeaponPistol:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.Weapon_Pistol,
                        (player) =>
                        {
                            if (player is Player_Ghost)
                                return false;

                            if (player.Gun.GetType() == typeof(Gun_Basic))
                                return false;

                            player.Gun = new Gun_Basic();
                            Manager_Sound.Sound_GunCocking.Play();
                            return false;
                        });
                case Y_PowerUps.WeaponShotgun:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.Weapon_Shotgun,
                        (player) =>
                        {
                            if (player is Player_Ghost)
                                return false;

                            if (player.Gun.GetType() == typeof(Gun_ShotGun))
                                return false;

                            player.Gun = new Gun_ShotGun();
                            Manager_Sound.Sound_GunCocking.Play();
                            return false;
                        });
                case Y_PowerUps.WeaponFunky:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.Weapon_RedGun,
                        (player) =>
                        {
                            if (player is Player_Ghost)
                                return false;

                            if (player.Gun.GetType() == typeof(Gun_Funky))
                                return false;

                            player.Gun = new Gun_Funky();
                            Manager_Sound.Sound_GunCocking.Play();
                            return false;
                        });
                case Y_PowerUps.Life:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_SpinningHeart(),
                        (player) =>
                        {
                            var alives = Manager_Players.Players.Where(x => x.WhatAreYou() == X_LevelElements.Victim).ToArray();
                            bool ret = false;
                            foreach (SimplePlayer p in alives)
                            {
                                if (p.LifePoints < p.LifePointsMax)
                                {
                                    p.Heal();
                                    ret = true; // consume powerup
                                }
                            }
                            return ret;
                        });
                default: // case Y_PowerUps.Revive:
                    return new PickUp(type, location, width, height, scale, Manager_Sprites.NewAnimatedSprite_SpinningPlus(),
                        (player) =>
                        {
                            var ghosts = Manager_Players.Players.Where(x => !x.IsAlive() && x is not Player_Ghost).ToArray();
                            bool ret = false;
                            foreach (var ghost in ghosts)
                            {
                                ghost.Revive();
                                ret = true;
                            }
                            return ret;
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
