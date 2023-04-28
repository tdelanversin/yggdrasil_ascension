using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace YGR
{
    public static class Manager_Enemies
    {
        public static Dictionary<string, Texture2D> enemy_textures;
        private static List<IEnemy> _enemies = new List<IEnemy>();
        private static bool _initialized = false;
        private static bool level_clear = false;

        public static void Initialize(ContentManager content)
        {
            enemy_textures = new Dictionary<string, Texture2D>()
            {
                { "default_enemy", content.Load<Texture2D>("SpritesCharacters/tester_60") },
                { "gigachad", content.Load<Texture2D>("SpritesCharacters/gigachad") },
            };

            _initialized = true;
        }
        private static void check()
        {
            if (!_initialized) Logger.Error("Manager_Enemies not initialized: call Manager_Enemies.Initialize(ContentManager) somewhere!");
        }

        public static void ClearEnemies()
        {
            _enemies.Clear();
        }

        public static void AddEnemy_SimpleEnemy(Vector2 position, Y_Level level, IList<IVictim> players)
        {
            check();
            _enemies.Add(new Enemy_Basic(position, level, players));
        }

        internal static void AddEnemy_Gigachad(Vector2 position, Y_Level level, IList<IVictim> players)
        {
            check();
            _enemies.Add(new Enemy_Gigachad(position, level, players));
        }

        public static ReadOnlyCollection<IEnemy> GetEnemies()
        {
            return _enemies.AsReadOnly();
        }

        public static void Update(GameTime gameTime)
        {
            check();
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
            if (!level_clear && _enemies.Count <= 0)
            {
                // Private variable to reset each time we start a new level
                level_clear = true;
                // TODO: add a wating time for the level clear sound
                //System.Threading.Thread.Sleep(1000);
                // Manager_Sound.Sound_LevelCleared.Play();

                // ---> For now, this is is done in the Y_CMRoom Update()
                // method, but later we might move game play state handling
                // somewhere else entirely
            }
        }

        public static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            check();
            foreach (IEnemy enemy in _enemies)
            {
                enemy.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public static void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            check();
            foreach (IEnemy enemy in _enemies)
            {
                enemy.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}