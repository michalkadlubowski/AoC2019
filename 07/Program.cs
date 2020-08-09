using System;
using System.Collections.Generic;
using System.Linq;

namespace _07
{
    class Program
    {
        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        static void Main(string[] args)
        {
            string input = System.IO.File.ReadAllText("input.txt");
            var program = input.Split(',').Select(Int32.Parse).ToArray();
            var inputs = new List<int> {5,6,7,8,9};
            var maxOutput = int.MinValue;
            //GetOutput(program, new int[] { 9,8,7,6,5});
            foreach(var permutation in GetPermutations(inputs, inputs.Count))
            {
                var output = GetOutput(program, permutation.ToArray());
                if (output>maxOutput)
                {
                    maxOutput = output;
                    Console.WriteLine(string.Join(',',permutation.Select(i => i.ToString())) + " = " + output );
                }
            }
            Console.WriteLine(maxOutput);
            Console.WriteLine("Hello World!");
        }

         public static int GetOutput(int[] program, int[] combination)
        {
            var programs = combination.Select(c => new SpaceProgram(program.Clone() as int[])).ToArray();
            var signals = combination.Select(c => 0 ).ToArray();
            Func<bool> lastProgramTerminated = () => programs[programs.Length-1].HasTerminated;
            for(int i = 0; i < programs.Length;i++)
                programs[i].Label = "Amp " + i;
            for (int i = 0; i < combination.Length; i++)
            {
                var currentPosition = i;                
                var amplifier = programs[i];
                Action<int> outputFactory = (output) =>
                {
                    Console.WriteLine(output);
                    var currentPositionLocal = currentPosition;
                    var programIndexToExecute = currentPositionLocal == combination.Length -1? 0 : currentPositionLocal + 1;
                    var programToExecute = programs[programIndexToExecute];
                    if (!lastProgramTerminated())
                    {
                        signals[programIndexToExecute] = output;
                        Console.WriteLine("Executing amplifier: " + programToExecute.Label + " from amplifier " + amplifier.Label + " with output " + output);
                        programToExecute.Execute();
                    }
                };
                bool firstInputProvided = false;
                bool zeroProvidedtoFirst = false;
                Func<int> inputProviderFactory = () => 
                {
                    int returnValue = 0;
                    var currentPositionLocal = currentPosition;
                    if(!firstInputProvided)
                    {
                        firstInputProvided = true;
                        returnValue = combination[currentPositionLocal];
                    }
                    else if(!zeroProvidedtoFirst && currentPositionLocal == 0)
                    {
                        zeroProvidedtoFirst = true;
                        returnValue = 0;
                    }
                    // Console.WriteLine("providing signal " + signal + " from " + signalOrigin);
                    else
                    {
                        returnValue = signals[currentPositionLocal];
                    }
                    //Console.WriteLine($"Providing Amp {currentPositionLocal} with {returnValue}");
                    return returnValue;
                };
                var opFactory = new OpCodeFactory(inputProviderFactory, outputFactory);
                amplifier.SetOpCodeFactory(opFactory);
            }
            programs[0].Execute();
            return signals.First();
        }
    }

    public class SpaceProgram
    {
        public string Label {get; set;}
        public int LastOutput {get; set;}
        private OpCodeFactory _opCodeFactory;

        public SpaceProgram(int[] program, OpCodeFactory opCodeFactory = null)
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
        public int[] Program { get; }
        public int CurrentPosition { get; set; }

        public void Execute()
        {
            var operationsCache = new Dictionary<int, Operation>();
            while (true)
            {
                //Console.WriteLine($"{Label} - State: {string.Join(',',Program.Select(p => p.ToString()))}");
                if(!operationsCache.ContainsKey(CurrentPosition))
                {
                    operationsCache[CurrentPosition] = _opCodeFactory.GetOperation(Program[CurrentPosition]);
                }
                var operation = operationsCache[CurrentPosition];
                Console.WriteLine(Label + " at position " + CurrentPosition + " with operation " + operation.GetType().Name);
                if (operation is HaltOperation)
                {
                    HasTerminated = true;
                    break;
                }
                var parameters = ParameterFactory.GetParameters(this, operation.ParamsCount);
                operation.Execute(parameters, this);
                if (operation is OutputOperation)
                {
                    break;
                }                
            }
        }
    }


    public abstract class Operation
    {
        public abstract int ParamsCount { get; }

        protected Parameter[] parameters;

        public abstract void Execute(Parameter[] parameters, SpaceProgram program);
    }

    public class AddOperation : Operation
    {
        public override int ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var par1 = parameters[0].GetParameterValue(program.Program);
            var par2 = parameters[1].GetParameterValue(program.Program);
            var result = par1+par2;
            Console.WriteLine($"{par1} + {par2} = {result}");
            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = result;
            program.CurrentPosition++;
        }
    }

    public class MultiplyOperation : Operation
    {
        public override int ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var par1 = parameters[0].GetParameterValue(program.Program);
            var par2 = parameters[1].GetParameterValue(program.Program);
            var result = par1*par2;
            Console.WriteLine($"{par1} * {par2} = {result}");
            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = result;
            program.CurrentPosition++;
        }
    }

    public class InputOperation : Operation
    {
        Lazy<int> input;

        public InputOperation(Func<int> inputProvider)
        {
            input = new Lazy<int>(inputProvider);
        }

        public override int ParamsCount => 0;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = input.Value;
            program.CurrentPosition++;
            Console.WriteLine("Used input: " + input.Value);
        }
    }

    public class OutputOperation : Operation
    {
        Action<int> _outputAction;

        public OutputOperation(Action<int> outputAction)
        {
            _outputAction = outputAction;
        }

        public override int ParamsCount => 1;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var result = parameters[0].GetParameterValue(program.Program);
            _outputAction(result);
            program.LastOutput = result;
        }
    }

    public class JumpIfTrueOperation : Operation
    {
        public override int ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var value = parameters[0].GetParameterValue(program.Program);
            var jummpTo = parameters[1].GetParameterValue(program.Program);
            if (value != 0)
            {
                program.CurrentPosition = jummpTo;
            }
        }
    }


    public class JumpIfFalseOperation : Operation
    {
        public override int ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var value = parameters[0].GetParameterValue(program.Program);
            var jummpTo = parameters[1].GetParameterValue(program.Program);
            if (value == 0)
            {
                program.CurrentPosition = jummpTo;
            }
        }
    }
    public class LessThanOperation : Operation
    {
        public override int ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var first = parameters[0].GetParameterValue(program.Program);
            var second = parameters[1].GetParameterValue(program.Program);
            var res = first < second ? 1 : 0;

            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = res;
            program.CurrentPosition++;

        }
    }
    public class EqualsOperation : Operation
    {
        public override int ParamsCount => 2;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var first = parameters[0].GetParameterValue(program.Program);
            var second = parameters[1].GetParameterValue(program.Program);
            var res = first == second ? 1 : 0;

            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = res;
            program.CurrentPosition++;
        }
    }
    public class HaltOperation : Operation
    {
        public override int ParamsCount => 0;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            Console.WriteLine("HALT");
        }
    }
    public static class ParameterFactory
    {
        public static Parameter[] GetParameters(SpaceProgram program, int expectedCount)
        {

            int[] paramTypes = GetParameterTypes(program.Program[program.CurrentPosition]);
            while (paramTypes.Length < expectedCount)
                paramTypes = paramTypes.Append(0).ToArray();

            program.CurrentPosition++;

            List<Parameter> parameters = new List<Parameter>();
            for (int i = 0; i < expectedCount; i++)
            {
                var param = new Parameter(paramTypes[i], program.Program[program.CurrentPosition]);
                parameters.Add(param);
                program.CurrentPosition++;
            }
            return parameters.ToArray();
        }

        private static int[] GetParameterTypes(int v)
        {
            return v.ToString().ToCharArray().Reverse()
            .Skip(2).Select(c => Int32.Parse(c.ToString())).ToArray();
        }
    }

    public class OpCodeFactory
    {
        private Func<int> _inputProviderFactory;
        private Action<int> _outputFactory;

        public OpCodeFactory(Func<int> inputProviderFactory, Action<int> outputFactory)
        {
            _inputProviderFactory = inputProviderFactory;
            _outputFactory = outputFactory;
        }

        public Operation GetOperation(int instruction)
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
                case 99:
                default:
                    return new HaltOperation();
            }
        }
    }

    public class Parameter
    {
        private Func<int[], int, int> _getParameter;

        private int _value;
        public Parameter(int type, int value)
        {
            _value = value;
            switch (type)
            {
                //position mode
                case 0:
                    _getParameter = (program, position) => program[position];
                    break;
                //immediate
                case 1:
                default:
                    _getParameter = (program, position) => position;
                    break;
            }
        }

        public int GetParameterValue(int[] program)
        {
            return _getParameter(program, _value);
        }
    }
}
