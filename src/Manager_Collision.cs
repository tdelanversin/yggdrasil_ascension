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
        public static Rectangle Intersect(Rectangle rect1, Rectangle rect2)
        {
            return Rectangle.Intersect(rect1, rect2);
        }
    }
}
