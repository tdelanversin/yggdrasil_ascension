using Microsoft.Xna.Framework;

namespace YGR
{
    public class Player_NerdyGirl : Player_Basic
    {
        public Player_NerdyGirl(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            PlayerType type,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
            ) : base(playerIndex, initialPosition, level, gun, type, controlLayout)
        {
            CharacterSprite = Manager_Sprites.NewAnimatedSprite_NerdyGirl();

            Ability = new Ability_Confusion();
            Type = PlayerType.Nerd;
            Name = "Nerdy Girl";

            if (gun != null)
                Gun = gun;
            else
                Gun = Util.getRandomGun(this);

            SetupPlayerRect(IPlayer.PlayerBaseHeight);

            LifePointsMax = IPlayer.PlayerBaseHealth;
            LifePoints = LifePointsMax;

            VelocityMax = IPlayer.PlayerBaseVelocity;
        }
    }
}
