using Microsoft.Xna.Framework;

namespace YGR
{
    public class Player_Professor : Player_Basic
    {
        public Player_Professor(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            PlayerType type,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
            ) : base(playerIndex, initialPosition, level, gun, type, controlLayout)
        {
            CharacterSprite = Manager_Sprites.NewAnimatedSprite_Professor();

            Ability = new Ability_Confusion();
            Type = PlayerType.Professor;
            Name = "Professor";

            if (gun != null)
                Gun = gun;
            else
                // Gun = Util.getRandomGun(this);
                Gun = new Gun_Book(this);

            SetupPlayerRect(IPlayer.PlayerBaseHeight);

            LifePointsMax = IPlayer.PlayerBaseHealth;
            LifePoints = LifePointsMax;
            SetupPlayerRect(height: IPlayer.PlayerBaseHeight * 1.25f);

            // Professor is old and no longer as fast
            VelocityMax = IPlayer.PlayerBaseVelocity * 0.8f;
        }
    }
}
