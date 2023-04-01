using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace YGR
{

    /// <summary>
    /// Class <c>Factory_Projectiles</c> contains methods to create instances of <c>Y_Projectiles</c>.
    /// </summary>
    public static class Manager_Projectile
    {
        public static Dictionary<string, Texture2D> projectile_textures;
        private static List<IProjectile> _projectiles = new List<IProjectile>();
        private static bool _initialized = false;
        private static double _maxlifetime = 1500.0;

        public static void Initialize(ContentManager content)
        {
            projectile_textures = new Dictionary<string, Texture2D>()
            {
                { "default_projectile", content.Load<Texture2D>("projectile") },
                { "smaller_projectile", content.Load<Texture2D>("smaller_projectile")}
            };

            _initialized = true;
        }

        private static void check()
        {
            if (!_initialized) Logger.Error("Factory_Projectiles not initialized: call Factory_Projectiles.Initialize(ContentManager) somewhere!");
        }

        public static void AddProjectile(IProjectile projectile)
        {
            check();
            _projectiles.Add(projectile);

            Logger.Debug("Added projectile " + projectile.Name);
        }

        public static void Update(GameTime gameTime)
        {
            check();
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (IProjectile projectile in _projectiles)
            {
                projectile.Update(gameTime);
            }

            // TODO: Add projectile collision detection

            _projectiles.RemoveAll(projectile => (projectile.TimeCreated + _maxlifetime < gameTime.TotalGameTime.TotalMilliseconds) || projectile.DeleteNext);
        }

        public static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            check();
            foreach (IProjectile projectile in _projectiles)
            {
                projectile.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public static void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            check();
            foreach (IProjectile projectile in _projectiles)
            {
                projectile.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}