using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Reflection.Emit;
using static YGR.Manager_Collision;

namespace YGR
{
    public class X_CollisionModel_Room
    {
        private int[][] _collisionTemplate;
        private Rectangle[] _collisionRectangles;
        private List<List<Rectangle>> _floor;
        private bool[] _collisionRectanglesHit;
        private Point _location;
        private List<Manager_Collision.Record> _records;

        public int TileWidth;
        public int TileHeight;
        private bool _isRoomCollisionModel;

        public X_CollisionModel_Room(
            int[][] collisionTemplate,
            int tileWidth,
            int tileHeight,
            bool isRoomCollisionModel = true
        )
        {
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            _collisionTemplate = collisionTemplate;
            _isRoomCollisionModel = isRoomCollisionModel;

            var components = fitRectangles(_collisionTemplate);
            createCollisionModelRectangles(components);

            _collisionTemplate = cleanUpCollisionTemplate(_collisionTemplate);

            _records = new List<Manager_Collision.Record>();

            _floor = CreateFloorRectangles(TileWidth);
    }

        public void UpdateCollisionRectangles(List<Rectangle> collisionRectangles)
        {
            _collisionRectangles = collisionRectangles.ToArray();
            _collisionRectanglesHit = Enumerable.Repeat<bool>(false, collisionRectangles.Count).ToArray();
        }

        public int[][] GetCollisionTemplate()
        {
            return _collisionTemplate;
        }

        public Rectangle[] GetCollisionRectangles()
        {
            return _collisionRectangles;
        }

        public Dictionary<X_DoorState, List<Rectangle>> SplitCollisionVerticallyAt(Point p, int tileCount)
        {
            var col = _collisionRectangles.ToList();
            int index;
            for (index = 0; index < col.Count(); ++index)
            {
                if (col[index].Contains(p)) break;
            }

            if (index >= col.Count())
            {
                return new Dictionary<X_DoorState, List<Rectangle>>();
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

            var res = new Dictionary<X_DoorState, List<Rectangle>> { 
                { X_DoorState.Open, new List<Rectangle> { rectTop, rectBottom } }, 
                { X_DoorState.Closed, new List<Rectangle> { col.ElementAt(index) } } };

            col.RemoveAt(index);
            //col.Add(rectTop);
            //col.Add(rectBottom);
            _collisionRectangles = col.ToArray();
            _collisionRectanglesHit = Enumerable.Repeat<bool>(false, _collisionRectangles.Count()).ToArray();

            return res;
        }

        public Dictionary<X_DoorState, List<Rectangle>> SplitCollisionHorizontallyAt(Point p, int tileCount)
        {
            var col = _collisionRectangles.ToList();
            int index;
            for (index = 0; index < col.Count(); ++index)
            {
                if (col[index].Contains(p)) break;
            }

            if (index >= col.Count()) return new Dictionary<X_DoorState, List<Rectangle>>();

            var old = col[index];
            Rectangle rectLeft = new Rectangle(
                old.X, old.Y,
                p.X - old.X - TileWidth / 2 - (tileCount - 3) / 2 * TileWidth - 1,
                old.Height);
            Rectangle rectRight = new Rectangle(
                old.X + rectLeft.Width + 2 + (tileCount - 2) * TileWidth, old.Y,
                old.Width - rectLeft.Width - (tileCount - 2) * TileWidth - 1,
                old.Height);

            var res = new Dictionary<X_DoorState, List<Rectangle>> { 
                { X_DoorState.Open, new List<Rectangle> { rectLeft, rectRight } }, 
                { X_DoorState.Closed, new List<Rectangle> { col.ElementAt(index) } } };

            col.RemoveAt(index);

            //col.Add(rectLeft);
            //col.Add(rectRight);
            _collisionRectangles = col.ToArray();
            _collisionRectanglesHit = Enumerable.Repeat<bool>(false, _collisionRectangles.Count()).ToArray();

            return res;
        }

        private int[][] cleanUpCollisionTemplate(int[][] collision)
        {
            int[][] pattern = collision.Clone() as int[][];
            //output(pattern, "./logs/pattern.csv");
            for (int i = 0; i < pattern.Length; i++)
            {
                for (int j = 0; j < pattern[0].Length; ++j)
                {
                    if (pattern[i][j] > 0) pattern[i][j] = (int)X_TileType.Roof;
                }
            }

            for (int i = 1; i < pattern.Length; i++)
            {
                for (int j = 0; j < pattern[0].Length; ++j)
                {
                    if (pattern[i - 1][j] == (int)X_TileType.Roof && pattern[i][j] == 0) pattern[i][j] = (int)X_TileType.Wall;
                }
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                for (int j = 0; j < pattern[0].Length; ++j)
                {
                    if (pattern[i][j] == 0) pattern[i][j] = (int)X_TileType.Floor;
                }
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                for (int j = 0; j < pattern[0].Length; ++j)
                {
                    if (pattern[i][j] < 0) pattern[i][j] = (int)X_TileType.Outside;
                }
            }
            //output(pattern, "./logs/pattern.csv");
            return pattern;
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

        public List<List<Rectangle>> CreateFloorRectangles(int tileSize)
        {
            List<List<Rectangle>> floor = new List<List<Rectangle>>();
            //output(collisionTemplate, "./logs/pattern.csv");

            for (int i = 0; i < _collisionTemplate.Length; ++i)
            {
                floor.Add(new List<Rectangle>());
                for (int j = 0; j < _collisionTemplate[0].Length; ++j)
                {
                    if (_collisionTemplate[i][j] == (int)X_TileType.Floor || _collisionTemplate[i][j] == (int)X_TileType.Wall)
                        floor.Last().Add(
                            new Rectangle(
                                j * tileSize + _location.X,
                                i * tileSize + _location.Y,
                                tileSize, tileSize));
                }
            }

            //output(_collisionTemplate, "logs/blub.csv");
            floor.RemoveAll(x => x.Count() == 0);
            return floor;
        }

        public List<List<Rectangle>> CreateWallRectangles(int tileSize)
        {
            List<List<Rectangle>> wall = new List<List<Rectangle>>();
            //output(collisionTemplate, "./logs/pattern.csv");

            for (int i = 0; i < _collisionTemplate.Length; ++i)
            {
                wall.Add(new List<Rectangle>());
                for (int j = 0; j < _collisionTemplate[0].Length; ++j)
                {
                    if (_collisionTemplate[i][j] == (int)X_TileType.Wall)
                        wall.Last().Add(
                            new Rectangle(
                                j * tileSize + _location.X,
                                i * tileSize + _location.Y,
                                tileSize, tileSize));
                }
            }

            //output(_collisionTemplate, "logs/blub.csv");
            wall.RemoveAll(x => x.Count() == 0);
            return wall;
        }

        private Dictionary<string, Dictionary<int, List<Tuple<int, int>>>> fitRectangles(int[][] pattern)
        {
            //output(pattern, "output0.csv");

            Dictionary<int, List<Tuple<int, int>>> hLines = new Dictionary<int, List<Tuple<int, int>>>();
            Dictionary<int, List<Tuple<int, int>>> vLines = new Dictionary<int, List<Tuple<int, int>>>();
            Dictionary<int, List<Tuple<int, int>>> rectangles = new Dictionary<int, List<Tuple<int, int>>>();

            int key = 2;
            // find all loose rectangles

            for (int x=1; x<pattern.Length-1; ++x)
            {
                for(int y=1; y < pattern[0].Length-1; ++y)
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

            if (_isRoomCollisionModel)
            {
                // find the horizontal outlines
                int xend = pattern.Length - 1;
                for (int y = 0; y < pattern[0].Length; ++y)
                {
                    List<Tuple<int, int>> list;
                    if (!hLines.TryGetValue(key, out list))
                    {
                        hLines.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(0, y) });
                    }
                    else
                    {
                        list.Add(new Tuple<int, int>(0, y));
                    }
                    pattern[0][y] = key;
                }
                key++;

                for (int y = 0; y < pattern[0].Length; ++y)
                {
                    List<Tuple<int, int>> list;
                    if (!hLines.TryGetValue(key, out list))
                    {
                        hLines.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(xend, y) });
                    }
                    else
                    {
                        list.Add(new Tuple<int, int>(xend, y));
                    }
                    pattern[xend][y] = key;
                }
                key++;

                int yend = pattern[0].Length - 1;
                for (int x = 0; x < pattern.Length; ++x)
                {
                    List<Tuple<int, int>> list;
                    if (!vLines.TryGetValue(key, out list))
                    {
                        vLines.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, 0) });
                    }
                    else
                    {
                        list.Add(new Tuple<int, int>(x, 0));
                    }
                    pattern[x][0] = key;
                }
                key++;

                for (int x = 0; x < pattern.Length; ++x)
                {
                    List<Tuple<int, int>> list;
                    if (!vLines.TryGetValue(key, out list))
                    {
                        vLines.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, yend) });
                    }
                    else
                    {
                        list.Add(new Tuple<int, int>(x, yend));
                    }
                    pattern[x][yend] = key;
                }
                key++;
            }
            

            //output(pattern, "output0.csv");

            // first find all horizontal ones
            Dictionary<int, List<Tuple<int, int>>> temp = new Dictionary<int, List<Tuple<int, int>>>();
            for (int x = 0; x < pattern.Length; ++x)
            {
                int y = 0;
                while (y < pattern[0].Length - 1)
                {
                    bool add = false;
                    while (y < pattern[0].Length - 1 && pattern[x][y] >= 1 && pattern[x][y + 1] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!temp.TryGetValue(key, out list))
                        {
                            temp.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x, y + 1) });
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

            if (_isRoomCollisionModel)
            {
                while (temp.Count() > 0)
                {
                    var first = temp.First();
                    int starty = first.Value.First().Item2;
                    int endy = first.Value.Last().Item2;
                    var take = temp.Where(x => x.Value.First().Item2 == starty && x.Value.Last().Item2 == endy).ToList();

                    List<Tuple<int, int>> newList = new List<Tuple<int, int>>();
                    int lastx = take.First().Value.First().Item1;
                    foreach (var t in take)
                    {
                        if (t.Value.First().Item1 > lastx + 1) break;
                        lastx = t.Value.First().Item1;
                        newList.AddRange(t.Value);
                        temp.Remove(t.Key);
                    }
                    hLines.Add(key, newList);
                    key++;
                }
            }
            else
            {
                foreach(var t in temp)
                {
                    hLines.Add(t.Key, t.Value);
                }
            }

            // then go vertical
            Dictionary<int, List<Tuple<int, int>>> temp2 = new Dictionary<int, List<Tuple<int, int>>>();
            for (int y = 0; y < pattern[0].Length; ++y)
            {
                int x = 0;
                while (x < pattern.Length - 1)
                {
                    bool add = false;
                    while (x < pattern.Length - 1 && pattern[x][y] >= 1 && pattern[x + 1][y] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!temp2.TryGetValue(key, out list))
                        {
                            temp2.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x + 1, y) });
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
                    if (add) key++;
                    x++;
                }
            }

            if (_isRoomCollisionModel)
            {
                // merge lines that start and end at the same x coordinate
                while (temp2.Count() > 0)
                {
                    var first = temp2.First();
                    int startx = first.Value.First().Item1;
                    int endx = first.Value.Last().Item1;
                    var take = temp2.Where(x => x.Value.First().Item1 == startx && x.Value.Last().Item1 == endx).ToList();

                    List<Tuple<int, int>> newList = new List<Tuple<int, int>>();
                    int lasty = take.First().Value.First().Item2;
                    foreach (var t in take)
                    {
                        if (t.Value.First().Item2 > lasty + 1) break;
                        lasty = t.Value.First().Item2;
                        newList.AddRange(t.Value);
                        temp2.Remove(t.Key);
                    }
                    vLines.Add(key, newList);
                    key++;
                }
            }
            else
            {
                foreach(var t in temp2)
                {
                    vLines.Add(t.Key, t.Value);
                }
            }

            return new Dictionary<string, Dictionary<int, List<Tuple<int, int>>>>() { { "vertical", vLines }, { "horizontal", hLines }, { "rectangle", rectangles } };
        }

        private void createCollisionModelRectangles(Dictionary<string, Dictionary<int, List<Tuple<int, int>>>> components)
        {
            var rects = new List<Rectangle>();

            int offset = 2;
            foreach (var line in components["horizontal"])
            {
                var list = line.Value;
                var topleft = list.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                int tlX = topleft.First().Item1;
                int tlY = topleft.First().Item2;
                int brX = topleft.Last().Item1;
                int brY = topleft.Last().Item2;
                int width, height;

                // make these a little bit shorter and offset them a little bit
                //if (tlX == brX)
                //{
                height = (brX - tlX + 1);
                width = (brY - tlY + 1);
                // make these a little bit shorter and offset them a little bit
                rects.Add(new Rectangle(
                    tlY * TileWidth + _location.X + offset,
                    tlX * TileHeight + _location.Y,
                    width * TileWidth - 2 * offset,
                    height * TileHeight));
            }

            foreach (var line in components["vertical"]) 
            {
                var list = line.Value;
                var topleft = list.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                int tlX = topleft.First().Item1;
                int tlY = topleft.First().Item2;
                int brX = topleft.Last().Item1;
                int brY = topleft.Last().Item2;
                int width, height;

                width = (brY - tlY + 1);
                height = (brX - tlX + 1);
                // make these a little bit less high and offset them a little bit
                rects.Add(new Rectangle(
                    tlY * TileWidth + _location.X,
                    tlX * TileHeight + _location.Y + offset,
                    width * TileWidth,
                    height * TileHeight - 2 * offset));
            }

            foreach(var rectangle in components["rectangle"])
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

        public void DrawOutline(GameTime gameTime, Vector2 globalOffset, SpriteBatch spriteBatch)
        {
            foreach (var rectRow in _floor)
            {
                foreach(var rect in rectRow)
                {
                    Factory_Debug.DrawRectangle(
                        rect.X, rect.Y, rect.Width, rect.Height,
                        1, Color.Gray, spriteBatch);
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

            List<List<Rectangle>> newList = new List<List<Rectangle>>();
            foreach (var row in _floor)
            {
                List<Rectangle> r = new List<Rectangle>();
                foreach(var rect in row)
                {
                    r.Add(new Rectangle(
                        rect.X + offset.X, rect.Y + offset.Y,
                        rect.Width, rect.Height));
                }
                newList.Add(r);
            }
            _floor = newList;
        }

        public bool Intersect(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal)
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            //float uHit;
            List<Manager_Collision.Record> collided = new List<Record>();
            bool collision = Manager_Collision.DynamicRectVsStaticRects(
                ref movingRect, ref velocity, timeStepMS,
                _collisionRectangles, _collisionRectanglesHit,
                ref collided
            );

            if (collision)
            {
                contactPoint = collided.First().ContactPoint;
                contactNormal = collided.First().ContactNormal;
            }

            return collision;
        }

        public bool IntersectFast(ref Rectangle movingRect, ref Vector2 velocity, int timeStepMS, out Point contactPoint, out Vector2 contactNormal)
        {
            contactPoint = Point.Zero;
            contactNormal = Vector2.Zero;
            List<Manager_Collision.Record> collided = new List<Record>();
            bool collision = Manager_Collision.FastRectVsStaticRects(
                ref movingRect, velocity, timeStepMS, _collisionRectangles, _collisionRectanglesHit, ref collided);

            if (collision)
            {
                // unifyCollisions(ref collided, ref movingRect, out contactPoint, out contactNormal);
                velocity = Vector2.Zero;
            }

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
