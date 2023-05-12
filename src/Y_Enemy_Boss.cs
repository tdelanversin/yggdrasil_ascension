using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class Enemy_Boss : Enemy_Basic, IEnemyBoss
    {
        public BossAttack Attack { get; set; } = BossAttack.Spawn;
        int _attackTimer = 0;
        Dictionary<BossAttack, IShooter> _attacks = new Dictionary<BossAttack, IShooter>();
        Dictionary<BossAttack, int> _attackLength = new Dictionary<BossAttack, int>();

        int Phase = 1;
        float Phase1HP;
        float Phase2HP;
        float Phase3HP;
        Dictionary<BossAttack, float> _attackWeights = new Dictionary<BossAttack, float>();
        Dictionary<BossAttack, float> _attackWeightsPhase1 = new Dictionary<BossAttack, float>();
        Dictionary<BossAttack, float> _attackWeightsPhase3 = new Dictionary<BossAttack, float>();
        IList<IEnemy> _minions = new List<IEnemy>();

        // Vairables for AOE pattern avoiding attack
        Vector2 _savedTargetDirection = new Vector2(1, 0);
        bool _alreadyShot = false;



        public Enemy_Boss(
            Vector2 position,
            AnimatedSprite sprite,
            Y_Level level
        ) : base(position, sprite, level)
        {
            Name = "Slime Boss";
            Gun = new Gun_BasicEnemy(this);

            _attacks = new Dictionary<BossAttack, IShooter>() {
                { BossAttack.Scatter, new Gun_BossScatter(this) },
                { BossAttack.Precise, new Gun_BossPrecise(this) },
                { BossAttack.AOE, new Gun_BossAOE(this) },
                { BossAttack.AvoidPattern, new Gun_BossAvoidPattern(this) },
                { BossAttack.Wait, new Gun_BasicEnemy(this) },
                { BossAttack.Spawn, new Gun_BasicEnemy(this) },
            };
            _attackLength = new Dictionary<BossAttack, int>() {
                { BossAttack.Scatter, 5000 },
                { BossAttack.Precise, 5000 },
                { BossAttack.AOE, 10000 },
                { BossAttack.AvoidPattern, 10000 },
                { BossAttack.Wait, 3000 },
                { BossAttack.Spawn, 3000 },
            };

            _attackWeightsPhase1 = new Dictionary<BossAttack, float>() {
                { BossAttack.Scatter, 0.35f },
                { BossAttack.Precise, 0.35f },
                { BossAttack.AOE, 0.30f },
                { BossAttack.AvoidPattern, 0.0f },
                { BossAttack.Wait, 0.0f },
                { BossAttack.Spawn, 0.0f}
            };
            _attackWeightsPhase3 = new Dictionary<BossAttack, float>() {
                { BossAttack.Scatter, 0.25f },
                { BossAttack.Precise, 0.25f },
                { BossAttack.AOE, 0.25f },
                { BossAttack.AvoidPattern, 0.25f },
                { BossAttack.Wait, 0.0f },
                { BossAttack.Spawn, 0.0f}
            };

            _attackWeights = _attackWeightsPhase1;

            _hitColor = Color.OrangeRed;
            Color = new Color(255, 255, 98);
            _currentColor = Color;

            _maxVelocity = 0.075f;

            var _mass = 8.0f; // Heavier than other entities
            Collision = new X_CollisionModel_Victim(_mass, 0.0f);

            // Collision bounds
            int height = 250;
            int width = (int)(height / CharacterSprite.SpriteDimension.Y * CharacterSprite.SpriteDimension.X);

            // Offset the enitity to center it on the spawner tile
            _position = position - new Vector2(width / 2, height / 2);
            _rect = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                width,
                height
            );

            // Set the drawing scale to make the character fit into the collision bounds
            CharacterScale = Util.GetSpriteScale(_rect, CharacterSprite.SpriteDimension);
            CharacterOffset = Vector2.Zero;

            // Create the minions
            for (int i = 0; i < 22; i++)
            {
                _minions.Add(Manager_Enemies.MakeEnemy_BossMinion(Level, Color));
            }

            Phase1HP = 200;
            Phase2HP = 0;
            foreach (var minion in _minions)
            {
                Phase2HP += Math.Max(0, minion.LifePoints);
            }
            Phase3HP = 200;

            LifePointsMax = Phase1HP + Phase2HP + Phase3HP;

            LifePoints = LifePointsMax;
            fleeingHPTreshold = 0;
        }

        public override X_LevelElements WhatAreYou()
        {
            return Attack == BossAttack.Hide ? X_LevelElements.Invincible : X_LevelElements.Enemy;
        }

        private Vector2 BossCenter()
        {
            return _rect.Center.ToVector2() + new Vector2(0, 80 * Y_Level.GlobalScale);
        }

        public override void Update(GameTime gameTime)
        {
            if (State == EnemyState.Inactive) { return; }

            if (Attack == BossAttack.Spawn)
            {
                CharacterSprite.Update(gameTime, AnimationState.Spawn);
                if (CharacterSprite.Direction != AnimationState.Spawn)
                {
                    Attack = BossAttack.Wait;
                }
                else
                {
                    return;
                }
            }

            UpdateHitCounters(gameTime);

            if (LifePoints <= Phase2HP + Phase3HP && Phase == 1)
            {
                Phase = 2;
                Attack = BossAttack.Hide;
                CharacterSprite.Update(gameTime, AnimationState.Jump);
            }

            if (Phase == 2)
            {
                if (CharacterSprite.Direction == AnimationState.Jump)
                {
                    if (CharacterSprite.DirectionalIndex != 10)
                    {
                        CharacterSprite.Update(gameTime, AnimationState.Jump);
                        return;
                    }
                    // Spawn in enemies
                    foreach (IEnemy minion in _minions)
                    {
                        Manager_Enemies.AddEnemy_BossMinion(BossCenter(), minion);
                    }
                }
                CharacterSprite.Update(gameTime, AnimationState.Hide);

                float Health = 0;
                foreach (var minion in _minions)
                {
                    Health += Math.Max(0, minion.LifePoints);
                }

                LifePoints = Phase3HP + Health;

                if (Health <= 0)
                {
                    Attack = BossAttack.Spawn;
                    Phase = 3;
                    _attackWeights = _attackWeightsPhase3;
                    CharacterSprite.Update(gameTime, AnimationState.Spawn);
                }
                return;
            }

            FindTarget();
            UpdateState(gameTime);

            Vector2 movement = Vector2.Zero;
            switch (Attack)
            {
                case BossAttack.Scatter:
                    movement = Wander(gameTime);
                    break;
                case BossAttack.Precise:
                    movement = Chase(gameTime);
                    break;
                case BossAttack.AOE:
                    break;
                case BossAttack.AvoidPattern:
                    break;
                case BossAttack.Wait:
                    movement = ForceChace(gameTime);
                    break;
                default:
                    movement = Vector2.Zero;
                    break;
            }

            UpdateVelocity(movement, gameTime);
            UpdateCollision(gameTime);

            foreach (var attack in _attacks)
            {
                attack.Value.Update(gameTime);
            }

            if (_attackLength[Attack] < _attackTimer + gameTime.ElapsedGameTime.Milliseconds)
            {
                _attackTimer = 0;
                if (Attack != BossAttack.Wait)
                {
                    Attack = BossAttack.Wait;
                    _alreadyShot = false;
                }
                else
                {
                    var rand = new Random();
                    float randVal = (float)rand.NextDouble();
                    foreach (var attack in _attackWeights)
                    {
                        if (randVal >= attack.Value)
                        {
                            randVal -= attack.Value;
                            continue;
                        }
                        Attack = attack.Key;
                        if (Attack == BossAttack.AvoidPattern && Target != null)
                        {
                            _savedTargetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                            _savedTargetDirection.Normalize();
                        }
                        break;
                    }
                }
            }
            else
            {
                _attackTimer += gameTime.ElapsedGameTime.Milliseconds;
            }

            Vector2 targetDirection = Vector2.One;
            if (Target != null)
            {
                targetDirection = Target.Rect.Center.ToVector2() - _rect.Center.ToVector2();
                targetDirection.Normalize();
            }

            switch (Attack)
            {
                case BossAttack.Scatter:
                    CharacterSprite.Update(gameTime, movement);
                    break;
                case BossAttack.Precise:
                    CharacterSprite.Update(gameTime, movement);
                    break;
                case BossAttack.AOE:
                    CharacterSprite.Update(gameTime, AnimationState.Jump);
                    if (CharacterSprite.DirectionalIndex != 10) // frame 10 and 11 are the first frames where the boss hits the ground
                        return;
                    break;
                case BossAttack.AvoidPattern:
                    CharacterSprite.Update(gameTime, AnimationState.Jump);
                    if (!_alreadyShot){
                        if (CharacterSprite.DirectionalIndex != 10) // frame 10 and 11 are the first frames where the boss hits the ground
                            return;
                        _alreadyShot = true;
                    }
                    targetDirection = _savedTargetDirection;
                    break;
                case BossAttack.Wait:
                    CharacterSprite.Update(gameTime, movement);
                    return;
                default:
                    CharacterSprite.Update(gameTime, movement);
                    break;
            }

            _attacks[Attack].Shoot(gameTime, BossCenter(), targetDirection, Level, this);
        }

        
        protected override void DrawHealthbar(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            Vector2 dim = Manager_Sprites.HealthbarEmpty.Bounds.Size.ToVector2();
            float scale = 2;
            dim *= scale;
            Vector2 offset = new Vector2((Rect.Width - dim.X) / 2, -dim.Y - 5);
            Vector2 pos = _rect.Location.ToVector2() + offset;
            spriteBatch.Draw(
                texture: Manager_Sprites.HealthbarEmpty,
                position: pos,
                sourceRectangle: null,
                color: Color.White,
                rotation: 0,
                origin: Vector2.Zero,
                scale: scale,
                effects: SpriteEffects.None,
                layerDepth: 0);

            // Fill the healthbar
            if (LifePoints > 0)
            {
                float healthPerc = LifePoints / (float)LifePointsMax;
                Rectangle infill = Manager_Sprites.HealthbarInfill.Bounds;
                infill.Width = (int)(infill.Width * healthPerc);
                spriteBatch.Draw(
                    texture: Manager_Sprites.HealthbarInfill,
                    position: pos,
                    sourceRectangle: infill,
                    color: Color.OrangeRed,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: scale,
                    effects: SpriteEffects.None,
                    layerDepth: 0);
            }
        }

        /* Deal with being hit by projectile, basically physical therapy */
        public override void Hit(IProjectile projectile)
        {
            lock (this)
            {
                if (State == EnemyState.Inactive || Attack == BossAttack.Hide || Attack == BossAttack.Spawn) { return; }

                // Return if alread dead, otherwise player kill stats are inaccurate
                if (LifePoints <= 0) { return; }

                if (projectile.WhatAreYou() == X_LevelElements.ConfusionProjectile)
                {
                    Manager_Confusion.AddConfusion(this, ((Projectile_Confusion)projectile).ConfusionDuration);
                }

                LifePoints -= projectile.Damage;
                _hitFramesCounter = 1;
                _currentColor = Color.Lerp(_hitColor, Color, 0.1f);

                // @statistics
                var p = (IPlayer)projectile.WhoFiredMe;
                p.Stats.DamageDealt += projectile.Damage;
                p.Stats.TimesHit++;
                if (this is IEnemyBoss) { p.Stats.BossDamageDealt += projectile.Damage; }
                if (LifePoints <= 0) { p.Stats.Kills++; }
            }
        }
    }
}
