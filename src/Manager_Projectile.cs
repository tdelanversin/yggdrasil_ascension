using Assimp.Configs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace YGR
{

    /// <summary>
    /// Class <c>Factory_Projectiles</c> contains methods to create instances of <c>Y_Projectiles</c>.
    /// </summary>
    public static class Manager_Projectile
    {
        private static List<IProjectile> _projectiles = new List<IProjectile>();

        public static ReadOnlyCollection<IProjectile> GetProjectiles()
        {
            return _projectiles.AsReadOnly();
        }

        //. Returns true only when position is inside the room
        public static bool BoundsCheckSimple(Vector2 position, IWalkable room)
        {
            return room.Rect.Contains(position);
        }

        /// Returns true only when position is inside the room and not inside any collision obstacles
        public static bool BoundsCheckFull(Vector2 position, IWalkable room)
        {
            if (!BoundsCheckSimple(position, room)) return false;

            // TODO: implement, get all collision rectangles of the room and check if we're colliding with them

            return true;
        }

        /// Basic, well rounded projectile
        public static void AddProjectile_StarterProjectile(Vector2 startPosition, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (!BoundsCheckSimple(startPosition, ((IVictim)who).Room)) return;
         
            _projectiles.Add(
                new Projectile_Basic(
                    position: startPosition,
                    direction: direction,
                    sprite: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeOuter(), // TODO: dedicated sprite
                    level: level,
                    who: who,
                    scale: 0.35f,
                    damage: 1,
                    maxAge: 2500,
                    speed: 0.55f,
                    mass: 0.5f,
                    fakeAcceleration: 0.0f // make stuff fly on impact: 1.0f is the exact elastic impact. <1.0f is fake slower, > 1.0f is fake faster
                )
            );
        }

        /// Lighter, smaller, slower and more short-lived than normal projectile
        public static void AddProjectile_ShotGunProjectile(Vector2 startPosition, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (!BoundsCheckSimple(startPosition, ((IVictim)who).Room)) return;

            _projectiles.Add(
                new Projectile_Basic(
                    position: startPosition,
                    direction: direction,
                    sprite: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeOuter(), // TODO: dedicated sprite
                    level: level,
                    who: who,
                    scale: 0.25f,
                    damage: 1,
                    maxAge: 1500,
                    speed: 0.45f,
                    mass: 0.1f,
                    fakeAcceleration: 0.0f // make stuff fly on impact: 1.0f is the exact elastic impact. <1.0f is fake slower, > 1.0f is fake faster
                )
            );
        }

        /// Fast and strong projectiles for the funky gun
        public static void AddProjectile_Strong(Vector2 startPosition, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (!BoundsCheckFull(startPosition, ((IVictim)who).Room)) return;

            _projectiles.Add(
                new Projectile_Basic(
                    position: startPosition,
                    direction: direction,
                    sprite: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeInner(), // TODO: dedicated sprite
                    level: level,
                    who: who,
                    scale: 0.45f,
                    damage: 1,
                    maxAge: 1500,
                    speed: 0.85f,
                    mass: 0.5f,
                    fakeAcceleration: 0.0f // make stuff fly on impact: 1.0f is the exact elastic impact. <1.0f is fake slower, > 1.0f is fake faster
                )
            );
        }

        /// Larger, slower, heavier, longer lived and deals more damage
        public static void AddProjectile_EnemySlimeProjectile(Vector2 startPosition, Vector2 direction, Y_Level level, IGameElement who)
        {
            if (!BoundsCheckSimple(startPosition, ((IVictim)who).Room)) return;

            _projectiles.Add(
                new Projectile_Slime(
                    position: startPosition,
                    direction: direction,
                    spriteOuter: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeOuter(),
                    spriteInner: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeInner(),
                    level: level,
                    who: who,
                    scale: 1f,
                    damage: 2,
                    maxAge: 3500,
                    speed: 0.25f,
                    mass: 0.8f,
                    fakeAcceleration: 0.0f // make stuff fly on impact: 1.0f is the exact elastic impact. <1.0f is fake slower, > 1.0f is fake faster
                )
            );
        }

        /// Larger, slower, heavier, longer lived and deals more damage
        public static void AddProjectile_BossProjectile(Vector2 startPosition, Vector2 direction, Y_Level level, IGameElement who, float speed = 0.35f)
        {
            if (!BoundsCheckSimple(startPosition, ((IVictim)who).Room)) return;

            _projectiles.Add(
                new Projectile_Slime(
                    position: startPosition,
                    direction: direction,
                    spriteOuter: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeOuter(),
                    spriteInner: Manager_Sprites.NewAnimatedSprite_ProjectileSlimeInner(),
                    level: level,
                    who: who,
                    scale: 1f,
                    damage: 2,
                    maxAge: 8000,
                    speed: speed,
                    mass: 0.8f,
                    fakeAcceleration: 1.0f // make stuff fly on impact: 1.0f is the exact elastic impact. <1.0f is fake slower, > 1.0f is fake faster
                )
            );
        }


        public static void AddProjectile_Confusion(Vector2 startPosition, Vector2 direction, int confusionDuration, Y_Level level, IGameElement who)
        {
            _projectiles.Add(
                new Projectile_Confusion(
                    position: startPosition,
                    direction: direction,
                    Manager_Sprites.NewAnimatedSprite_Confusion(),
                    confusionDuration: confusionDuration,
                    level: level,
                    who: who,
                    scale: 0.55f,
                    damage: 0,
                    maxAge: 2500,
                    speed: 0.4f,
                    mass: 0.0f
                 )
             );
        }

        public static void AddProjectile_PinkHammer(Vector2 startPosition, Vector2 direction,Y_Level level, IGameElement who)
        {
            _projectiles.Add(
                new Projectile_Directed(
                    position: startPosition,
                    direction: direction,
                    Manager_Sprites.NewAnimatedSprite_ProjectileHammer(),
                    level: level,
                    who: who,
                    scale: 2f,
                    damage: 0,
                    maxAge: 10000,
                    speed: 0.4f,
                    mass: 50.0f,
                    fakeAcceleration: 2.0f // make stuff fly on impact: 1.0f is the exact elastic impact. <1.0f is fake slower, > 1.0f is fake faster
                 )
             );
        }

        public static void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (IProjectile projectile in _projectiles)
            {
                projectile.Update(gameTime);
            }
            _projectiles.RemoveAll(projectile => (projectile.Age > projectile.MaxAge) || projectile.DeleteNext);
        }

        public static void Draw(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (IProjectile projectile in _projectiles)
            {
                projectile.Draw(gameTime, globalOffset, spriteBatch);
            }
        }

        public static void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (IProjectile projectile in _projectiles)
            {
                projectile.DrawOutline(gameTime, globalOffset, spriteBatch);
            }
        }
    }
}