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

    public enum BossAttack
    {
        Scatter,
        Precise,
        AOE,
        AvoidPattern1,
        AvoidPattern2,
        Wait,
        Spawn,
        Hide,
    }

    public interface IEnemy : IVictim
    {
        public Vector2 FacingDirection { get; set; }
        public EnemyState State { get; set; }
        public IShooter Gun { get; set; }
        public void DropSomethingJuicyMaybe();
        public void Kill();
        public void WakeUp();
        public void ChangePosition(Vector2 newPosition);
    }

    public interface IEnemyBoss : IEnemy { 
        public BossAttack Attack { get; set; }
                    
        public void DrawBossHealthBar(GameTime gameTime, SpriteBatch spriteBatch);
    }
}
