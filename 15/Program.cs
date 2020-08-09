using System;
using System.Collections.Generic;
using System.Linq;
using static _15.Program;

namespace _15
{

    public enum ObjType
    {
        Wall = 0,
        Empty = 1,
        OxygenSystem = 2,
    }

    public enum Direction
    {
        North = 1,
        South = 2,
        West = 3,
        East = 4
    }

    class Program
    {

        public class GameObject
        {
            public int X { get; set; }
            public int Y { get; set; }
            public ObjType Type { get; set; }
        }


        static char Render(GameObject o)
        {
            switch (o.Type)
            {
                case ObjType.OxygenSystem:
                    return '!';
                case ObjType.Wall:
                    return '█';
                case ObjType.Empty:
                default:
                    return ' ';
            }
        }

        static void Render(IEnumerable<GameObject> objects)
        {
            var rowGroups = objects.GroupBy(o => o.Y).OrderBy(g => g.Key);
            var adjust = Math.Abs(objects.Min(x => x.X));
            foreach (var g in rowGroups)
            {
                var rowStr = "                                                         ";
                foreach(var col in g)
                {
                    var charToDraw = Render(col);
                    var charArrr = rowStr.ToCharArray();
                    charArrr[col.X + adjust] = charToDraw;
                    rowStr = new string(charArrr);
                }
                Console.WriteLine(rowStr);
            }
            //System.Threading.Thread.Sleep(100);
        }

        public class MapCreator
        {
            private Dictionary<string, GameObject> _objects;

            public MapCreator(Dictionary<string, GameObject> objects)
            {
                _objects = objects;
            }
            public void GameObjectDetected(GameObject obj)
            {
                _objects[$"{obj.X},{obj.Y}"] = obj;
            }
        }

        public class Robot
        {
            public Stack<Direction> _unwind;            

            private MapCreator _map;

            public Robot(MapCreator map, Stack<Direction> unwind)
            {
                _map = map;
                _unwind = unwind;
                _map.GameObjectDetected(new GameObject() { X = this.X, Y = this.Y, Type = ObjType.Empty });
            }

            private Dictionary<Direction, Direction> revDict = new Dictionary<Direction, Direction>()
            {
                {Direction.West, Direction.East},
                {Direction.East, Direction.West},
                {Direction.North, Direction.South},
                {Direction.South, Direction.North}
            };

            public int X { get; set; }
            public int Y { get; set; }

            private Direction _direvtionMoved;
            private bool _isUnwind = false;

            public void Move(Direction direction, bool isUnwind = false)
            {
                _direvtionMoved = direction;
                _isUnwind = isUnwind;
            }

            public void ProcessMoveResult(ObjType type)
            {
                var xModifier = 0;
                var yModifier = 0;
                if (_direvtionMoved == Direction.West)
                    xModifier = -1;
                if (_direvtionMoved == Direction.North)
                    yModifier = 1;
                if (_direvtionMoved == Direction.East)
                    xModifier = 1;
                if (_direvtionMoved == Direction.South)
                    yModifier = -1;

                var gameObject = new GameObject() { X = X + xModifier, Y = Y + yModifier, Type = type };
                if (type != ObjType.Wall)
                {
                    this.X = gameObject.X;
                    this.Y = gameObject.Y;
                    if(!_isUnwind)
                        _unwind.Push(revDict[_direvtionMoved]);
                }
                _map.GameObjectDetected(gameObject);
            }
        }

        static void Main(string[] args)
        {
            string input = System.IO.File.ReadAllText("input.txt");
            var programList = input.Split(',').Select(long.Parse).ToList();
            programList.AddRange(Enumerable.Range(1, 1000).Select(i => 0l));
            var program = programList.ToArray();

            var gameObjects = new Dictionary<string, GameObject>();
            var mapCreator = new MapCreator(gameObjects);
            var unwind = new Stack<Direction>();
            var robot = new Robot(mapCreator, unwind);


            var rand = new Random();
            int counter = 0;
            Dictionary<string, HashSet<Direction>> moves = new Dictionary<string, HashSet<Direction>>();
            var allDirections = new List<Direction>() { Direction.West, Direction.North, Direction.East, Direction.South};
            // Func<long> inputProviderFactory = () =>
            // {
            //     if (counter++ % 1000000 == 0)
            //     {
            //         Render(gameObjects.Values);
            //         if(gameObjects.Any(g => g.Value.Type == ObjType.OxygenSystem))
            //         {
            //             Console.WriteLine(GetShortestPath(gameObjects.Values));
            //             Console.WriteLine(GetSizeFromOxygen(gameObjects.Values));
            //         }
            //     }


            //     var move = (Direction)rand.Next(1, 5);
            //     robot.Move(move);
            //     return (long)move;
            //     //return 1;
            // };
            Action<long> outputFactory = (output) =>
                {
                    robot.ProcessMoveResult((ObjType)output);

                };

            Func<long> inputProviderFactory = () =>
            {
                var position = $"{robot.X},{robot.Y}";
                if(moves.ContainsKey(position) == false)
                    moves.Add(position, new HashSet<Direction>(allDirections));
                var availableMoves = moves[position];
                if(availableMoves.Any())
                {
                    var selectedMove = availableMoves.First();
                    availableMoves.Remove(selectedMove);
                    robot.Move(selectedMove);
                    return(int)selectedMove;
                }
                else if(unwind.Any())
                {
                    var selectedMove = unwind.Pop();
                    robot.Move(selectedMove, true);
                    return (int)selectedMove;
                }
                else
                {
                    Render(gameObjects.Values);
                    Console.WriteLine(GetShortestPath(gameObjects.Values));
                    Console.WriteLine(GetSizeFromOxygen(gameObjects.Values));                    
                    return 1;
                }
                //return 1;
            };            
            var opFactory = new OpCodeFactory(inputProviderFactory, outputFactory);

            var spaceProgram = new SpaceProgram(program, opFactory);
            spaceProgram.Execute();
            //Console.WriteLine(gameObjects.Where(g => g.Type == ObjType.Block).Count());
            Console.WriteLine("Hello World!");
        }

        public static int GetShortestPath(IEnumerable<GameObject> objects)
        {
            var graph = GraphNode.CreateGraph(objects, null);
            int counter = 0;
            int result = -1;
            IEnumerable<GraphNode> nodes = new List<GraphNode>(){graph};
            List<GraphNode> nextNodes = new List<GraphNode>();
            while(result == -1)
            {
                foreach(var node in nodes)
                {
                    node.Label = counter;
                    node.Visited = true;
                    if(node.Node.Type == ObjType.OxygenSystem)
                        result = node.Label;
                }
                nextNodes = nodes.SelectMany(n => n.ConnectedNodes.Where(x => x.Visited == false)).ToList();
                nodes = nextNodes;
                counter++;
            }
            return result;
        }

        public static int GetSizeFromOxygen(IEnumerable<GameObject> objects)
        {
            var graph = GraphNode.CreateGraph(objects, ObjType.OxygenSystem);
            int counter = 0;
            bool allFound = false;
            IEnumerable<GraphNode> nodes = new List<GraphNode>(){graph};
            List<GraphNode> nextNodes = new List<GraphNode>();
            while(allFound == false)
            {
                foreach(var node in nodes)
                {
                    node.Label = counter;
                    node.Visited = true;
                }
                nextNodes = nodes.SelectMany(n => n.ConnectedNodes.Where(x => x.Visited == false)).ToList();
                nodes = nextNodes;
                if(nextNodes.Any() == false)
                    allFound =true;
                counter++;
            }
            return counter-1;
        }        

        public class GraphNode
        {

            private GraphNode()
            { 
                ConnectedNodes = new List<GraphNode>();
            }
            public static GraphNode CreateGraph(IEnumerable<GameObject> objects, ObjType? type)
            {
                var allNodes = objects.Where(o => o.Type != ObjType.Wall).Select(o => new GraphNode() {Node = o}).ToList();
                var sameColumns = allNodes.GroupBy(n => n.Node.X);
                foreach(var column in sameColumns)
                {
                    GraphNode previos = null;
                    foreach(var node in column.OrderBy(x => x.Node.Y))
                    {
                        if(previos != null && Math.Abs(Math.Abs(previos.Node.Y) - Math.Abs(node.Node.Y)) == 1)
                        {
                            previos.ConnectedNodes.Add(node);
                            node.ConnectedNodes.Add(previos);
                        }
                        previos = node;
                    }
                }
                var sameRows = allNodes.GroupBy(n => n.Node.Y);
                foreach(var row in sameRows)
                {
                    GraphNode previos = null;
                    foreach(var node in row.OrderBy(x => x.Node.X))
                    {
                        if(previos != null && Math.Abs(Math.Abs(previos.Node.X) - Math.Abs(node.Node.X)) == 1)
                        {
                            previos.ConnectedNodes.Add(node);
                            node.ConnectedNodes.Add(previos);
                        }
                        previos = node;
                    }
                }
                if(type == null)
                    return allNodes.Single(n => n.Node.X == 0 && n.Node.Y == 0);
                else
                    return allNodes.FirstOrDefault(n => n.Node.Type == type.Value);
            }
            public GameObject Node { get; set; }

            public int Label {get; set;}

            public bool Visited {get; set;}
            public List<GraphNode> ConnectedNodes {get; set;}
        }
    }



    public class SpaceProgram
    {
        public long RelativeBase { get; set; }
        public string Label { get; set; }
        public long LastOutput { get; set; }
        private OpCodeFactory _opCodeFactory;

        public SpaceProgram(long[] program, OpCodeFactory opCodeFactory = null)
        {
            Program = program;
            CurrentPosition = 0;
            _opCodeFactory = opCodeFactory;
        }

        public void SetOpCodeFactory(OpCodeFactory factory)
        {
            _opCodeFactory = factory;
        }

        public bool HasTerminated { get; private set; }
        public long[] Program { get; }
        public long CurrentPosition { get; set; }

        public void Execute()
        {
            var operationsCache = new Dictionary<long, Operation>();
            while (true)
            {
                //Console.WriteLine($"{Label} - State: {string.Join(',',Program.Select(p => p.ToString()))}");
                if (!operationsCache.ContainsKey(CurrentPosition))
                {
                    operationsCache[CurrentPosition] = _opCodeFactory.GetOperation(Program[CurrentPosition]);
                }
                var operation = operationsCache[CurrentPosition];
                if (operation is HaltOperation)
                {
                    HasTerminated = true;
                    break;
                }
                var parameters = ParameterFactory.GetParameters(this, operation.ParamsCount);
                operation.Execute(parameters, this);
            }
        }
    }


    public abstract class Operation
    {
        public abstract long ParamsCount { get; }

        protected Parameter[] parameters;

        public abstract void Execute(Parameter[] parameters, SpaceProgram program);
    }

    public class AddOperation : Operation
    {
        public override long ParamsCount => 3;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var par1 = parameters[0].GetParameterValue(program);
            var par2 = parameters[1].GetParameterValue(program);
            var result = par1 + par2;
            parameters[2].WriteToParameter(program, result);
            //program.CurrentPosition++;
        }
    }

    public class MultiplyOperation : Operation
    {
        public override long ParamsCount => 3;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var par1 = parameters[0].GetParameterValue(program);
            var par2 = parameters[1].GetParameterValue(program);
            var result = par1 * par2;
            parameters[2].WriteToParameter(program, result);
            //program.CurrentPosition++;
        }
    }

    public class InputOperation : Operation
    {
        Func<long> input;

        public InputOperation(Func<long> inputProvider)
        {
            input = inputProvider;
        }

        public override long ParamsCount => 1;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            parameters[0].WriteToParameter(program, input());
            //Console.WriteLine("Written");
        }
    }

    public class OutputOperation : Operation
    {
        Action<long> _outputAction;

        public OutputOperation(Action<long> outputAction)
        {
            _outputAction = outputAction;
        }

        public override long ParamsCount => 1;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var result = parameters[0].GetParameterValue(program);
            _outputAction(result);
            program.LastOutput = result;
        }
    }

    public class JumpIfTrueOperation : Operation
    {
        public override long ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var value = parameters[0].GetParameterValue(program);
            var jummpTo = parameters[1].GetParameterValue(program);
            if (value != 0)
            {
                program.CurrentPosition = jummpTo;
            }
        }
    }


    public class JumpIfFalseOperation : Operation
    {
        public override long ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var value = parameters[0].GetParameterValue(program);
            var jummpTo = parameters[1].GetParameterValue(program);
            if (value == 0)
            {
                program.CurrentPosition = jummpTo;
            }
        }
    }
    public class LessThanOperation : Operation
    {
        public override long ParamsCount => 3;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var first = parameters[0].GetParameterValue(program);
            var second = parameters[1].GetParameterValue(program);
            var res = first < second ? 1 : 0;

            parameters[2].WriteToParameter(program, res);
            //program.CurrentPosition++;

        }
    }
    public class EqualsOperation : Operation
    {
        public override long ParamsCount => 3;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var first = parameters[0].GetParameterValue(program);
            var second = parameters[1].GetParameterValue(program);
            var res = first == second ? 1 : 0;

            parameters[2].WriteToParameter(program, res);
            //program.CurrentPosition++;
        }
    }
    public class AdjustRelativeBaseOperation : Operation
    {
        public override long ParamsCount => 1;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var value = parameters[0].GetParameterValue(program);
            program.RelativeBase += value;
        }
    }

    public class HaltOperation : Operation
    {
        public override long ParamsCount => 0;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            Console.WriteLine("HALT");
        }
    }
    public static class ParameterFactory
    {
        public static Parameter[] GetParameters(SpaceProgram program, long expectedCount)
        {

            long[] paramTypes = GetParameterTypes(program.Program[program.CurrentPosition]);
            while (paramTypes.Length < expectedCount)
                paramTypes = paramTypes.Append(0).ToArray();

            program.CurrentPosition++;

            List<Parameter> parameters = new List<Parameter>();
            for (long i = 0; i < expectedCount; i++)
            {
                var param = new Parameter(paramTypes[i], program.Program[program.CurrentPosition]);
                parameters.Add(param);
                program.CurrentPosition++;
            }
            return parameters.ToArray();
        }

        private static long[] GetParameterTypes(long v)
        {
            return v.ToString().ToCharArray().Reverse()
            .Skip(2).Select(c => long.Parse(c.ToString())).ToArray();
        }
    }

    public class OpCodeFactory
    {
        private Func<long> _inputProviderFactory;
        private Action<long> _outputFactory;

        public OpCodeFactory(Func<long> inputProviderFactory, Action<long> outputFactory)
        {
            _inputProviderFactory = inputProviderFactory;
            _outputFactory = outputFactory;
        }

        public Operation GetOperation(long instruction)
        {
            var instructionCode = instruction % 100;
            switch (instructionCode)
            {
                case 1:
                    return new AddOperation();
                case 2:
                    return new MultiplyOperation();
                case 3:
                    return new InputOperation(_inputProviderFactory);
                case 4:
                    return new OutputOperation(_outputFactory);
                case 5:
                    return new JumpIfTrueOperation();
                case 6:
                    return new JumpIfFalseOperation();
                case 7:
                    return new LessThanOperation();
                case 8:
                    return new EqualsOperation();
                case 9:
                    return new AdjustRelativeBaseOperation();
                case 99:
                default:
                    return new HaltOperation();
            }
        }
    }

    public class Parameter
    {
        private Func<SpaceProgram, long, long> _getParameter;
        private Action<SpaceProgram, long> _writeParameter;

        private long _value;
        public Parameter(long type, long value)
        {
            _value = value;
            switch (type)
            {
                //position mode
                case 0:
                    _getParameter = (program, position) => program.Program[position];
                    _writeParameter = (program, val) => program.Program[_value] = val;
                    break;
                //immediate
                case 1:
                    _getParameter = (program, position) => position;
                    _writeParameter = (program, val) => program.Program[program.CurrentPosition] = val; //??
                    break;
                //relative
                case 2:
                default:
                    _getParameter = (program, position) => program.Program[position + program.RelativeBase];
                    _writeParameter = (program, val) => program.Program[_value + program.RelativeBase] = val;
                    break;
            }
        }

        public long GetParameterValue(SpaceProgram program)
        {
            return _getParameter(program, _value);
        }

        public void WriteToParameter(SpaceProgram program, long valueToWrite) => _writeParameter(program, valueToWrite);
    }
}
