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

        public bool Intersect(IVictim me, int timeStepMS, out Vector2 newVelocity, out IList<Point> contactPoint, out IList<Vector2> contactNormal, out IList<IGameElement> who)
        {
            who = new List<IGameElement>();
            contactPoint = new List<Point>();
            contactNormal = new List<Vector2>();

            bool result = false;
            Point point;
            Vector2 normal;
            Rectangle myRect = me.Rect;
            newVelocity = me.Velocity;
            // check the room
            IWalkable room = me.Level.GetRoom(me, me.Room);
            me.Room = room;
            if(room != null)
            {
                if(room.WhatAreYou() == X_LevelElements.Room)
                {
                    ((Y_CMRoom)room).ApplyPowerUps(me);
                }
                if (room.Collision.Intersect(ref myRect, ref newVelocity, timeStepMS, out point, out normal))
                {
                    result = true;
                    who.Add(room);
                    contactPoint.Add(point);
                    contactNormal.Add(normal);
                    //Logger.Debug("impacted at " + point.ToString() + " with room " + room.Name);
                }

                // check the connected connectors, just to be sure
                foreach (var door in room.DoorRooms)
                {
                    if (door.Value.First().Collision.Intersect(ref myRect, ref newVelocity, timeStepMS, out point, out normal))
                    {
                        result = true;
                        who.Add(room);
                        contactPoint.Add(point);
                        contactNormal.Add(normal);
                        //Logger.Debug("impacted at " + point.ToString() + " with room " + room.Name);
                    }
                }
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
