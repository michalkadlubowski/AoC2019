using System;
using System.Collections.Generic;
using System.Linq;

namespace _19
{
    class Program
    {

        static void Main(string[] args)
        {
            string input = System.IO.File.ReadAllText("input.txt");
            var programList = input.Split(',').Select(long.Parse).ToList();
            programList.AddRange(Enumerable.Range(1, 100).Select(i => 0l));
            var program = programList.ToArray();

            HashSet<Point> paintedPoints = new HashSet<Point>();
            Dictionary<Point, int> colorDict = new Dictionary<Point, int>();
            colorDict.Add(new Point(0, 0), 1);

            //402 ma 100
            var xrange = Enumerable.Range(929, 10);
            var yrange = Enumerable.Range(812, 10);
            var allPoints = xrange.Join(yrange, r => true, r => true, (x, y) => new Point(x, y)).ToList();

            var paintingRobot = new Robot() { Position = new Point(0, 0) };

            //IList<MapPixel> map = new List<MapPixel>();

            foreach (var point in allPoints)
            {
                bool res = CheckPoint(program, point);
                if(res == true)
                {
                    Console.WriteLine($"{point.X},{point.Y} - {res}");
                }
            }

            //RenderMap(map);

            // var mapDict = map.Where(z => z.Type == 35).ToDictionary(z => z.Location, z => z.Type);
            // var intersections = new List<Point>();
            // foreach (var kv in mapDict)
            // {
            //     var loc = kv.Key;
            //     var intersectionPoints = new List<Point>() { new Point(loc.X - 1, loc.Y), new Point(loc.X + 1, loc.Y), new Point(loc.X, loc.Y - 1), new Point(loc.X, loc.Y + 1) };
            //     if (intersectionPoints.All(p => mapDict.ContainsKey(p)))
            //         intersections.Add(loc);
            // }
            // Console.WriteLine(intersections.Sum(l => l.X * l.Y));
            // var elements = FindPaths(map.Where(z => z.Type == 35 || z.Type == (int)'^'));
            // var intersected = elements;
            // foreach(var intersection in intersections)
            // {
            //     intersected = intersected.SelectMany(e => e.Intersect(intersection)).ToList();
            // }

            // foreach(var i in intersected)
            // {
            //     Console.WriteLine(i.ToString());
            // }

        }

        private static bool CheckPoint(long[] program, Point refPoint)
        {
            var pointsToCheck = new List<Point>() { refPoint, new Point(refPoint.X + 100, refPoint.Y), new Point(refPoint.X, refPoint.Y + 100) };
            bool allAttracted = true;
            foreach (var point in pointsToCheck)
            {
                bool xProvided = false;
                bool isAttracted = false;
                // int x = 0;
                // int y = -1;
                //bool paintInstruction = true;
                Action<long> outputFactory = (output) =>
                {
                    isAttracted = output == 1;
                };

                Func<long> inputProviderFactory = () =>
                {
                    if (!xProvided)
                    {
                        xProvided = true;
                        return point.X;
                    }
                    return point.Y;
                };

                var opFactory = new OpCodeFactory(inputProviderFactory, outputFactory);

                var spaceProgram = new SpaceProgram(program.Clone() as long[], opFactory);
                spaceProgram.Execute();
                allAttracted &= isAttracted;
            }
            return allAttracted;
        }

        public static List<Element> FindPaths(IEnumerable<MapPixel> pixels)
        {
            List<Element> elements = new List<Element>();
            var byRow = pixels.GroupBy(p => p.Location.Y);
            foreach (var row in byRow.OrderBy(r => r.Key))
            {
                Point? previousPoint = null;
                Element current = null;
                foreach (var col in row.OrderBy(r => r.Location.X))
                {
                    if (previousPoint?.X == col.Location.X - 1)
                    {
                        if (current == null)
                        {
                            current = new Element() { Start = previousPoint.Value };
                        }
                        else if (row.OrderBy(r => r.Location.X).Last() == col)
                        {
                            current.End = col.Location;
                            elements.Add(current);
                            current = null;
                        }
                    }
                    else
                    {
                        if (current != null)
                        {
                            current.End = previousPoint.Value;
                            elements.Add(current);
                            current = null;
                        }
                    }
                    previousPoint = col.Location;
                }
            }
            var byColumn = pixels.GroupBy(p => p.Location.X);
            foreach (var column in byColumn.OrderBy(g => g.Key))
            {
                Point? previousPoint = null;
                Element current = null;
                foreach (var row in column.OrderBy(r => r.Location.X))
                {
                    if (previousPoint?.Y == row.Location.Y - 1)
                    {
                        if (current == null)
                        {
                            current = new Element() { Start = previousPoint.Value };
                        }
                        else if (column.OrderBy(r => r.Location.Y).Last() == row)
                        {
                            current.End = row.Location;
                            elements.Add(current);
                            Console.WriteLine(current);
                            current = null;
                        }
                    }
                    else
                    {
                        if (current != null)
                        {
                            current.End = previousPoint.Value;
                            elements.Add(current);
                            Console.WriteLine(current);
                            current = null;
                        }
                    }

                    previousPoint = row.Location;
                }

            }
            return elements;

        }

        public static void RenderMap(IEnumerable<MapPixel> pixels)
        {
            var byRow = pixels.GroupBy(p => p.Location.Y);
            foreach (var row in byRow)
            {
                string rowPrint = "";
                foreach (var col in row.OrderBy(r => r.Location.X))
                    rowPrint = rowPrint + col.Type;
                Console.WriteLine(rowPrint);
            }
        }
    }

    public class Element
    {
        public override string ToString()
        {
            return $"{Start.X},{Start.Y} - {End.X},{End.Y}, len = {Length}";
        }
        public int Length => Math.Abs(Start.X - End.X + Start.Y - End.Y);
        public Point Start { get; set; }
        public Point End { get; set; }

        public bool Contains(Point p)
        {
            if (p.X == Start.X && p.X == End.X)
            {
                return Start.Y <= p.Y && End.Y >= p.Y;
            }
            if (p.Y == Start.Y && p.Y == End.Y)
            {
                return Start.X <= p.X && End.X >= p.X;
            }
            return false;
        }

        public IEnumerable<Element> Intersect(Point p)
        {
            if (!Contains(p) || Start.Equals(p) || End.Equals(p))
                return new List<Element>() { this };
            else
            {
                if (p.X == Start.X && p.X == End.X)
                {
                    return new List<Element>();
                }
                if (p.Y == Start.Y && p.Y == End.Y)
                {
                    return new List<Element>() { new Element() { Start = Start, End = p }, new Element { Start = p, End = End } };
                }
                throw new Exception("Nope");
            }
        }

    }
    public class MapPixel
    {
        public Point Location { get; set; }
        public int Type { get; set; }
    }

    public class Robot
    {
        public Point Position { get; set; }
        public int Angle { get; set; }

        public void TurnAndMove(int direction)
        {
            if (direction == 0)
            {
                //left
                if (Angle == 0)
                    Angle = 360;
                Angle -= 90;
            }
            else if (direction == 1)
            {
                //right 90 deg
                Angle += 90;
                if (Angle == 360)
                    Angle = 0;
            }
            else
                throw new ArgumentException(nameof(direction));

            switch (Angle)
            {
                case 0:
                    Position = new Point(Position.X, Position.Y + 1);
                    break;
                case 90:
                    Position = new Point(Position.X + 1, Position.Y);
                    break;
                case 180:
                    Position = new Point(Position.X, Position.Y - 1);
                    break;
                case 270:
                    Position = new Point(Position.X - 1, Position.Y);
                    break;
                default:
                    throw new Exception("Unexpected angle");
            }
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
