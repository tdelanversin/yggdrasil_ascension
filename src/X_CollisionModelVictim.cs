using Microsoft.Xna.Framework;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public class X_CollisionModelVictim
    {
        public float Mass { get; }
        public float Cr { get; }

        public X_CollisionModelVictim(float mass, float cr) 
        {
            Mass = mass;
            Cr = cr;
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
            foreach (var victim in me.Room.Victims)
            {
                otherRect = victim.Rect;
                otherVelocity = victim.Velocity;
                result = Manager_Collision.MovingRectVsMovingRect(
                    ref myRect, ref myVelocity, Mass,
                    ref otherRect, ref otherVelocity, victim.Collision.Mass,
                    Cr, timeStepMS, out point);
                if (result)
                {
                    who.Add(victim);
                    contactPoint.Add(point);
                    contactNormal.Add(Vector2.Zero);
                    me.Velocity = myVelocity;
                    victim.Velocity = otherVelocity;
                    Logger.Info("impacted with someone at " + contactPoint.ToString());
                }
            }

            if (me.Room.Collision.Intersect(ref myRect, ref myVelocity, timeStepMS, out point, out normal))
            {
                result = true;
                who.Add(me.Room);
                contactPoint.Add(point);
                contactNormal.Add(normal);
                me.Velocity = myVelocity;
                Logger.Info("impacted at " + contactPoint.ToString() + " with room " + me.Room.Name);
            }

            ///* ##########################################################################
            // * Collision with other victim-sprites handling
            // * ########################################################################## */
            //Vector2 contactNormal;
            //Point contactPoint;
            //Rectangle rect = Rect;
            //foreach (var victim in _room.Victims)
            //{
            //    if (victim.Collision.Intersect(this, deltaTime, out contactPoint))
            //    {
            //        Logger.Info("impacted with someone");
            //    }
            //}

            ///* ##########################################################################
            // * Collision with the room handling
            // * ########################################################################## */
            //Vector2 velocity = Velocity;
            //if (_room.Collision.Intersect2(ref rect, ref velocity, deltaTime, out contactPoint, out contactNormal))
            //{
            //    Velocity = velocity;
            //    Logger.Info("impacted at " + contactPoint.ToString());
            //}

            return result;
        }
    }
}
