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
        public static int PlayerBaseHealth = 15;
        public static int PlayerBaseHeight = 2 * Y_Level.InGameTileSize / Y_Level.TextureTileSize;
        public static float PlayerBaseVelocity = 0.35f;
        public static float PlayerBaseMass = 0.35f;

        public ControlLayout ControlLayout { get; set; }
        public IShooter Gun { get; set; }
        public IAbility Ability { get; set; }
        public PlayerIndex PlayerIndex { get; }
        public bool IsActive { get; }
        public bool IsInvincible { get; }
        public bool IsDashing { get; }
        public PlayerType Type { get; }
        public Statistics Stats { get; set; }

        public AnimatedSprite GetSprite();
        public bool IsAlive();
        public void Heal();
        public void Heal(int healAmount);
        public void Revive();
        public void Revive(int healAmount);
        public void Godmode();
    }
}
