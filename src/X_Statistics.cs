using System;
using System.Collections.Generic;

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
        public int ProjectilesDodged; // X_CollisionModel_Projectile.Intersect()
        public int TimesAbilitated; // Ability.Triggered()

        private HashSet<WeakReference> ProjectilesTracked = new HashSet<WeakReference> { };
        public void TrackDodgedProjectile(IProjectile projectile)
        {
            // Reverse look-up to see if we already track it
            bool alreadyTracked = false;
            foreach (var weakRef in ProjectilesTracked)
            {
                if (weakRef.Target == projectile)
                {
                    alreadyTracked = true;
                    break;
                }
            }
            if (!alreadyTracked)
            {
                ProjectilesDodged++;
                ProjectilesTracked.Add(new WeakReference(projectile));
            }
            
            // Amortized lazy clean-up
            if (ProjectilesTracked.Count > 32)
                ProjectilesTracked.RemoveWhere(x => x.Target == null);
        }
    }
}
