using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YGR
{
    public class X_CollisionModel_Room
    {
        private int[][] _collisionTemplate;
        private Rectangle[] _collisionRectangles;
        private int[,] _collisionModel;
        private string _locationResourseFile;
        private bool[] _collisionRectanglesHit;
        private Point _location;
        int _tileWidth;
        int _tileHeight;

        private List<Manager_Collision.Record> _records;

        public X_CollisionModel_Room(
            string resourceFile,
            int tileWidth,
            int tileHeight,
            Point location
        )
        {
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            if (resourceFile.Substring(0, 1) == "/")
                _locationResourseFile = "." + resourceFile;
            else if (resourceFile.Substring(0, 2) != "./") 
                _locationResourseFile = "./" + resourceFile; 
            else 
                _locationResourseFile = resourceFile;

            load();

            _location = location;
            _records = new List<Manager_Collision.Record>();
        }

        private void load()
        {
            string[] lines = File.ReadAllLines(_locationResourseFile);
            _collisionTemplate = new int[lines.Length][];
            int counter = 0;
            foreach (var line in lines)
            {
                _collisionTemplate[counter] = line.Split(',').Where(i => i != "").Select(int.Parse).ToArray();
                counter++;
            }

            var components = fitRectangles(_collisionTemplate);

            createCollisionModelRectangles(components);
        }

        private Dictionary<int, List<Tuple<int, int>>> fitRectangles(int[][] pattern)
        {
            Dictionary<int, List<Tuple<int, int>>> components = new Dictionary<int, List<Tuple<int, int>>>();

            int key = 2;
            // first find all horizontal ones
            for (int x = 0; x < pattern.Length; ++x)
            {
                int y = 0;
                while (y < pattern[0].Length - 1)
                {
                    while (y < pattern[0].Length - 1 && pattern[x][y] >= 1 && pattern[x][y + 1] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!components.TryGetValue(key, out list))
                        {
                            components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x, y + 1) });
                        }
                        else
                            list.Add(new Tuple<int, int>(x, y + 1));

                        // extend es fahr as possible
                        // check if top and bottom is free. If not, leave 1 otherwise put the key
                        if (((x + 1 < pattern.Length && pattern[x + 1][y] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x - 1][y] == 0) || x - 1 < 0))
                            pattern[x][y] = key;
                        if (((x + 1 < pattern.Length && pattern[x + 1][y + 1] == 0) || x + 1 >= pattern.Length) && ((x - 1 >= 0 && pattern[x - 1][y + 1] == 0) || x - 1 < 0))
                            pattern[x][y + 1] = key;
                        y++;
                    }
                    key++;
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
                    while (x < pattern.Length - 1 && pattern[x][y] >= 1 && pattern[x + 1][y] == 1)
                    {
                        List<Tuple<int, int>> list;
                        if (!components.TryGetValue(key, out list))
                        {
                            components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y), new Tuple<int, int>(x + 1, y) });
                        }
                        else
                            list.Add(new Tuple<int, int>(x + 1, y));

                        // extend es fahr as possible
                        pattern[x][y] = key;
                        pattern[x + 1][y] = key;
                        x++;
                    }
                    key++;
                    x++;
                }
            }

            //output(pattern, "output2.csv");

            for (int x = 0; x < pattern.Length; ++x)
            {
                for (int y = 0; y < pattern[0].Length; ++y)
                {
                    if (pattern[x][y] == 1)
                    {
                        key++;
                        pattern[x][y] = key;

                        List<Tuple<int, int>> list;
                        if (!components.TryGetValue(key, out list))
                        {
                            components.Add(key, new List<Tuple<int, int>> { new Tuple<int, int>(x, y) });
                        }
                        else
                            list.Add(new Tuple<int, int>(x, y));
                    }
                }
            }

            //output(pattern, "output3.csv");
            return components;
        }

        private void createCollisionModelRectangles(Dictionary<int, List<Tuple<int, int>>> components)
        {
            _collisionModel = new int[_collisionTemplate.Length, _collisionTemplate[0].Length];
            var rects = new List<Rectangle>();

            int offset = 2;
            foreach (var component in components)
            {
                var list = component.Value;
                var topleft = list.OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                int tlX = topleft.First().Item1;
                int tlY = topleft.First().Item2;
                int brX = topleft.Last().Item1;
                int brY = topleft.Last().Item2;
                int width, height;
                if (tlX == brX)
                {
                    height = 1;
                    width = (brY - tlY + 1);
                    // make these a little bit shorter and offset them a little bit
                    rects.Add(new Rectangle(
                        tlY * _tileWidth + _location.X + offset,
                        tlX * _tileHeight + _location.Y,
                        width * _tileWidth - 2 * offset,
                        height * _tileHeight));
                }
                else
                {
                    width = 1;
                    height = (brX - tlX + 1);
                    // make these a little bit less high and offset them a little bit
                    rects.Add(new Rectangle(
                        tlY * _tileWidth + _location.X,
                        tlX * _tileHeight + _location.Y + offset,
                        width * _tileWidth,
                        height * _tileHeight - 2 * offset));
                }
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
                        y * _tileWidth + _location.X, x * _tileHeight + _location.Y, _tileWidth, _tileHeight,
                        lineWidth, color, spriteBatch);
                    }
                    //else
                    //{
                    //    Factory_Debug.DrawRectangle(
                    //    y * _tileWidth + _location.X, x * _tileHeight + _location.Y, _tileWidth, _tileHeight,
                    //    3, Color.Yellow, spriteBatch);
                    //}
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
                        _collisionRectangles[i].X + _location.X,
                        _collisionRectangles[i].Y + _location.Y,
                        _collisionRectangles[i].Width,
                        _collisionRectangles[i].Height,
                        1, color, spriteBatch);
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

        public void MoveTo(Point position)
        {
            _location.X = position.X;
            _location.Y = position.Y;
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
