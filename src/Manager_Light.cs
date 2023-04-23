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
        public Color Color { get; }
        public float ShadowP { get; }
        public float Scale { get; }

        private Rectangle _illuminationRect;
        private Rectangle _scaledIlluminationRect;
        private Vector3 _position;
        private Vector3 _scaledPosition;

        public X_Light(Vector3 position, Rectangle illuminationRect, float scale)
        {
            _position = position/scale; // new Vector3(position.Y, position.X, position.Z);
            _scaledPosition = position;
            Scale = scale;
            _illuminationRect = new Rectangle(
                (int)(illuminationRect.X/scale), 
                (int)(illuminationRect.Y/scale), 
                (int)(illuminationRect.Width/scale), 
                (int)(illuminationRect.Height/scale));

            _scaledIlluminationRect = illuminationRect;
        }

        public Vector3 GetScaledPosition()
        {
            return _scaledPosition;
        }

        public Vector3 GetUnscaledPosition()
        {
            return _position;
        }

        public ref Rectangle GetScaledIlluminationRect()
        {
            return ref _scaledIlluminationRect;
        }

        public ref Rectangle GetUnscaledIlluminationRect()
        {
            return ref _illuminationRect;
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            int xpos = (int)(_scaledPosition.X);
            int ypos = (int)(_scaledPosition.Y - _scaledPosition.Z);
            int zpos = (int)(_scaledPosition.Z);
            Factory_Debug.DrawPoint(xpos, ypos, 10, Color.Yellow, spriteBatch);
            Factory_Debug.DrawLine(xpos, ypos, zpos, (float)Math.PI/2, 5, Color.Yellow, spriteBatch);
            Factory_Debug.DrawRectangle(
                (int)(_scaledIlluminationRect.X),
                (int)(_scaledIlluminationRect.Y),
                (int)(_scaledIlluminationRect.Width),
                (int)(_scaledIlluminationRect.Height), 5, Color.Orange, spriteBatch);
        }

        public string GetIdentifier(IWalkable room)
        {
            Rectangle rect = room.Rect;
            float px = _scaledPosition.X - rect.X;
            float py = _scaledPosition.Y - rect.Y;
            int ipx = _scaledIlluminationRect.X - rect.X;
            int ipy = _scaledIlluminationRect.Y - rect.Y;
            int ipw = _scaledIlluminationRect.Width;
            int iph = _scaledIlluminationRect.Height;
            float scale = room.Scale;
            return "PX" + px + "PY" + py + "IRX" + ipx + "IRY" + ipy + "IRW" + ipw + "IRH" + iph + "S" + scale;
        }

        public void MoveBy(Point dp)
        {
            var dp3 = new Vector3(dp.X, dp.Y, 0);
            var dp3s = dp3 / Scale;
            var dp2s = new Point((int)(dp.X / Scale), (int)(dp.Y / Scale));
            _position += dp3s;
            _scaledPosition += dp3;
            _illuminationRect.Offset(dp2s);
            _scaledIlluminationRect.Offset(dp);
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
        public enum Caster
        {
            Light=0,
            Shadow
        }

        public static List<X_Cube> IlluminationModelOpened { get; private set; }
        public static List<X_Cube> IlluminationModelClosed { get; private set; }

        public static string ShadeVersion { get { return "V3"; } }

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

        public static bool[] Illuminate(
            List<X_Light> lights,
            //IWalkable room,
            int[][] collisionTemplate,
            int tileSize,
            Vector3 unscaledOffset,
            bool open = true,
            Caster casterType = Caster.Shadow
        )
        {
            if((open && IlluminationModelOpened == null) || (!open && IlluminationModelClosed == null))
            {
                Logger.Error("Trying to illuminate a room without illumination model for the " + (open? "opened" : "closed") + " state of the level!");
            }

            // if we cast shadow type, then the shadowed part will contain the value false
            // if we cast light type then the shadowed part will contain the value true
            //bool sign = (casterType == Caster.Shadow ? false : true);
            bool light = (casterType == Caster.Shadow ? true : false);

            var model = (open ? IlluminationModelOpened : IlluminationModelClosed);

            var template = collisionTemplate; // room.Collision.GetCollisionTemplate();
            //var tileSize = room.TextureTileSize;

            int width = template[0].Length * tileSize;
            int length = width * template.Length * tileSize;
            bool[] lighted = Enumerable.Repeat<bool>(!light, length).ToArray();

            Vector3 offset = unscaledOffset; // new Vector3(room.Rect.Location.X / room.Scale, room.Rect.Location.Y / room.Scale, 0);

            bool[] shadowMap = Enumerable.Repeat<bool>(true, length).ToArray();
            Vector3[] coords = Enumerable.Repeat<Vector3>(Vector3.Zero, length).ToArray();
            int[] textureMap = Enumerable.Repeat<int>(-1, length).ToArray();
            X_TileType[] tileType = Enumerable.Repeat<X_TileType>(X_TileType.Floor, length).ToArray();

            int shadowSpotSize = 1;
            float tto = 0.001f;
            Parallel.For(0, template.Length, h =>
            //for (int h=0; h < template.Length; ++h)
            {
                for (int w = 0; w < template[0].Length; ++w)
                {
                    if (template[h][w] == (int)X_TileType.DontCare) continue;

                    int fromX = w * tileSize + shadowSpotSize / 2;
                    int fromY = h * tileSize + shadowSpotSize / 2;
                    int toX = fromX + tileSize - shadowSpotSize / 2;
                    int toY = fromY + tileSize - shadowSpotSize / 2;

                    // pretest if any part of the tile is not visible. If any edge is not visible from the light
                    // we need to calculate shadows for it
                    List<Vector3> pts = new List<Vector3>();
                    if (template[h][w] == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
                    {
                        pts.Add(new Vector3(fromX + tto, fromY + tto - tileSize, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize, fromY + tto - tileSize, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX + tto, fromY - tto - tileSize + tileSize, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize, fromY - tto - tileSize + tileSize, -tileSize - 0.001f) + offset);
                    }
                    else if (template[h][w] == (int)X_TileType.Wall)
                    {
                        pts.Add(new Vector3(fromX + tto, fromY + tto + 0.001f, -(0)) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize, fromY + tto + 0.001f, -(0)) + offset);
                        pts.Add(new Vector3(fromX + tto, fromY - tto + tileSize + 0.001f, -(tileSize)) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize - tto, fromY + 0.001f, -(tileSize)) + offset);
                    }
                    foreach (var lightSource in lights)
                    {
                        Vector3 orig = lightSource.GetUnscaledPosition();
                        foreach (var p in pts)
                        {
                            //if (light.GetUnscaledIlluminationRect().Contains(new Point((int)p.X, (int)p.Y)))
                            //{
                                foreach (var cube in model)
                                {
                                    var intersects = cube.RayIntersect(orig, p - orig);
                                    if (intersects)
                                    {
                                        goto add_tile;
                                    }
                                }
                            //}
                        }
                    }

                    // if we have shadow, assume that non intersected parts are automatically in the light
                    // otherwise keep them in the shadow and only light the parts that have direct line of sight
                    for (int x = fromX; x < toX; x += shadowSpotSize)
                    {
                        for (int y = fromY; y < toY; y += shadowSpotSize)
                        {
                            lighted[y * width + x] = light;
                        }
                    }
                    continue;

                    add_tile:
                    for (int x = fromX; x < toX; x += shadowSpotSize)
                    {
                        for (int y = fromY; y < toY; y += shadowSpotSize)
                        {
                            tileType[y * width + x] = (X_TileType)template[h][w];
                            if (template[h][w] == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
                            {
                                coords[y * width + x] = new Vector3(x, y - tileSize, -tileSize - 0.001f) + offset;
                                textureMap[y * width + x] = (y) * width + x;
                            }
                            else if (template[h][w] == (int)X_TileType.Wall)
                            {
                                coords[y * width + x] = new Vector3(x, fromY + 0.001f, -(y - fromY)) + offset;
                                textureMap[y * width + x] = (y) * width + x;
                            }
                            if (template[h][w] == (int)X_TileType.Roof || template[h][w] == (int)X_TileType.Outside)
                            {
                                shadowMap[y * width + x] = false;
                            }
                        }
                    }
                }
            });

            foreach (var lightSource in lights)
            {
                Vector3 orig = lightSource.GetUnscaledPosition();
                //float scale = room.Scale;
                Parallel.For(0, length, i =>
                //for (int i = 0; i < data.Length; ++i)
                {
                    //if (!light.GetUnscaledIlluminationRect().Contains(new Point((int)coords[i].X, (int)coords[i].Y)))
                    //    return;

                    int hit2 = textureMap[i];
                    if (hit2 < 0) return;

                    if (lighted[hit2] == light) return;

                    var sm = shadowMap[hit2];
                    if (!sm)
                    {
                        lighted[hit2] = light;
                        return;
                    }

                    //var tt = tileType[hit2];
                    bool intersected = false;
                    foreach (var cube in model)
                    {
                        if (cube.RayIntersect(orig, coords[i] - orig))
                        {
                            intersected = true;
                            break;
                        }
                    }

                    //if (casterType == Caster.Shadow)
                    //{
                        if (!intersected)
                        {
                        lighted[hit2] = light;
                    }
                    //}
                    //else
                    //{
                    //    if (intersected)
                    //    {
                    //        lighted[hit2] = !sign;
                    //    }
                    //}
                });
            }

            //if (casterType == Caster.Light)
            //{
            //    for (int i = 0; i < lighted.Length; ++i)
            //    {
            //        lighted[i] = !lighted[i];
            //    }
            //}

            return lighted;
            //saveShadeToFile(lighted, room, lights);
        }

        private static string combineIdentifiers(List<X_Light> lights, IWalkable room)
        {
            string identifier = "";
            foreach (var light in lights)
            {
                if (light.GetScaledIlluminationRect().Intersects(room.Rect))
                {
                    identifier += light.GetIdentifier(room) + "|";
                }
            }
            if (identifier.EndsWith("|"))
                identifier = identifier.Substring(0, identifier.Length - 1);

            return identifier;
        }

        public static void SaveShadeToFile(bool[] shadeTemplate, bool open, IWalkable room, List<X_Light> lights)
        {
            if (room.ResourceFolder != "")
            {
                string fileName = "shade_" + (open ? "opened_" : "closed_");
                string identifier = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + "\n";
                identifier += Manager_Light.ShadeVersion + "\n";

                identifier += combineIdentifiers(lights, room);
                identifier += "\n";
                identifier += string.Join("", shadeTemplate.Select(x => x ? "1" : "0"));

                var srcPath = Util.GetAbsResourceFolderPath(room.ResourceFolder);
                fileName += Util.CreateGenericIdentifier();
                fileName += ".shade";

                // write to all available directories: current runtime directory and source code directory
                File.WriteAllText(room.ResourceFolder + fileName, identifier);
                File.WriteAllText(srcPath + fileName, identifier);
            }
        }

        public static bool[] LoadShadeFromFile(bool open, IWalkable room, List<X_Light> lights)
        {
            string fileName = "shade_" + (open ? "opened_" : "closed_");
            var files = Directory.GetFiles(room.ResourceFolder);
            string dataFileName = Path.GetFileName(files
                .Where(x => Path.GetFileName(x).Contains(fileName) && Path.GetFileName(x).EndsWith(".shade"))
                .FirstOrDefault());

            if (dataFileName == null) return null;

            var contents = File.ReadAllLines(room.ResourceFolder + dataFileName);
            var vCorr = contents[1] == Manager_Light.ShadeVersion;
            if (!vCorr) return null;

            var iCorr = contents[2] == combineIdentifiers(lights, room);
            if (!iCorr) return null;

            return contents[3].Select(c => c == '1').ToArray();
        }

        public static void RemoveAllShadeFiles(IWalkable room, bool open)
        {
            string fileName = "shade_" + (open ? "opened_" : "closed_");
            var files = Directory.GetFiles(room.ResourceFolder);
            var sFiles = files.Where(x => Path.GetFileName(x).Contains(fileName) && Path.GetFileName(x).EndsWith(".shade")).ToArray();
            foreach(var f in sFiles)
            {
                string dataFileName = Path.GetFileName(f);
                File.Delete(dataFileName);
            }
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

        public static void Initialize(Y_Level level)
        {
            Func<Y_Level, bool, List<X_Cube>> elevate = (level, open) => {
                List<X_Cube> cubes = new List<X_Cube>();

                foreach (var room in level.Rooms.Values)
                {
                    // make sure that we open the door first because
                    // we want that state of the door to be shaded
                    if (open && room.WhatAreYou() == X_LevelElements.Door)
                    {
                        ((Y_Door)room).OpenDoor();
                    }
                    var rects = room.Collision.GetCollisionRectangles().Clone() as Rectangle[];

                    if (open && room.WhatAreYou() == X_LevelElements.Door)
                    {
                        ((Y_Door)room).CloseDoor();
                    }

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
                return cubes;
            };

            IlluminationModelOpened = elevate(level, true);

            IlluminationModelClosed = elevate(level, false);

            //toWavefrontObj(cubes, "./logs/cubes.obj");
            //return cubes;
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
