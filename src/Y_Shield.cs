using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using SkiaSharp;
using MonoGame.Extended;
using System.Reflection.Metadata;

namespace YGR
{
    public class Y_Shield : IGameElement
    {
        public Rectangle Rect { get; set; }
        public int ElementLevel { get { return 1; } set { } }
        public Player_Basic Owner { get; set; }

        AnimatedSprite _sprite;

        Point _center;
        float _a;
        float _b;
        float _scale;
        float _angle;

        public Y_Shield(Player_Basic owner, AnimatedSprite sprite)
        {
            Owner = owner;
            _sprite = sprite;

            _scale = 1.0f;
            _center = new Point(128, 48);
            _a = 1.0f;
            _b = 1.0f;
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Draw an indicator only if a) the player is actively aiming on the gamepad or b) is using mouse to aim
            if (
                Owner.CurrentAimInput == Player_Basic.InputType.Controller && Input.IsButtonDown(Owner.PlayerIndex, Keybinds.GamePadAbility) || 
                Owner.CurrentAimInput == Player_Basic.InputType.KeyboardMouse && Input.IsRightMousePressed()
            )
            {
                float angle = (float)(Math.Atan2(Owner.AimDirection.Y, Owner.AimDirection.X) + Math.PI / 2);
                spriteBatch.Draw(
                    _sprite.Texture, Owner.Rect.Center.ToVector2(),
                    _sprite.SourceRectangle,
                    Color.Blue, angle, _sprite.SpriteDimension * 0.5f, _scale, SpriteEffects.None, 0
                );
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime, AnimationState.Idle);
        }

        public X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Shield;
        }
    }
}
