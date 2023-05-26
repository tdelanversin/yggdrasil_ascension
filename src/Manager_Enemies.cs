using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
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
            SpikyEnemy,
            GigaChad
        };

        private static List<IEnemy> _enemies = new List<IEnemy>();
        private static List<IEnemy> _enemiesToAdd = new List<IEnemy>();
        //private static List<IEnemyBoss> _bosses = new List<IEnemyBoss>();

        //private static List<IEnemyBoss> _gigaChadMain = new List<IEnemyBoss>();
        //private static List<IEnemyBoss> _gigaChadComplement1 = new List<IEnemyBoss>();

        public static void ClearEnemies()
        {
            _enemies.Clear();
            //_bosses.Clear();
        }

        public static void ClearEnemies(IWalkable room)
        {
            foreach (var enemy in _enemies)
            {
                if (enemy.Room == room) enemy.Kill();
            }

            _enemies.RemoveAll(enemy => enemy.LifePoints <= 0);

            //foreach (var boss in _bosses)
            //{
            //    if (boss.Room == room) boss.Kill();
            //}

            //_bosses.RemoveAll(boss => boss.LifePoints <= 0);
        }

        public static void KillAllNormalEnemies()
        {
            _enemies.RemoveAll(e => e is not IEnemyBoss);
        }

        public static void AddEnemy_SimpleEnemy(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Enemy_Basic(position, Manager_Sprites.NewAnimatedSprite_TestCharacter(), level));
        }

        public static void AddEnemy_SlimeSpiky(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Enemy_Slime_Spiky(position, Manager_Sprites.NewAnimatedSprite_EnemySlimeSpiky(), level));
        }

        public static void AddEnemy_Slime(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Enemy_Slime(position, Manager_Sprites.NewAnimatedSprite_EnemySlime(), level));
        }

        internal static void AddEnemy_Gigachad(Vector2 position, Y_Level level)
        {
            IEnemy enemy = new Enemy_Gigachad(position, Manager_Sprites.NewAnimatedSprite_Gigachad(), level);
            _enemies.Add(enemy);
            //_bosses.Add((IEnemyBoss)enemy);
        }

        internal static void AddEnemy_Boss(Vector2 position, Y_Level level)
        {
            IEnemy enemy = new Enemy_Boss(position, Manager_Sprites.NewAnimatedSprite_EnemyBoss(), level);
            _enemies.Add(enemy);
            //_bosses.Add((IEnemyBoss)enemy);
        }

        internal static IEnemy MakeEnemy_BossMinion(Y_Level level, Color color)
        {
            var enemy = new Enemy_Slime(Vector2.Zero, Manager_Sprites.NewAnimatedSprite_EnemySlime(), level);
            enemy.ChangeColor(color);
            return enemy;
        }

        internal static IEnemy MakeEnemy_BossMinionSpiky(Y_Level level, Color color)
        {
            var enemy = new Enemy_Slime_Spiky(Vector2.Zero, Manager_Sprites.NewAnimatedSprite_EnemySlimeSpiky(), level);
            enemy.ChangeColor(color);
            return enemy;
        }

        internal static void AddEnemy_BossMinion(Vector2 position, IEnemy minion)
        {
            var random = new System.Random();
            position += new Vector2(random.Next(-150, 150), random.Next(-150, 150));
            minion.ChangePosition(position);
            minion.WakeUp();
            _enemiesToAdd.Add(minion);
        }

        /* Offer method to place gravestone. Players should be able to hit them
            for fun, plus we really don't need a Manager_Gravestones */
        public static void AddGraveStone(Vector2 position, Y_Level level)
        {
            _enemies.Add(new Gravestone(position, level));
        }

        public static int CountRegularEnemies()
        {
            return _enemies.Where(e => e is not IEnemyBoss).Count();
        }

        public static ReadOnlyCollection<IEnemy> GetEnemies()
        {
            return _enemies.AsReadOnly();
        }

        public static ReadOnlyCollection<Enemy_Boss> GetBosses()
        {
            var temp = _enemies.Where(x => x is Enemy_Boss).Cast<Enemy_Boss>().ToList();
            return temp.AsReadOnly();
        }

        public static ReadOnlyCollection<Enemy_Gigachad> GetGigaChads()
        {
            var temp = _enemies.Where(x => x is Enemy_Gigachad).Cast<Enemy_Gigachad>().ToList();
            return temp.AsReadOnly();
        }

        public static void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (IEnemy enemy in _enemies)
            {
                // no need to update stuff that isn't inside the camera view either
                if (Rectangle.Intersect(Camera.VisibleArea, enemy.Rect) == Rectangle.Empty) continue;

                enemy.Update(gameTime);
                if (enemy.LifePoints <= 0)
                {
                    var sound = new List<SoundEffect>() { Manager_Sound.Sound_EnemyDeath, Manager_Sound.Sound_Splash1, Manager_Sound.Sound_Splash2, Manager_Sound.Sound_Splash3, Manager_Sound.Sound_Splash4 };
                    sound.Shuffle(Util.random).First().Play(1.0f, 0f, 0f);
                }
            }

            // select all dying enemies
            var list = _enemies.Where(x => x.LifePoints <= 0).ToList();
            // drop, or maybe not, something jucy
            foreach (var enemy in list)
            {
                enemy.DropSomethingJuicyMaybe();
            }
            _enemies.RemoveAll(enemy => enemy.LifePoints <= 0);

            // remove bosses
            //_bosses.RemoveAll(boss => boss.LifePoints <= 0);

            // add new enemies (this one only contains stuff if the boss or gigachad is dead)
            if (_enemiesToAdd.Count > 0)
            {
                _enemies.AddRange(_enemiesToAdd);
                _enemiesToAdd.Clear();
            }
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