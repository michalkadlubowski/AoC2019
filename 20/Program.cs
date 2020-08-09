using System;
using System.Collections.Generic;
using System.Linq;

namespace _20
{


    class Program
    {

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

        public class GameObject
        {
            public override string ToString()
            {
                return $"{Point} - {Type}";
            }
            public GameObject(Point p, string type)
            {
                Point = p;
                Type = type;
            }
            public Point Point { get; private set; }
            public string Type { get; set; }
        }

        public class GraphNode
        {

            public void PrintPathToClosestPortal()
            {
                string path = "";
                foreach(var node in GetAllNodes())
                {
                    if(node.Node.Type == this.Node.Type)
                        continue;
                    path+= node.Node.Type;
                    if(node.Node.Type.Length>1)
                        break;
                }
                Console.WriteLine(path);
            }
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

            public static List<GraphNode> CreateGraph(IEnumerable<GameObject> objects)
            {
                var allNodes = objects.Where(o => o.Type != "#").Select(o => new GraphNode() { Node = o }).ToList();
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

                return allNodes;
            }

            public GraphNode RemoveByType(string type, HashSet<GraphNode> visited = null)
            {
                if (visited == null)
                {
                    visited = new HashSet<GraphNode>();
                }
                while (ConnectedNodes.Any(c => c.Node.Type == type))
                {
                    var toRemove = ConnectedNodes.First(n => n.Node.Type == type);
                    var toRepin = toRemove.ConnectedNodes.Where(x => x != this);
                    foreach (var node in toRepin)
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
        static void Main(string[] args)
        {
            var gameObjs = LoadGameObjects();
            var relevantPoints = gameObjs.Where(x => x.Type != "#" && x.Type != " ");
            var graph = GraphNode.CreateGraph(relevantPoints);
            graph = graph.OrderBy(g => g.Node.Type).ToList();            
            DetectPortalNodes(graph);
            WirePortals(graph);
           // var bu = graph.Last(g => g.Node.Type == "BU");
            var start = graph.Last(g => g.Node.Type == "AA");
            var finish = graph.Single(g => g.Node.Type == "ZZ");
            //var finish = graph.Single(x => x.Node.Point.Equals(new Point(2,17)));
            Console.WriteLine(GetShortestPats(start, finish) - 2);


            //Console.WriteLine(gameObjects.Where(g => g.Type == ObjType.Block).Count());
            Console.WriteLine("Hello World!");
        }

        private static int GetShortestPats(GraphNode startNode, GraphNode target)
        {
            int counter = 0;
            int result = -1;
            IEnumerable<GraphNode> nodes = new List<GraphNode>() { startNode };
            List<GraphNode> nextNodes = new List<GraphNode>();
            while (result == -1 && nodes.Count() > 0)
            {
                foreach (var node in nodes)
                {
                    node.Label = counter;
                    node.Visited = true;
                    if (node == target)
                        result = node.Label;
                }
                nextNodes = nodes.SelectMany(n => n.ConnectedNodes.Where(x => x.Visited == false)).ToList();
                nodes = nextNodes;
                counter++;
            }
            return result;
        }
        private static void WirePortals(List<GraphNode> graph)
        {
            var portals = graph.Where(n => n.Node.Type != ".").OrderBy(p => p.Node.Type).ToList();
            for (int i = 0; i < portals.Count; i++)
            {
                var portal = portals[i];
                if (portal.Node.Type == "AA" || portal.Node.Type == "ZZ")
                    continue;
                var correspondingPortal = portals[i + 1];
                Console.WriteLine($"Wireing up {portal.Node.Type} with {correspondingPortal.Node.Type}");
                portal = portal.ConnectedNodes.Single();
                correspondingPortal = correspondingPortal.ConnectedNodes.Single();
                portal.ConnectedNodes.Add(correspondingPortal);
                correspondingPortal.ConnectedNodes.Add(portal);
                // for (int i1 = 0; i1 < correspondingPortal.ConnectedNodes.Count; i1++)
                // {
                //     GraphNode cpNode = correspondingPortal.ConnectedNodes[i1];
                //     if(cpNode == portal)
                //         continue;
                //     cpNode.ConnectedNodes.Add(portal);
                // }

                //graph.Remove(correspondingPortal);
                i++;
            }
        }

        private static void DetectPortalNodes(List<GraphNode> graph)
        {
            for (int i = 0; i < graph.Count; i++)
            {
                var node = graph[i];
                if (node.Node.Type == ".")
                    continue;
                var sameType = node.ConnectedNodes.SingleOrDefault(n => n.Node.Type != ".");
                if (sameType != null)
                {
                    Console.WriteLine($"Dectected sametype: {node.Node.Type}  {sameType.Node.Type}");
                    var newType = string.Join("", new List<string>() { node.Node.Type, sameType.Node.Type }.OrderBy(x => x));
                    node.ConnectedNodes.AddRange(sameType.ConnectedNodes.Where(n => n != node));
                    for (int i1 = 0; i1 < sameType.ConnectedNodes.Count; i1++)
                    {
                        GraphNode sNode = sameType.ConnectedNodes[i1];
                        sNode.ConnectedNodes.Remove(sameType);
                    }
                    node.ConnectedNodes.Remove(sameType);
                    node.Node.Type = newType;
                    graph.Remove(sameType);
                }
            }
        }

        private static List<GameObject> LoadGameObjects()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            var lines = System.IO.File.ReadAllLines("input.txt");
            int y = 0;
            foreach (var line in lines)
            {
                int x = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    char type = line[i];
                    var point = new Point(x, y);
                    var gameObject = new GameObject(point, type.ToString());
                    gameObjects.Add(gameObject);
                    x++;
                }
                y++;
            }
            return gameObjects;
        }
    }
}