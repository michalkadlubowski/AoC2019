using System;
using System.Collections.Generic;
using System.Linq;

namespace _13
{

    public enum ObjType
    {
        Empty = 0,
        Wall = 1,
        Block = 2,
        HorizontalPaddle = 3,
        Ball = 4,
        Score = 99
    }

    class Program
    {

        public class GameObject
        {
            public int X {get; set;}
            public int Y {get; set;}
            public ObjType Type {get; set;}
        }
        public class OutputParser
        {
            int counter = 0;
            int currentX = 0;
            int currenyY =0;

            public GameObject Accept(long l)
            {
                var i = (int)l;
                if(counter == 0)
                {
                    currentX = (int)i;
                }
                if(counter == 1)
                {
                    currenyY = (int)i;
                }
                if(counter == 2)
                {
                    counter = 0;
                    if(currentX == -1 && currenyY == 0)
                    {
                        Console.WriteLine($"Current score: " +i);
                    }
                    return new GameObject(){X = currentX, Y = currenyY, Type = (ObjType)i};
                }
                counter++;
                return null;
            }

        }

        public static class JoysticPredictor
        {
            public static int GetJoystickMove(GameObject paddleObject, GameObject ballObject)
            {
                var res = ballObject.X.CompareTo(paddleObject.X);
                Console.WriteLine($"Ball {ballObject.X}, Paddle {paddleObject.X}, Result {res}");
                return res;
            }
        }

        static char Render(GameObject o)
        {
            switch(o.Type)
            {
                case ObjType.Ball:
                    return 'o';
                case ObjType.Block:
                    return '#';
                case ObjType.HorizontalPaddle:
                    return '_';                                                            
                case ObjType.Wall:
                    return 'X';    
                case ObjType.Empty:
                default:
                    return ' ';                                    
            }
        }

        static void Render(IEnumerable<GameObject> objects)
        {
            var rowGroups = objects.GroupBy(o => o.Y).OrderBy(g => g.Key);
            foreach(var g in rowGroups)
            {
                var cols = new String(g.OrderBy(go => go.X).Select(go => Render(go)).ToArray());
                Console.WriteLine(cols);
            }
            System.Threading.Thread.Sleep(100);
        }

        static void Main(string[] args)
        {
            string input = System.IO.File.ReadAllText("input.txt");
            var programList = input.Split(',').Select(long.Parse).ToList();
            programList.AddRange(Enumerable.Range(1,1000).Select(i => 0l));
            var program = programList.ToArray();
            var parser = new OutputParser();

            var gameObjects = new Dictionary<string,GameObject>();

            Action<long> outputFactory = (output) =>
                {
                    var result = parser.Accept(output);
                    if(result!= null)
                        gameObjects[$"{result.X},{result.Y}"] = result;
                };

            Func<long> inputProviderFactory = () =>
            {   
                Render(gameObjects.Values);
                var paddle = gameObjects.Values.Single(o => o.Type == ObjType.HorizontalPaddle);
                var ball = gameObjects.Values.Single(o => o.Type == ObjType.Ball);
                var move = JoysticPredictor.GetJoystickMove(paddle, ball);
                return move;
            };
            var opFactory = new OpCodeFactory(inputProviderFactory, outputFactory);

            var spaceProgram = new SpaceProgram(program, opFactory);
            spaceProgram.Execute();
            //Console.WriteLine(gameObjects.Where(g => g.Type == ObjType.Block).Count());
            Console.WriteLine("Hello World!");
        }
    }

    public class SpaceProgram
    {
        public long RelativeBase {get; set;}
        public string Label {get; set;}
        public long LastOutput {get; set;}
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
                if(!operationsCache.ContainsKey(CurrentPosition))
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
            var result = par1+par2;
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
            var result = par1*par2;
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
            Console.WriteLine("Written");
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
