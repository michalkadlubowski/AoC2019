using System;
using System.Collections.Generic;
using System.Linq;

namespace _06
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var data = System.IO.File.ReadAllLines("input.txt").Select(ParseString).ToList();
            Dictionary<string, Node> nodes = new Dictionary<string, Node>();
            foreach(var orbitData in data)
            {
                if(!nodes.ContainsKey(orbitData.CenterObject))
                    nodes.Add(orbitData.CenterObject, new Node(orbitData.CenterObject));
                var centerObject = nodes[orbitData.CenterObject];
                if(!nodes.ContainsKey(orbitData.OrbitingObject))
                    nodes.Add(orbitData.OrbitingObject, new Node(orbitData.OrbitingObject));
                var orbitingObject = nodes[orbitData.OrbitingObject];
                centerObject.AddNode(orbitingObject);
            }
            // var centerNode = nodes["COM"];
            // var total = centerNode.Expand().Sum(c => c.GetDistanceToRoot());

            var you = nodes["YOU"];
            you.Bfs(0);
            var san = nodes["SAN"];            
            Console.WriteLine(san.Label - 2);

        }

        private static OrbitData ParseString(string orbitData)
        {
            var centerObject = orbitData.Substring(0,orbitData.IndexOf(')'));
            var orbitingObject = orbitData.Substring(orbitData.IndexOf(')') + 1);
            return new OrbitData{CenterObject = centerObject, OrbitingObject = orbitingObject};
        }
    }

    public class OrbitData
    {
        public string CenterObject { get; set; }
        public string OrbitingObject { get; set; }
    }

    public class Node
    {
        public int? Label {get; set;}
        public void Bfs(int sourceLabel)
        {
            this.Label = sourceLabel;
            if(_parent != null && _parent.Label == null)
                _parent.Bfs(this.Label.Value + 1);
            foreach(var node in _connectedNodes.Where(n => n.Label == null))
                node.Bfs(this.Label.Value + 1);
        }
        // public int HowManyStepsTo(Node node)
        // {
        //     if(!this.Expand().Contains(node) && _parent._visited)
        //         return -1;
        //     else if(!this.Expand().Contains(node) && !_parent._visited)
        //         return 1 + _parent.HowManyStepsTo(node);
        //     if(this._connectedNodes.Contains(node))
        //         return 1;
        //     return this.Expand().Where(n => !n._visited && HowManyStepsTo(node) > -1 ).Min(n => n.HowManyStepsTo(node)) + 1;
        // }

        public IEnumerable<Node> Expand()
        {
            yield return this;
            foreach(var node in _connectedNodes.SelectMany(c => c.Expand()))
                yield return node;
        }

        public int GetDistanceToRoot()
        {
            int res = 0;
            var currentParent = _parent;
            while(currentParent != null)
            {
                res++;
                currentParent = currentParent._parent;
            }
            return res;
        }

        public Node(string name)
        {
            ObjectName = name;
        }
        public string ObjectName { get; private set;}

        private List<Node> _connectedNodes = new List<Node>();

        private Node _parent = null;

        public void AddNode(Node node)
        {
            node._parent = this;
            _connectedNodes.Add(node);
        }
        
    }
}
