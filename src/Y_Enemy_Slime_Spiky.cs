using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Slime_Spiky : Enemy_Basic
    {

        private Y_PowerUps _carriedPowerUp;
        private AnimatedSprite _carriedPowerUpSprite;
        private AnimatedSprite _carriedImage;

        protected static List<Color> SlimeyColors = new List<Color> {
            new Color(252, 45, 218),
            new Color(142, 176, 0),
            new Color(144, 252, 127),
            new Color(107, 253, 22),
            new Color(164, 72, 179),
            new Color(103, 28, 166),
            new Color(121, 238, 96),
            new Color(96, 238, 168),
        };

        public Enemy_Slime_Spiky(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        ) : base(position, sprite, level)
            {
            LifePointsMax = 35;
            LifePoints = LifePointsMax;
            fleeingHPTreshold = LifePointsMax / 2;

            Name = "Slime Spiky";
            Gun = new Gun_ShotGunEnemy(this);

            Color = SlimeyColors[Util.random.Next(SlimeyColors.Count)]; // Slimey green, picked from the colored png files
            _currentColor = Color;
            _hitColor = Color.DarkRed;

            var _mass = 2.0f;
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 130;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);

            // Offset the enitity to center it on the spawner tile
            _position = position - new Vector2(width / 2, height / 2);
            _rect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero;
        }

        public void SetPowerUp(Y_PowerUps carriedPowerUp)
        {
            _carriedPowerUp = carriedPowerUp;
            if(_carriedPowerUp == Y_PowerUps.LevelUpProfessor)
            {
                _carriedPowerUpSprite = Manager_Sprites.NewAnimatedSprite_LevelUp_LightFlash_NoShade();
                _carriedImage = Manager_Sprites.NewAnimatedSprite_Ninja();
            }
            else if (_carriedPowerUp == Y_PowerUps.LevelUpNerd)
            {
                _carriedPowerUpSprite = Manager_Sprites.NewAnimatedSprite_LevelUp_LightFlash_NoShade();
                _carriedImage = Manager_Sprites.NewAnimatedSprite_NerdyGirl();
            }
            else if (_carriedPowerUp == Y_PowerUps.LevelUpMailman)
            {
                _carriedPowerUpSprite = Manager_Sprites.NewAnimatedSprite_LevelUp_LightFlash_NoShade();
                _carriedImage = Manager_Sprites.NewAnimatedSprite_Mailman();
            }
            else
            {
                _carriedPowerUpSprite = Manager_Sprites.NewAnimatedSprite_LevelUp_Bullet_NoShade();
            }
        }

        public void ChangeColor(Color color)
        {
            Color = color;
            _currentColor = color;
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            // Culling
            if (Rectangle.Intersect(Camera.VisibleArea, Rect) == Rectangle.Empty)
            {
                return;
            }

            // then everything else on top of it
            base.Draw(gameTime, globalOffset, spriteBatch);

            if (_carriedPowerUpSprite != null)
            {
                var scale = (float)Rect.Height / (float)_carriedPowerUpSprite.SourceRectangle.Height;
                scale *= 0.4f;
                var offset = new Vector2(
                    scale * _carriedPowerUpSprite.SourceRectangle.Width * 2 / 3, 
                    scale * _carriedPowerUpSprite.SourceRectangle.Height / 3);

                spriteBatch.Draw(
                        _carriedPowerUpSprite.Texture, Rect.Center.ToVector2() - offset,
                        _carriedPowerUpSprite.SourceRectangle,
                        Color.White*0.3f, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
            if(_carriedImage != null)
            {
                var target = _carriedImage.AnimationSourceRects[AnimationState.WalkRight][0];
                var scale = (float)Rect.Height / (float)target.Height;
                scale *= 0.3f;
                var offset = new Vector2(
                    Rect.Width * 0.4f,
                    0.8f * Rect.Height - target.Height*scale
                );

                spriteBatch.Draw(
                        _carriedImage.Texture, Rect.Location.ToVector2() + offset,
                        target,
                        Color.White * 0.4f, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (_carriedPowerUpSprite != null)
            {
                _carriedPowerUpSprite.Update(gameTime, AnimationState.Idle);
            }
            base.Update(gameTime);
        }

        public override void DropSomethingJuicyMaybe()
        {
            // drop a grave stone with absolute certainty
            if (Room.WhatAreYou() == X_LevelElements.Room)
            {
                // drop something jucy in any case
                var room = (Y_CMRoom)Room;
                var p = new Point(Rect.Location.X + Rect.Width / 2, Rect.Location.Y + Rect.Height / 2);
                room.PickUps.Add(PickUp.Factory(_carriedPowerUp, p, Y_Level.TextureTileSize, Y_Level.TextureTileSize, Y_Level.GlobalScale));
            }
        }
    }
}
