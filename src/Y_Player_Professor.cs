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
            // TODO: No sprite yet :(
            // CharacterSprite = Manager_Sprites.NewAnimatedSprite_Professor();

            Ability = new Ability_Confusion();
            Type = PlayerType.Professor;
            Name = "Professor";

            // Leave him the hammer for now (inherited from Player_Basic)
            // if (gun != null)
            //     Gun = gun;
            // else
            //     Gun = Util.getRandomGun(this);

            SetupPlayerRect(IPlayer.PlayerBaseHeight);

            LifePointsMax = IPlayer.PlayerBaseHealth;
            LifePoints = LifePointsMax;

            // Professor is old and no longer as fast
            VelocityMax = IPlayer.PlayerBaseVelocity * 0.8f;
        }
    }
}
