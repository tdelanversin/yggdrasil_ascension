//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;


//namespace YGR { 

//    public struct Cubes
//    {
//        public Vector3[] Vertices;
//    }

//    public class X_Cube
//    {
//        private Vector3[] _vertices;
//        private int[,] _triangles;
//        public bool IsWall { get; }
//        public X_Cube(Vector3[] vertices, int[,] triangles, bool isWall)
//        {
//            _vertices = vertices;
//            _triangles = triangles;
//            IsWall = isWall;
//        }

//        public Vector3[] GetVertices()
//        {
//            return _vertices;
//        }

//        public int VertexCount()
//        {
//            return _vertices.Length;
//        }

//        public int TriangleCount()
//        {
//            return _triangles.GetLength(0);
//        }

//        public string toWavefrontObj(int offset)
//        {
//            string s = "";
//            foreach (var v in _vertices)
//            {
//                s += ("v " + v.X + " " + v.Y + " " + v.Z + "\n");
//            }
//            s += "\n";
//            for (int i = 0; i < _triangles.GetLength(0); ++i)
//            {
//                s += ("f " + (_triangles[i, 0] + 1 + offset) + " " + (_triangles[i, 1] + 1 + offset) + " " + (_triangles[i, 2] + 1 + offset) + "\n");
//            }
//            s += "\n";

//            return s;
//        }

//        /*
//         * Source: Nori
//         */
//        //public bool RayIntersect(Vector3 origin, Vector3 direction)
//        //{
//        //    float length = direction.Length();
//        //    direction.Normalize();

//        //    int len = _triangles.GetLength(0);

//        //    for (int ind=0; ind < len; ++ind)
//        //    {
//        //        Vector3 p0 = _vertices[_triangles[ind, 0]];
//        //        Vector3 p1 = _vertices[_triangles[ind, 1]];
//        //        Vector3 p2 = _vertices[_triangles[ind, 2]];

//        //        // Find vectors for two edges sharing v[0]
//        //        Vector3 edge1 = p1 - p0; Vector3 edge2 = p2 - p0;

//        //        // remove backside
//        //        Vector3 n = Manager_Light.CrossProduct(edge1, edge2);
//        //        n.Normalize();
//        //        if (Manager_Light.Dot(direction, n) > float.Epsilon)
//        //            continue;

//        //        // Begin calculating determinant - also used to calculate U parameter
//        //        Vector3 pvec = Manager_Light.CrossProduct(direction, edge2);

//        //        // If determinant is near zero, ray lies in plane of triangle
//        //        float det = Manager_Light.Dot(edge1, pvec);

//        //        if (det > -float.Epsilon && det < float.Epsilon)
//        //            continue;
//        //            //return false;
//        //        float invDet = 1.0f / det;

//        //        // Calculate distance from v[0] to ray origin
//        //        Vector3 tvec = origin - p0;

//        //        // Calculate U parameter and test bounds
//        //        float u = Manager_Light.Dot(tvec, pvec) * invDet;
//        //        if (u < 0.0 || u > 1.0)
//        //            continue;
//        //            //return false;

//        //        // Prepare to test V parameter
//        //        Vector3 qvec = Manager_Light.CrossProduct(tvec, edge1);

//        //        // Calculate V parameter and test bounds
//        //        float v = Manager_Light.Dot(direction, qvec) * invDet;
//        //        if (v < 0.0 || u + v > 1.0)
//        //            continue;
//        //            //return false;

//        //        // Ray intersects triangle -> compute t
//        //        float t = Manager_Light.Dot(edge2, qvec) * invDet;

//        //        if (t >= float.Epsilon && t <= length) {
//        //            //hitPoint = origin + t * direction;
//        //            return true;
//        //        }
//        //    }

//        //    //hitPoint = new Vector3(0, 0, 0);
//        //    return false;
//        //}
//    }

//    public static class Manager_Light
//    {
//        public enum Caster
//        {
//            Light = 0,
//            Shadow
//        }

//        public struct Coordinate
//        {
//            public Vector3 Position;
//            public int Index;
//            public int Lighted;
//            public int GlobalID;
//        }

//        public struct X_Vector3
//        {
//            public float X;
//            public float Y;
//            public float Z;
//            public int GlobalID;
//            public int Index;
//            public int Lighted;
//        }

//        public struct X_Point3
//        {
//            public float X;
//            public float Y;
//            public float Z;
//        }

//        //public static List<X_Cube> IlluminationModelOpened { get; private set; }
//        //public static List<X_Cube> IlluminationModelClosed { get; private set; }

//        public static Vector3[] IlluminationModelOpened { get; private set; }
//        public static Vector3[] IlluminationModelClosed { get; private set; }

//        public static Dictionary<string, bool[]> LoadedIlluminationTemplates { get; private set; }
//        public static string ShadeVersion { get { return "V3"; } }
//        public static GraphicsDevice GraphicsDevice_ { get; set; }

//        private static StructuredBuffer IlluminationBufferOpened { get; set; }
//        private static StructuredBuffer IlluminationBufferClosed { get; set; }
//        private static StructuredBuffer LightsBuffer { get; set; }
//        private static StructuredBuffer LightedBuffer { get; set; }
//        private static StructuredBuffer CoordsBuffer { get; set; }
//        private static Effect IlluminationShaderOpened { get; set; }
//        private static Effect IlluminationShaderClosed { get; set; }

//        static int[] indices = new int[]
//        {
//            /* bottom */ 0, 1, 2,   0, 2, 3,
//            /* right  */ 2, 6, 3,   3, 6, 7,
//            /* front  */ 1, 5, 2,   2, 5, 6,
//            /* left   */ 0, 4, 1,   1, 4, 5,
//            /* back   */ 0, 3, 7,   0, 7, 4,
//            /* top    */ 5, 4, 6,   6, 4, 7
//        };

//        static public Vector3[] Vertices;
//        static public int NumVerts;
//        static public int NumLights;
//        static public int NumCoords;
//        static public X_Point3[] TestVals;
//        static public float eps = 1.0e-7f;

//        static public bool RayIntersect(Vector3 origin, Vector3 direction, int globalIDx)
//        {
//            float length = direction.Length();
//            direction.Normalize();

//            for (int ind = 0; ind < NumVerts; ind += 8)
//            {
//                int index = 0;
//                for (int i = 0; i < 36; i += 3)
//                {
//                    Vector3 p0 = Vertices[ind + indices[i]];
//                    Vector3 p1 = Vertices[ind + indices[i + 1]];
//                    Vector3 p2 = Vertices[ind + indices[i + 2]];

//                    // Find vectors for two edges sharing v[0]
//                    Vector3 edge1 = p1 - p0;
//                    Vector3 edge2 = p2 - p0;

//                    // remove backside
//                    Vector3 n = Manager_Light.CrossProduct(edge1, edge2);
//                    n.Normalize();
//                    if (Manager_Light.Dot(direction, n) > eps)
//                        continue;

//                    // Begin calculating determinant - also used to calculate U parameter
//                    Vector3 pvec = Manager_Light.CrossProduct(direction, edge2);

//                    // If determinant is near zero, ray lies in plane of triangle
//                    float det = Manager_Light.Dot(edge1, pvec);

//                    if (det > -float.Epsilon && det < eps)
//                        continue;
//                    //return false;
//                    float invDet = 1.0f / det;

//                    // Calculate distance from v[0] to ray origin
//                    Vector3 tvec = origin - p0;

//                    // Calculate U parameter and test bounds
//                    float u = Manager_Light.Dot(tvec, pvec) * invDet;
//                    if (u < 0.0 || u > 1.0)
//                        continue;
//                    //return false;

//                    // Prepare to test V parameter
//                    Vector3 qvec = Manager_Light.CrossProduct(tvec, edge1);

//                    // Calculate V parameter and test bounds
//                    float v = Manager_Light.Dot(direction, qvec) * invDet;
//                    if (v < 0.0 || u + v > 1.0)
//                        continue;
//                    //return false;

//                    // Ray intersects triangle -> compute t
//                    float t = Manager_Light.Dot(edge2, qvec) * invDet;

//                    if (t >= eps && t <= length)
//                    {
//                        //hitPoint = origin + t * direction;
//                        //if (globalIDx > -1)
//                        //{
//                        //    if (index == 2)
//                        //    {
//                        //        TestVals[globalIDx].X = 1;
//                        //        TestVals[globalIDx].Y = 0;
//                        //        TestVals[globalIDx].Z = 0;
//                        //    }
//                        //    index++;
//                        //}
//                        return true;
//                    }
//                }
//                //hitPoint = new Vector3(0, 0, 0);
//            }
//            return false;
//        }

//        public static float Dot(Vector3 lhs, Vector3 rhs)
//        {
//            return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
//        }

//        public static Vector3 CrossProduct(Vector3 lhs, Vector3 rhs)
//        {
//            return new Vector3(
//                lhs.Y * rhs.Z - lhs.Z * rhs.Y,
//                lhs.Z * rhs.X - lhs.X * rhs.Z,
//                lhs.X * rhs.Y - lhs.Y * rhs.X
//                );
//        }

//        public static void Initialize(string resourceFolder, ContentManager content, GraphicsDevice graphicsDevice)
//        {
//            LoadedIlluminationTemplates = new Dictionary<string, bool[]>();

//            IlluminationShaderClosed = content.Load<Effect>("Illuminator");
//            IlluminationShaderOpened = content.Load<Effect>("Illuminator");

//            GraphicsDevice_ = graphicsDevice;
//            //var files = Directory.GetDirectories(resourceFolder);
//            //foreach(var f in files)
//            //{
//            //    var roomName = f.Split(Path.DirectorySeparatorChar).Last();
//            //    //var name = Path.GetDirectoryName(f);
//            //}
//            ////string dataFileName = Path.GetFileName(files
//            ////    .Where(x => Path.GetFileName(x).Contains(fileName) && Path.GetFileName(x).EndsWith(".shade"))
//            ////    .FirstOrDefault());
//        }

//        public struct LightedState
//        {
//            public int IsLighted;
//        }

//        public static bool[] Illuminate(
//            List<X_Light> lights,
//            //IWalkable room,
//            int[][] collisionTemplate,
//            int tileSize,
//            Vector3 unscaledOffset,
//            bool open = true,
//            Caster casterType = Caster.Shadow
//        )
//        {
//            if ((open && IlluminationModelOpened == null) || (!open && IlluminationModelClosed == null))
//            {
//                Logger.Error("Trying to illuminate a room without illumination model for the " + (open ? "opened" : "closed") + " state of the level!");
//            }

//            // if we cast shadow type, then the shadowed part will contain the value false
//            // if we cast light type then the shadowed part will contain the value true
//            //bool sign = (casterType == Caster.Shadow ? false : true);
//            bool light = (casterType == Caster.Shadow ? true : false);

//            //var model = (open ? IlluminationModelOpened : IlluminationModelClosed);
//            Vertices = (open ? IlluminationModelOpened : IlluminationModelClosed);
//            NumLights = lights.Count();
//            NumVerts = Vertices.Count();

//            var template = collisionTemplate;

//            int width = template[0].Length * tileSize;
//            int length = width * template.Length * tileSize;
//            bool[] lighted = Enumerable.Repeat<bool>(!light, length).ToArray();

//            Vector3 offset = unscaledOffset;
//            X_Vector3[] coords = new X_Vector3[length]; // Enumerable.Repeat<Vector3>(Vector3.Zero, length).ToArray();

//            int shadowSpotSize = 1;
//            float tto = 0.001f;
//            int shadowSize = (tileSize + shadowSpotSize / 2) * (tileSize + shadowSpotSize / 2);
//            int baseIndex = 0;

//            Parallel.For(0, template.Length, h =>
//            //for (int h=0; h < template.Length; ++h)
//            {
//                for (int w = 0; w < template[0].Length; ++w)
//                {
//                    if (template[h][w] == (int)X_TileType.DontCare) continue;

//                    int fromX = w * tileSize + shadowSpotSize / 2;
//                    int fromY = h * tileSize + shadowSpotSize / 2;
//                    int toX = fromX + tileSize - shadowSpotSize / 2;
//                    int toY = fromY + tileSize - shadowSpotSize / 2;

//                    if (template[h][w] == (int)X_TileType.Roof || template[h][w] == (int)X_TileType.Outside)
//                    {
//                        continue;
//                        //shadowMap[y * width + x] = false;
//                    }

//                    // pretest if any part of the tile is not visible. If any edge is not visible from the light
//                    // we need to calculate shadows for it
//                    List<Vector3> pts = new List<Vector3>();
//                    if (template[h][w] == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
//                    {
//                        pts.Add(new Vector3(fromX + tto, fromY + tto - tileSize, -tileSize - 0.001f) + offset);
//                        pts.Add(new Vector3(fromX - tto + tileSize, fromY + tto - tileSize, -tileSize - 0.001f) + offset);
//                        pts.Add(new Vector3(fromX + tto, fromY - tto - tileSize + tileSize, -tileSize - 0.001f) + offset);
//                        pts.Add(new Vector3(fromX - tto + tileSize, fromY - tto - tileSize + tileSize, -tileSize - 0.001f) + offset);
//                    }
//                    else if (template[h][w] == (int)X_TileType.Wall)
//                    {
//                        pts.Add(new Vector3(fromX + tto, fromY + tto + 0.001f, -(0)) + offset);
//                        pts.Add(new Vector3(fromX - tto + tileSize, fromY + tto + 0.001f, -(0)) + offset);
//                        pts.Add(new Vector3(fromX + tto, fromY - tto + tileSize + 0.001f, -(tileSize)) + offset);
//                        pts.Add(new Vector3(fromX - tto + tileSize - tto, fromY + 0.001f, -(tileSize)) + offset);
//                    }
//                    foreach (var lightSource in lights)
//                    {
//                        Vector3 orig = lightSource.GetUnscaledPosition();
//                        foreach (var p in pts)
//                        {
//                            //if (light.GetUnscaledIlluminationRect().Contains(new Point((int)p.X, (int)p.Y)))
//                            //{
//                            //foreach (var cube in model)
//                            //{
//                            var intersects = Manager_Light.RayIntersect(orig, p - orig, -1);
//                            if (intersects)
//                            {
//                                goto add_tile;
//                            }
//                            //}
//                            //}
//                        }
//                    }

//                    // if we have shadow, assume that non intersected parts are automatically in the light
//                    // otherwise keep them in the shadow and only light the parts that have direct line of sight
//                    for (int x = fromX; x < toX; x += shadowSpotSize)
//                    {
//                        for (int y = fromY; y < toY; y += shadowSpotSize)
//                        {
//                            lighted[y * width + x] = light;
//                        }
//                    }
//                    continue;

//                add_tile:
//                    int end = Interlocked.Add(ref baseIndex, shadowSize);
//                    int index = end - shadowSize;
//                    for (int x = fromX; x < toX; x += shadowSpotSize)
//                    {
//                        for (int y = fromY; y < toY; y += shadowSpotSize)
//                        {
//                            //if (template[h][w] == (int)X_TileType.Roof || template[h][w] == (int)X_TileType.Outside)
//                            //{
//                            //    continue;
//                            //    //shadowMap[y * width + x] = false;
//                            //}
//                            //tileType[y * width + x] = (X_TileType)template[h][w];
//                            if (template[h][w] == (int)X_TileType.Floor || template[h][w] == (int)X_TileType.Roof)
//                            {
//                                var s = new X_Vector3();
//                                s.Index = (y) * width + x;
//                                var temp = new Vector3(x, y - tileSize, -tileSize - 0.001f) + offset;
//                                s.X = temp.X;
//                                s.Y = temp.Y;
//                                s.Z = temp.Z;
//                                s.GlobalID = -1;
//                                s.Lighted = 0;

//                                coords[index] = s;
//                                index++;
//                                //coords[y * width + x] = new Vector3(x, y - tileSize, -tileSize - 0.001f) + offset;
//                                //textureMap[y * width + x] = (y) * width + x;
//                            }
//                            else if (template[h][w] == (int)X_TileType.Wall)
//                            {
//                                var s = new X_Vector3();
//                                s.Index = (y) * width + x;
//                                var temp = new Vector3(x, fromY + 0.001f, -(y - fromY)) + offset;
//                                s.X = temp.X;
//                                s.Y = temp.Y;
//                                s.Z = temp.Z;
//                                s.GlobalID = -1;
//                                s.Lighted = 0;
//                                coords[index] = s;
//                                index++;
//                                //coords[y * width + x] = new Vector3(x, fromY + 0.001f, -(y - fromY)) + offset;
//                                //textureMap[y * width + x] = (y) * width + x;
//                            }
//                        }
//                    }
//                }
//            });

//            //TestVals = Enumerable.Repeat<X_Point3>(new X_Point3(), baseIndex).ToArray();
//            bool cpu = false;
//            X_Vector3[] result;
//            if (cpu)
//            {
//                Vector3[] Lights = lights.Select(x => x.GetUnscaledPosition()).ToArray();
//                //float scale = room.Scale;
//                Parallel.For(0, baseIndex, i =>
//                //for (int i = 0; i < baseIndex; ++i)
//                {
//                    ref var p = ref coords[i];
//                    //bool intersected = false;
//                    //int l = 0;
//                    for (int l = 0; l < Lights.Length; ++l)
//                    {
//                        //Vector3 lightPos = lightSource.GetUnscaledPosition();
//                        if (p.Lighted == 1) return;
//                        Vector3 pos = new Vector3(p.X, p.Y, p.Z);
//                        Vector3 direction = pos - Lights[l];
//                        if (!Manager_Light.RayIntersect(Lights[l], direction, i))
//                        {
//                            p.Lighted = 1;
//                            //lighted[p.Index] = light;
//                            return;
//                            //intersected = true;
//                            //break;
//                        }
//                    }
//                });

//                result = coords;
//            }

//            // =============================================================================

//            else if (baseIndex > 0)
//            {
//                //var testVals = new StructuredBuffer(
//                //    GraphicsDevice_, typeof(X_Point3), baseIndex, BufferUsage.None, ShaderAccess.ReadWrite);
//                //// =============================================================================
//                //// fucking shader model (open or closed)
//                if (lights.Count() > 1)
//                    Logger.Info("");
//                var shaderModel = (open ? IlluminationShaderOpened : IlluminationShaderClosed);
//                if (shaderModel.Parameters["NumLights"] != null)
//                    shaderModel.Parameters["NumLights"].SetValue(lights.Count());
//                if (shaderModel.Parameters["NumCoords"] != null)
//                    shaderModel.Parameters["NumCoords"].SetValue(baseIndex);
//                //// =============================================================================

//                //X_Vector3[] coordsVec = new X_Vector3[baseIndex];
//                //Parallel.For(0, baseIndex, i =>
//                ////for (int i = 0; i < baseIndex; ++i)
//                //{
//                //    ref var c = ref coords[i];
//                //    coordsVec[i].X = c.X;
//                //    coordsVec[i].Y = c.Y;
//                //    coordsVec[i].Z = c.Z;
//                //    coordsVec[i].Index = c.Index;
//                //    coordsVec[i].Lighted = 0;
//                //    coordsVec[i].GlobalID = -1;
//                //});

//                //// =============================================================================
//                //// fucking set input coordinates buffer
//                CoordsBuffer.SetData<X_Vector3>(coords);
//                if (shaderModel.Parameters["Coords"] != null)
//                    shaderModel.Parameters["Coords"].SetValue(CoordsBuffer);

//                //// fucking set light positions
//                var lightsPos = lights.Select(x => x.GetUnscaledPosition()).ToArray();
//                X_Point3[] lightsVec = new X_Point3[lightsPos.Length];
//                for (int i = 0; i < lightsPos.Length; ++i)
//                {
//                    lightsVec[i].X = lightsPos[i].X;
//                    lightsVec[i].Y = lightsPos[i].Y;
//                    lightsVec[i].Z = lightsPos[i].Z;
//                }
//                LightsBuffer.SetData<X_Point3>(lightsVec);
//                if (shaderModel.Parameters["Lights"] != null)
//                    shaderModel.Parameters["Lights"].SetValue(LightsBuffer);

//                //retValBuf.SetData<int>(new int[] { 0 });
//                //if (shaderModel.Parameters["NumStructs"] != null)
//                //    shaderModel.Parameters["NumStructs"].SetValue(retValBuf);

//                // create fucking buffer that is not needed currently
//                //StructuredBuffer input = new StructuredBuffer(
//                //    GraphicsDevice_, typeof(Vector3), baseIndex, BufferUsage.None, ShaderAccess.ReadWrite);

//                //X_Point3[] array = new X_Point3[baseIndex];
//                //testVals.SetData(array);
//                //if (shaderModel.Parameters["TestVals"] != null)
//                //    shaderModel.Parameters["TestVals"].SetValue(testVals);

//                foreach (var pass in shaderModel.CurrentTechnique.Passes)
//                {
//                    pass.ApplyCompute();
//                    int dispatchCount = (int)Math.Ceiling((double)baseIndex / 64.0);
//                    GraphicsDevice_.DispatchCompute(dispatchCount, 1, 1);
//                }

//                X_Vector3[] backCoords = new X_Vector3[baseIndex];
//                CoordsBuffer.GetData<X_Vector3>(backCoords, 0, baseIndex);

//                result = backCoords;

//                //X_Point3[] retv1 = new X_Point3[NumVerts];
//                //if (open)
//                //{
//                //    IlluminationBufferOpened.GetData(retv1, 0, NumVerts);
//                //}
//                //else
//                //{
//                //    IlluminationBufferClosed.GetData(retv1, 0, NumVerts);
//                //}

//                //X_Point3[] retv3 = new X_Point3[lights.Count()];
//                //LightsBuffer.GetData(retv3, 0, lights.Count());

//                //var outTestVals = new X_Point3[array.Length];
//                //testVals.GetData(outTestVals, 0, outTestVals.Length);

//                //for (int i = 0; i < baseIndex; ++i)
//                //{
//                //    ref var c = ref coords[i];
//                //    ref var oc = ref backCoords[i];
//                //    ref var t = ref TestVals[i];
//                //    ref var ot = ref outTestVals[i];

//                //    var tx = (int)Math.Round(t.X, 1);
//                //    var ty = (int)Math.Round(t.Y, 1);
//                //    var tz = (int)Math.Round(t.Z, 1);

//                //    var otx = (int)Math.Round(ot.X, 1);
//                //    var oty = (int)Math.Round(ot.Y, 1);
//                //    var otz = (int)Math.Round(ot.Z, 1);

//                //    if (!(tx <= otx + 1 && tx >= otx - 1) || 
//                //        !(ty <= oty + 1 && ty >= oty - 1) || 
//                //        !(tz <= otz + 1 && tz >= otz - 1))
//                //        Logger.Info("");
//                //}

//                //for (int i = 0; i < baseIndex; ++i)
//                //{
//                //    ref var c = ref coords[i];
//                //    ref var oc = ref backCoords[i];

//                //    if (!(c.Lighted == oc.Lighted))
//                //        Logger.Info("");
//                //}

//                //var count = backCoords.Where(x => x.Lighted == 0).Count();
//                //var count2 = backCoords.Where(x => x.Lighted == 1).Count();
//            }
//            else
//            {
//                result = new X_Vector3[0];
//            }

//            Parallel.For(0, baseIndex, i =>
//            //for (int i = 0; i < baseIndex; ++i)
//            {
//                if (result[i].Lighted == 1)
//                {
//                    lighted[result[i].Index] = true;
//                }
//            });

//            return lighted;
//            //saveShadeToFile(lighted, room, lights);
//        }

//        private static string combineIdentifiers(List<X_Light> lights, IWalkable room)
//        {
//            string identifier = "";
//            foreach (var light in lights)
//            {
//                if (light.GetScaledIlluminationRect().Intersects(room.Rect))
//                {
//                    identifier += light.GetIdentifier(room) + "|";
//                }
//            }
//            if (identifier.EndsWith("|"))
//                identifier = identifier.Substring(0, identifier.Length - 1);

//            return identifier;
//        }

//        public static void SaveShadeToFile(bool[] shadeTemplate, bool open, IWalkable room, List<X_Light> lights)
//        {
//            if (room.ResourceFolder != "")
//            {
//                string fileName = "shade_" + (open ? "opened_" : "closed_");
//                string identifier = DateTime.Now.ToLongDateString() + " - " + DateTime.Now.ToLongTimeString() + "\n";
//                identifier += Manager_Light.ShadeVersion + "\n";

//                identifier += combineIdentifiers(lights, room);
//                identifier += "\n";
//                identifier += string.Join("", shadeTemplate.Select(x => x ? "1" : "0"));

//                fileName += Util.CreateGenericIdentifier();
//                fileName += ".shade";

//                // write to all available directories: current runtime directory and source code directory
//                File.WriteAllText(room.ResourceFolder + fileName, identifier);
//                if (Debugger.IsAttached)
//                {
//                    var srcPath = Util.GetAbsResourceFolderPath(room.ResourceFolder);
//                    File.WriteAllText(srcPath + fileName, identifier);
//                }
//                LoadedIlluminationTemplates.Add(room.ResourceFolder + fileName, shadeTemplate);
//            }
//        }

//        public static bool[] LoadShadeFromFile(bool open, IWalkable room, List<X_Light> lights)
//        {
//            string fileName = "shade_" + (open ? "opened_" : "closed_");
//            var files = Directory.GetFiles(room.ResourceFolder);
//            string dataFileName = Path.GetFileName(files
//                .Where(x => Path.GetFileName(x).Contains(fileName) && Path.GetFileName(x).EndsWith(".shade"))
//                .FirstOrDefault());

//            if (dataFileName == null) return null;

//            if (LoadedIlluminationTemplates.ContainsKey(room.ResourceFolder + fileName))
//                return LoadedIlluminationTemplates[room.ResourceFolder + fileName];

//            var contents = File.ReadAllLines(room.ResourceFolder + dataFileName);
//            var vCorr = contents[1] == Manager_Light.ShadeVersion;
//            if (!vCorr) return null;

//            var iCorr = contents[2] == combineIdentifiers(lights, room);
//            if (!iCorr) return null;

//            LoadedIlluminationTemplates.Add(room.ResourceFolder + fileName, contents[3].Select(c => c == '1').ToArray());

//            return LoadedIlluminationTemplates[room.ResourceFolder + fileName];
//        }

//        public static void RemoveAllShadeFiles(IWalkable room, bool open)
//        {
//            string fileName = "shade_" + (open ? "opened_" : "closed_");
//            var files = Directory.GetFiles(room.ResourceFolder);
//            var sFiles = files.Where(x => Path.GetFileName(x).Contains(fileName) && Path.GetFileName(x).EndsWith(".shade")).ToArray();
//            foreach (var f in sFiles)
//            {
//                string dataFileName = Path.GetFileName(f);
//                string dataRessourceFolder = Util.GetAbsResourceFolderPath(room.ResourceFolder);
//                File.Delete(dataRessourceFolder + dataFileName);
//            }
//        }

//        private static void output(bool[,] pattern, string name)
//        {
//            string s = "";
//            for (int x = 0; x < pattern.GetLength(0); ++x)
//            {
//                s += string.Join("\t", Enumerable.Range(0, pattern.GetLength(1)).Select(y => pattern[x, y]).ToArray());
//                s += "\n";
//            }

//            File.WriteAllText(name, s);
//        }

//        private static void output(int[][] pattern, string name)
//        {
//            string s = "";
//            for (int x = 0; x < pattern.GetLength(0); ++x)
//            {
//                s += string.Join("\t", pattern[x]);
//                s += "\n";
//            }

//            File.WriteAllText(name, s);
//        }

//        public static void CreateModel(Y_Level level)
//        {
//            Func<Y_Level, bool, List<X_Cube>> elevate = (level, open) => {
//                List<X_Cube> cubes = new List<X_Cube>();

//                foreach (var room in level.Rooms.Values)
//                {
//                    // make sure that we open the door first because
//                    // we want that state of the door to be shaded
//                    if (open && room.WhatAreYou() == X_LevelElements.Door)
//                    {
//                        ((Y_Door)room).OpenDoor();
//                    }
//                    var rects = room.Collision.GetCollisionRectangles().Clone() as Rectangle[];

//                    if (open && room.WhatAreYou() == X_LevelElements.Door)
//                    {
//                        ((Y_Door)room).CloseDoor();
//                    }

//                    int[,] indicesRoof = new int[,]
//                    {
//                /* bottom */ {0,1,2}, {0,2,3},
//                /* right  */ {2,6,3}, {3,6,7},
//                /* front  */ {1,5,2}, {2,5,6},
//                /* left   */ {0,4,1}, {1,4,5},
//                /* back   */ {0,3,7}, {0,7,4},
//                /* top    */ {5,4,6}, {6,4,7}
//                    };

//                    float scale = room.Scale;
//                    float elev = room.Collision.TileHeight / scale;
//                    int shift = 5;
//                    foreach (var rect in rects)
//                    {
//                        float x = (rect.X - shift + 0.5f) / scale;
//                        float y = (rect.Y - shift) / scale;
//                        float h = (rect.Height + shift) / scale;
//                        float w = (rect.Width + shift) / scale;
//                        float e = elev + 1;
//                        Vector3[] vertices = new Vector3[] {
//                            new Vector3(x, y, -e),
//                            new Vector3(x, y+h, -e),
//                            new Vector3(x+w, y+h, -e),
//                            new Vector3(x+w, y, -e),
//                            new Vector3(x, y, shift),
//                            new Vector3(x, y+h, shift),
//                            new Vector3(x+w, y+h, shift),
//                            new Vector3(x+w, y, shift)
//                        };

//                        cubes.Add(new X_Cube(vertices, indicesRoof, false));
//                    }
//                }
//                return cubes;
//            };

//            var illuminationModelOpened = elevate(level, true);
//            var list = new List<Vector3>();
//            foreach (var cube in illuminationModelOpened)
//            {
//                list.AddRange(cube.GetVertices());
//            }
//            IlluminationModelOpened = list.ToArray();

//            var illuminationModelClosed = elevate(level, false);
//            list.Clear();
//            foreach (var cube in illuminationModelClosed)
//            {
//                list.AddRange(cube.GetVertices());
//            }
//            IlluminationModelClosed = list.ToArray();

//            // =====================================================================================

//            IlluminationBufferClosed = new StructuredBuffer(
//                GraphicsDevice_,
//                typeof(X_Point3), IlluminationModelClosed.Length,
//                BufferUsage.None, ShaderAccess.Read);

//            IlluminationBufferOpened = new StructuredBuffer(
//                GraphicsDevice_,
//                typeof(X_Point3), IlluminationModelOpened.Length,
//                BufferUsage.None, ShaderAccess.Read);

//            var illuminationModelClosedX = new X_Point3[IlluminationModelClosed.Count()];
//            for (int i = 0; i < illuminationModelClosedX.Length; ++i)
//            {
//                illuminationModelClosedX[i] = new X_Point3();
//                illuminationModelClosedX[i].X = IlluminationModelClosed[i].X;
//                illuminationModelClosedX[i].Y = IlluminationModelClosed[i].Y;
//                illuminationModelClosedX[i].Z = IlluminationModelClosed[i].Z;
//            }

//            var illuminationModelOpenedX = new X_Point3[IlluminationModelOpened.Count()];
//            for (int i = 0; i < illuminationModelOpenedX.Length; ++i)
//            {
//                illuminationModelOpenedX[i] = new X_Point3();
//                illuminationModelOpenedX[i].X = IlluminationModelOpened[i].X;
//                illuminationModelOpenedX[i].Y = IlluminationModelOpened[i].Y;
//                illuminationModelOpenedX[i].Z = IlluminationModelOpened[i].Z;
//            }

//            IlluminationBufferClosed.SetData(illuminationModelClosedX, 0, illuminationModelClosedX.Length);
//            IlluminationBufferOpened.SetData(illuminationModelOpenedX, 0, illuminationModelOpenedX.Length);

//            if (IlluminationShaderClosed.Parameters["Vertices"] != null)
//                IlluminationShaderClosed.Parameters["Vertices"].SetValue(IlluminationBufferClosed);
//            if (IlluminationShaderOpened.Parameters["Vertices"] != null)
//                IlluminationShaderOpened.Parameters["Vertices"].SetValue(IlluminationBufferOpened);

//            if (IlluminationShaderClosed.Parameters["NumVerts"] != null)
//                IlluminationShaderClosed.Parameters["NumVerts"].SetValue(illuminationModelClosedX.Length);
//            if (IlluminationShaderOpened.Parameters["NumVerts"] != null)
//                IlluminationShaderOpened.Parameters["NumVerts"].SetValue(illuminationModelOpenedX.Length);

//            if (IlluminationShaderClosed.Parameters["NumVerts"] != null)
//                IlluminationShaderClosed.Parameters["NumVerts"].SetValue(illuminationModelClosedX.Length);
//            if (IlluminationShaderOpened.Parameters["NumVerts"] != null)
//                IlluminationShaderOpened.Parameters["NumVerts"].SetValue(illuminationModelOpenedX.Length);
//            // =====================================================================================

//            //Vector3[] retv1 = new Vector3[32];
//            //Vector3[] retv2 = new Vector3[32];
//            //IlluminationBufferClosed.GetData(retv1, 0, 32);
//            //IlluminationBufferOpened.GetData(retv2, 0, 32);

//            // get the largest room dimensions
//            int maxLen = int.MinValue;
//            foreach (var room in level.Rooms)
//            {
//                int width = room.Value.Collision.GetCollisionTemplate()[0].Length * room.Value.TextureTileSize;
//                int length = width * room.Value.Collision.GetCollisionTemplate().Length * room.Value.TextureTileSize;
//                if (length > maxLen) maxLen = length;
//            }

//            LightsBuffer = new StructuredBuffer(
//                GraphicsDevice_, typeof(X_Vector3), 5, BufferUsage.None, ShaderAccess.Read);

//            CoordsBuffer = new StructuredBuffer(
//                GraphicsDevice_, typeof(X_Vector3), maxLen, BufferUsage.None, ShaderAccess.ReadWrite);

//            //toWavefrontObj(cubes, "./logs/cubes.obj");
//            //return cubes;
//        }

//        private static void toWavefrontObj(List<X_Cube> cubes, string name)
//        {
//            string s = "";
//            int i = 0;
//            int offset = 0;
//            foreach (var cube in cubes)
//            {
//                s += "o cube" + i + "\n";
//                s += (cube.toWavefrontObj(offset));
//                offset += cube.VertexCount();
//                i++;
//            }

//            File.WriteAllText(name, s);
//        }
//    }
//}
