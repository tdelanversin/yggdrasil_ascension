using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace YGR
{
    public class X_Polygon
    {
        Vector2[][] _vertices;

        /// <summary>
        /// X_Polygon constructor if the polygon has only one part
        /// </summary>
        /// <param name="vertices">Array with vertices of polygon</param>
        public X_Polygon(float[,] vertices)
        {
            if(vertices.GetLength(1) != 2)
            {
                Logger.Error("Dimension of vertices must be Nx2 but is " + vertices.GetLength(0).ToString() + "x" + vertices.GetLength(1).ToString() + "!");
            }

            _vertices = new Vector2[1][];
            _vertices[0] = new Vector2[vertices.GetLength(0) + 1];
            for(int i=0; i< _vertices[0].Length - 1; ++i)
            {
                _vertices[0][i] = new Vector2(vertices[i, 0], vertices[i, 1]);
            }
            _vertices[0][_vertices[0].Length - 1] = new Vector2(vertices[0, 0], vertices[0, 1]);
        }

        /// <summary>
        /// X_Polygon constructor if the polygon has multiple parts
        /// </summary>
        /// <param name="vertices">Array of 2D arrays with vertices of all parts of the polygon</param>
        public X_Polygon(float[][,] vertices)
        {
            if(vertices.Length < 1)
            {
                Logger.Error("At least one set of vertices is required!");
            }
            if (vertices[0].GetLength(1) != 2)
            {
                Logger.Error("Dimension of vertices must be Nx2 but is " + vertices.GetLength(0).ToString() + "x" + vertices.GetLength(1).ToString() + "!");
            }

            _vertices = new Vector2[vertices.Length][];
            for(int j=0; j<vertices.Length; ++j)
            {
                _vertices[j] = new Vector2[vertices[j].GetLength(0) + 1];
                for (int i = 0; i < _vertices[j].Length - 1 ; ++i)
                {
                    _vertices[j][i] = new Vector2(vertices[j][i, 0], vertices[j][i, 1]);
                }
                _vertices[j][_vertices[j].Length - 1] = new Vector2(vertices[j][0, 0], vertices[j][0, 1]);
            }
        }

        /// <summary>
        /// This Method will draw all internal structures for debugging purposes
        /// Note: potentially heavy impact on performace
        /// </summary>
        /// <param name="gameTime">Monogame GameTime</param>
        /// <param name="globalOffset">If you don't know what, put Vector2.Zero</param>
        /// <param name="spriteBatch">Active Monogame SpriteBatch</param>
        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            for (var j = 0; j < _vertices.Length; ++j)
            {
                for (var i = 1; i < _vertices[j].Length; ++i)
                {
                    Vector2 w = _vertices[j][i - 1];
                    Vector2 dw = _vertices[j][i] - w;

                    Texture2D tex = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                    tex.SetData(new[] { Color.White });
                    int distance = (int)dw.Length();
                    var angle = (float)Math.Atan2(dw.Y, dw.X);
                    spriteBatch.Draw(tex, new Rectangle((int)(w.X + globalOffset.X), (int)(w.Y + globalOffset.Y), distance, 3), null, Color.Red, angle, new Vector2(0, 0), SpriteEffects.None, 0);
                }
            }
        }

        public bool Inside(Vector2 p, Vector2 toLocal)
        {
            return true;
        }

        /// <summary>
        /// This method returns true if point p is inside the X_Polygon
        /// 
        /// Source: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// =====================================================================
        /// UNTESTED! ### UNTESTED! ### UNTESTED! ### UNTESTED! ### UNTESTED! ###
        /// =====================================================================
        /// </summary>
        /// <param name="p">Point to test in local coordinates</param>
        /// <returns>Returns true if p is inside the polygon</returns>
        public bool Inside(Vector2 p)
        {
            bool result = false;
            foreach (var polygon in _vertices)
            {
                int j = polygon.Length - 1;
                for (int i = 0; i < polygon.Length; i++)
                {
                    if (polygon[i].Y < p.Y && polygon[j].Y >= p.Y || polygon[j].Y < p.Y && polygon[i].Y >= p.Y)
                    {
                        if (polygon[i].X + (p.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < p.X)
                        {
                            result = !result;
                        }
                    }
                    j = i;
                }
            }
            
            return result;
        }

        /// <summary>
        /// This method splits vector dp into two components and checks them individually for collisions.
        /// </summary>
        /// <param name="p">Position before update in global coordinates</param>
        /// <param name="dp">Delta-Position before update</param>
        /// <param name="toLocal">Transformation from global to local coordinates for the current room</param>
        /// <param name="collision">Reference parameter: true if collision happened</param>
        /// <returns>This method returns the new position after clamping according to collisions</returns>
        public Vector2 Clamp(Vector2 p, Vector2 dp, Vector2 toLocal, ref bool collision)
        {
            Vector2 dpx = new Vector2(dp.X, float.Epsilon);
            Vector2 dpy = new Vector2(float.Epsilon, dp.Y);
            collision = false;

            Vector2 pLocal = p + toLocal;
            for (var j=0; j<_vertices.Length; ++j)
            {
                for (var i = 1; i < _vertices[j].Length; ++i)
                {
                    Vector2 wLocal = _vertices[j][i - 1];
                    Vector2 dw = _vertices[j][i] - wLocal;

                    float u = -1.0f;
                    float v = -1.0f;
                    if (intersect(pLocal, dpx, wLocal, dw, ref u, ref v))
                    {
                        dpx.X *= u;
                        collision = true;
                    }

                    u = -1.0f;
                    v = -1.0f;
                    if (intersect(pLocal, dpy, wLocal, dw, ref u, ref v))
                    {
                        dpy.Y *= u;
                        collision = true;
                    }

                    if (collision)
                    {
                        return new Vector2(p.X + dpx.X, p.Y + dpy.Y);
                    }

                }
            }

            return p + dp;
        }

        /// <summary>
        /// This method intersects two rays in the most standard way possible. Note: don't normalize the directional vectors
        /// </summary>
        /// <param name="p">Origin of ray 1</param>
        /// <param name="dp">Unnormalized direction of ray 1</param>
        /// <param name="w">Origin of ray 2</param>
        /// <param name="dw">Unnormalized direction of ray 2</param>
        /// <param name="u">Reference parameter will be between 0 and 1 if collision happened</param>
        /// <param name="v">Reference parameter will be between 0 and 1 if collision happened</param>
        /// <returns>Returns true if an intersection exists</returns>
        private bool intersect(Vector2 p, Vector2 dp, Vector2 w, Vector2 dw, ref float u, ref float v)
        {
            if (Math.Abs(dp.X) > float.Epsilon)
            {
                v = (dp.X * (w.Y - p.Y) + dp.Y * (p.X - w.X)) / (dp.Y * dw.X - dw.Y * dp.X);
                u = (w.X + dw.X * v - p.X) / dp.X;
            }
            else
            {
                u = (dw.X * (w.Y - p.Y) + dw.Y * (p.X - w.X)) / (dp.Y * dw.X);
                v = (p.X - w.X) / dw.X;
            }

            if (v > 0.0f && u > 0.0f && v < 1.0f && u < 1.0f)
            {
                return true;
            }

            return false;
        }
    }
}
