using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    // Basic gun, does nothing special, shoots fast
    public class Ability_Confusion : IAbility
    {
        public string Name { get; protected set; }
        public Texture2D Sprite { get; protected set; }

        protected double NextShotCooldown = 0.0f;
        protected int ShotDelay = 240;

        public Ability_Confusion()
        {
            Name = "Confusion";
            Sprite = Manager_Sprites.Effect_Confusion;
        }

        public virtual bool Trigger(GameTime gameTime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (NextShotCooldown > 0.0f)
                return false;

            Manager_Sound.Sound_Confusion.Play(0.5f, 0, 0);

            NextShotCooldown = ShotDelay;

            var duration = 0;
            if (who.ElementLevel == 1) duration = 3000;
            else if (who.ElementLevel == 2) duration = 5000;
            else duration = 8000;
            Manager_Projectile.AddProjectile_Confusion(origin, direction, duration, level, who);
            return true;
        }

        public virtual void Update(GameTime gameTime)
        {
            NextShotCooldown = Math.Max(0, NextShotCooldown - gameTime.ElapsedGameTime.TotalMilliseconds);
        }
    }

    // Ability for Ghosts. Does as much as absolutely nothing but make lives easier for stupid programmers
    public class Ability_Ghost : IAbility
    {
        public string Name { get { return ""; } }

        public Texture2D Sprite { get { return Manager_Sprites.White; } }

        public bool Trigger(GameTime gametime, Vector2 origin, Vector2 direction, Y_Level level, IGameElement who) { return false; }

        public void Update(GameTime gameTime) { }
    }
}
