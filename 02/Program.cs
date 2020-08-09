using System;
using System.Collections.Generic;
using System.Linq;

namespace _02
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var input = System.IO.File.ReadAllText("input.txt");
            var cableInputs = input.Split("\r\n");
            var firstCableInstructions = cableInputs[0].Split(',');
            var secondCableInstructions = cableInputs[1].Split(',');
            var firstCablePoints = new HashSet<Point>(ParseToPoionts(firstCableInstructions));
            var secondCablePoints = new HashSet<Point>(ParseToPoionts(secondCableInstructions));
            var intersections =firstCablePoints.Where(p => secondCablePoints.Contains(p)).Where(p => p.x != 0 && p.y != 0);
            // Func<Point,int> distanceCalc = (p) => Math.Abs(p.x) + Math.Abs(p.y);            
            // var minDist = intersections.Min(p => distanceCalc(p));
            var dists = intersections.Select(i => firstCablePoints.First(p => p.Equals(i)).steps + secondCablePoints.First(p => p.Equals(i)).steps);
            Console.WriteLine(dists.Min());
        }

        private static List<Point> ParseToPoionts(string[] instructions)
        {
            List<Point> points = new List<Point>() {new Point(){x=0, y=0}};
            int stepsTaken = 0;
            for(int i = 0; i<instructions.Length ; i++)
            {
                var instruction = instructions[i];
                var type = instruction[0];
                var pointsToGo = Int32.Parse(instruction.Substring(1));
                Func<Point, Point> generator;
                switch(type)
                {
                    case 'U':
                        generator = (p) => p.Up();
                        break;
                    case 'D':
                        generator = (p) => p.Down();
                        break;                        
                    case 'R':
                        generator = (p) => p.Right();
                        break;
                    case 'L':
                    default:
                        generator = (p) => p.Left();
                        break;                                                
                }
                for(int y = 0; y < pointsToGo; y++)
                {
                    stepsTaken++;
                    var lastPoint = points[points.Count-1];     
                    var newPoint = generator(lastPoint);
                    newPoint.steps = stepsTaken;               
                    points.Add(newPoint);
                }
            }
            return points;            
        }
    }

    public struct Point
    {
        public int x { get; set; }  
        public int y { get; set; }
        public int steps {get;set;}

        public Point Right()
        {
            return new Point {x= this.x +1, y = this.y};
        }
        
        public Point Left()
        {
            return new Point {x= this.x -1, y = this.y};
        }
        public Point Up()
        {
            return new Point {x= this.x, y = this.y +1};
        }
        public Point Down()
        {
            return new Point {x= this.x, y = this.y -1};
        }

        public override bool Equals(object obj)
        {
            return obj is Point point &&
                   x == point.x &&
                   y == point.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}
