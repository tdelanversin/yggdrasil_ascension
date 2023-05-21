using Microsoft.Xna.Framework;

namespace YGR
{
    public class Player_Ninja : Player_Basic
    {
        public Player_Ninja(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            Y_Level level,
            IShooter gun,
            PlayerType type,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
            ) : base(playerIndex, initialPosition, level, gun, type, controlLayout)
        {
            CharacterSprite = Manager_Sprites.NewAnimatedSprite_Ninja();

            Ability = new Ability_Invicible(this); // no ability for the ninja
            Type = PlayerType.Ninja;
            Name = "Ninja";

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
