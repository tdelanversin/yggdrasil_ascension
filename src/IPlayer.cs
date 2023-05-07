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
        public PlayerIndex PlayerIndex { get; }
        public bool IsActive { get; }
        public PlayerType Type { get; }

        public AnimatedSprite GetSprite();
        public bool IsAlive();
        public void Revive();
    }
}
