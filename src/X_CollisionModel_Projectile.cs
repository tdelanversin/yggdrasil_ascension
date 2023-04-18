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
        public float Cr { get; }

        private List<Manager_Collision.Record> _records;

        public X_CollisionModel_Projectile(float mass, float cr)
        {
            Mass = (mass == 0) ? float.Epsilon : mass;
            Cr = cr;
            _records = new List<Manager_Collision.Record>();
        }

        public bool Intersect(IProjectile me, int timeStepMS, out IList<Point> contactPoint, out IList<Vector2> contactNormal, out IList<IGameElement> who)
        {
            // TODO: fix issue eith the widespread projectiles causing too many sounds for monogame == crashes game
            // Manager_Sound.AddSound_Explosion().Play();

            who = new List<IGameElement>();
            contactPoint = new List<Point>();
            contactNormal = new List<Vector2>();

            bool result = false;
            Point point;
            Vector2 normal;
            Rectangle myRect = me.Rect;
            Vector2 myVelocity = me.Velocity;

            if(me.WhoFiredMe.WhatAreYou() == X_LevelElements.Victim)
            {
                // regular player shot the projectile
                foreach (var enemy in Manager_Enemies.GetEnemies())
                {
                    if (enemy == me.WhoFiredMe) continue;

                    if(handlePotentialImpact(me, (IVictim)enemy, ref myRect, ref myVelocity, ref contactPoint, ref contactNormal, ref who, timeStepMS))
                    {
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                // enemy shot the projectile
                foreach (var victim in me.Level.Victims)
                {
                    if (victim == me.WhoFiredMe) continue;

                    if (handlePotentialImpact(me, victim, ref myRect, ref myVelocity, ref contactPoint, ref contactNormal, ref who, timeStepMS))
                    {
                        result = true;
                        break;
                    }
                }
            }

            if (!result)
            {
                var room = me.Level.GetRoom(me, me.Room);
                if (room != null)
                {
                    if (room.Collision.IntersectFast(ref myRect, ref myVelocity, timeStepMS, out point, out normal))
                    {
                        result = true;
                        who.Add(room);
                        contactPoint.Add(point);
                        contactNormal.Add(normal);
                        me.Velocity = Vector2.Zero;
                        //Logger.Debug("### " + room.WhatAreYou().ToString() + " => " + me.WhoFiredMe.WhatAreYou().ToString() + ":impacted at " + point.ToString() + " with room 1 " + room.Name);
                    }

                    // check the connected connectors, just to be sure
                    foreach (var door in room.DoorRooms)
                    {
                        if (door.Value.First().Collision.Intersect(ref myRect, ref myVelocity, timeStepMS, out point, out normal))
                        {
                            result = true;
                            who.Add(room);
                            contactPoint.Add(point);
                            contactNormal.Add(normal);
                            me.Velocity = myVelocity;
                            //Logger.Debug("### " + room.WhatAreYou().ToString() + " => " + me.WhoFiredMe.WhatAreYou().ToString() + ":impacted at " + point.ToString() + " with room 2 " + room.Name);
                        }
                    }
                }
            }

            if (result)
            {
                me.DeleteNext = true;
            }

            Rectangle rect = me.Rect;
            rect.Location += (me.Velocity * timeStepMS).ToPoint();
            me.Rect = rect;

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
                Cr, timeStepMS, out point, out normal);

            if (result)
            {
                who.Add(impactedObject);
                contactPoint.Add(point);
                contactNormal.Add(normal);
                me.Velocity = Vector2.Zero;
                impactedObject.Velocity = otherVelocity;
                //Logger.Debug("### " + victim.WhatAreYou().ToString() + " => " + me.WhoFiredMe.WhatAreYou().ToString() + ": impacted with someone at " + point.ToString());

                _records.Add(new Manager_Collision.Record(
                    (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds,
                    point, Vector2.Zero, 0));
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
