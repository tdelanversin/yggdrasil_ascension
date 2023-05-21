using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static Assimp.Metadata;
using static YGR.Manager_Collision;

namespace YGR
{
    public class X_CollisionModel_Projectile
    {
        public float Mass { get; }
        public float FakeAcceleration { get; }

        private List<Manager_Collision.Record> _records;

        public X_CollisionModel_Projectile(float mass, float fakeAcceleration)
        {
            Mass = (mass == 0) ? float.Epsilon : mass;
            FakeAcceleration = fakeAcceleration;
            _records = new List<Manager_Collision.Record>();
        }

        public bool Intersect(IProjectile me, int timeStepMS, out Vector2 newVelocity, out IList<Point> contactPoint, out IList<Vector2> contactNormal, out IList<IGameElement> who)
        {
            who = new List<IGameElement>();
            contactPoint = new List<Point>();
            contactNormal = new List<Vector2>();

            bool result = false;
            Point point;
            Vector2 normal;
            Rectangle myRect = me.Rect;
            newVelocity = me.Velocity;

            IWalkable room = me.Level.GetRoom(me, me.Room);
            me.Room = room;
            var whoFiredMe = (IVictim)me.WhoFiredMe;
            if (whoFiredMe is IPlayer)
            {
                // regular player shot the projectile
                foreach (var enemy in Manager_Enemies.GetEnemies())
                {
                    if (enemy.Room != me.Room) continue;

                    if (enemy.WhatAreYou() == X_LevelElements.Invincible) continue;

                    if (handlePotentialImpact(me, (IVictim)enemy, ref myRect, ref newVelocity, ref contactPoint, ref contactNormal, ref who, timeStepMS))
                    {
                        result = true;
                        me.DeleteNext = true;
                        break;
                    }
                }
            }
            else
            {
                // enemy shot the projectile
                foreach (var victim in Manager_Players.Players)
                {
                    if (victim == me.WhoFiredMe) continue;

                    // Can't touch ghost
                    if (victim.WhatAreYou() == X_LevelElements.Ghost)
                    {
                        // check all the blanks
                        if (victim.DeadAbility != null && victim.DeadAbility is Ability_Blank)
                        {
                            var blank = (Ability_Blank)victim.DeadAbility;
                            if (blank.HitByProjectile(me, timeStepMS))
                            {
                                result = true;
                                me.DeleteNext = true;
                                continue;
                            }
                        }
                        continue;
                    }

                    if (victim.Room != me.Room) continue;

                    // first check all the shields
                    if (victim.Ability != null && victim.Ability is Ability_Shield)
                    {
                        var shield = (Ability_Shield)victim.Ability;
                        if (shield.HitByProjectile(me, timeStepMS))
                        {
                            shield.Hit(me.Damage);
                            result = true;
                            me.DeleteNext = true;
                            continue;
                        }
                    }


                    // Pass through player if they are currently invincible
                    if (victim.WhatAreYou() == X_LevelElements.Invincible)
                    {
                        // Bullets pass through dashing players, but we still
                        // want to track how many a player doged through.
                        // Dashing through bullets while being invincible
                        // doesn't count, that's not very brave.
                        var p = (IPlayer)victim;
                        if (!p.IsDashing || p.IsInvincible) { continue; }

                        Rectangle otherRect = victim.Rect;
                        if (Manager_Collision.FastRectVsRect(ref myRect, newVelocity, timeStepMS, ref otherRect, out point, out normal))
                        {
                            p.Stats.TrackDodgedProjectile(me);
                        }

                        continue;
                    }

                    if (handlePotentialImpact(me, victim, ref myRect, ref newVelocity, ref contactPoint, ref contactNormal, ref who, timeStepMS))
                    {
                        result = true;
                        me.DeleteNext = true;
                        break;
                    }
                }
            }

            if (!result)
            {
                if (room != null)
                {
                    if (room.Collision.Intersect(ref myRect, ref newVelocity, timeStepMS, out point, out normal))
                    {
                        result = true;
                        me.DeleteNext = true;
                        who.Add(room);
                        contactPoint.Add(point);
                        contactNormal.Add(normal);
                        newVelocity = Vector2.Zero;
                    }

                    // check the connected connectors, just to be sure
                    foreach (var door in room.DoorRooms)
                    {
                        if (door.Value.First().Collision.Intersect(ref myRect, ref newVelocity, timeStepMS, out point, out normal))
                        {
                            result = true;
                            me.DeleteNext = true;
                            who.Add(room);
                            contactPoint.Add(point);
                            contactNormal.Add(normal);
                        }
                    }
                }
            }

            if (!result)
            {
                newVelocity = me.Velocity;
            }

            return result;
        }

        private bool handlePotentialImpact(IProjectile me, IVictim impactedObject, ref Rectangle myRect, ref Vector2 myVelocity, ref IList<Point> contactPoint, ref IList<Vector2> contactNormal, ref IList<IGameElement> who, int timeStepMS)
        {
            Point point;
            Vector2 normal;
            Rectangle otherRect = impactedObject.Rect;
            Vector2 otherVelocity = impactedObject.Velocity;
            bool result = Manager_Collision.MovingRectVsMovingRectFast(
                ref myRect, ref myVelocity, Mass,
                ref otherRect, ref otherVelocity, impactedObject.Collision.Mass,
                FakeAcceleration, timeStepMS, out point, out normal);

            if (result)
            {
                who.Add(impactedObject);
                contactPoint.Add(point);
                contactNormal.Add(normal);
                myVelocity = Vector2.Zero;
                //me.Velocity = Vector2.Zero;

                impactedObject.ImpactVelocity = otherVelocity;

                _records.Add(new Manager_Collision.Record(
                    (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds,
                    point, Vector2.Zero, 0));

                impactedObject.WhoKilledMe = me.WhoFiredMe;

                return true;
            }
            return false;
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            _records.RemoveAll(rec => (rec.TimeStampMS + Manager_Collision.DrawTimeoutMS < (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds));
            foreach (var rec in _records)
            {
                Factory_Debug.DrawPoint(
                    rec.ContactPoint.X, rec.ContactPoint.Y, 17, Color.Cyan, spriteBatch);
            }
        }
    }
}
