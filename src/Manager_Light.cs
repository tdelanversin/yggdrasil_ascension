using Assimp;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace YGR
{
    public class X_Light
    {
        public Vector3 Position { get; }
        public Color Color { get; }
        public float ShadowP { get; }
        public float Scale { get; }
        public Rectangle IlluminationRect { get; }

        public X_Light(Vector3 position, Rectangle illuminationRect, Color color, float shadowP, float scale)
        {
            Position = position/scale; // new Vector3(position.Y, position.X, position.Z);
            Color = color;
            ShadowP = shadowP;
            Scale = scale;
            IlluminationRect = illuminationRect;
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            int xpos = (int)(Position.X * Scale);
            int ypos = (int)(Position.Y * Scale - Position.Z*Scale);
            Factory_Debug.DrawPoint(xpos, ypos, 10, Color.Yellow, spriteBatch);
            Factory_Debug.DrawLine(xpos, ypos, (int)(Position.Z * Scale), (float)Math.PI/2, 5, Color.Yellow, spriteBatch);
            Factory_Debug.DrawRectangle(
                (int)(IlluminationRect.X),
                (int)(IlluminationRect.Y),
                (int)(IlluminationRect.Width),
                (int)(IlluminationRect.Height), 5, Color.Orange, spriteBatch);
        }

        public string GetIdentifier(IWalkable room)
        {
            Rectangle rect = room.Rect;
            float px = Position.X - rect.X;
            float py = Position.Y - rect.Y;
            int ipx = IlluminationRect.X - rect.X;
            int ipy = IlluminationRect.Y - rect.Y;
            int ipw = IlluminationRect.Width;
            int iph = IlluminationRect.Height;
            float scale = room.Scale;
            return "PX" + px + "PY" + py + "IRX" + ipx + "IRY" + ipy + "IRW" + ipw + "IRH" + iph + "S" + scale;
        }
    }
    public class X_Cube
    {
        Vector3[] _vertices;
        int[,] _triangles;
        public bool IsWall { get; }
        public X_Cube(Vector3[] vertices, int[,] triangles, bool isWall)
        {
            _vertices = vertices;
            _triangles = triangles;
            IsWall = isWall;
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
                    //hitPoint = origin + t * direction;
                    return true;
                }
            }

            //hitPoint = new Vector3(0, 0, 0);
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
            List<X_Light> lights,
            IWalkable room,
            List<X_Cube> cubes
        )
        {
            Texture2D texture = room.GetFloor();
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData<Color>(data);
            bool[] lighted = Enumerable.Repeat<bool>(false, data.Length).ToArray();

            Vector3 offset = new Vector3(room.Rect.Location.X / room.Scale, room.Rect.Location.Y / room.Scale, 0);

            bool[] shadowMap = Enumerable.Repeat<bool>(true, data.Length).ToArray();
            Vector3[] coords = Enumerable.Repeat<Vector3>(Vector3.Zero, data.Length).ToArray();
            int[] textureMap = Enumerable.Repeat<int>(-1, data.Length).ToArray();
            X_TileType[] tileType = Enumerable.Repeat<X_TileType>(X_TileType.Floor, data.Length).ToArray();
            var template = room.Collision.GetCollisionTemplate();
            var tileSize = room.TextureTileSize;

            int shadowSpotSize = 1;

            Parallel.For(0, template.Length, h =>
            //for (int h=0; h < template.Length; ++h)
            {
                for (int w = 0; w < template[0].Length; ++w)
                {
                    int fromX = w * tileSize + shadowSpotSize / 2;
                    int fromY = h * tileSize + shadowSpotSize / 2;
                    int toX = fromX + tileSize - shadowSpotSize / 2;
                    int toY = fromY + tileSize - shadowSpotSize / 2;
                    for (int x = fromX; x < toX; x += shadowSpotSize)
                    {
                        for (int y = fromY; y < toY; y += shadowSpotSize)
                        {
                            tileType[y * texture.Width + x] = (X_TileType)template[h][w];
                            if (template[h][w] == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
                            {
                                coords[y * texture.Width + x] = new Vector3(x, y - room.TextureTileSize, -room.TextureTileSize - 0.001f) + offset;
                                textureMap[y * texture.Width + x] = (y) * texture.Width + x;
                                //tileType[y * texture.Width + x] = (X_TileType)template[h][w];
                            }
                            else if (template[h][w] == (int)X_TileType.Wall)
                            {
                                coords[y * texture.Width + x] = new Vector3(x, fromY + 0.001f, -(y - fromY)) + offset;
                                textureMap[y * texture.Width + x] = (y) * texture.Width + x;
                                //tileType[y * texture.Width + x] = (X_TileType)template[h][w];
                            }
                            if (template[h][w] == (int)X_TileType.Roof || template[h][w] == (int)X_TileType.Outside)
                            {
                                shadowMap[y * texture.Width + x] = false;
                            }
                        }
                    }
                }
            });

            foreach (var light in lights)
            {
                Vector3 orig = light.Position;
                float scale = room.Scale;
                Parallel.For(0, data.Length, i =>
                //for (int i = 0; i < data.Length; ++i)
                {
                    if (!light.IlluminationRect.Contains(new Point((int)((coords[i].X) * scale), (int)((coords[i].Y) * scale)))) 
                        return;

                    int hit2 = textureMap[i];
                    if (hit2 < 0) return;

                    if (lighted[hit2]) return;

                    var sm = shadowMap[hit2];
                    if (!sm) return;

                    //var tt = tileType[hit2];
                    bool intersected = false;
                    foreach (var cube in cubes)
                    {
                        if (cube.RayIntersect(orig, coords[i] - orig))
                        {
                            intersected = true;
                            break;
                        }
                    }
                    if (!intersected)
                    {
                        lighted[hit2] = true;
                    }
                });
            }

            for (int i = 0; i < lighted.Length; ++i)
            {
                if (!lighted[i] && shadowMap[i] )
                {
                    var col = data[i];
                    Color nCol = Color.White;
                    nCol.R = (byte)((1 - 0.4f) * col.R + 0.4f * Color.Black.R);
                    nCol.G = (byte)((1 - 0.4f) * col.G + 0.4f * Color.Black.G);
                    nCol.B = (byte)((1 - 0.4f) * col.B + 0.4f * Color.Black.B);
                    data[i] = nCol;
                }
            }

            saveShadeToFile(lighted, room, lights);

            texture.SetData<Color>(data);
        }

        private static void saveShadeToFile(bool[] shadeTemplate, IWalkable room, List<X_Light> lights)
        {
            if (room.ResourceFolder != "")
            {
                string fileName = "shade_";
                string identifier = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + "\n";
                identifier += "V2\n";
                foreach (var light in lights)
                {
                    if (light.IlluminationRect.Intersects(room.Rect))
                    {
                        identifier += light.GetIdentifier(room) + "|";
                    }
                }
                if(identifier.EndsWith("|"))
                    identifier = identifier.Substring(0, identifier.Length - 1);
                identifier += "\n";
                identifier += string.Join("", shadeTemplate.Select(x => x ? "1" : "0"));

                var srcPath = Util.GetAbsResourceFolderPath(room.ResourceFolder);
                fileName += Util.CreateGenericIdentifier();

                // write to all available directories: current runtime directory and source code directory
                File.WriteAllText(room.ResourceFolder + fileName, identifier);
                File.WriteAllText(srcPath + fileName, identifier);
            }
        }

        //private static bool[] loadShadeFromFile()
        //{

        //}

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

        public static List<X_Cube> Elevate(Y_Level level)
        {
            List<X_Cube> cubes = new List<X_Cube>();

            foreach(var room in level.Rooms.Values)
            {
                var rects = room.Collision.GetCollisionRectangles().Clone() as Rectangle[];

                int[,] indicesRoof = new int[,]
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
                int shift = 5;
                foreach (var rect in rects)
                {
                    float x = (rect.X - shift + 0.5f) / scale;
                    float y = (rect.Y - shift) / scale;
                    float h = (rect.Height + shift) / scale;
                    float w = (rect.Width + shift) / scale;
                    float e = elev + 1;
                    Vector3[] vertices = new Vector3[] {
                    new Vector3(x, y, -e),
                    new Vector3(x, y+h, -e),
                    new Vector3(x+w, y+h, -e),
                    new Vector3(x+w, y, -e),
                    new Vector3(x, y, shift),
                    new Vector3(x, y+h, shift),
                    new Vector3(x+w, y+h, shift),
                    new Vector3(x+w, y, shift)
                };

                    cubes.Add(new X_Cube(vertices, indicesRoof, false));
                }
            }

            //toWavefrontObj(cubes, "./logs/cubes.obj");
            return cubes;
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
