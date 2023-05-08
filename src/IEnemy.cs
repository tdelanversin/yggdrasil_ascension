using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public enum EnemyState
    {
        Idle,
        Wander,
        Chase,
        Flee,
        Inactive,
    }

    public interface IEnemy : IVictim
    {
        public Vector2 FacingDirection { get; set; }
        public EnemyState State { get; set; }
        public IShooter Gun { get; set; }
        public void DropSomethingJuicyMaybe();
    }

    public interface IEnemyBoss : IEnemy { }
}
