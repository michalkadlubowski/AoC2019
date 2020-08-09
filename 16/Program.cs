using System;
using System.Collections.Generic;
using System.Linq;

namespace _16
{
    class Program
    {
        static void Main(string[] args)
        {
            int toSkip = 5978783;
            Console.WriteLine("Hello World!");
            var input = System.IO.File.ReadAllText("input.txt");
            var basePattern = new int[] { 0, 1, 0, -1 };
            var inputAsIntsOriginal = input.ToCharArray().Select(c => Int32.Parse(c.ToString())).ToList();
            var inputAsInts = input.ToCharArray().Select(c => Int32.Parse(c.ToString())).ToList();
            for(int i = 0; i < 10000 - 1; i++)
                inputAsInts.AddRange(inputAsIntsOriginal);

            inputAsInts = inputAsInts.Skip(toSkip).ToList();

            //phase
            int phasescount = 100;
            List<int> current = inputAsInts;
            for(int i =0; i< phasescount; i++)
            {
                current = Phase(basePattern, current, toSkip);
            }
            Console.WriteLine(string.Join('.',current.Select(c => c.ToString()).Take(8)));
        }

        private static List<int> Phase(int[] basePattern, List<int> inputAsInts, int skipped)
        {
            List<int> finalResult = new List<int>();

            int previousResult = 0;
            bool firstCalculated = false;
            for (int i = 0; i < inputAsInts.Count; i++)
            {
                int result = 0;
                if(firstCalculated == false)
                {
                    for (int y = 0; y < inputAsInts.Count; y++)
                    {
                        var multiplier = GetForPosition(i + 1 + skipped, basePattern, y + skipped);
                        var digitResult = inputAsInts[y] * multiplier;
                        result += digitResult;
                    }
                    previousResult = result;
                    firstCalculated = true;
                }
                else
                {
                    result = previousResult - inputAsInts[i-1];
                    previousResult = result;
                }
                finalResult.Add(Math.Abs(result % 10));
            }
            Console.WriteLine(string.Join('.',finalResult.Select(c => c.ToString()).Take(10)));
            return finalResult;
        }

        public static int GetForPosition(int repeatcount, int[] pattern, int position)
        {
            position = position + 1;
            var offset = (position/repeatcount)%4;
            var res= pattern[offset];
            return res;
        }
    }
}
