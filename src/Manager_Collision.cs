using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

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

        public static bool RectVsRect(Rectangle rect1, Rectangle rect2)
        {
            return rect1.Intersects(rect2);
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

        public static bool DynamicRectVsRect(
            ref Rectangle movingRect, 
            Vector2 velocity,
            int timeStep, 
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
            Vector2 newVelocity = velocity * timeStep;
            if (RayVsRect(ref origin, ref newVelocity, ref expanded, out contactPoint, out contactNormal, out uHit))
            {
                contactPoint.X -= (int)contactNormal.X * movingRect.Width / 2;
                contactPoint.Y -= (int)contactNormal.Y * movingRect.Height / 2;
                return (uHit >= 0.0f && uHit <= 1.0f);
            }

            return false;
        }

        public static void ResolveDynamicRectVsRect(
            ref Vector2 velocity,
            ref List<Manager_Collision.Record> records
        )
        {
            foreach(var rec in records)
            {
                Vector2 v = new Vector2(Math.Abs(velocity.X), Math.Abs(velocity.Y)) * (1 - rec.UHit);
                velocity += rec.ContactNormal * v;
            }
        }

        public static bool DynamicRectVsRects(
            ref Rectangle movingRect,
            ref Vector2 velocity,
            int timeStepMS,
            Rectangle[] staticRects,
            Color[] staticRectsColor,
            Color? hit,
            Color? miss,
            out List<Record> collided
        )
        {
            if (!(staticRectsColor.Length == 0 || (staticRectsColor.Length == staticRects.Length && hit != null && miss != null)))
                Logger.Error("Colors array must be of same length as rect array or empty");

            Point contactPoint;
            Vector2 contactNormal;
            collided = new List<Record>();

            float uHit;
            for (int i = 0; i < staticRects.Length; ++i)
            {
                bool result = DynamicRectVsRect(
                    ref movingRect, velocity, timeStepMS,
                    ref staticRects[i], out contactPoint, out contactNormal, out uHit);

                if (result)
                {
                    collided.Add(new Record((DateTime.Now - StartTime).TotalMilliseconds, contactPoint, contactNormal, uHit));
                    if (staticRectsColor.Length > 0) staticRectsColor[i] = (hit != null) ? hit.Value : Color.Red;
                }
                else
                {
                    if (staticRectsColor.Length > 0) staticRectsColor[i] = (hit != null) ? miss.Value : Color.Green;
                }
            }

            ResolveDynamicRectVsRect(ref velocity, ref collided);

            return collided.Count > 0;
        }
    }
}
