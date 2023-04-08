using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;

namespace YGR
{
    public class X_CollisionModel_Room
    {
        private int[][] _collisionTemplate;
        private Rectangle[] _collisionRectangles;
        private int[,] _collisionModel;
        private bool[] _collisionRectanglesHit;
        private Point _location;
        private List<Manager_Collision.Record> _records;

        public int TileWidth;
        public int TileHeight;

        public X_CollisionModel_Room(
            int[][] collisionTemplate,
            int tileWidth,
            int tileHeight
        )
        {
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            _collisionTemplate = collisionTemplate;

            var components = fitRectangles(_collisionTemplate);
            createCollisionModelRectangles(components.Item1, components.Item2);
            _records = new List<Manager_Collision.Record>();
        }

        public void SplitCollisionVerticallyAt(Point p, int tileCount)
        {
            var col = _collisionRectangles.ToList();
            int index;
            for (index = 0; index < col.Count(); ++index)
            {
                if (col[index].Contains(p)) break;
            }

            var old = col[index];
            Rectangle rectTop = new Rectangle(
                old.X, old.Y,
                old.Width,
                p.Y - old.Y - TileHeight / 2 - (tileCount - 3) / 2 * TileHeight - 1);
            Rectangle rectBottom = new Rectangle(
                old.X, 
                old.Y + rectTop.Height + 2 + (tileCount - 2) * TileHeight,
                old.Width,
                old.Height - rectTop.Height - (tileCount - 2) * TileHeight - 1);

            col.RemoveAt(index);
            col.Add(rectTop);
            col.Add(rectBottom);
            _collisionRectangles = col.ToArray();
            _collisionRectanglesHit = Enumerable.Repeat<bool>(false, _collisionRectangles.Count()).ToArray();
        }

        public void SplitCollisionHorizontallyAt(Point p, int tileCount)
        {
            var col = _collisionRectangles.ToList();
            int index;
            for (index = 0; index < col.Count(); ++index)
            {
                if (col[index].Contains(p)) break;
            }

            var old = col[index];
            Rectangle rectLeft = new Rectangle(
                old.X, old.Y,
                p.X - old.X - TileWidth / 2 - (tileCount - 3) / 2 * TileWidth - 1,
                old.Height);
            Rectangle rectRight = new Rectangle(
                old.X + rectLeft.Width + 2 + (tileCount - 2) * TileWidth, old.Y,
                old.Width - rectLeft.Width - (tileCount - 2) * TileWidth - 1,
                old.Height);

            col.RemoveAt(index);
            col.Add(rectLeft);
            col.Add(rectRight);
            _collisionRectangles = col.ToArray();
            _collisionRectanglesHit = Enumerable.Repeat<bool>(false, _collisionRectangles.Count()).ToArray();
        }

        private int[,] createPattern(int sx, int sy)
        {
            int[,] match = new int[sx + 2, sy + 2];
            for(int x=0; x<sx+2; ++x)
            {
                for(int y=0; y<sy+2; ++y)
                {
                    if (x == 0 || x == sx + 1 || y == 0 || y == sy + 1)
                        match[x, y] = 0;
                    else
                        match[x, y] = 1;
                }
            }

            return match;
        }

        private IList<Tuple<int, int>> findMatch(int[,] match, int[][] pattern)
        {
            int lx = match.GetLength(0);
            int ly = match.GetLength(1);
            int px = pattern.Length - lx-1;
            int py = pattern[0].Length - ly-1;
            IList<Tuple<int, int>> matchPos = new List<Tuple<int, int>>();

            for (int x = 1; x < px; ++x)
            {
                for (int y = 1; y < py; ++y)
                {
                    for (int mx = 0; mx < lx; ++mx)
                    {
                        for (int my = 0; my < ly; ++my)
                        {
                            if (pattern[x - 1 + mx][y - 1 + my] != match[mx, my]) goto no_match;
                        }
                    }
                    matchPos.Add(new Tuple<int, int>(x, y));
                no_match: continue;
                }
            }

            return matchPos;
        }

        private IList<List<Tuple<int, int>>> coverMatch(int key, IList<Tuple<int, int>> matchPos, int sx, int sy, int[][] pattern)
        {
            IList<List<Tuple<int, int>>> coords = new List<List<Tuple<int, int>>>();
            foreach(var match in matchPos)
            {
                coords.Add(new List<Tuple<int, int>>());
                for (int mx = 0; mx < sx; ++mx)
                {
                    for (int my = 0; my < sy; ++my)
                    {
                        pattern[match.Item1 + mx][match.Item2 + my] = key;
                        coords.Last().Add(new Tuple<int, int>(match.Item1 + mx, match.Item2 + my));
                    }
                }
            }
            return coords;
        }

        private Tuple<Dictionary<int, List<Tuple<int, int>>>, Dictionary<int, List<Tuple<int, int>>>> fitRectangles(int[][] pattern)
        {
            Dictionary<int, List<Tuple<int, int>>> lines = new Dictionary<int, List<Tuple<int, int>>>();
            Dictionary<int, List<Tuple<int, int>>> rectangles = new Dictionary<int, List<Tuple<int, int>>>();

            int key = 2;
            // find all loose rectangles

            for (int x=1; x<pattern.Length-1; ++x)
            {
                for(int y=1; y<pattern.Length-1; ++y)
                {
                    var match = createPattern(x, y);
                    var matchPos = findMatch(match, pattern);
                    if(matchPos.Count() > 0)
                    {
                        var coords = coverMatch(key, matchPos, match.GetLength(0) - 2, match.GetLength(1) - 2, pattern);
                        foreach(var c in coords)
                        {
                            rectangles.Add(key, c.ToList());
                            key++;
                        }
                    }
                }
            }

            //output(pattern, "output0.csv");

            // first find all horizontal ones
            for (int x = 0; x < pattern.Length; ++x)
            {
                int y = 0;
                while (y < pattern[0].Length - 1)
                {
                    bool add = false;
                    while (y < pattern[0].Length - 1 && pattern[x][y] >= 1 && pattern[x][y + 1] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!lines.TryGetValue(key, out list))
                        {
                            lines.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x, y + 1) });
                            add = true;
                        }
                        else
                        {
                            list.Add(new Tuple<int, int>(x, y + 1));
                            add = true;
                        }

                        // extend es fahr as possible
                        // check if top and bottom is free. If not, leave 1 otherwise put the key
                        if (((x + 1 < pattern.Length && pattern[x + 1][y] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x - 1][y] == 0) || x - 1 < 0))
                            pattern[x][y] = key;
                        if (((x + 1 < pattern.Length && pattern[x + 1][y + 1] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x - 1][y + 1] == 0) || x - 1 < 0))
                            pattern[x][y + 1] = key;
                        y++;
                    }
                    if(add) key++;
                    y++;
                }
            }

            //output(pattern, "output1.csv");

            // then go vertical
            for (int y = 0; y < pattern[0].Length; ++y)
            {
                int x = 0;
                while (x < pattern.Length - 1)
                {
                    bool add = false;
                    while (x < pattern.Length - 1 && pattern[x][y] >= 1 && pattern[x + 1][y] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!lines.TryGetValue(key, out list))
                        {
                            lines.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x + 1, y) });
                            add = true;
                        }
                        else
                        {
                            list.Add(new Tuple<int, int>(x + 1, y));
                            add = true;
                        }

                        // extend es fahr as possible
                        pattern[x][y] = key;
                        pattern[x + 1][y] = key;
                        x++;
                    }
                    if(add) key++;
                    x++;
                }
            }

            //output(pattern, "output2.csv");

            //for (int x = 0; x < pattern.Length; ++x)
            //{
            //    for (int y = 0; y < pattern[0].Length; ++y)
            //    {
            //        if (pattern[x][y] == 1)
            //        {
            //            key++;
            //            pattern[x][y] = key;

            //            List<Tuple<int, int>> list;
            //            if (!components.TryGetValue(key, out list))
            //            {
            //                components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y) });
            //            }
            //            else
            //                list.Add(new Tuple<int, int>(x, y));
            //        }
            //    }
            //}

            //output(pattern, "output3.csv");
            return new Tuple<Dictionary<int, List<Tuple<int, int>>>, Dictionary<int, List<Tuple<int, int>>>>(lines, rectangles);
        }

        private void createCollisionModelRectangles(Dictionary<int, List<Tuple<int, int>>> lines, Dictionary<int, List<Tuple<int, int>>> rectangles)
        {
            _collisionModel = new int[_collisionTemplate.Length, _collisionTemplate[0].Length];
            var rects = new List<Rectangle>();

            int offset = 2;
            foreach (var line in lines)
            {
                var list = line.Value;
                var topleft = list.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                int tlX = topleft.First().Item1;
                int tlY = topleft.First().Item2;
                int brX = topleft.Last().Item1;
                int brY = topleft.Last().Item2;
                int width, height;

                // make these a little bit shorter and offset them a little bit
                if (tlX == brX)
                {
                    height = 1;
                    width = (brY - tlY + 1);
                    // make these a little bit shorter and offset them a little bit
                    rects.Add(new Rectangle(
                        tlY * TileWidth + _location.X + offset,
                        tlX * TileHeight + _location.Y,
                        width * TileWidth - 2 * offset,
                        height * TileHeight));
                }
                else
                {
                    width = 1;
                    height = (brX - tlX + 1);
                    // make these a little bit less high and offset them a little bit
                    rects.Add(new Rectangle(
                        tlY * TileWidth + _location.X,
                        tlX * TileHeight + _location.Y + offset,
                        width * TileWidth,
                        height * TileHeight - 2 * offset));
                }
            }

            foreach(var rectangle in rectangles)
            {
                var list = rectangle.Value;
                var topleft = list.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                int tlX = topleft.First().Item1;
                int tlY = topleft.First().Item2;
                int brX = topleft.Last().Item1;
                int brY = topleft.Last().Item2;

                int height = (brX - tlX + 1);
                int width = (brY - tlY + 1);
                // make these a little bit shorter and offset them a little bit
                rects.Add(new Rectangle(
                    tlY * TileWidth + _location.X,
                    tlX * TileHeight + _location.Y,
                    width * TileWidth,
                    height * TileHeight));
            }

            _collisionRectangles = rects.ToArray();
            _collisionRectanglesHit = Enumerable.Repeat<bool>(false, _collisionRectangles.Count()).ToArray();
        }

        private void output(int[][] pattern, string name)
        {
            using (StreamWriter writer = new StreamWriter(name))
            {
                for (int x = 0; x < pattern.GetLength(0); ++x)
                {
                    string s = string.Join("\t", pattern[x]);
                    writer.WriteLine(s);
                }
            }
        }

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            for (int x = 0; x < _collisionModel.GetLength(0); ++x)
            {
                for (int y = 0; y < _collisionModel.GetLength(1); ++y)
                {
                    var color = Color.Gray;
                    var lineWidth = 1;
                    if (_collisionTemplate[x][y] == 0)
                    {
                        Factory_Debug.DrawRectangle(
                        y * TileWidth + _location.X, x * TileHeight + _location.Y, TileWidth, TileHeight,
                        lineWidth, color, spriteBatch);
                    }
                }
            }

            for (int i = 0; i < _collisionRectangles.Count(); ++i)
            {
                Color color = Manager_Collision.MissColor;
                if (_collisionRectanglesHit[i])
                {
                    color = Manager_Collision.HitColor;
                    _collisionRectanglesHit[i] = false;
                }
                Factory_Debug.DrawRectangle(
                        _collisionRectangles[i].X,
                        _collisionRectangles[i].Y,
                        _collisionRectangles[i].Width,
                        _collisionRectangles[i].Height,
                        3, color, spriteBatch);
            }

            _records.RemoveAll(rec => (rec.TimeStampMS + Manager_Collision.DrawTimeoutMS < (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds));
            foreach (var rec in _records)
            {
                Factory_Debug.DrawPoint(
                    rec.ContactPoint.X, rec.ContactPoint.Y, 11, Color.Magenta, spriteBatch);

                Factory_Debug.DrawLine(
                    (int)rec.ContactNormal.X + rec.ContactPoint.X,
                    (int)rec.ContactNormal.Y + rec.ContactPoint.Y,
                    25, (float)Math.Atan2(rec.ContactNormal.Y, rec.ContactNormal.X),
                    3, Color.Yellow, spriteBatch);
            }
        }

        public void MoveBy(Point offset)
        {
            _location.X = _location.X + offset.X;
            _location.Y = _location.Y + offset.Y;

            for(int i=0; i<_collisionRectangles.Length; ++i)
            {
                var rect = _collisionRectangles[i];
                _collisionRectangles[i] = new Rectangle(
                    rect.X + offset.X, rect.Y + offset.Y, 
                    rect.Width, rect.Height);
            }
        }

        public bool Intersect(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal)
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            //float uHit;
            List<Manager_Collision.Record> collided;
            bool collision = Manager_Collision.DynamicRectVsStaticRects(
                ref movingRect, ref velocity, timeStepMS,
                _collisionRectangles, _collisionRectanglesHit,
                out collided
            );

            if (collision) unifyCollisions(ref collided, ref movingRect, out contactPoint, out contactNormal);

            return collision;
        }

        public bool IntersectFast(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal)
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            List<Manager_Collision.Record> collided;
            bool collision = Manager_Collision.FastRectVsStaticRects(
                ref movingRect, velocity, timeStepMS, _collisionRectangles, _collisionRectanglesHit, out collided);

            if(collision) unifyCollisions(ref collided, ref movingRect, out contactPoint, out contactNormal);

            return collision;
        }

        private void unifyCollisions(ref List<Manager_Collision.Record> collisions, ref Rectangle movingRect, out Point contactPoint, out Vector2 contactNormal)
        {
            if (collisions.Count > 1)
            {
                contactNormal = Vector2.Zero;
                foreach (var col in collisions)
                {
                    contactNormal += col.ContactNormal;
                }
                contactNormal.Normalize();

                Point offset = (new Vector2(movingRect.Width / 2, movingRect.Height / 2) * -contactNormal).ToPoint();
                contactPoint.X = movingRect.Location.X + movingRect.Width / 2 + offset.X;
                contactPoint.Y = movingRect.Location.Y + movingRect.Height / 2 + offset.Y;

                _records.Add(new Manager_Collision.Record(
                    (DateTime.Now - Manager_Collision.StartTime).TotalMilliseconds,
                    contactPoint, contactNormal, 0));
            }
            else
            {
                _records.AddRange(collisions);
                contactNormal = collisions.First().ContactNormal;
                contactPoint = collisions.First().ContactPoint;
            }
        }
    }
}
