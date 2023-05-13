using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public class Player_Mailman : Player_Basic
    {
        public Player_Mailman(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            PlayerType type,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
            ) : base(playerIndex, initialPosition, level, gun, type, controlLayout)
        {
            CharacterSprite = Manager_Sprites.NewAnimatedSprite_Mailman();
            Ability = new Ability_Shield(this);
            Type = PlayerType.Mailman;
            Name = "Mailman";

            if (gun != null)
                Gun = gun;
            else
                Gun = Util.getRandomGun(this);

            // Big guy can take a lot
            LifePointsMax = IPlayer.PlayerBaseHealth * 2;
            LifePoints = LifePointsMax;

            // Carrying all those packages you ordered is not easy...
            VelocityMax = IPlayer.PlayerBaseVelocity * 0.8f;

            // ...but we can still do a decent sprint if needed
            _dashSpeed /= 0.8f;

            // Big guy. Tall. Heavy. What else it there to say.
            SetupPlayerRect(height: IPlayer.PlayerBaseHeight * 1.25f);
            _mass = IPlayer.PlayerBaseMass * 2f;
            _impactDeceleration = IPlayer.PlayerBaseDeceleration * 1.25f;
            Collision = new X_CollisionModel_Victim(_mass, _cr);
        }

        public override void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(_rect.X, _rect.Y, _rect.Width, _rect.Height, 1, Color.OrangeRed, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
            ((Ability_Shield)Ability).DrawOutline(gameTime, globalOffset, spriteBatch);
        }
    }
}
