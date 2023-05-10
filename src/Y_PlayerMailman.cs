using Microsoft.Xna.Framework;

namespace YGR
{
    public class Player_Mailman : SimplePlayer
    {
        public Player_Mailman(
            PlayerIndex playerIndex,
            Vector2 initialPosition,
            AnimatedSprite sprite,
            Y_Level level,
            IShooter gun,
            IAbility ability,
            PlayerType type,
            ControlLayout controlLayout = ControlLayout.ControllerOnly
            ) : base(playerIndex, initialPosition, sprite, level, gun, ability, type, controlLayout)
        {
            CharacterSprite = Manager_Sprites.NewAnimatedSprite_Mailman();
            Ability = new Ability_Shield();
            Type = PlayerType.Mailman;
            Name = "Mailman";

            // Big guy can take a lot
            LifePointsMax = IPlayer.PlayerBaseHealth * 2;
            LifePoints = LifePointsMax;

            // Carrying all those packages you ordered is not easy...
            VelocityMax = IPlayer.PlayerBaseVelocity * 0.6f;

            // ...but we can still do a decent sprint if needed
            _dashSpeed /= 0.6f;

            // Big guy. Tall. Heavy. What else it there to say.
            SetupPlayerRect(height: IPlayer.PlayerBaseHeight * 1.5f);
            _mass = IPlayer.PlayerBaseMass * 2.5f;
            Collision = new X_CollisionModel_Victim(_mass, _cr);
        }
    }
}
