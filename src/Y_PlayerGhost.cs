using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public class Player_Ghost : SimplePlayer
    {
        public Player_Ghost(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly,
            float scale = 1.0f
            ) : base(playerIndex, initialPosition, Manager_Sprites.NewAnimatedSprite_Ghost(), level, new Gun_Ghost(), controlLayout)
        {
            VelocityMax *= 1.5f;
            IsActive = false;
        }

        public override X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Ghost;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateRoom(gameTime);

            Vector2 input = Vector2.Zero;
            HandleGamepadInput(gameTime, ref input);
            HandleMouseKeyboardInput(gameTime, ref input);

            if (input != Vector2.Zero || _isAiming) IsActive = true;

            GhostSprite.Update(gameTime, input);

            UpdateVelocity(input, gameTime);
            UpdateDash(gameTime);
            UpdateCollision(gameTime);

            UpdateColor(gameTime);
        }

        // Render ghosty 👻
        protected override void DrawGhost(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: GhostSprite.Texture,
                position: _rect.Location.ToVector2() + GhostOffset,
                sourceRectangle: GhostSprite.SourceRectangle,
                color: _ghostColor * (IsActive ? 1f : 0.5f),
                rotation: 0,
                origin: Vector2.Zero,
                scale: GhostScale,
                effects: SpriteEffects.None,
                layerDepth: 0);
        }

        protected override void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            string str = "P" + (int)PlayerIndex + ": " + (IsActive ? "Entered" : "Move to register");
            Vector2 str_size = Fonts.Normal.MeasureString(str);
            spriteBatch.DrawString(Fonts.Normal, str, new Vector2(_rect.Location.X + _rect.Width / 2 - str_size.X / 2, _rect.Location.Y - str_size.Y + GhostOffset.Y), Color.Wheat);
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            DrawGhost(gameTime, globalOffset, spriteBatch);
            // DrawOverheadString(gameTime, globalOffset, spriteBatch);
        }
    }
}
