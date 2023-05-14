using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace YGR
{
    /* Render some notifications in-game that automatically fade away and disappear again */
    public static class Notifications
    {
        public class Notification
        {
            public int Age = 0;
            public int AgeMax = 4000;
            internal string Message = "";
            internal Color Color = Color.BlanchedAlmond;
            internal Vector2 Size;
            internal SpriteFont Font;

            internal Notification(string message)
            {
                Message = message;
                Font = Fonts.Small;
                Size = Font.MeasureString(message);
            }

            internal Notification(
                string message,
                Color color,
                int ageMax
            ) : this(message)
            {
                AgeMax = ageMax;
                Color = color;
            }

            internal Notification(
                string message,
                Color color,
                int ageMax,
                SpriteFont font
            ) : this(message, color, ageMax)
            {
                Font = font;
                Size = Font.MeasureString(message);
            }

        }

        internal static List<Notification> _notifications { get; private set; } = new List<Notification> { };

        public static void Clear()
        {
            _notifications.Clear();
        }

        public static ReadOnlyCollection<Notification> GetNotifications()
        {
            return _notifications.AsReadOnly();
        }

        public static void New(string message)
        {
            _notifications.Add(new Notification(message));
        }

        public static void New(string message, Color color, int ageMax)
        {
            _notifications.Add(new Notification(message, color, ageMax));
        }

        public static void New(string message, Color color, int ageMax, SpriteFont font)
        {
            _notifications.Add(new Notification(message, color, ageMax, font));
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var n in _notifications)
            {
                n.Age += gameTime.ElapsedGameTime.Milliseconds;
            }
            _notifications.RemoveAll(n => n.Age > n.AgeMax);
        }

        public static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            int x = Camera.Bounds.Width / 2;
            int y = Camera.Bounds.Height / 32;

            foreach (var n in _notifications)
            {
                Vector2 pos = new Vector2(x - n.Size.X / 2, y);
                Color color = n.Color;
                int timeLeft = n.AgeMax - n.Age;
                float fadeTime = 1500;
                if (timeLeft < fadeTime)
                {   // Fade out
                    color *= timeLeft / fadeTime;
                }
                Util.DrawString(n.Font, n.Message, pos, color, spriteBatch);
                y += (int)(n.Size.Y * 1.5);
            }
        }
    }
}
