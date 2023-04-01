using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace YGR
{
    public interface ICollidable
    {

    }

    // Implement these kinds of things:
    // https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp

    // Youtube tutorial
    // https://www.youtube.com/watch?v=8JJ-4JgR7Dg

    public static class Manager_Collision
    {
        public readonly static DateTime StartTime = DateTime.Now;
        public static Color HitColor = Color.Red;
        public static Color MissColor = Color.Green;

        public struct Record
        {
            public Vector2 ContactNormal;
            public Point ContactPoint;
            public float UHit;
            public double TimeStampMS;

            public Record(double timeStampMS, Point contactPoint, Vector2 contactNormal, float uHit)
            {
                TimeStampMS = timeStampMS; ContactPoint = contactPoint; ContactNormal = contactNormal; UHit = uHit;
            }
        }

        public static bool PointVsRect(Vector2 p, Rectangle rect)
        {
            return rect.Contains(p);
        }

        public static bool RayVsRect(
            ref Point origin, 
            ref Vector2 dir, 
            ref Rectangle target, 
            out Point contactPoint,
            out Vector2 contactNormal,
            out float uHit
        )
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            uHit = 0.0f;

            // preassign stuff
            Vector2 rayOrigin = new Vector2(origin.X, origin.Y);
            Vector2 invDir = new Vector2(1.0f / dir.X, 1.0f / dir.Y);
            Vector2 targetPos = new Vector2(target.Location.X, target.Location.Y);
            Vector2 targetSize = new Vector2(target.Width, target.Height);

            // calculate intersections ray to rectangle
            Vector2 tNear = (targetPos - rayOrigin) * invDir;
            Vector2 tFar = (targetPos + targetSize - rayOrigin) * invDir;


            //if (Single.IsInfinity(tFar.Y) || Single.IsInfinity(tFar.X)) return false;
            //if (Single.IsInfinity(tNear.Y) || Single.IsInfinity(tNear.X)) return false;

            // sort distances => swap variables without temp var
            if (tNear.X > tFar.X) (tNear.X, tFar.X) = (tFar.X, tNear.X);
            if (tNear.Y > tFar.Y) (tNear.Y, tFar.Y) = (tFar.Y, tNear.Y);

            // early rejection
            if (tNear.X > tFar.Y || tNear.Y > tFar.X) return false;

            // closest contact
            uHit = Math.Max(tNear.X, tNear.Y);

            // furthest contact
            //float uHitFar = Math.Min(tFar.X, tFar.Y);

            // if we are behind ourselves
            if (uHit < 0) return false;

            // calculate the contact point
            contactPoint = (origin.ToVector2() + uHit * dir).ToPoint();

            // calculate the normal vector
            if(tNear.X > tNear.Y)
            {
                if (invDir.X < 0) contactNormal = new Vector2(1, 0);
                else contactNormal = new Vector2(-1, 0);
            }
            else if(tNear.X < tNear.Y)
            {
                if (invDir.Y < 0) contactNormal = new Vector2(0, 1);
                else contactNormal = new Vector2(0, -1);
            }

            return true;
        }

        public static bool DynamicRectVsStaticRect(
            ref Rectangle movingRect, 
            Vector2 velocity,
            int timeStepMS, 
            ref Rectangle staticRect, 
            out Point contactPoint, 
            out Vector2 contactNormal,
            out float uHit
        )
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            uHit = 0;

            if (velocity.X == 0 && velocity.Y == 0) return false;

            Rectangle expanded = new Rectangle(
                staticRect.X - movingRect.Width / 2, staticRect.Y - movingRect.Height / 2, 
                staticRect.Width + movingRect.Width, staticRect.Height + movingRect.Height);

            Point origin = new Point(
                movingRect.X + movingRect.Width / 2 + Math.Sign(velocity.X), 
                movingRect.Y + movingRect.Height / 2 + Math.Sign(velocity.Y)
            );
            Vector2 newVelocity = velocity * timeStepMS;
            if (RayVsRect(ref origin, ref newVelocity, ref expanded, out contactPoint, out contactNormal, out uHit))
            {
                contactPoint.X -= (int)contactNormal.X * movingRect.Width / 2;
                contactPoint.Y -= (int)contactNormal.Y * movingRect.Height / 2;
                return (uHit >= 0.0f && uHit <= 1.0f);
            }

            return false;
        }

        public static void ResolveDynamicRectVsStaticRect(
            ref Vector2 velocity,
            ref List<Record> records
        )
        {
            foreach(var rec in records)
            {
                Vector2 v = new Vector2(Math.Abs(velocity.X), Math.Abs(velocity.Y)) * (1 - rec.UHit);
                velocity += rec.ContactNormal * v;
            }
        }

        public static bool DynamicRectVsStaticRects(
            ref Rectangle movingRect,
            ref Vector2 velocity,
            int timeStepMS,
            Rectangle[] staticRects,
            bool[] staticRectsHit,
            out List<Record> collided
        )
        {
            if (!(staticRects.Length == 0 || (staticRects.Length == staticRects.Length)))
                Logger.Error("Colors array must be of same length as rect array or empty");

            Point contactPoint;
            Vector2 contactNormal;
            collided = new List<Record>();

            float uHit;
            for (int i = 0; i < staticRects.Length; ++i)
            {
                bool result = DynamicRectVsStaticRect(
                    ref movingRect, velocity, timeStepMS,
                    ref staticRects[i], out contactPoint, out contactNormal, out uHit);

                if (result)
                {
                    collided.Add(new Record((DateTime.Now - StartTime).TotalMilliseconds, contactPoint, contactNormal, uHit));
                    if (staticRectsHit.Length > 0) staticRectsHit[i] = true;
                }
            }

            ResolveDynamicRectVsStaticRect(ref velocity, ref collided);

            return collided.Count > 0;
        }

        public static bool FastRectVsRect(
            ref Rectangle myRect,
            Vector2 velocity,
            int timeStepMS,
            ref Rectangle otherRect,
            out Point contactPoint,
            out Vector2 contactNormal
        )
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            Rectangle inter = Rectangle.Intersect(myRect, otherRect);

            bool collision = inter.Width != 0 || inter.Height != 0;

            contactPoint.X = inter.X + inter.Width / 2;
            contactPoint.Y = inter.Y + inter.Height / 2;
            contactNormal = -velocity;
            contactNormal.Normalize();

            return collision;
        }

        public static bool FastRectVsStaticRects(
            ref Rectangle myRect,
            Vector2 velocity,
            int timeStepMS,
            Rectangle[] staticRects,
            bool[] staticRectsHit,
            out List<Record> collided
        )
        {
            Point contactPoint;
            Vector2 contactNormal;
            collided = new List<Record>();
            for (int i = 0; i < staticRects.Length; ++i)
            {
                bool result = FastRectVsRect(ref myRect, velocity, timeStepMS, ref staticRects[i], out contactPoint, out contactNormal);
                {
                    if (result)
                    {
                        collided.Add(new Record((DateTime.Now - StartTime).TotalMilliseconds, contactPoint, contactNormal, 0));
                        if (staticRectsHit.Length > 0) staticRectsHit[i] = true;
                    }
                }
            }

            return collided.Count > 0;
        }

        public static bool MovingRectVsMovingRect(
            ref Rectangle myRect, ref Vector2 myVelocity, float myMass,
            ref Rectangle otherRect, ref Vector2 otherVelocity, float otherMass,
            float cr, int timeStepMS,
            out Point contactPoint)
        {
            Vector2 contactNormal = Vector2.Zero;
            float uHit = 0;
            // first assume that the other one is static with respect to the relative speed
            bool contact = DynamicRectVsStaticRect(
                ref myRect, myVelocity - otherVelocity, timeStepMS, ref otherRect, 
                out contactPoint, out contactNormal, out uHit);

            if (!contact) return false;

            // then resolve the speeds according to the elastic impact rules
            float massSum = myMass + otherMass;
            Vector2 f = myMass * myVelocity + otherMass * otherVelocity;
            Vector2 myNewVelocity = (cr * otherMass * (otherVelocity - myVelocity) + f) / massSum;
            Vector2 otherNewVelocity = (cr * myMass * (myVelocity - otherVelocity) + f) / massSum;

            myVelocity = myNewVelocity;
            otherVelocity = otherNewVelocity;

            return true;
        }

        public static bool MovingRectVsMovingRectFast(ref Rectangle myRect, ref Vector2 myVelocity, float myMass,
            ref Rectangle otherRect, ref Vector2 otherVelocity, float otherMass,
            float cr, int timeStepMS,
            out Point contactPoint)
        {
            Vector2 contactNormal;
            bool contact = FastRectVsRect(
                ref myRect, myVelocity - otherVelocity, timeStepMS, 
                ref otherRect, out contactPoint, out contactNormal);

            if (!contact) return false;

            float massSum = myMass + otherMass;
            Vector2 f = myMass * myVelocity + otherMass * otherVelocity;
            Vector2 myNewVelocity = (cr * otherMass * (otherVelocity - myVelocity) + f) / massSum;
            Vector2 otherNewVelocity = (cr * myMass * (myVelocity - otherVelocity) + f) / massSum;

            myVelocity = myNewVelocity;
            otherVelocity = otherNewVelocity;

            return true;
        }
    }
}
