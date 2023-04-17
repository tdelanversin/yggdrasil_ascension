using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace YGR
{
    public interface IEnemy : IVictim
    {
        public Vector2 FacingDirection { get; set; }
        public Texture2D Sprite { get; set; }

        public IShooter Gun { get; set; }
        public string Name { get; set; }
    }
}
