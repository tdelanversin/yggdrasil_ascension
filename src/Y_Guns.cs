using Microsoft.Xna.Framework;
#nullable enable

namespace YGR
{
    public class Y_StarterGun: IShooter
    {
        double lastShot = 0.0f;
        int shotDelay = 1000;

        public Y_StarterGun() {}

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, IWalkable room) {
            if (lastShot == 0.0f || gameTime.TotalGameTime.TotalMilliseconds - lastShot > shotDelay) {
                lastShot = gameTime.TotalGameTime.TotalMilliseconds;

                var projectile = new X_StarterProjectile(
                    origin,
                    direction,
                    gameTime.TotalGameTime.TotalMilliseconds,
                    room
                );
                ProjectileManager.AddProjectile(projectile);
            }
        }
    }

    public class Y_SimpleEnemyGun: IShooter
    {
        double lastShot = 0.0f;
        int shotDelay = 1000;

        public Y_SimpleEnemyGun() {}

        public void Shoot(GameTime gameTime, Vector2 origin, Vector2 direction, IWalkable room) {
            if (lastShot == 0.0f || gameTime.TotalGameTime.TotalMilliseconds - lastShot > shotDelay) {
                lastShot = gameTime.TotalGameTime.TotalMilliseconds;

                var projectile = new X_StarterProjectile(
                    origin,
                    direction,
                    gameTime.TotalGameTime.TotalMilliseconds,
                    room
                );
                ProjectileManager.AddProjectile(projectile);
            }
        }
    }
}