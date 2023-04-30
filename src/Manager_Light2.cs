using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace YGR
{
    public static class Manager_Light2
    {
        public enum Type
        {
            Tiles = 0,
            CPU,
            GPU
        }

        public struct X_Vector3
        {
            public float X;
            public float Y;
            public float Z;
            //public int GlobalID;
            public int Index;
            public int Lighted;

            public static implicit operator X_Vector3(Vector4 vector)
            {
                return new X_Vector3() { X = vector.X, Y = vector.Y, Z = vector.Z, Index = (int)vector.W, Lighted = 0 };
            }
        }

        public struct X_Point3
        {
            public float X;
            public float Y;
            public float Z;

            public static implicit operator X_Point3(Vector3 point)
            {
                return new X_Point3() { X = point.X, Y = point.Y, Z = point.Z };
            }
        }

        public static Vector3[] IlluminationModelOpened { get; private set; }
        public static Vector3[] IlluminationModelClosed { get; private set; }

        public static Dictionary<string, bool[]> LoadedIlluminationTemplates { get; private set; }
        public static string ShadeVersion { get { return "V3"; } }
        public static GraphicsDevice GraphicsDevice_ { get; set; }

        public static Type Platform { get; set; }

        private static int LightsBufferSize = 5;
        private static int VerticesBufferSize = 5000;
        private static int CoordsBufferSize = 2500000;
        private static StructuredBuffer LightsBuffer;
        private static StructuredBuffer VerticesBuffer;
        private static StructuredBuffer CoordsBuffer;

        private static Effect IlluminationShader { get; set; }

        static int[] indices = new int[]
        {
            /* bottom */ 0, 1, 2,   0, 2, 3,
            /* right  */ 2, 6, 3,   3, 6, 7,
            /* front  */ 1, 5, 2,   2, 5, 6,
            /* left   */ 0, 4, 1,   1, 4, 5,
            /* back   */ 0, 3, 7,   0, 7, 4,
            /* top    */ 5, 4, 6,   6, 4, 7
        };

        static public X_Vector3[] Coords;
        static public X_Point3[] Vertices;
        static public int NumVerts;
        static public int NumLights;
        static public int NumCoords;
        static public X_Point3[] Lights;

        static public float eps = 1.0e-7f;

        static public bool RayIntersect(Vector3 origin, Vector3 direction, int globalIDx)
        {
            float length = direction.Length();
            direction.Normalize();

            for (int ind = 0; ind < NumVerts; ind += 8)
            {
                //int index = 0;
                for (int i = 0; i < 36; i += 3)
                {
                    var p00 = Vertices[ind + indices[i]];
                    var p01 = Vertices[ind + indices[i + 1]];
                    var p02 = Vertices[ind + indices[i + 2]];
                    Vector3 p0 = new Vector3(p00.X, p00.Y, p00.Z);
                    Vector3 p1 = new Vector3(p01.X, p01.Y, p01.Z);
                    Vector3 p2 = new Vector3(p02.X, p02.Y, p02.Z);

                    // Find vectors for two edges sharing v[0]
                    Vector3 edge1 = p1 - p0;
                    Vector3 edge2 = p2 - p0;

                    // remove backside
                    Vector3 n = Manager_Light2.CrossProduct(edge1, edge2);
                    n.Normalize();
                    if (Manager_Light2.Dot(direction, n) > eps)
                        continue;

                    // Begin calculating determinant - also used to calculate U parameter
                    Vector3 pvec = Manager_Light2.CrossProduct(direction, edge2);

                    // If determinant is near zero, ray lies in plane of triangle
                    float det = Manager_Light2.Dot(edge1, pvec);

                    if (det > -float.Epsilon && det < eps)
                        continue;
                    float invDet = 1.0f / det;

                    // Calculate distance from v[0] to ray origin
                    Vector3 tvec = origin - p0;

                    // Calculate U parameter and test bounds
                    float u = Manager_Light2.Dot(tvec, pvec) * invDet;
                    if (u < 0.0 || u > 1.0)
                        continue;

                    // Prepare to test V parameter
                    Vector3 qvec = Manager_Light2.CrossProduct(tvec, edge1);

                    // Calculate V parameter and test bounds
                    float v = Manager_Light2.Dot(direction, qvec) * invDet;
                    if (v < 0.0 || u + v > 1.0)
                        continue;

                    // Ray intersects triangle -> compute t
                    float t = Manager_Light2.Dot(edge2, qvec) * invDet;

                    if (t >= eps && t <= length)
                    {
                        //hitPoint = origin + t * direction;
                        //if (globalIDx > -1)
                        //{
                        //    if (index == 2)
                        //    {
                        //        TestVals[globalIDx].X = 1;
                        //        TestVals[globalIDx].Y = 0;
                        //        TestVals[globalIDx].Z = 0;
                        //    }
                        //    index++;
                        //}
                        return true;
                    }
                }
            }
            return false;
        }

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

        public static void Initialize(string resourceFolder, ContentManager content, GraphicsDevice graphicsDevice)
        {
            GraphicsDevice_ = graphicsDevice;
            IlluminationShader = content.Load<Effect>("Shader/Illuminator");
        }

        public static bool[] Illuminate(
            IWalkable room,
            Vector3 unscaledOffset
        )
        {
            var watch = new Stopwatch();
            watch.Start();

            bool light = true;

            var tileSize = room.TextureTileSize;
            var template = room.Collision.GetCollisionTemplate();

            int width = template[0].Length * tileSize;
            int length = width * template.Length * tileSize;
            bool[] lighted = Enumerable.Repeat<bool>(!light, length).ToArray();

            Vector3 offset = unscaledOffset;
            Coords = new X_Vector3[length]; // Enumerable.Repeat<Vector3>(Vector3.Zero, length).ToArray();
            Lights = room.Lights.Select(x => x.GetUnscaledPosition()).ToArray();
            Vertices = CreateModel(room); /* (open ? IlluminationModelOpened : IlluminationModelClosed);*/
            NumLights = Lights.Count();
            NumVerts = Vertices.Count();
            NumCoords = 0;

            if (Platform == Type.GPU)
            {

                if (LightsBuffer == null || NumLights > LightsBufferSize)
                {
                    LightsBufferSize = Lights.Count();
                    LightsBuffer = new StructuredBuffer(
                        GraphicsDevice_, typeof(X_Point3), LightsBufferSize, BufferUsage.None, ShaderAccess.Read);
                }

                if (CoordsBuffer == null || Coords.Length > CoordsBufferSize)
                {
                    CoordsBufferSize = Coords.Length;
                    CoordsBuffer = new StructuredBuffer(
                        GraphicsDevice_, typeof(X_Vector3), CoordsBufferSize, BufferUsage.None, ShaderAccess.ReadWrite);
                }

                if (VerticesBuffer == null || NumVerts > VerticesBufferSize)
                {
                    VerticesBufferSize = NumVerts;
                    VerticesBuffer = new StructuredBuffer(
                        GraphicsDevice_, typeof(X_Point3), VerticesBufferSize, BufferUsage.None, ShaderAccess.Read);
                }
            }

            int shadowSpotSize = 1;
            float tto = 0.001f;
            int shadowSize = (tileSize + shadowSpotSize / 2) * (tileSize + shadowSpotSize / 2);
            int baseIndex = 0;

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

                    if (template[h][w] == (int)X_TileType.Roof || template[h][w] == (int)X_TileType.Outside)
                    {
                        continue;
                    }

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
                    foreach (var lightSource in Lights)
                    {
                        Vector3 orig = new Vector3(lightSource.X, lightSource.Y, lightSource.Z);
                        foreach (var p in pts)
                        {
                            var intersects = Manager_Light2.RayIntersect(orig, p - orig, -1);
                            if (intersects)
                            {
                                goto add_tile;
                            }
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
                    int end = Interlocked.Add(ref baseIndex, shadowSize);
                    int index = end - shadowSize;
                    for (int x = fromX; x < toX; x += shadowSpotSize)
                    {
                        for (int y = fromY; y < toY; y += shadowSpotSize)
                        {
                            if (template[h][w] == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
                            {
                                X_Vector3 s = new Vector4(x + offset.X, y - tileSize + offset.Y, -tileSize - 0.001f + offset.Z, (y) * width + x);
                                Coords[index] = s;
                                index++;
                            }
                            else if (template[h][w] == (int)X_TileType.Wall)
                            {

                                X_Vector3 s = new Vector4(x + offset.X, fromY + 0.001f + offset.Y, -(y - fromY) + offset.Z, (y) * width + x);
                                Coords[index] = s;
                                index++;
                            }
                        }
                    }
                }
            });

            NumCoords = baseIndex;

            Logger.Info("Precompute tiles: " + watch.ElapsedMilliseconds.ToString() + " containing " + Coords.Length.ToString() + " coordinates");

            if (Platform == Type.CPU)
            {
                //float scale = room.Scale;
                Parallel.For(0, NumCoords, i =>
                //for (int i = 0; i < baseIndex; ++i)
                {
                    ref var p = ref Coords[i];
                    for (int l = 0; l < Lights.Length; ++l)
                    {
                        if (p.Lighted == 1) return;
                        Vector3 lightPos = new Vector3(Lights[l].X, Lights[l].Y, Lights[l].Z);
                        Vector3 pos = new Vector3(p.X, p.Y, p.Z);
                        Vector3 direction = pos - lightPos;
                        if (!Manager_Light2.RayIntersect(lightPos, direction, i))
                        {
                            p.Lighted = 1;
                            //lighted[p.Index] = light;
                            return;;
                        }
                    }
                });
            }

            // =============================================================================

            else if(Platform == Type.GPU && NumCoords > 0)
            {

                VerticesBuffer.SetData(Vertices, 0, NumVerts);
                CoordsBuffer.SetData(Coords, 0, NumCoords);
                LightsBuffer.SetData(Lights, 0, NumLights);

                if (IlluminationShader.Parameters["Vertices"] != null)
                    IlluminationShader.Parameters["Vertices"].SetValue(VerticesBuffer);

                if (IlluminationShader.Parameters["Coords"] != null)
                    IlluminationShader.Parameters["Coords"].SetValue(CoordsBuffer);

                if (IlluminationShader.Parameters["Lights"] != null)
                    IlluminationShader.Parameters["Lights"].SetValue(LightsBuffer);

                if (IlluminationShader.Parameters["NumLights"] != null)
                    IlluminationShader.Parameters["NumLights"].SetValue(NumLights);

                if (IlluminationShader.Parameters["NumCoords"] != null)
                    IlluminationShader.Parameters["NumCoords"].SetValue(NumCoords);

                if (IlluminationShader.Parameters["NumVerts"] != null)
                    IlluminationShader.Parameters["NumVerts"].SetValue(NumVerts);

                foreach (var pass in IlluminationShader.CurrentTechnique.Passes)
                {
                    pass.ApplyCompute();
                    int dispatchCount = (int)Math.Ceiling((double)baseIndex / 64.0);
                    GraphicsDevice_.DispatchCompute(dispatchCount, 1, 1);
                }
                CoordsBuffer.GetData<X_Vector3>(Coords, 0, NumCoords);
            }

            Parallel.For(0, NumCoords, i =>
            //for (int i = 0; i < baseIndex; ++i)
            {
                if (Coords[i].Lighted == 1)
                {
                    lighted[Coords[i].Index] = true;
                }
            });

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
                identifier += Manager_Light2.ShadeVersion + "\n";

                identifier += combineIdentifiers(lights, room);
                identifier += "\n";
                identifier += string.Join("", shadeTemplate.Select(x => x ? "1" : "0"));

                fileName += Util.CreateGenericIdentifier();
                fileName += ".shade";

                // write to all available directories: current runtime directory and source code directory
                File.WriteAllText(room.ResourceFolder + fileName, identifier);
                if (Debugger.IsAttached)
                {
                    var srcPath = Util.GetAbsResourceFolderPath(room.ResourceFolder);
                    File.WriteAllText(srcPath + fileName, identifier);
                }
                LoadedIlluminationTemplates.Add(room.ResourceFolder + fileName, shadeTemplate);
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

            if (LoadedIlluminationTemplates.ContainsKey(room.ResourceFolder + fileName))
                return LoadedIlluminationTemplates[room.ResourceFolder + fileName];

            var contents = File.ReadAllLines(room.ResourceFolder + dataFileName);
            var vCorr = contents[1] == Manager_Light2.ShadeVersion;
            if (!vCorr) return null;

            var iCorr = contents[2] == combineIdentifiers(lights, room);
            if (!iCorr) return null;

            LoadedIlluminationTemplates.Add(room.ResourceFolder + fileName, contents[3].Select(c => c == '1').ToArray());

            return LoadedIlluminationTemplates[room.ResourceFolder + fileName];
        }

        public static void RemoveAllShadeFiles(IWalkable room, bool open)
        {
            string fileName = "shade_" + (open ? "opened_" : "closed_");
            var files = Directory.GetFiles(room.ResourceFolder);
            var sFiles = files.Where(x => Path.GetFileName(x).Contains(fileName) && Path.GetFileName(x).EndsWith(".shade")).ToArray();
            foreach (var f in sFiles)
            {
                string dataFileName = Path.GetFileName(f);
                string dataRessourceFolder = Util.GetAbsResourceFolderPath(room.ResourceFolder);
                File.Delete(dataRessourceFolder + dataFileName);
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

        public static X_Point3[] CreateModel(IWalkable room)
        {
            var watch = new Stopwatch();
            watch.Start();
            List<Rectangle> rects = new List<Rectangle>();
            if (room.WhatAreYou() == X_LevelElements.Door)
            {
                rects.AddRange(((Y_Door)room).GetOpenDoorCollisionRects());
            }
            else
            {
                rects.AddRange(room.Collision.GetCollisionRectangles());
                foreach(var door in room.DoorRooms)
                {
                    // add all doors and make sure they are in the opened state
                    var d = (Y_Door)door.Value.First();
                    if (d.DoorIsOpen())
                        rects.AddRange(d.GetOpenDoorCollisionRects());
                    else
                        rects.AddRange(d.GetClosedDoorCollisionRects());
                }
            }


            float scale = room.Scale;
            float elev = room.Collision.TileHeight / scale;
            int shift = 5;
            List<X_Point3> points = new List<X_Point3>();
            foreach (var rect in rects)
            {
                float x = (rect.X - shift + 0.5f) / scale;
                float y = (rect.Y - shift) / scale;
                float h = (rect.Height + shift) / scale;
                float w = (rect.Width + shift) / scale;
                float e = elev + 1;
                X_Point3[] vertices = new X_Point3[] {
                            new Vector3(x, y, -e),
                            new Vector3(x, y+h, -e),
                            new Vector3(x+w, y+h, -e),
                            new Vector3(x+w, y, -e),
                            new Vector3(x, y, shift),
                            new Vector3(x, y+h, shift),
                            new Vector3(x+w, y+h, shift),
                            new Vector3(x+w, y, shift)
                };

                points.AddRange(vertices);
            }

            Logger.Info("Calcualte light model for room " + room.Name + ": " + watch.ElapsedMilliseconds.ToString());

            return points.ToArray();
        }

        //private static void toWavefrontObj(List<X_Cube> cubes, string name)
        //{
        //    string s = "";
        //    int i = 0;
        //    int offset = 0;
        //    foreach (var cube in cubes)
        //    {
        //        s += "o cube" + i + "\n";
        //        s += (cube.toWavefrontObj(offset));
        //        offset += cube.VertexCount();
        //        i++;
        //    }

        //    File.WriteAllText(name, s);
        //}
    }
}
