using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YGR
{
    public class X_RoomGraph
    {

        private Y_CMRoom room;
        private int[][] collisions;
        private Point tileSize;

        public X_RoomGraph(Y_CMRoom room, int[][] collisions, int tileWidth, int tileHeight)
        {
            this.room = room;
            this.collisions = collisions;
            tileSize = new Point(tileWidth, tileHeight);
        }


        public Vector2 GetDirectionToTarget(IGameElement me, IGameElement target)
        {
            Point start = me.Rect.Center;
            Point end = target.Rect.Center;
            if (room.Rect.Contains(start) && room.Rect.Contains(end))
            {
                return FindNextStep(start - room.Rect.Location, end - room.Rect.Location);
            }
            else
            {
                return Vector2.Zero;
            }
        }

        private Vector2 FindNextStep(Point start, Point end)
        {
            List<Point> path = FindShortestPath(start / tileSize, end / tileSize);
            if (path.Count > 1)
            {
                return (path[1] * tileSize).ToVector2() - start.ToVector2();
            }
            else
            {
                return Vector2.Zero;
            }
        }

        private List<Point> FindShortestPath(Point start, Point end)
        {
            // Initialize the open and closed sets
            HashSet<Point> openSet = new HashSet<Point>();
            HashSet<Point> closedSet = new HashSet<Point>();

            // Add the start node to the open set
            openSet.Add(start);

            // Initialize the dictionary to store the parents of each node
            Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();

            // Initialize the dictionary to store the cost of getting to each node
            Dictionary<Point, int> gScore = new Dictionary<Point, int>();
            foreach (Point p in AllPoints())
            {
                gScore[p] = int.MaxValue;
            }
            gScore[start] = 0;

            // Initialize the dictionary to store the estimated cost of getting to the end from each node
            Dictionary<Point, int> fScore = new Dictionary<Point, int>();
            foreach (Point p in AllPoints())
            {
                fScore[p] = int.MaxValue;
            }
            fScore[start] = Heuristic(start, end);

            // Loop until the open set is empty
            while (openSet.Count > 0)
            {
                // Find the node in the open set with the lowest fScore
                Point current = openSet.First();
                int lowestFScore = int.MaxValue;
                foreach (Point p in openSet)
                {
                    if (fScore[p] < lowestFScore)
                    {
                        current = p;
                        lowestFScore = fScore[p];
                    }
                }

                // If we've reached the end, reconstruct the path and return it
                if (current.Equals(end))
                {
                    return ReconstructPath(cameFrom, current);
                }

                // Remove the current node from the open set and add it to the closed set
                openSet.Remove(current);
                closedSet.Add(current);

                // Consider each neighbor of the current node
                foreach (Point neighbor in Neighbors(current))
                {
                    // If the neighbor is in the closed set, skip it
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // Calculate the tentative gScore for the neighbor
                    int tentativeGScore = gScore[current] + Cost(current, neighbor);

                    // If the neighbor is not in the open set, add it
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= gScore[neighbor])
                    {
                        // If the tentative gScore is not better than the current gScore, skip this neighbor
                        continue;
                    }

                    // This path is the best so far, record it
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
                }
            }

            // If we get here, there is no path from start to end
            return null;
        }

        // Helper function to get all the points in the graph
        private IEnumerable<Point> AllPoints()
        {
            for (int x = 0; x < collisions.Length; x++)
            {
                for (int y = 0; y < collisions[0].Length; y++)
                {
                    yield return new Point(x, y) * tileSize;
                }
            }
        }

        // Helper function to get the neighbors of a point
        private IEnumerable<Point> Neighbors(Point p)
        {
            int x = p.X;
            int y = p.Y;
            int left = Math.Max(x - 1, 0);
            int right = Math.Min(x + 1, collisions.Length - 1);
            int top = Math.Max(y - 1, 0);
            int bottom = Math.Min(y + 1, collisions[0].Length - 1);

            for (int i = left; i <= right; i++)
            {
                for (int j = top; j <= bottom; j++)
                {
                    if (i == x && j == y)
                    {
                        continue;
                    }
                    if (collisions[i][j] != 0)
                    {
                        continue;
                    }
                    yield return new Point(i, j) * tileSize;
                }
            }
        }


        // Helper function to get the cost of moving from one point to another
        private int Heuristic(Point a, Point b)
        {
            int dx = b.X - a.X;
            int dy = b.Y - a.Y;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }

        private List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            List<Point> path = new List<Point>();
            path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }

            return path;
        }

        private int Cost(Point current, Point neighbor)
        {
            int dx = Math.Abs(current.X - neighbor.X);
            int dy = Math.Abs(current.Y - neighbor.Y);
            return (dx + dy == 1) ? 10 : 14;
        }
    }
}