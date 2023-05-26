using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public static class Manager_Confusion
    {
        private static List<Y_Confusion> _confusion = new List<Y_Confusion>();

        private static List<Color> _confusionColors = new List<Color>();

        public static void Initialize()
        {
            _confusionColors.Add(Color.Aquamarine);
            _confusionColors.Add(Color.AliceBlue);
            _confusionColors.Add(Color.BlueViolet);
            _confusionColors.Add(Color.MediumVioletRed);
            _confusionColors.Add(Color.Orange);
            _confusionColors.Add(Color.GreenYellow);
        }

        public static void AddConfusion(IVictim victim, int durationMS)
        {
            _confusion.Add(new Y_Confusion(victim, Manager_Sprites.NewAnimatedSprite_Confusion(), durationMS));
            victim.Confused = true;
        }

        public static void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (var confusion in _confusion)
            {
                confusion.Update(gameTime);
            }

            var list = _confusion.Where(conf => conf.TimeUp() || conf.Target.LifePoints <= 0).ToList();
            foreach(var l in list)
            {
                l.Target.Confused = false;
            }
            _confusion.RemoveAll(conf => conf.TimeUp() || conf.Target.LifePoints <= 0);
        }

        public static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var confusion in _confusion)
            {
                confusion.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public static void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var confusion in _confusion)
            {
                confusion.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}
