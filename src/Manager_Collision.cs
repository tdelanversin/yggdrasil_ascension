using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    // Implement these kinds of things:
    // https://github.com/OneLoneCoder/Javidx9/blob/master/PixelGameEngine/SmallerProjects/OneLoneCoder_PGE_Rectangles.cpp

    // Youtube tutorial
    // https://www.youtube.com/watch?v=8JJ-4JgR7Dg

    public static class Manager_Collision
    {
        public static bool PointVsRect(Vector2 p, Rectangle rect)
        {
            return rect.Contains(p);
        }

        public static bool RectVsRect(Rectangle rect1, Rectangle rect2)
        {
            return rect1.Intersects(rect2);
        }

        public static bool RayVsRect(
            Point origin, 
            Vector2 dir, 
            Rectangle target, 
            out Point contactPoint,
            out Vector2 contactNormal,
            out float u
        )
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            u = 0.0f;

            // preassign stuff
            Vector2 vContactPoint = Vector2.Zero;
            Vector2 rayOrigin = new Vector2(origin.X, origin.Y);
            Vector2 invDir = new Vector2(1.0f / dir.X, 1.0f / dir.Y);
            Vector2 targetPos = new Vector2(target.Location.X, target.Location.Y);
            Vector2 targetSize = new Vector2(target.Width, target.Height);

            // calculate intersections ray to rectangle
            Vector2 tNear = (targetPos - rayOrigin) * invDir;
            Vector2 tFar = (targetPos + targetSize - rayOrigin) * invDir;

            // sort distances => swap variables without temp var
            if (tNear.X > tFar.X) (tNear.X, tFar.X) = (tFar.X, tNear.X);
            if (tNear.Y > tFar.Y) (tNear.Y, tFar.Y) = (tFar.Y, tNear.Y);

            // early rejection
            if (tNear.X > tFar.Y || tNear.Y > tFar.X) return false;

            return true;
        }
    }
}
