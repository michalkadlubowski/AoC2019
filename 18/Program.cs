using System;
using System.Collections.Generic;
using System.Linq;

namespace _18
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = System.IO.File.ReadAllLines("input.txt");
            var gameObjects = MapToObjects(lines);
            var graph = GraphNode.CreateGraph(gameObjects, '@');
            var allNodes = graph.GetAllNodes().ToList();
            

            //Create copy of grath without empty spaces
            graph.RemoveByType('.');
            allNodes = graph.GetAllNodes().ToList();

            //Generate all posibilities
            List<string> allPossibilities = GeneratePossibilities(graph).ToList();

            //Evaluate on original graph

            //Find shortest
        }
        private static IEnumerable<string> GeneratePossibilities(GraphNode graph, List<char> traversed = null)
        {
            if (traversed == null)
                traversed = new List<char>() { '@' };

            var currentPossibilities = GetPossibilities(graph, traversed, new HashSet<char>(traversed)).ToList();
            if (currentPossibilities.Count == 0)
            {
                var result =  new string(traversed.ToArray());
                Console.WriteLine(result);
                yield return result;
            }
            else
            {
                foreach(var possibility in currentPossibilities)
                {
                    var newList = new List<char>(traversed);
                    newList.Add(possibility.Node.Type);
                    newList.Add(possibility.Node.Type.ToString().ToUpper()[0]);
                    foreach(var future in GeneratePossibilities(graph, newList))
                        yield return future;
                }
            }
        }

        private static IEnumerable<GraphNode> GetPossibilities(GraphNode node, List<char> traversed, HashSet<char> traversedHashSet)
        {
            var possibleNodes = node.GetAllNodes().Where(n => !traversedHashSet.Contains(n.Node.Type) && n.ConnectedNodes.Any(x => traversedHashSet.Contains(x.Node.Type)));
            foreach (var possibleNode in possibleNodes)
            {
                //bool isDoorAndHasKey = possibleNode.Node.Type >= 65 && possibleNode.Node.Type <= 90 && traversed.Contains(possibleNode.Node.Type.ToString().ToLower()[0]);
                bool isPosition = possibleNode.Node.Type == 64;
                bool isNotDoor = (possibleNode.Node.Type >= 97 && possibleNode.Node.Type <= 122) || isPosition;
                if (isNotDoor || isPosition)
                {
                    yield return possibleNode;
                }
            }
        }

        static List<GameObject> MapToObjects(IEnumerable<string> lines)
        {
            List<GameObject> results = new List<GameObject>();

            int currentY = 0;
            foreach (var line in lines)
            {
                int currentX = 0;
                var chars = line.ToCharArray();
                for (int i = 0; i < chars.Length; i++)
                {
                    var point = new Point(currentX, currentY);
                    var gameObject = new GameObject(point, chars[i]);
                    results.Add(gameObject);
                    currentX++;
                }
                currentY++;
            }
            return results;
        }
    }

    public class GameObject
    {
        public override string ToString()
        {
            return $"{Point} - {Type}";
        }
        public GameObject(Point p, char type)
        {
            Point = p;
            Type = type;
        }
        public Point Point { get; private set; }
        public char Type { get; private set; }
    }

    public struct Point
    {
        public override string ToString()
        {
            return $"{X},{Y}";
        }

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

    public class GraphNode
    {
        public IEnumerable<GraphNode> GetAllNodes(HashSet<GraphNode> traversed = null)
        {
            if (traversed == null)
                traversed = new HashSet<GraphNode>();

            traversed.Add(this);
            yield return this;
            foreach (var connectedNode in ConnectedNodes.Where(c => !traversed.Contains(c)).SelectMany(c => c.GetAllNodes(traversed)))
                yield return connectedNode;
        }

        public override string ToString()
        {
            string res = $"{Node}";
            foreach (var node in ConnectedNodes)
            {
                res += $" => {node.Node}";
            }
            return res;
        }
        private GraphNode()
        {
            ConnectedNodes = new List<GraphNode>();
        }

        public static GraphNode CreateGraph(IEnumerable<GameObject> objects, char type)
        {
            var allNodes = objects.Where(o => o.Type != '#').Select(o => new GraphNode() { Node = o }).ToList();
            var sameColumns = allNodes.GroupBy(n => n.Node.Point.X);
            foreach (var column in sameColumns)
            {
                GraphNode previos = null;
                foreach (var node in column.OrderBy(x => x.Node.Point.Y))
                {
                    if (previos != null && Math.Abs(Math.Abs(previos.Node.Point.Y) - Math.Abs(node.Node.Point.Y)) == 1)
                    {
                        previos.ConnectedNodes.Add(node);
                        node.ConnectedNodes.Add(previos);
                    }
                    previos = node;
                }
            }
            var sameRows = allNodes.GroupBy(n => n.Node.Point.Y);
            foreach (var row in sameRows)
            {
                GraphNode previos = null;
                foreach (var node in row.OrderBy(x => x.Node.Point.X))
                {
                    if (previos != null && Math.Abs(Math.Abs(previos.Node.Point.X) - Math.Abs(node.Node.Point.X)) == 1)
                    {
                        previos.ConnectedNodes.Add(node);
                        node.ConnectedNodes.Add(previos);
                    }
                    previos = node;
                }
            }

            return allNodes.FirstOrDefault(n => n.Node.Type == type);
        }

        public GraphNode RemoveByType(char type, HashSet<GraphNode> visited = null)
        {
            if (visited == null)
            {
                visited = new HashSet<GraphNode>();
            }
            while (ConnectedNodes.Any(c => c.Node.Type == type))
            {
                var toRemove = ConnectedNodes.First(n => n.Node.Type == type);
                var toRepin = toRemove.ConnectedNodes.Where(x => x != this);
                foreach(var node in toRepin)
                    node.ConnectedNodes.Add(this);
                this.ConnectedNodes.AddRange(toRepin);
                this.ConnectedNodes.Remove(toRemove);
                foreach (var node in toRemove.ConnectedNodes)
                {
                    node.ConnectedNodes.Remove(toRemove);
                }
            }
            visited.Add(this);
            foreach (var connected in ConnectedNodes.Where(n => !visited.Contains(n)))
            {
                connected.RemoveByType(type, visited);
            }
            return this;
        }


        public GameObject Node { get; set; }

        public int Label { get; set; }

        public bool Visited { get; set; }
        public List<GraphNode> ConnectedNodes { get; set; }
    }
}
