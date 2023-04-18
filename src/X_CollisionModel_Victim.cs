using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // check the room
            IWalkable room = me.Level.GetRoom(me, me.Room);
            if(room != null)
            {
                if (room.Collision.Intersect(ref myRect, ref myVelocity, timeStepMS, out point, out normal))
                {
                    result = true;
                    who.Add(room);
                    contactPoint.Add(point);
                    contactNormal.Add(normal);
                    me.Velocity = myVelocity;
                    //Logger.Debug("impacted at " + point.ToString() + " with room " + room.Name);
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
                        //Logger.Debug("impacted at " + point.ToString() + " with room " + room.Name);
                    }
                }

                Rectangle rect = me.Rect;
                rect.Location += (me.Velocity * timeStepMS).ToPoint();
                me.Rect = rect;
            }

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
