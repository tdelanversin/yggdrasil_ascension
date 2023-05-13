using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            public int WX;
            public int WY;

            public static implicit operator X_Vector3(Vector3 vector)
            {
                return new X_Vector3() { X = vector.X, Y = vector.Y, Z = vector.Z };
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

        public static GraphicsDevice GraphicsDevice_ { get; set; }

        public static Type Platform { get; set; }
        private static int LightsBufferSize = 10;
        private static int VerticesBufferSize = 5000;
        private static int CoordsBufferSize = 100000;
        private static StructuredBuffer LightsBuffer;
        private static StructuredBuffer VerticesBuffer;
        private static StructuredBuffer CoordsBuffer;

        private static ConcurrentQueue<IWalkable> ToPrecompute;
        private static ConcurrentQueue<IWalkable> ToIlluminate;

        private static int KeepAliveSize = 50;
        private static StructuredBuffer KeepAllive;

        public static Effect IlluminationShader { get; set; }

        private static bool _working;
        private static bool _preworking;
        private static int _cooldown = 2;
        //private static int _cooldownMin = 1;
        //private static int _cooldownMax = 4;
        private static int _cooldownTimer = _cooldown;

        public static float ShadeFloat = 0.6f;

        static int[] indices = new int[]
        {
            /* bottom */ 0, 1, 2,   0, 2, 3,
            /* right  */ 2, 6, 3,   3, 6, 7,
            /* front  */ 1, 5, 2,   2, 5, 6,
            /* left   */ 0, 4, 1,   1, 4, 5,
            /* back   */ 0, 3, 7,   0, 7, 4,
            /* top    */ 5, 4, 6,   6, 4, 7
        };

        static public float eps = 1.0e-7f;

        static public bool RayIntersect(IWalkable room, Vector3 origin, Vector3 direction, int globalIDx)
        {
            float length = direction.Length();
            direction.Normalize();

            for (int ind = 0; ind < room.IlluminationResources.NumVerts; ind += 8)
            {
                //int index = 0;
                for (int i = 0; i < 36; i += 3)
                {
                    var p00 = room.IlluminationResources.Vertices[ind + indices[i]];
                    var p01 = room.IlluminationResources.Vertices[ind + indices[i + 1]];
                    var p02 = room.IlluminationResources.Vertices[ind + indices[i + 2]];
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
            ToPrecompute = new ConcurrentQueue<IWalkable>();
            ToIlluminate = new ConcurrentQueue<IWalkable>();
            _working = false;
            _preworking = false;
            KeepAllive = new StructuredBuffer(GraphicsDevice_, typeof(int), KeepAliveSize, BufferUsage.None, ShaderAccess.Read);
        }

        public static void IlluminateSync(List<IWalkable> rooms)
        {
            foreach(var room in rooms)
            {
                Precompute(room);
            }
            foreach (var room in rooms)
            {
                while (room.IlluminationResources.NumCoords > room.IlluminationResources.NumOffset)
                    Compute(room);
                room.SwitchTargetShadeIndex();
            }
        }

        public static void Illuminate(
            IWalkable room
        )
        {
            if (room.IlluminationResources.InFlight) return;
            room.IlluminationResources.InFlight = true;
            ToPrecompute.Enqueue(room);
        }

        public static void Update(GameTime gameTime)
        {
            if (ToPrecompute.Count() > 0)
            {
                if (!_preworking)
                {
                    _preworking = true;
                    IWalkable room;
                    if(ToPrecompute.TryDequeue(out room))
                    {
                        var t = Task.Run(() =>
                        {
                            //Logger.Info("Start precompute... room " + room.Name);
                            Precompute(room);
                            ToIlluminate.Enqueue(room);
                            _preworking = false;
                        });
                    }
                }
            }

            if (_cooldownTimer < _cooldown)
            {
                _cooldownTimer++;
                return;
            }

            if (ToIlluminate.Count() > 0)
            {
                if (!_working)
                {
                    _working = true;
                    IWalkable room;
                    if(ToIlluminate.TryPeek(out room))
                    {
                        //Logger.Info("Start computations... room " + room.Name);
                        Compute(room);

                        if (room.IlluminationResources.NumCoords - room.IlluminationResources.NumOffset == 0)
                        {
                            while (!ToIlluminate.TryDequeue(out room));

                            room.SwitchTargetShadeIndex();
                            room.IlluminationResources.InFlight = false;
                        }
                        //Logger.Info("End computations... room " + room.Name);
                    }
                    _working = false;
                    _cooldownTimer = 0;
                    _cooldown = 1; // Util.random.Next(_cooldownMin, _cooldownMax);
                }
            }
            else
            {
                //KeepAllive.SetData(Enumerable.Repeat<int>(0, KeepAliveSize).ToArray());
                //IlluminationShader.Parameters["KeepAllive"].SetValue(KeepAllive);
                //foreach (var pass in IlluminationShader.CurrentTechnique.Passes)
                //{
                //    pass.ApplyCompute();
                //    int dispatchCount = 512;
                //    GraphicsDevice_.DispatchCompute(dispatchCount, 1, 1);
                //}
            }
        }

        public static void Precompute(IWalkable room)
        {
            bool light = true;
            var tileSize = Y_Level.TextureTileSize;
            var template = room.Collision.GetCollisionTemplate();

            int width = template[0].Length * tileSize;
            int length = width * template.Length * tileSize;
            room.IlluminationResources.BLighted = Enumerable.Repeat<bool>(!light, length).ToArray();

            Vector3 offset = room.Offset;
            room.IlluminationResources.Coords = new X_Vector3[length]; // Enumerable.Repeat<Vector3>(Vector3.Zero, length).ToArray();
            room.IlluminationResources.Index = new int[length];

            //room.IlluminationResources.Lights = room.Lights.Select(x => x.GetUnscaledPosition()).ToArray();
            room.IlluminationResources.Lights = room.GetAllRelevantLights().Select(x => x.GetUnscaledPosition()).ToArray();

            string model = "";
            int globalOffset = 0;
            room.IlluminationResources.Vertices = CreateModel(room, false, ref model, ref globalOffset); /* (open ? IlluminationModelOpened : IlluminationModelClosed);*/
            room.IlluminationResources.NumLights = room.IlluminationResources.Lights.Count();
            room.IlluminationResources.NumVerts = room.IlluminationResources.Vertices.Count();
            room.IlluminationResources.NumCoords = 0;

            int shadowSpotSize = 1;
            float tto = 0.001f;
            int shadowSize = (tileSize + shadowSpotSize / 2) * (tileSize + shadowSpotSize / 2);
            int baseIndex = 0;
            //Logger.Info("A Room " + room.Name + ": " + watch.ElapsedMilliseconds);
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
                        //pts.Add(new Vector3(fromX + tileSize/2, fromY + tileSize/2, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX + tto, fromY + tto - tileSize, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize, fromY + tto - tileSize, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX + tto, fromY - tto - tileSize + tileSize, -tileSize - 0.001f) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize, fromY - tto - tileSize + tileSize, -tileSize - 0.001f) + offset);
                    }
                    else if (template[h][w] == (int)X_TileType.Wall)
                    {
                        //pts.Add(new Vector3(fromX + tileSize/2, fromY + tileSize/2 + 0.001f, -(0)) + offset);
                        pts.Add(new Vector3(fromX + tto, fromY + tto + 0.001f, -(0)) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize, fromY + tto + 0.001f, -(0)) + offset);
                        pts.Add(new Vector3(fromX + tto, fromY - tto + tileSize + 0.001f, -(tileSize)) + offset);
                        pts.Add(new Vector3(fromX - tto + tileSize - tto, fromY + 0.001f, -(tileSize)) + offset);
                    }

                    bool[] visible = new bool[] { false, false, false, false};
                    foreach (var lightSource in room.IlluminationResources.Lights)
                    {
                        Vector3 orig = new Vector3(lightSource.X, lightSource.Y, lightSource.Z);
                        int ind = 0;
                        foreach (var p in pts)
                        {
                            if(!visible[ind] && !Manager_Light2.RayIntersect(room, orig, p - orig, -1))
                            {
                                visible[ind] = true;
                            }
                            ind++;
                        }
                    }

                    if (!visible.All(x => x == true))
                    {
                        goto add_tile;
                    }

                    // if we have shadow, assume that non intersected parts are automatically in the light
                    // otherwise keep them in the shadow and only light the parts that have direct line of sight
                    for (int x = fromX; x < toX; x += shadowSpotSize)
                    {
                        for (int y = fromY; y < toY; y += shadowSpotSize)
                        {
                            room.IlluminationResources.BLighted[y * width + x] = light;
                            //room.Shade[y * width + x] = Color.Green;
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
                                X_Vector3 s = new Vector3(x + offset.X, y - tileSize + offset.Y, -tileSize - 0.001f + offset.Z);
                                s.WX = x;
                                s.WY = y;
                                room.IlluminationResources.Index[index] = (y) * width + x;
                                room.IlluminationResources.Coords[index] = s;
                                index++;
                            }
                            else if (template[h][w] == (int)X_TileType.Wall)
                            {

                                X_Vector3 s = new Vector3(x + offset.X, fromY + 0.001f + offset.Y, -(y - fromY) + offset.Z);
                                s.WX = x;
                                s.WY = y;
                                room.IlluminationResources.Index[index] = (y) * width + x;
                                room.IlluminationResources.Coords[index] = s;
                                index++;
                            }
                            //room.Shade[y * width + x] = Color.Yellow;
                        }
                    }
                }
            });

            room.ShadeTexture[room.GetTargetShadeIndex()].SetData(room.Shade);

            room.IlluminationResources.NumCoords = baseIndex;
            room.IlluminationResources.NumOffset = 0;
        }

        private static void Compute(IWalkable room)
        {
            if (Platform == Type.CPU)
            {
                room.IlluminationResources.Lighted = Enumerable.Repeat<int>(0, room.IlluminationResources.NumCoords).ToArray();
                //float scale = room.Scale;
                Parallel.For(0, room.IlluminationResources.NumCoords, i =>
                //for (int i = 0; i < baseIndex; ++i)
                {
                    ref var p = ref room.IlluminationResources.Coords[i];
                    for (int l = 0; l < room.IlluminationResources.Lights.Length; ++l)
                    {
                        if (room.IlluminationResources.Lighted[i] == 1) return;
                        Vector3 lightPos = new Vector3(
                            room.IlluminationResources.Lights[l].X, 
                            room.IlluminationResources.Lights[l].Y, 
                            room.IlluminationResources.Lights[l].Z);

                        Vector3 pos = new Vector3(p.X, p.Y, p.Z);
                        Vector3 direction = pos - lightPos;
                        if (!Manager_Light2.RayIntersect(room, lightPos, direction, i))
                        {
                            room.IlluminationResources.Lighted[i] = 1;
                            //lighted[p.Index] = light;
                            return;
                        }
                    }
                });

                Parallel.For(0, room.IlluminationResources.NumCoords, i =>
                //for (int i = 0; i < baseIndex; ++i)
                {
                    if (room.IlluminationResources.Lighted[i] == 0)
                    {
                        room.Shade[room.IlluminationResources.Index[i]].A = 1;
                    }
                });

                room.ShadeTexture[room.GetTargetShadeIndex()].SetData(room.Shade);
            }

            // =============================================================================

            else if(Platform == Type.GPU && room.IlluminationResources.NumCoords > 0)
            {
                if (LightsBuffer == null || room.IlluminationResources.NumLights > LightsBufferSize)
                {
                    if(room.IlluminationResources.NumLights > LightsBufferSize)
                        LightsBufferSize = room.IlluminationResources.Lights.Count();
                    LightsBuffer = new StructuredBuffer(
                        GraphicsDevice_, typeof(X_Point3), LightsBufferSize, BufferUsage.None, ShaderAccess.Read);
                }

                if (CoordsBuffer == null)
                {
                    CoordsBuffer = new StructuredBuffer(
                        GraphicsDevice_, typeof(X_Vector3), CoordsBufferSize, BufferUsage.None, ShaderAccess.Read);
                }

                if (VerticesBuffer == null || room.IlluminationResources.NumVerts > VerticesBufferSize)
                {
                    if(room.IlluminationResources.NumVerts > VerticesBufferSize)
                        VerticesBufferSize = room.IlluminationResources.NumVerts;
                    VerticesBuffer = new StructuredBuffer(
                        GraphicsDevice_, typeof(X_Point3), VerticesBufferSize, BufferUsage.None, ShaderAccess.Read);
                }

                Manager_Light2.IlluminationShader.Parameters["Shade"].SetValue(room.ShadeTexture[room.GetTargetShadeIndex()]);
                VerticesBuffer.SetData(room.IlluminationResources.Vertices, 0, room.IlluminationResources.NumVerts);

                int use = Math.Min(CoordsBufferSize, room.IlluminationResources.NumCoords - room.IlluminationResources.NumOffset);
                CoordsBuffer.SetData(room.IlluminationResources.Coords, room.IlluminationResources.NumOffset, use);

                LightsBuffer.SetData(room.IlluminationResources.Lights, 0, room.IlluminationResources.NumLights);

                KeepAllive.SetData(Enumerable.Repeat<int>(1, KeepAliveSize).ToArray());
                IlluminationShader.Parameters["KeepAllive"].SetValue(KeepAllive);

                //LightedBuffer.SetData(Lighted, 0, NumCoords);
                if (IlluminationShader.Parameters["Vertices"] != null)
                    IlluminationShader.Parameters["Vertices"].SetValue(VerticesBuffer);

                if (IlluminationShader.Parameters["Coords"] != null)
                    IlluminationShader.Parameters["Coords"].SetValue(CoordsBuffer);

                if (IlluminationShader.Parameters["Lights"] != null)
                    IlluminationShader.Parameters["Lights"].SetValue(LightsBuffer);

                if (IlluminationShader.Parameters["NumLights"] != null)
                    IlluminationShader.Parameters["NumLights"].SetValue(room.IlluminationResources.NumLights);

                if (IlluminationShader.Parameters["NumCoords"] != null)
                    IlluminationShader.Parameters["NumCoords"].SetValue(room.IlluminationResources.NumCoords);

                if (IlluminationShader.Parameters["NumVerts"] != null)
                    IlluminationShader.Parameters["NumVerts"].SetValue(room.IlluminationResources.NumVerts);

                foreach (var pass in IlluminationShader.CurrentTechnique.Passes)
                {
                    pass.ApplyCompute();
                    int dispatchCount = (int)Math.Ceiling((double)room.IlluminationResources.NumCoords / 512);
                    GraphicsDevice_.DispatchCompute(dispatchCount, 1, 1);
                }

                room.IlluminationResources.NumOffset += use;
            }
        }

        public static X_Point3[] CreateModel(IWalkable room, bool visualize, ref string model, ref int globalOffset)
        {
            int minEC, maxEC;
            List<Rectangle> rects = new List<Rectangle>();
            if (room.WhatAreYou() == X_LevelElements.Door)
            { 
                rects.AddRange(((Y_Door)room).GetOpenDoorCollisionRects());
                maxEC = rects.Count();
                if (!visualize)
                {
                    foreach (var door in room.DoorRooms)
                    {
                        var d = (Y_CMRoom)door.Value.First();
                        rects.AddRange(d.Collision.GetCollisionRectangles());
                    }
                }
                minEC = 4;
            }
            else
            {
                rects.AddRange(room.Collision.GetCollisionRectangles());
                if (!visualize)
                {
                    foreach (var door in room.DoorRooms)
                    {
                        // add all doors and make sure they are in the opened state
                        var d = (Y_Door)door.Value.First();
                        if (d.DoorIsOpen())
                            rects.AddRange(d.GetOpenDoorCollisionRects());
                        else
                            rects.AddRange(d.GetClosedDoorCollisionRects());
                    }
                }
                minEC = 0;
                maxEC = 0;
            }


            int ridiculouslyHeighWall = Y_Level.TextureTileSize * (visualize ? 5 : 15);
            float scale = Y_Level.GlobalScale;
            float elev = Y_Level.TextureTileSize;
            int shift = 0;
            List<X_Point3> points = new List<X_Point3>();
            int counter = 0;
            int tEH;
            foreach (var rect in rects)
            {
                if(counter >= minEC && counter < maxEC) tEH = ridiculouslyHeighWall;
                else tEH = 0;

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
                            new Vector3(x, y, shift + tEH),
                            new Vector3(x, y+h, shift + tEH),
                            new Vector3(x+w, y+h, shift + tEH),
                            new Vector3(x+w, y, shift + tEH)
                };

                counter++;
                points.AddRange(vertices);
            }

            if(visualize)
            {
                if (room.WhatAreYou() == X_LevelElements.Door)
                {
                    var room1 = room.DoorRooms.Values.ToList().First().First().Name;
                    var room2 = room.DoorRooms.Values.ToList().Last().First().Name;
                    model += toWavefrontObj(points, "./logs/Door_" + room1 + "_to_" + room2 + ".obj", room, ref globalOffset);
                }
                else
                {
                    model += toWavefrontObj(points, "./logs/Room_" + room.Name + ".obj", room, ref globalOffset);
                }
            }

            return points.ToArray();
        }

        private static string toWavefrontObj(List<X_Point3> points, string name, IWalkable room, ref int globalOffset)
        {
            string sc = "\n# Level part " + name;
            string s = "# Level part " + name;
            int index = 0;

            var scale = 0.001f;
            for (int v = 0; v < points.Count(); v++)
            {
                if (v % 8 == 0)
                {
                    s += "o cube" + index + "\n";
                    sc += "o cube" + index + "\n";
                    index++;
                }

                var p00 = points[v];
                s += ("v " + scale * (p00.X - room.Rect.Center.X) + " " + scale * (p00.Y - room.Rect.Center.Y) + " " + scale * p00.Z + "\n");
                sc += ("v " + scale * p00.X + " " + scale * p00.Y + " " + scale * p00.Z + "\n");
            }

            s += "\n";
            for (int offset = globalOffset; offset < points.Count()+ globalOffset; offset += 8)
            {
                for (int i = 0; i < 36; i += 3)
                {
                    sc += ("f " + (indices[i] + 1 + offset) + " " + (indices[i + 1] + 1 + offset) + " " + (indices[i + 2] + 1 + offset) + "\n");
                    s += ("f " + (indices[i] + 1 + offset) + " " + (indices[i + 1] + 1 + offset) + " " + (indices[i + 2] + 1 + offset) + "\n");
                }
            }

            globalOffset += points.Count();

            File.WriteAllText(name, s);
            return sc;
        }

        //public string toWavefrontObj(int offset)
        //{
        //    string s = "";
        //    foreach (var v in _vertices)
        //    {
        //        s += ("v " + v.X + " " + v.Y + " " + v.Z + "\n");
        //    }
        //    s += "\n";
        //    for (int i = 0; i < _triangles.GetLength(0); ++i)
        //    {
        //        s += ("f " + (_triangles[i, 0] + 1 + offset) + " " + (_triangles[i, 1] + 1 + offset) + " " + (_triangles[i, 2] + 1 + offset) + "\n");
        //    }
        //    s += "\n";

        //    return s;
        //}
    }
}
