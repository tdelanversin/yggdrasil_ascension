using Microsoft.Xna.Framework;

namespace YGR
{

    public enum PlayerType
    {
        Nerd = 0,
        Ninja,
        Mailman,
        Professor,
        Random,
        Ghost
    };

    public interface IPlayer : IVictim
    {
        public static float PlayerBaseHealth = 15;
        public static int PlayerBaseHeight = 2 * Y_Level.InGameTileSize;
        public static float PlayerBaseVelocity = 0.35f;
        public static float PlayerBaseMass = 0.35f;
        public static float PlayerBaseAcceleration = 0.008f;
        public static float PlayerBaseDeceleration = 0.004f;
        public static float PlayerBaseImpactDeceleration = 0.01f;

        public ControlLayout ControlLayout { get; set; }
        public IShooter Gun { get; set; }
        public IAbility Ability { get; set; }
        public IAbility DeadAbility { get; set; }

        // need something to immobilze players during interuption sequeces (for example when introducing gigachad
        public bool Immobilized { get; set; }

        public PlayerIndex PlayerIndex { get; }
        public bool IsActive { get; }
        public bool IsInvincible { get; }
        public bool IsDashing { get; }
        public bool IsSpedUp { get; }
        public PlayerType Type { get; }
        public Statistics Stats { get; set; }
        public float VelocityMax { get; }
        public float VelocitySpeedUp { get; }

        public void TeleportTo(Point target);

        public bool LevelUp();
        public AnimatedSprite GetSprite();
        public bool IsAlive();
        public void Heal();
        public void Heal(float healAmount);
        public void Revive();
        public void Revive(float healAmount);
        public void Godmode();
        public void SetInvincible(bool invincible);
        public void SpeedUp(float factor, int duration);
    }
}
