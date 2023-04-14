using Assimp;
using Assimp.Configs;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors;
using Microsoft.Xna.Framework.Graphics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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

    public class X_Light
    {
        public Vector3 Position { get; }
        public Vector3 PointTo { get; }
        public int Width { get; }
        public int Height { get; }
        public Vector3 Power { get; }

        public X_Light(Vector3 position, Vector3 pointTo, int width, int height, Vector3 power)
        {
            Position = position; // new Vector3(position.Y, position.X, position.Z);
            PointTo = pointTo; // new Vector3(pointTo.Y, pointTo.X, pointTo.Z);
            Width = width;
            Height = height;
            Power = power;
        }

    }

    public class X_Cube
    {
        Vector3[] _vertices;
        int[,] _triangles;

        public X_Cube(Vector3[] vertices, int[,] triangles)
        {
            _vertices = vertices;
            _triangles = triangles;
        }

        public int VertexCount()
        {
            return _vertices.Length;
        }

        public int TriangleCount()
        {
            return _triangles.GetLength(0);
        }

        public string toWavefrontObj(int offset)
        {
            string s = "";
            foreach(var v in _vertices)
            {
                s += ("v " + v.X + " " + v.Y + " " + v.Z + "\n");
            }
            s += "\n";
            for(int i=0; i<_triangles.GetLength(0); ++i)
            {
                s += ("f " + (_triangles[i,0]+1+offset) + " " + (_triangles[i, 1]+1+offset) + " " + (_triangles[i, 2]+1+offset) + "\n");
            }
            s += "\n";

            return s;
        }

        /*
         * Source: Nori
         */
        public bool RayIntersect(Vector3 origin, Vector3 direction, out Vector3 hitPoint)
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

                // remove backside
                Vector3 n = Manager_Light.CrossProduct(edge1, edge2);
                n.Normalize();
                if (Manager_Light.Dot(direction, n) > float.Epsilon)
                    continue;

                // Begin calculating determinant - also used to calculate U parameter
                Vector3 pvec = Manager_Light.CrossProduct(direction, edge2);

                // If determinant is near zero, ray lies in plane of triangle
                float det = Manager_Light.Dot(edge1, pvec);

                if (det > -float.Epsilon && det < float.Epsilon)
                    continue;
                    //return false;
                float invDet = 1.0f / det;

                // Calculate distance from v[0] to ray origin
                Vector3 tvec = origin - p0;

                // Calculate U parameter and test bounds
                float u = Manager_Light.Dot(tvec, pvec) * invDet;
                if (u < 0.0 || u > 1.0)
                    continue;
                    //return false;

                // Prepare to test V parameter
                Vector3 qvec = Manager_Light.CrossProduct(tvec, edge1);

                // Calculate V parameter and test bounds
                float v = Manager_Light.Dot(direction, qvec) * invDet;
                if (v < 0.0 || u + v > 1.0)
                    continue;
                    //return false;

                // Ray intersects triangle -> compute t
                float t = Manager_Light.Dot(edge2, qvec) * invDet;

                if (t >= float.Epsilon && t <= length) {
                    float w = 1.0f - u - v;
                    hitPoint = p0 * w + p1 * u + p2 * v;
                    return true;
                }
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
            hitPoint = new Vector3(0, 0, 0);
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

        public static void Illuminate(
            X_Light light,
            IWalkable room
        )
        {
            Vector3 lightDirection = light.PointTo - light.Position;
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
            R.M41 = light.Position.X;
            R.M42 = light.Position.Y;
            R.M43 = light.Position.Z;
            R.M44 = 1;

            var cubes = Manager_Light.Elevate(room);
            var wallCubes = Manager_Light.CreateWall(room);

            //int xpos = -light.Width / 2;
            //int ypos = -light.Height / 2;
            //int xend = -xpos;
            //int yend = -ypos;

            float p = 0.5f;

            //if(color != Color.Transparent)
            //{
            //    color.R = (byte)(p * lightPower.X + (1 - p) * color.R);
            //    color.G = (byte)(p * lightPower.Y + (1 - p) * color.G);
            //    color.B = (byte)(p * lightPower.Z + (1 - p) * color.B);
            //    data[h * texture.Width + w] = color;
            //}
            var floor = room.Collision.CreateFloorRectangles(room.TextureTileSize);


            var walls = room.Collision.CreateWallRectangles(room.TextureTileSize);

            //for (int x = xpos; x < xend; ++x)
            //{
            //    for (int y = ypos; y < yend; ++y)
            //    {

            //Vector4 origin4 = new Vector4(0, 0, 0, 1);
            //origin4 = Vector4.Transform(origin4, R);
            //Vector3 origin = new Vector3(origin4.X, origin4.Y, origin4.Z);

            Vector3 origin = light.Position; // new Vector3(-100, 50, 100);
            float shadowP = 0.8f;
            Texture2D texture = room.GetFloor();
            Color[] data = new Color[texture.Width * texture.Height];
            float[] shadow = Enumerable.Repeat<float>(0.0f, data.Length).ToArray();

            bool[] shadowMap = Enumerable.Repeat<bool>(true, data.Length).ToArray();
            Vector3[] floorCoords = Enumerable.Repeat<Vector3>(Vector3.Zero, data.Length).ToArray();
            Vector3[] wallCoords = Enumerable.Repeat<Vector3>(Vector3.Zero, data.Length).ToArray();
            int[] textureMapFloor = Enumerable.Repeat<int>(0, data.Length).ToArray();
            int[] textureMapWall = Enumerable.Repeat<int>(0, data.Length).ToArray();
            X_TileType[] tileType = new X_TileType[data.Length];
            var template = room.Collision.GetCollisionTemplate();
            
            var tileSize = room.TextureTileSize;
            for(int h=0; h < template.Length; ++h)
            {
                for (int w = 0; w < template[0].Length; ++w)
                {
                    int fromX = w*tileSize; // wall.X
                    int fromY = h* tileSize; // wall.Y
                    int toX = fromX + tileSize;
                    int toY = fromY + tileSize;
                    for (int x = fromX; x < toX; ++x)
                    {
                        for (int y = fromY; y < toY; ++y)
                        {
                            if (template[h][w] > 0) // == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
                            {
                                floorCoords[y * texture.Width + x] = new Vector3(x, y, -room.TextureTileSize - 0.1f);
                                textureMapFloor[y * texture.Width + x] = (y + tileSize) * texture.Width + x;
                                tileType[y * texture.Width + x] = (X_TileType)template[h][w];
                                //shadowMap[y * texture.Width + x] = true;
                            }
                            //if (template[h][w] > 0) //template[h][w] == (int)X_TileType.Wall)
                            //{
                            //    floorCoords[y * texture.Width + x] = new Vector3(x, fromY - 0.1f, -(y - fromY));
                            //    textureMapFloor[y * texture.Width + x] = (y) * texture.Width + x;
                            //    //shadowMap[y * texture.Width + x] = true;
                            //}
                            if(template[h][w] == (int)X_TileType.Roof)
                            {
                                shadowMap[y * texture.Width + x] = false;
                            }
                            //if (template[h][w] == (int)X_TileType.Wall)
                            //{
                            //    //coords[y * texture.Width + x] = new Vector3(x, fromY - 0.1f, -(y - fromY));
                            //    //shadowMap[y * texture.Width + x] = true;
                            //}
                            //if (template[h][w] == (int)X_TileType.Roof)
                            //{
                            //    coords[y * texture.Width + x] = new Vector3(x, y, 0);
                            //    shadowMap[y * texture.Width + x] = true;
                            //    textureMapFloor[y * texture.Width + x] = y * texture.Width + x;
                            //}
                        }
                    }
                }
            }

            //foreach (var row in floor.Take(floor.Count - 1))
            //{
            //    foreach (var floorRect in row)
            //    {
            //        int fromX = floorRect.X;
            //        int fromY = floorRect.Y;
            //        int toX = fromX + floorRect.Width;
            //        int toY = fromY + floorRect.Height;
            //        for (int h = fromY; h < toY; ++h)
            //        {
            //            for (int w = fromX; w < toX; ++w)
            //            {
            //                Vector3 target = new Vector3(w, h, -room.TextureTileSize - 0.1f)

            //foreach (var row in walls)
            //{
            //    foreach (var wall in row)
            //    {
            //        int minx = int.MaxValue;
            //        int maxx = int.MinValue;
            //        int miny = int.MaxValue;
            //        int maxy = int.MinValue;
            //        int offsetH = wall.Y;
            //        int offsetW = wall.X;
            //        int inset = 1;
            //        bool[,] mask = new bool[wall.Height + 2*inset, wall.Width + 2*inset];

                            //        for (int w = offsetW; w <= wall.Width + offsetW; ++w)
                            //        {
                            //            for (int h = 0; h <= wall.Height; ++h)
                            //            {
                            //                foreach (var cube in wallCubes)
                            //                {
                            //                    if (cube.RayIntersect(orig, (new Vector3(w, offsetH - 0.1f, -h) - orig), out hitPoint))
                            //                    {

            texture.GetData<Color>(data);
            Vector3 orig = new Vector3(12*16, (int)(7.5*16), 50);
            Vector3 hitPoint;
            for (int i=0; i<data.Length; ++i)
            {
                //if (shadowMap[i] == true)
                //{
                //    continue;
                //    //data[i] = Color.White;
                //}
                //else
                //{

                foreach (var cube in cubes)
                {
                    if (cube.RayIntersect(orig, floorCoords[i] - orig, out hitPoint))
                    {
                        int hitH = (int)(hitPoint.Y + Math.Abs(hitPoint.Z));
                        int hitW = (int)(hitPoint.X);
                        int hit = hitH * texture.Width + hitW;
                        int hit2 = textureMapFloor[i];

                        //if (tileType[hit] == X_TileType.Wall) data[hit] = Color.Blue;

                        //if (shadowMap[textureMapFloor[i]]) data[textureMapFloor[i]] = Color.Red;
                        if (shadowMap[hit2] && tileType[hit2] != X_TileType.Wall)
                        {
                            data[hit2] = Color.Red;
                            break;
                            // we hit the floor
                        }
                    }
                }




                //if (!intersectFloor && shadowMap[textureMapFloor[i]]) data[textureMapFloor[i]] = Color.Red;
                //if (!intersectWall && shadowMap[textureMapWall[i]]) data[textureMapWall[i]] = Color.Blue;
                //}
            }
            texture.SetData<Color>(data);
            return;

            //for (int i = 1; i < 150; ++i)
            //{
            //    for (int j = 0; j <= 16; ++j)
            //    {
            //        foreach (var cube in wallCubes)
            //        {
            //            Vector3 orig = new Vector3(i, 50, -j);
            //            if (cube.RayIntersect(orig, new Vector3(i, -200, -j) - orig, out hitPoint))
            //            {
            //                // we hit the floor
            //                data[(int)(hitPoint.Y + Math.Abs(hitPoint.Z)) * width + (int)hitPoint.X] = Color.Red;
            //            }
            //        }
            //    }
            //}

            int width = texture.Width;
            int height = texture.Height;
            //Vector3 hitPoint;
            foreach (var row in floor.Take(floor.Count - 1))
            {
                foreach (var floorRect in row)
                {
                    int fromX = floorRect.X;
                    int fromY = floorRect.Y;
                    int toX = fromX + floorRect.Width;
                    int toY = fromY + floorRect.Height;
                    for (int h = fromY; h < toY; ++h)
                    {
                        for (int w = fromX; w < toX; ++w)
                        {
                            Vector3 target = new Vector3(w, h, -room.TextureTileSize - 0.1f);
                            foreach (var cube in cubes)
                            {
                                if (cube.RayIntersect(origin, target - origin, out hitPoint))
                                {
                                    // we hit the floor
                                    int index = (h + room.TextureTileSize) * width + w;
                                    //int index = (h) * width + w;
                                    if (index < shadow.Length)
                                        shadow[index] = shadowP;
                                    //data[(h + room.TextureTileSize) * width + w] = Color.Black;
                                }
                            }
                        }
                    }
                }
            }

            /**
             * Test calculating the precise vector and then check intersection with frame
             */
            int counter = 0;
            foreach (var row in walls)
            {
                foreach (var wall in row)
                {
                    //int minx = int.MaxValue;
                    //int maxx = int.MinValue;
                    //int miny = int.MaxValue;
                    //int maxy = int.MinValue;
                    int offsetH = wall.Y;
                    int offsetW = wall.X;
                    int inset = 1;
                    bool[,] mask = new bool[wall.Height + 2 * inset, wall.Width + 2 * inset];

                    for (int w = offsetW; w <= wall.Width + offsetW; ++w)
                    {
                        for (int h = 0; h <= wall.Height; ++h)
                        {
                            foreach (var wc in wallCubes)
                            {
                                if (wc.RayIntersect(origin, (new Vector3(w, offsetH - 0.1f, -h) - origin), out hitPoint))
                                {
                                    foreach (var cube in cubes)
                                    {
                                        Vector3 hitPoint2;
                                        if (cube.RayIntersect(origin, hitPoint - origin, out hitPoint2))
                                        {
                                            int hitH = (int)(hitPoint.Y + Math.Abs(hitPoint.Z));
                                            int hitW = (int)(hitPoint.X);
                                            //data[hitH * width + hitW] = Color.Red;
                                            //if (!mask[hitH - offsetH + inset, hitW - offsetW + inset])
                                            //{
                                                int index = hitH * width + hitW;
                                                if (index < shadow.Length)
                                                    shadow[index] = shadowP;
                                            //}
                                            // we hit the floor
                                            //data[(h + room.TextureTileSize) * width + w] = Color.Black;

                                            //if (minx > hitW) minx = hitW;
                                            //if (maxx < hitW) maxx = hitW;
                                            //if (miny > hitH) miny = hitH;
                                            //if (maxy < hitH) maxy = hitH;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Logger.Info(" minx " + minx + "\t| maxx " + maxx + "\t| miny " + miny + "\t| maxy " + maxy + "\t| offsetW " + offsetW + "\t| offsetH " + offsetH);

                    //output(mask, "./logs/mask" + counter + ".csv");
                    //counter++;
                }

            }

            for(int i=0; i<shadow.Length; ++i)
            {
                if (shadow[i] > 0)
                {
                    data[i].R = (byte)(shadow[i] * Color.Black.R + (1 - shadow[i]) * data[i].R);
                    data[i].G = (byte)(shadow[i] * Color.Black.G + (1 - shadow[i]) * data[i].G);
                    data[i].B = (byte)(shadow[i] * Color.Black.B + (1 - shadow[i]) * data[i].B);
                }
            }

            ///**
            // * Test to calculate all dots on the wall from the wall boxes
            // */
            //Vector3 orig = new Vector3(100, 100, 100);
            //Vector3 orig = origin;

            //int counter = 0;
            //foreach (var row in walls)
            //{
            //    foreach (var wall in row)
            //    {
            //        int minx = int.MaxValue;
            //        int maxx = int.MinValue;
            //        int miny = int.MaxValue;
            //        int maxy = int.MinValue;
            //        int offsetH = wall.Y;
            //        int offsetW = wall.X;
            //        int inset = 1;
            //        bool[,] mask = new bool[wall.Height + 2*inset, wall.Width + 2*inset];

            //        for (int w = offsetW; w <= wall.Width + offsetW; ++w)
            //        {
            //            for (int h = 0; h <= wall.Height; ++h)
            //            {
            //                foreach (var cube in wallCubes)
            //                {
            //                    if (cube.RayIntersect(orig, (new Vector3(w, offsetH - 0.1f, -h) - orig), out hitPoint))
            //                    {
            //                        int hitH = (int)(hitPoint.Y + Math.Abs(hitPoint.Z));
            //                        int hitW = (int)(hitPoint.X);
            //                        //data[hitH * width + hitW] = Color.Red;
            //                        if (!mask[hitH - offsetH + inset, hitW - offsetW + inset])
            //                        {
            //                            mask[hitH - offsetH+inset, hitW - offsetW+inset] = true;
            //                            data[hitH * width + hitW] = Color.Red;
            //                        }

            //                        // we hit the floor
            //                        if (minx > hitW) minx = hitW;
            //                        if (maxx < hitW) maxx = hitW;
            //                        if (miny > hitH) miny = hitH;
            //                        if (maxy < hitH) maxy = hitH;
            //                    }
            //                }
            //            }
            //        }

            //        Logger.Info(" minx " + minx + "\t| maxx " + maxx + "\t| miny " + miny + "\t| maxy " + maxy + "\t| offsetW " + offsetW + "\t| offsetH " + offsetH);

            //        output(mask, "./logs/mask" + counter + ".csv");
            //        counter++;
            //    }

            //}


            ///**
            // * Test from some origin to all dots on the wall
            // */
            //Vector3 orig = new Vector3(100, 100, 100);
            //int minx = int.MaxValue;
            //int maxx = int.MinValue;
            //int miny = int.MaxValue;
            //int maxy = int.MinValue;
            //bool[,] mask = new bool[16, 80];

            //int offsetH = 32;
            //int offsetW = 32;
            //for (int i = 20; i <= 115; ++i)
            //{
            //    for (int j = -1; j <= 30; ++j)
            //    {
            //        foreach (var cube in wallCubes)
            //        {
            //            if (cube.RayIntersect(orig, (new Vector3(i, 29, -j) - orig), out hitPoint))
            //            {
            //                int hitH = (int)(hitPoint.Y + Math.Abs(hitPoint.Z));
            //                int hitW = (int)(hitPoint.X);

            //                if (!mask[hitH-offsetH, hitW-offsetW])
            //                {
            //                    mask[hitH - offsetH, hitW - offsetW] = true;
            //                    data[hitH * width + hitW] = Color.Red;
            //                }

            //            // we hit the floor
            //            if (minx > hitW) minx = hitW;
            //                if (maxx < hitW) maxx = hitW;
            //                if (miny > hitH) miny = hitH;
            //                if (maxy < hitH) maxy = hitH;
            //            }
            //        }
            //    }
            //}
            //Logger.Info(" minx " + minx + "\t| maxx " + maxx + "\t| miny " + miny + "\t| maxy " + maxy);

            //output(mask, "./logs/mask.csv");

            ///**
            // * Test to get all dots on the wall
            // */
            //int minx = int.MaxValue;
            //int maxx = int.MinValue;
            //int miny = int.MaxValue;
            //int maxy = int.MinValue;
            //foreach (var row in walls)
            //{
            //    foreach (var wall in row)
            //    {
            //        int fromX = wall.X;
            //        int fromY = wall.Y;
            //        int toX = fromX + wall.Width;
            //        int toY = fromY + wall.Height;
            //        int fromZ = 0;
            //        int toZ = wall.Height;
            //        if (minx > fromX) minx = fromX;
            //        if (maxx < toX) maxx = toX;
            //        if (miny > fromZ) miny = fromZ;
            //        if (maxy < toZ) maxy = toZ;
            //    }
            //}
            //Logger.Info(" minx " + minx + "\t| maxx " + maxx + "\t| miny " + miny + "\t| maxy " + maxy);

            //minx = int.MaxValue;
            //maxx = int.MinValue;
            //miny = int.MaxValue;
            //maxy = int.MinValue;
            //for (int i = 1; i < 150; ++i)
            //{
            //    for (int j = 0; j <= 16; ++j)
            //    {
            //        foreach (var cube in wallCubes)
            //        {
            //            Vector3 orig = new Vector3(i, 50, -j);
            //            if (cube.RayIntersect(orig, new Vector3(i, -200, -j) - orig, out hitPoint))
            //            {
            //                // we hit the floor
            //                data[(int)(hitPoint.Y + Math.Abs(hitPoint.Z)) * width + (int)hitPoint.X] = Color.Red;
            //                if (minx > i) minx = i;
            //                if (maxx < i) maxx = i;
            //                if (miny > j) miny = j;
            //                if (maxy < j) maxy = j;
            //            }
            //        }
            //    }
            //}

            //Logger.Info(" minx " + minx + "\t| maxx " + maxx + "\t| miny " + miny + "\t| maxy " + maxy);

            texture.SetData<Color>(data);
        }

        private static void output(bool[,] pattern, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.GetLength(0); ++x)
            {
                s += string.Join("\t", Enumerable.Range(0, pattern.GetLength(1)).Select(y => pattern[x, y]).ToArray());
                s += "\n";
            }

            File.WriteAllText(name, s);
        }

        private static void output(int[][] pattern, string name)
        {
            string s = "";
            for (int x = 0; x < pattern.GetLength(0); ++x)
            {
                s += string.Join("\t", pattern[x]);
                s += "\n";
            }

            File.WriteAllText(name, s);
        }

        public static List<X_Cube> Elevate(IWalkable room)
        {
            List<X_Cube> cubes = new List<X_Cube>();
            var rects = room.Collision.GetCollisionRectangles().Clone() as Rectangle[];

            int[,] indices = new int[,]
            {
                /* bottom */ {0,1,2}, {0,2,3},
                /* right  */ {2,6,3}, {3,6,7},
                /* front  */ {1,5,2}, {2,5,6},
                /* left   */ {0,4,1}, {1,4,5},
                /* back   */ {0,3,7}, {0,7,4},
                /* top    */ {5,4,6}, {6,4,7}
            };

            float scale = room.Scale;
            float elev = room.Collision.TileHeight / scale;
            foreach (var rect in rects)
            {
                float x = rect.X / scale;
                float y = rect.Y / scale;
                float h = rect.Height / scale;
                float w = rect.Width / scale;
                float e = elev;
                Vector3[] vertices = new Vector3[] {
                    new Vector3(x, y, -e),
                    new Vector3(x, y+h, -e),
                    new Vector3(x+w, y+h, -e),
                    new Vector3(x+w, y, -e),
                    new Vector3(x, y, 0),
                    new Vector3(x, y+h, 0),
                    new Vector3(x+w, y+h, 0),
                    new Vector3(x+w, y, 0)
                };

                cubes.Add(new X_Cube(vertices, indices));
            }

            toWavefrontObj(cubes, "./logs/cubes.obj");
            //int[,] indicesF = new int[,]
            //{
            //    /* bottom */ {0,2,1}, {0,3,2},
            //};

            //List<X_Cube> tiles = new List<X_Cube>();
            //int tileSize = room.TextureTileSize;
            //var floor = room.Collision.CreateFloorRectangles(tileSize);
            //int minX = int.MaxValue;
            //int minY = int.MaxValue;
            //int maxX = int.MinValue;
            //foreach (var row in floor)
            //{
            //    foreach(var rect in row)
            //    {
            //        float x = rect.X;
            //        float y = rect.Y;
            //        float h = rect.Height;
            //        float w = rect.Width;
            //        if (x < minX) minX = (int)x;
            //        if (y < minY) minY = (int)y;
            //        if (x > maxX) maxX = (int)x;
            //        Vector3[] vertices = new Vector3[] {
            //        new Vector3(x, y, 0),
            //        new Vector3(x, y+h, 0),
            //        new Vector3(x+w, y+h, 0),
            //        new Vector3(x+w, y, 0)
            //    };

            //        cubes.Add(new X_Cube(vertices, indicesF));
            //    }
            //}

            //toWavefrontObj(cubes, "./logs/cubes.obj");

            //List<X_Cube> tiles = new List<X_Cube>();
            //for (int x=minX; x<maxX+tileSize/2; x+=tileSize)
            //{
            //    Vector3[] vertices = new Vector3[] {
            //        new Vector3(x, minY, tileSize),
            //        new Vector3(x, minY, 0),
            //        new Vector3(x+tileSize, minY, 0),
            //        new Vector3(x+tileSize, minY, tileSize)
            //    };

            //    tiles.Add(new X_Cube(vertices, indicesF));
            //}

            //toWavefrontObj(tiles, "./logs/tiles.obj");

            return cubes;
        }

        public static List<X_Cube> CreateWall(IWalkable room)
        {
            int[,] indicesF = new int[,]
            {
                /* bottom */ {0,2,1}, {0,3,2},
            };

            var walls = room.Collision.CreateWallRectangles(room.TextureTileSize);
            List<X_Cube> tiles = new List<X_Cube>();
            int e = room.TextureTileSize;
            foreach (var row in walls) { 
                foreach(var rect in row)
                {
                    Vector3[] vertices = new Vector3[] {
                    new Vector3(rect.X, rect.Y, -float.Epsilon),
                    new Vector3(rect.X, rect.Y, -e-float.Epsilon),
                    new Vector3(rect.X+room.TextureTileSize, rect.Y, -e-float.Epsilon),
                    new Vector3(rect.X+room.TextureTileSize, rect.Y, -float.Epsilon) };

                    tiles.Add(new X_Cube(vertices, indicesF));
                }
            }
            toWavefrontObj(tiles, "./logs/tiles.obj");
            return tiles;
        }

        private static void toWavefrontObj(List<X_Cube> cubes, string name)
        {
            string s = "";
            int i = 0;
            int offset = 0;
            foreach (var cube in cubes)
            {
                s += "o cube" + i + "\n";
                s += (cube.toWavefrontObj(offset));
                offset += cube.VertexCount();
                i++;
            }

            File.WriteAllText(name, s);
        }
    }
}
