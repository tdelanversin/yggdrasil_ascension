using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public class Player_Ghost : Player_Basic
    {
        public Player_Ghost(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            ControlLayout controlLayout = ControlLayout.ControllerOnly,
            float scale = 1.0f
            ) : base(playerIndex, initialPosition, level, null, PlayerType.Ghost, controlLayout)
        {
            Name = "Ghost";
            Ability = new Ability_Blank(this);
            DeadAbility = new Ability_Blank(this);
            VelocityMax = IPlayer.PlayerBaseVelocity * 2; // Compensate for not being able to dash
            IsActive = false;
            LifePoints = LifePointsMax = 0;
            Gun = new Gun_Ghost(this);
        }

        public override X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Ghost;
        }

        protected override void DrawOverheadString(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            string str = "P" + (int)PlayerIndex + ": " + (IsActive ? "Entered" : "Move to register");
            Vector2 str_size = Fonts.Small.MeasureString(str);
            spriteBatch.DrawString(Fonts.Small, str, new Vector2(_rect.Location.X + _rect.Width / 2 - str_size.X / 2, _rect.Location.Y - str_size.Y + GhostOffset.Y), Color.Wheat);
        }

        public override void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            DrawGhost(gameTime, globalOffset, spriteBatch);
            // DrawOverheadString(gameTime, globalOffset, spriteBatch);
        }
    }
}
