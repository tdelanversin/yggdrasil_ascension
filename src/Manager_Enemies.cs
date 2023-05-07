using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace YGR
{
    public static class Manager_Enemies
    {
        public enum EnemyType
        {
            SimpleEnemy = 0,
            SlimeEnemy,
            BossEnemy,
        };

        private static List<IEnemy> _enemies = new List<IEnemy>();

        public static void ClearEnemies()
        {
            _enemies.Clear();
        }

        public static void AddEnemy_SimpleEnemy(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Enemy_Basic(position, Manager_Sprites.NewAnimatedSprite_TestCharacter(), level));
        }

        public static void AddEnemy_Slime(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Enemy_Slime(position, Manager_Sprites.NewAnimatedSprite_EnemySlime(), level));
        }

        internal static void AddEnemy_Gigachad(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Enemy_Gigachad(position, Manager_Sprites.NewAnimatedSprite_Gigachad(), level));
        }

        public static ReadOnlyCollection<IEnemy> GetEnemies()
        {
            return _enemies.AsReadOnly();
        }

        public static void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (IEnemy enemy in _enemies)
            {
                enemy.Update(gameTime);
                if (enemy.LifePoints <= 0)
                {
                    Manager_Sound.Sound_EnemyDeath.Play(0.8f, -0.5f, 0);
                }
            }
            _enemies.RemoveAll(enemy => enemy.LifePoints <= 0);
        }

        public static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            var enemiesSorted = _enemies.OrderBy(t => t.Rect.Y + t.Rect.Height);
            foreach (IEnemy enemy in enemiesSorted)
            {
                enemy.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public static void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (IEnemy enemy in _enemies)
            {
                enemy.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}