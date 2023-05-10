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
        public ControlLayout ControlLayout { get; set; }
        public IShooter Gun { get; set; }
        public IAbility Ability { get; set; }
        public PlayerIndex PlayerIndex { get; }
        public bool IsActive { get; }
        public bool IsInvincible { get; }
        public bool IsDashing { get; }
        public PlayerType Type { get; }
        public Statistics Stats { get; set; }
        public void TeleportTo(Point target);

        public AnimatedSprite GetSprite();
        public bool IsAlive();
        public void Heal();
        public void Heal(int healAmount);
        public void Revive();
        public void Revive(int healAmount);
        public void Godmode();
    }
}
