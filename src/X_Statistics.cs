using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace YGR
{
    public sealed class Statistics
    {
        public int Kills; // IEnemy.Hit()
        public int Deaths; // IPlayer.Hit()
        public int Revives; // IPlayer.Revive()
        public int DamageDealt; // IEnemy.Hit()
        public int DamageTaken;// IPlayer.Hit()
        public int BossDamageDealt; // IEnemy.Hit()
        public int BossDamageTaken; // IPlayer.Hit()
        public int ProjectilesFired; // Projectile()
        public int TimesFired; // IPlayer.Handle{Controller,Keyboard}Input()
        public int TimesHit; // IPlayer.Hit()
        public int TimesDashed; // IPlayer.UpdateDash()
        public int PowerUpsUsed; // TODO: We don't have powerups yet aside of Heal/Revive
        public int AmountHealed; // IPlayer.heal()
        public float DistanceTravelled; // IPlayer.UpdateCollision()
    }
}
