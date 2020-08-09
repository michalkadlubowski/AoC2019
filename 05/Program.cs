using System;
using System.Collections.Generic;
using System.Linq;

namespace _05
{
    public class SpaceProgram
    {
        public SpaceProgram(int[] program)
        {
            Program = program;
            CurrentPosition = 0;
        }
        public int[] Program { get; }
        public int CurrentPosition { get; set; }

        public void Execute()
        {
            while (true)
            {
                var operation = OpCodeFactory.GetOperation(Program[CurrentPosition]);
                if (operation is HaltOperation)
                    break;
                var parameters = ParameterFactory.GetParameters(this, operation.ParamsCount);
                operation.Execute(parameters, this);
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
            var result = parameters[0].GetParameterValue(program.Program) + parameters[1].GetParameterValue(program.Program);
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
            var result = parameters[0].GetParameterValue(program.Program) * parameters[1].GetParameterValue(program.Program);
            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = result;
            program.CurrentPosition++;
        }
    }

    public class InputOperation : Operation
    {
        public override int ParamsCount => 0;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            int input = 5;
            var positionToSet = program.Program[program.CurrentPosition];
            program.Program[positionToSet] = input;
            program.CurrentPosition++;
        }
    }

    public class OutputOperation : Operation
    {
        public override int ParamsCount => 1;

        public override void Execute(Parameter[] parameters, SpaceProgram program)
        {
            var result = parameters[0].GetParameterValue(program.Program);
            Console.WriteLine(result);
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

    public static class OpCodeFactory
    {
        public static Operation GetOperation(int instruction)
        {
            var instructionCode = instruction % 100;
            switch (instructionCode)
            {
                case 1:
                    return new AddOperation();
                case 2:
                    return new MultiplyOperation();
                case 3:
                    return new InputOperation();
                case 4:
                    return new OutputOperation();
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

    class Program
    {
        static void Main(string[] args)
        {
            string input = System.IO.File.ReadAllText("input.txt");
            var program = input.Split(',').Select(Int32.Parse).ToArray();
            var spaceProgram = new SpaceProgram(program);
            spaceProgram.Execute();

            Console.WriteLine("Hello World!");
        }
    }
}
