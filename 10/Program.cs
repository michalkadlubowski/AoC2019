using System;
using System.Collections.Generic;
using System.Linq;

namespace _10
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var lines = System.IO.File.ReadAllLines("input.txt");
            Console.WriteLine(GetAngle(new Point(11,13),new Point(11,12)));
            Console.WriteLine(GetAngle(new Point(11,13),new Point(12,4)));
            Console.WriteLine(GetAngle(new Point(11,13),new Point(12,8)));

            //extract points
            var points = new List<Point>();
            for (int i = 0; i < lines.Length; i++)
            {
                char[] charArray = lines[i].ToCharArray();
                for (int y = 0; y < charArray.Length; y++)
                {
                    char point = (char)charArray[y];
                    var isAsteroid = point == '#';
                    if(isAsteroid)
                    {
                        points.Add(new Point(y,i));
                        //Console.WriteLine($"Asteroid at {i},{y}");
                    }
                }
            }
            // var max = 0;
            // foreach(var p in points)
            // {
            //     int seen = GetPointsSeenFrom(p, points);
            //     if(seen > max)
            //     {
            //         max = seen;
            //         Console.WriteLine($"Asteroid seen from {p.X},{p.Y} = {seen}");
            //     }
            // }
            // Console.WriteLine(max);
            Console.WriteLine("Total count: " + points.Count);
            var pointsBydestructionOrder = GetPointsOrderedByDestructionOrder(new Point(11,13), points);
            Console.WriteLine("Total count: " + pointsBydestructionOrder.Count);
            for (int i = 0; i < pointsBydestructionOrder.Count; i++)
            {
                var point = pointsBydestructionOrder[i];
                Console.WriteLine($"{i} - {point.X},{point.Y}");
            }
            foreach(int i in new int[]{1,2,3,10,20,50,100,199,200,201,299})
            {
                var selected = pointsBydestructionOrder.Skip(i-1).First();
                Console.WriteLine($"The {i}th: {selected.X}, {selected.Y}");
            }
        }

        public static double GetAngle(Point p1, Point p2)
        {
                var xDiff = (float)(p2.X - p1.X);
                var yDiff = (p2.Y - p1.Y);
                var diff = yDiff/xDiff;
                var atan2 = Math.Atan2(xDiff, yDiff);
                return Math.Abs(((atan2 * 180 / Math.PI) -180) % 360);
        }

        public static double GetDistanceToPoint(Point p1, Point p2)
        {
            if(p1.X == p2.X)
                return Math.Abs(p2.Y-p1.Y);
            if(p1.Y == p2.Y)
                return Math.Abs(p2.X - p2.X);
            return Math.Sqrt(Math.Abs((p1.X - p2.X)) * (double)Math.Abs((p1.Y - p2.Y)));
        }

        public static List<Point> GetPointsOrderedByDestructionOrder(Point p, IEnumerable<Point> pointsToAnalyze)
        {
            Dictionary<double, List<Point>> dict = GetPointsDictByAngle(p, pointsToAnalyze);
            var pointsResult = new List<Point>();
            var allValues = dict.SelectMany(kv => kv.Value);
            Console.WriteLine("ALL " + allValues.Count());
            
            bool goOn = true;
            while(goOn)
            {
                goOn = false;
                foreach(var kv in dict.OrderBy(d => d.Key))
                {
                    if(kv.Value.Any())
                    {
                        var distances = kv.Value.Select(x => GetDistanceToPoint(p,x ));
                        var point = kv.Value.OrderBy(x => GetDistanceToPoint(p,x)).First();
                        pointsResult.Add(point);
                        kv.Value.Remove(point);
                    }
                    if(goOn == false && kv.Value.Any())
                        goOn =true;
                }
            }
            return pointsResult;
        }

        public static int GetPointsSeenFrom(Point p, IEnumerable<Point> pointsToAnalyze)
        {
            Dictionary<double, List<Point>> dict = GetPointsDictByAngle(p, pointsToAnalyze);

            return dict.Select(kv => kv.Value.OrderBy(x => GetDistanceToPoint(p, x)).First()).ToList().Count;
        }

        private static Dictionary<double, List<Point>> GetPointsDictByAngle(Point p, IEnumerable<Point> pointsToAnalyze)
        {
            var dict = new Dictionary<double, List<Point>>();
            foreach (var pta in pointsToAnalyze)
            {
                if (pta.X == p.X && pta.Y == p.Y)
                    continue;
                var angle = GetAngle(p, pta);
                if (!dict.ContainsKey(angle))
                    dict.Add(angle, new List<Point>());
                dict[angle].Add(pta);
            }

            return dict;
        }
    }

    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; set; }
        public int Y { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   X == point.X &&
                   Y == point.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}

