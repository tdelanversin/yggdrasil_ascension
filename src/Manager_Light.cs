using Assimp;
using Assimp.Configs;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    //public struct X_Vertex
    //{
    //    public int X;
    //    public int Y;
    //    public int Z;
    //    public X_Vertex(int x, int y, int z)
    //    {
    //        X = x; Y = y; Z = z;
    //    }
    //}

    public class X_Cube
    {
        Vector3[] _vertices;
        int[,] _triangles;

        public X_Cube(Vector3[] vertices, int[,] triangles)
        {
            _vertices = vertices;
            _triangles = triangles;
        }

        /*
         * Source: Nori
         */
        public bool RayIntersect(Vector3 origin, Vector3 direction)
        {
            float length = direction.Length();
            direction.Normalize();

            int len = _triangles.GetLength(0);
            for (int ind=0; ind < len; ++ind)
            {
                Vector3 p0 = _vertices[_triangles[ind, 0]];
                Vector3 p1 = _vertices[_triangles[ind, 1]];
                Vector3 p2 = _vertices[_triangles[ind, 2]];

                // Find vectors for two edges sharing v[0]
                Vector3 edge1 = p1 - p0; Vector3 edge2 = p2 - p0;

                // Begin calculating determinant - also used to calculate U parameter
                Vector3 pvec = Manager_Light.CrossProduct(direction, edge2);

                // If determinant is near zero, ray lies in plane of triangle
                float det = Manager_Light.Dot(edge1, pvec);

                if (det > -float.Epsilon && det < float.Epsilon)
                    return false;
                float invDet = 1.0f / det;

                // Calculate distance from v[0] to ray origin
                Vector3 tvec = origin - p0;

                // Calculate U parameter and test bounds
                float u = Manager_Light.Dot(tvec, pvec) * invDet;
                if (u < 0.0 || u > 1.0)
                    return false;

                // Prepare to test V parameter
                Vector3 qvec = Manager_Light.CrossProduct(tvec, edge1);

                // Calculate V parameter and test bounds
                float v = Manager_Light.Dot(direction, qvec) * invDet;
                if (v < 0.0 || u + v > 1.0)
                    return false;

                // Ray intersects triangle -> compute t
                float t = Manager_Light.Dot(edge2, qvec) * invDet;

                return t >= float.Epsilon && t <= length;
            }
            /*
            uint32_t i0 = m_F(0, index), i1 = m_F(1, index), i2 = m_F(2, index);
            const Point3f p0 = m_V.col(i0), p1 = m_V.col(i1), p2 = m_V.col(i2);

            // Find vectors for two edges sharing v[0]
            Vector3f edge1 = p1 - p0, edge2 = p2 - p0;

            // Begin calculating determinant - also used to calculate U parameter
            Vector3f pvec = ray.d.cross(edge2);

            // If determinant is near zero, ray lies in plane of triangle
            float det = edge1.dot(pvec);

            if (det > -1e-8f && det < 1e-8f)
                return false;
            float inv_det = 1.0f / det;

            // Calculate distance from v[0] to ray origin
            Vector3f tvec = ray.o - p0;

            // Calculate U parameter and test bounds
            u = tvec.dot(pvec) * inv_det;
            if (u < 0.0 || u > 1.0)
                return false;

            // Prepare to test V parameter
            Vector3f qvec = tvec.cross(edge1);

            // Calculate V parameter and test bounds
            v = ray.d.dot(qvec) * inv_det;
            if (v < 0.0 || u + v > 1.0)
                return false;

            // Ray intersects triangle -> compute t
            t = edge2.dot(qvec) * inv_det;

            return t >= ray.mint && t <= ray.maxt;
            */

            return false;
        }
    }

    public static class Manager_Light
    {
        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
        }

        public static Vector3 CrossProduct(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.X * rhs.Y - lhs.Y * rhs.X
                );
        }

        public static void Illuminate(Vector3 lightPosition, Vector3 pointTo, int lightWidth, int lightHeight, Color power)
        {
            Vector3 lightDirection = pointTo - lightPosition;
            // https://math.stackexchange.com/questions/180418/calculate-rotation-matrix-to-align-vector-a-to-vector-b-in-3d
            Vector3 n = new Vector3(0, 0, -1);
            lightDirection.Normalize();

            Vector3 v = Manager_Light.CrossProduct(n, lightDirection);
            Matrix vx = new Matrix(
                new Vector4(0, -v.Z, v.Y, 0),
                new Vector4(v.Z, 0, -v.X, 0),
                new Vector4(-v.Y, v.X, 0, 0),
                new Vector4(0, 0, 0, 1)
            );
            Matrix vx2 = Matrix.Multiply(vx, vx);
            float c = Manager_Light.Dot(n, lightDirection);

            Matrix R = Matrix.Identity + vx + vx2 * (1 / (1 + c));
            R.M14 = lightPosition.X;
            R.M24 = lightPosition.Y;
            R.M34 = lightPosition.Z;
            R.M44 = 1;

            int xpos = -lightWidth / 2;
            int ypos = -lightHeight / 2;
            int xend = -xpos;
            int yend = -ypos;
            for (int x=xpos; x<xend; ++x)
            {
                for(int y=ypos; y<yend; ++y)
                {
                    Vector3 origin = new Vector3(x + lightPosition.X, x + lightPosition.Y, lightPosition.Z);
                }
            }
        }

        public static List<X_Cube> Elevate(IWalkable room, int elevation)
        {
            List<X_Cube> cubes = new List<X_Cube>();
            var rects = room.Collision.GetCollisionRectangles().Clone() as Rectangle[];

            int[,] indices = new int[,]
            {
                /* bottom */ {0,1,2}, {0,2,3},
                /* right  */ {2,3,6}, {3,7,6},
                /* front  */ {1,2,5}, {2,6,5},
                /* left   */ {0,1,5}, {0,5,4},
                /* back   */ {3,0,7}, {0,4,7},
                /* top    */ {5,6,4}, {6,7,4}
            };

            foreach(var rect in rects)
            {
                Vector3[] vertices = new Vector3[] {
                    new Vector3(rect.X, rect.Y, 0),
                    new Vector3(rect.X, rect.Y + rect.Height, 0),
                    new Vector3(rect.X + rect.Width, rect.Y + rect.Height, 0),
                    new Vector3(rect.X + rect.Height, rect.Y, 0),
                    new Vector3(rect.X, rect.Y, elevation),
                    new Vector3(rect.X, rect.Y + rect.Height, elevation),
                    new Vector3(rect.X + rect.Width, rect.Y + rect.Height, elevation),
                    new Vector3(rect.X + rect.Height, rect.Y, elevation)
                };

                cubes.Add(new X_Cube(vertices, indices));
            }

            return cubes;
        }
    }
}
