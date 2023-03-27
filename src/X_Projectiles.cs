using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#nullable enable

namespace YGR
{
    public class X_StarterProjectile: IProjectile
    {
        public Texture2D _sprite { get; set; }
        public Rectangle _window { get; set; }
        public int _animationIndex { get; set; }
        public Vector2 _position { get; set; }
        public Vector2 _direction { get; set; }
        public double _timeCreated { get; set; }
        public double _speed { get; set; }
        public bool _isEnemy { get; set; }
        public string Name { get; set; }


        public X_StarterProjectile(
            Vector2 position,
            Vector2 direction,
            double timeCreated
        ) {
            _sprite = ProjectileManager.projectile_textures["default_projectile"];
            _window = new Rectangle(0, 0, 64, 64);
            _animationIndex = 0;
            _position = position;
            _direction = direction;
            _speed = 1;
            _timeCreated = timeCreated;
            _isEnemy = false;
            Name = "StarterProjectile";
        }

        public void Update(GameTime gameTime) {
            _position += _direction * (float)(_speed * gameTime.ElapsedGameTime.TotalMilliseconds);
            _animationIndex = (int)(5 - (gameTime.TotalGameTime.TotalMilliseconds - _timeCreated) / 300);
        }

        public void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch) {
            var destinationRectangle = new Rectangle(
                (int)(_position.X - globalOffset.X - _window.Width / 2),
                (int)(_position.Y - globalOffset.Y - _window.Height / 2),
                _window.Width,
                _window.Height
            );
            var sourceRectangle = new Rectangle(
                _window.X + _animationIndex * _window.Width,
                _window.Y,
                _window.Width,
                _window.Height
            );
            Logger.Debug("Drawing projectile at " + destinationRectangle.ToString() + " with source " + sourceRectangle.ToString());
            spriteBatch.Draw(
                _sprite,
                destinationRectangle,
                sourceRectangle,
                Color.White
            );
        }
    }
}