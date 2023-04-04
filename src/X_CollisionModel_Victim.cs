using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace YGR
{
    public class X_CollisionModel_Victim
    {
        public float Mass { get; }
        public float Cr { get; }

        private List<Manager_Collision.Record> _records;

        public X_CollisionModel_Victim(float mass, float cr) 
        {
            Mass = (mass == 0) ? float.Epsilon : mass;
            Cr = cr;
            _records = new List<Manager_Collision.Record>();
        }

        public bool Intersect(IVictim me, int timeStepMS, out IList<Point> contactPoint, out IList<Vector2> contactNormal, out IList<IGameElement> who)
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
            //foreach (var victim in me.Room.Victims)
            //{
            //    if (victim == me) continue;

            //    otherRect = victim.Rect;
            //    otherVelocity = victim.Velocity;
            //    result = Manager_Collision.MovingRectVsMovingRect(
            //        ref myRect, ref myVelocity, Mass,
            //        ref otherRect, ref otherVelocity, victim.Collision.Mass,
            //        Cr, timeStepMS, out point, out normal);
            //    if (result)
            //    {
            //        who.Add(victim);
            //        contactPoint.Add(point);
            //        contactNormal.Add(normal);
            //        me.Velocity = myVelocity;
            //        victim.Velocity = otherVelocity;
            //        Logger.Info("impacted with someone at " + point.ToString());

            //        _records.Add(new Manager_Collision.Record(
            //            (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds,
            //            point, Vector2.Zero, 0));
            //    }
            //}

            if (me.Room.Collision.Intersect(ref myRect, ref myVelocity, timeStepMS, out point, out normal))
            {
                result = true;
                who.Add(me.Room);
                contactPoint.Add(point);
                contactNormal.Add(normal);
                me.Velocity = myVelocity;
                Logger.Info("impacted at " + point.ToString() + " with room " + me.Room.Name);
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
