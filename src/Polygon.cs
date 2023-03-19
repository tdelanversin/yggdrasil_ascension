using Clearcove.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace YGR
{
    public class Polygon
    {
        Vector2[][] _vertices;

        public Polygon(float[,] vertices)
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

        public Polygon(float[][,] vertices)
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

        public void Draw(GameTime gameTime, int ox, int oy, SpriteBatch spriteBatch)
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
                    spriteBatch.Draw(tex, new Rectangle((int)(w.X - ox), (int)(w.Y - oy), distance, 3), null, Color.Red, angle, new Vector2(0, 0), SpriteEffects.None, 0);
                }
            }
        }

        /**
         * Split vector dp into two components and check them individually
         */
        public Vector2 Clamp(Vector2 p, Vector2 dp, ref bool collision)
        {
            Vector2 dpx = new Vector2(dp.X, float.Epsilon);
            Vector2 dpy = new Vector2(float.Epsilon, dp.Y);
            collision = false;

            for (var j=0; j<_vertices.Length; ++j)
            {
                for (var i = 1; i < _vertices[j].Length; ++i)
                {
                    Vector2 w = _vertices[j][i - 1];
                    Vector2 dw = _vertices[j][i] - w;

                    float u = -1.0f;
                    float v = -1.0f;
                    if (intersect(p, dpx, w, dw, ref u, ref v))
                    {
                        dpx.X *= u;
                        collision = true;
                    }

                    u = -1.0f;
                    v = -1.0f;
                    if (intersect(p, dpy, w, dw, ref u, ref v))
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
