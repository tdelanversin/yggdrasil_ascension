using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
#nullable enable

namespace YGR
{
    public class Projectile_Basic : IProjectile
    {
        public float Age { get; set; }
        public float MaxAge { get; set; }
        public bool DeleteNext { get; set; }
        public X_CollisionModel_Projectile Collision { get; set; }
        public Vector2 Velocity { get; set; }
        public Y_Level Level { get; set; }
        public IWalkable Room { get; set; }
        public IGameElement WhoFiredMe { get; set; }
        public int Damage { get; set; }
        public Color Color { get; set; }

        public float Scale { get; protected set; }
        public Rectangle Rect { get { return _rect; } set { _rect = value; } }

        protected Vector2 _position;
        protected AnimatedSprite _sprite;
        protected Vector2 _size;
        protected Vector2 _direction;
        protected bool _isEnemy;
        protected X_ConnectorSide _lastSide;
        protected Rectangle _rect;
        protected float _speed;
        protected float _mass;
        protected float _cr;
        protected float _scale;

        public Projectile_Basic(
            Vector2 position,
            Vector2 direction,
            AnimatedSprite sprite,
            Y_Level level,
            IGameElement who,
            float scale = 0.55f,
            int damage = 1,
            float maxAge = 2500,
            float speed = 0.55f,
            float mass = 0.5f
        )
        {
            // Constructor args
            _position = position;
            _direction = direction;
            _sprite = sprite;
            Level = level;
            WhoFiredMe = who;
            _scale = scale;
            Damage = damage;
            MaxAge = maxAge;
            _speed = speed;
            _mass = mass;

            // Other fields
            Age = 0f;
            _isEnemy = false;
            DeleteNext = false;
            Velocity = _speed * direction;
            Collision = new X_CollisionModel_Projectile(_mass, 1.0f);
            Color = Color.White; // neutral
                                 //Manager_Particles.GenParticleEffectProjectileTrails(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2), Color);

            // TODO: we could probably get rid of the global scale and simplify this at one point
            /* this one is NEEDED for initialization purpose if the start room is not at (0,0) */
            _rect = new Rectangle(
                (int)position.X - (int)(_size.X / 2.0f),
                (int)position.Y - (int)(_size.Y / 2.0f),
                (int)(_size.X),
                (int)(_size.Y));

            Room = Level.GetRoom(this, Room);
            Scale = _scale * Room.Scale;
            _size = Scale * _sprite.SpriteDimension;

            // Center on the initial position
            // Note that the position is only affected by the global scale, not the internal one
            _rect = new Rectangle(
                (int)position.X - (int)(_size.X / 2.0f),
                (int)position.Y - (int)(_size.Y / 2.0f),
                (int)(_size.X * Room.Scale),
                (int)(_size.Y * Room.Scale));
            // Manager_Particles._particleEffects[2].Trigger
            //(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));

            _position = _rect.Location.ToVector2();
        }

        public virtual void UpdateCollisionAndVelocity(GameTime gameTime)
        {
            int timeStepMS = (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            Age += timeStepMS;

            /* Collision / Velocity handling */
            IList<Vector2> contactNormal;
            IList<Point> contactPoint;
            IList<IGameElement> who;
            Vector2 newVelocity = Velocity;
            if (Collision.Intersect(this, timeStepMS, out newVelocity, out contactPoint, out contactNormal, out who))
            {
                //Logger.Debug("Collided with something");
                foreach (var obj in who)
                {
                    // No friendly fire between entities of same kind
                    if (obj.WhatAreYou() == WhoFiredMe.WhatAreYou()) continue;

                    // Can't touch ghost
                    if (obj.WhatAreYou() == X_LevelElements.Ghost) continue;

                    // Pass through player if they are currently invincible
                    if (obj.WhatAreYou() == X_LevelElements.Invincible) continue;

                    // Hit players and enemies
                    if (obj is IVictim)
                    {
                        Manager_Particles._particleEffects[(int)Manager_Particles.Effect.Impact].Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));
                        ((IVictim)obj).Hit(this);
                    }
                }
                Velocity = newVelocity;
            }
            _position += Velocity * timeStepMS;
            _rect.Location = _position.ToPoint();
        }

        /* Particle handling */
        public virtual void UpdateParticles(GameTime gameTime)
        {
            Manager_Particles._particleEffects[(int)Manager_Particles.Effect.ProjectileTrails].Emitters.ForEach(emitter => { emitter.Parameters.Color = Color.ToHsl(); });//new MonoGame.Extended.Range<HslColor>(Color.ToHsl());
            Manager_Particles._particleEffects[(int)Manager_Particles.Effect.ProjectileTrails].Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));
        }

        /* Sprite animation handling */
        public virtual void UpdateSprites(GameTime gameTime)
        {
            _sprite.Update(gameTime, AnimationState.Idle);
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateCollisionAndVelocity(gameTime);
            UpdateParticles(gameTime);
            UpdateSprites(gameTime);
        }

        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _sprite.Texture, _position + globalOffset,
                _sprite.SourceRectangle,
                Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        void IGameElement.DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 1, Color.BlueViolet, spriteBatch);
            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public virtual X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }
}
