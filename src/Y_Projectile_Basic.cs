using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Diagnostics;
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
        public float Damage { get; set; }
        public Color Color { get; set; }
        public int ElementLevel { get; set; }


        public float LocalScale { get; }
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
        protected float _fakeAcceleration;

        public Projectile_Basic(
            Vector2 position,
            Vector2 direction,
            AnimatedSprite sprite,
            Y_Level level,
            IGameElement who,
            float scale = 0.55f,
            float damage = 1,
            float maxAge = 2500,
            float speed = 0.55f,
            float mass = 0.5f,
            float fakeAcceleration = 1.0f
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
            _fakeAcceleration = fakeAcceleration;

            // Other fields
            Age = 0f;
            _isEnemy = false;
            DeleteNext = false;
            Velocity = _speed * direction;
            Collision = new X_CollisionModel_Projectile(_mass, fakeAcceleration);
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
            LocalScale = _scale * Y_Level.GlobalScale;
            _size = LocalScale * _sprite.SpriteDimension;

            // Center on the initial position
            // Note that the position is only affected by the global scale, not the internal one
            _rect = new Rectangle(
                (int)position.X - (int)(_size.X / 2.0f),
                (int)position.Y - (int)(_size.Y / 2.0f),
                (int)(_size.X * Y_Level.GlobalScale),
                (int)(_size.Y * Y_Level.GlobalScale));
            // Manager_Particles._particleEffects[2].Trigger
            //(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));

            _position = _rect.Location.ToVector2();

            if (WhoFiredMe is IPlayer)
            {
                var p = (IPlayer)WhoFiredMe;
                p.Stats.ProjectilesFired++;
            }
        }

        public virtual float LeveledDamage()
        {
            // double the damage on level 3 compared to level 1
            return this.Damage + (WhoFiredMe.ElementLevel - 1) * this.Damage / 2.0f;
        }

        protected virtual void ImpactParticles(IVictim obj, Vector2 contactNormal)
        {
            if (!Settings.ParticleEffects)
            {
                return;
            }

            if (obj is IEnemy)
            {
                Vector2 pos = new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2);
                pos = Vector2.Lerp(pos, obj.Rect.Center.ToVector2(), 0.3f); // move towards center of enemy (looks better)
                Vector2 nor = contactNormal;
                Manager_Particles.MakeSlimeImpactParticle((IEnemy)obj, pos, nor);
            }
            else
            {
                Manager_Particles.GetParticleEffect(Manager_Particles.Effect.Impact).Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));
            }
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

                    // Generate wall impact particles
                    if ((obj.WhatAreYou() == X_LevelElements.Room || obj.WhatAreYou() == X_LevelElements.Door) && WhoFiredMe is IPlayer)
                        Manager_Particles.MakeWallImpactParticle(contactPoint[0].ToVector2(), contactNormal[0]);

                    // Hit players and enemies
                    if (obj is IVictim)
                    {
                        lock(this)
                        {
                            ImpactParticles((IVictim)obj, contactNormal[0]);
                            ((IVictim)obj).Hit(this);
                        }
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
            if (!Settings.ParticleEffects)
            {
                return;
            }
            
            //Manager_Particles.GetParticleEffect(Manager_Particles.Effect.ProjectileTrails).Emitters.ForEach(emitter => { emitter.Parameters.Color = Color.ToHsl(); });//new MonoGame.Extended.Range<HslColor>(Color.ToHsl());
            //Manager_Particles.GetParticleEffect(Manager_Particles.Effect.ProjectileTrails).Trigger(new Vector2(_rect.Location.X + _rect.Width / 2, _rect.Location.Y + _rect.Height / 2));
        }

        /* Sprite animation handling */
        public virtual void UpdateSprites(GameTime gameTime)
        {
            _sprite.Update(gameTime, AnimationState.Idle);
        }

        public virtual void Update(GameTime gameTime)
        {
            //var watch = new Stopwatch();
            //watch.Start();
            //UpdateCollisionAndVelocity(gameTime);

            //var t1 = watch.ElapsedMilliseconds;
            UpdateParticles(gameTime);
            //var t2 = watch.ElapsedMilliseconds;
            UpdateSprites(gameTime);
            //var t3 = watch.ElapsedMilliseconds;

            //if(t1 > 0 || t2 > 0 || t3 > 0)
            //{
            //    Logger.Info(t1.ToString() + " " + t2.ToString() + " " + t2.ToString() + " - " + Manager_Projectile.GetProjectiles().Count);
            //}
        }

        public virtual void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                _sprite.Texture, _position + globalOffset,
                _sprite.SourceRectangle,
                Color, 0, Vector2.Zero, LocalScale, SpriteEffects.None, 0);
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Factory_Debug.DrawRectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height, 2, Color.BlueViolet, spriteBatch);

            int timeStepMS = gameTime.ElapsedGameTime.Milliseconds;
            int width = timeStepMS * (int)(Rect.Width);
            int height = timeStepMS * (int)(Rect.Height);
            Rectangle pretest = new Rectangle(Rect.X - width / 2, Rect.Y - height / 2, width, height);
            Factory_Debug.DrawRectangle(pretest.X, pretest.Y, pretest.Width, pretest.Height, 5, Color.BlueViolet, spriteBatch);

            Collision.DrawOutline(gameTime, globalOffset, spriteBatch);
        }

        public virtual X_LevelElements WhatAreYou()
        {
            return X_LevelElements.Projectile;
        }
    }
}
