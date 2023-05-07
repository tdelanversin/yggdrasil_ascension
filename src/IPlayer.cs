using Microsoft.Xna.Framework;

namespace YGR
{
    public interface IPlayer : IVictim
    {
        public ControlLayout ControlLayout { get; set; }
        public IShooter Gun { get; set; }
        public PlayerIndex PlayerIndex { get; }
        public bool IsActive { get; }

        public bool IsAlive();
        public void Revive();
    }
}
