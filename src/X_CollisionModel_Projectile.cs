using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
            who = new List<IGameElement>();
            contactPoint = new List<Point>();
            contactNormal = new List<Vector2>();

            bool result = false;
            Point point;
            Vector2 normal;
            Rectangle myRect = me.Rect;
            Vector2 myVelocity = me.Velocity;
            Rectangle otherRect;
            Vector2 otherVelocity;

            foreach (var victim in me.Level.Victims)
            {
                if (victim.WhatAreYou() == me.WhatAreYou()) continue;
                if (victim == me.WhoFiredMe) continue;

                otherRect = victim.Rect;
                otherVelocity = victim.Velocity;
                result = Manager_Collision.MovingRectVsMovingRectFast(
                    ref myRect, ref myVelocity, Mass,
                    ref otherRect, ref otherVelocity, victim.Collision.Mass,
                    Cr, timeStepMS, out point, out normal);
                if (result)
                {
                    who.Add(victim);
                    contactPoint.Add(point);
                    contactNormal.Add(normal);
                    me.Velocity = Vector2.Zero;
                    victim.Velocity = otherVelocity;
                    Logger.Debug("impacted with someone at " + point.ToString());

                    _records.Add(new Manager_Collision.Record(
                        (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds,
                        point, Vector2.Zero, 0));
                    break;
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
                        Logger.Debug("impacted at " + point.ToString() + " with room " + room.Name);
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
                            Logger.Debug("impacted at " + point.ToString() + " with room " + room.Name);
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
