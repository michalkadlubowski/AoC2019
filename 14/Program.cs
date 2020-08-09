using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace _14
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = System.IO.File.ReadAllLines("input.txt");
            long res = 0;
            var currentFactorMin = 1000l;
            var currentFactorMax = 1000000000000;
            var target = 1000000000000;
            while(res != 1000000000000)
            {
                var probe = (currentFactorMin + currentFactorMax)/2;
                var formulas = input.Select(l => new Formula(l)).ToList();
                var fuelFormula = formulas.Single(f => f.Result.Chemical == "FUEL");
                formulas.Remove(fuelFormula);
                formulas.Add(fuelFormula.Scale(probe));
                res = FormulaReducer.Reduce(formulas).Ingredients.Single().Quantity;
                Console.WriteLine($"Probed {probe} resulted in {res}");
                if(res < target)
                {
                    currentFactorMin = probe;
                }
                else
                {
                    currentFactorMax = probe;
                }
            }

            // var test1 = IngredientsFinder.FindSmallestOreRequirement(new ChemInfo("1 B"), formulas);
            // var test2 = IngredientsFinder.FindSmallestOreRequirement(new ChemInfo("1 E"), formulas);
            // var res = IngredientsFinder.FindSmallestOreRequirement(new ChemInfo("1 FUEL"), formulas);
            Console.WriteLine(res);
        }
    }

    public class ChemInfo
    {
        public override string ToString() 
        {
            return $"{Quantity} {Chemical}";
        }

        public ChemInfo()
        {
        }

        public ChemInfo(string chemInfoAsString)
        {
            var chemInfoRegex = new Regex(@"(\d+) (\w+)");
            var resultAsChemInfo = chemInfoRegex.Match(chemInfoAsString);
            Quantity = long.Parse(resultAsChemInfo.Groups[1].Value);
            Chemical = resultAsChemInfo.Groups[2].Value;
        }

        public ChemInfo Scale(long scale)
        {
            return new ChemInfo() { Quantity = this.Quantity * scale, Chemical = this.Chemical };
        }

        public long Quantity { get; set; }

        public string Chemical { get; set; }
    }

    public class ChemResult
    {
        public long Quantity {get;set;}
        public List<ChemInfo> Remains {get;set;}

    }

    public static class FormulaReducer
    {
        public static Formula Reduce(List<Formula> formulas)
        {
            if(formulas.Count() == 1)
                return formulas.First();

            //fing formlula that that no other result is taken as parameter except for FUEL
            var fuelFormula = formulas.Where(f => f.Result.Chemical == "FUEL").Single();
            // .Where(f => !formulas.Any(fi => fi.Result.Chemical != "FUEL" && fi.Ingredients.Any(i => i.Chemical == f.Result.Chemical)))
            // .FirstOrDefault();

            var allOtherForumlas = formulas.Where(f => f != fuelFormula);
            //get ing to reduce - one that no other takes as parameter
            var firstIngToReduce = fuelFormula.Ingredients.Where(i => !allOtherForumlas.Any(fi => fi.Ingredients.Any(ix => ix.Chemical == i.Chemical)))
            .FirstOrDefault();

            var formulaToReplaceIngWith = formulas.First(f => f.Result.Chemical == firstIngToReduce.Chemical);
            formulas.Remove(formulaToReplaceIngWith);

            if(formulaToReplaceIngWith.Result.Quantity < firstIngToReduce.Quantity)
            {
                var multiplier = (long)Math.Ceiling((double)firstIngToReduce.Quantity/formulaToReplaceIngWith.Result.Quantity);
                formulaToReplaceIngWith = formulaToReplaceIngWith.Scale(multiplier);
            }

            fuelFormula.Ingredients.Remove(firstIngToReduce);
            fuelFormula.Ingredients.AddRange(formulaToReplaceIngWith.Ingredients.ToList());
            fuelFormula.Reduce();

            return Reduce(formulas);
        }
    }

    // public static class IngredientsFinder
    // {
    //     public static ChemResult FindSmallestOreRequirement(ChemInfo info, IEnumerable<Formula> formulas, IEnumerable<ChemInfo> remains)
    //     {
    //         if(info.Chemical == "ORE")
    //         {
    //             return new ChemResult() { Quantity = info.Quantity, Remains = new List<ChemInfo>()};
    //         }

    //         var applicableFormulas = formulas.Where(f => f.Result.Chemical == info.Chemical);

    //         long minRequiredOre = long.MaxValue;
    //         foreach (var formula in applicableFormulas)
    //         {
    //             if(applicableFormulas.Count() > 1)
    //                 Console.WriteLine("More than 1 matching formula!!!!");
    //             var formulaToCalcRequirementsFrom = formula;
    //             long multiplier = 1;
    //             if (formula.Result.Quantity < info.Quantity)
    //             {
    //                 multiplier = (long)Math.Ceiling((double)info.Quantity / formula.Result.Quantity);
    //                 formulaToCalcRequirementsFrom = formula.Scale(multiplier);
    //             }

    //             long totalRequiredOre = 0;
    //             foreach(var ing in formulaToCalcRequirementsFrom.Ingredients)
    //             {
    //                 var inRemains = remains.FirstOrDefault(r => r.Chemical == ing.Chemical);
    //                 if(inRemains != null)
    //                 {
    //                     var reducedIng = new ChemInfo() {Quantity = ing.Quantity = inRemains.Quantity, Chemical = ing.Chemical};
    //                     totalRequiredOre += FindSmallestOreRequirement(reducedIng, formulas, remains);
    //                 }
    //             }
    //             if(totalRequiredOre < minRequiredOre)
    //                 minRequiredOre = totalRequiredOre;
    //         }
    //         Console.WriteLine($"Producing {info.Quantity} {info.Chemical} reuqires {minRequiredOre} ORE");
    //         return minRequiredOre;
    //     }
    // }

    public class Formula
    {
        public override string ToString()
        {
            var indgs = Ingredients.Select(i => i.ToString());
            var indgsAsSingleString = string.Join(',', indgs.ToArray());
            return $"{indgsAsSingleString} => {Result.ToString()}";
        }

        private Formula() {}
        public Formula(string formulaAsString)
        {
            var resultQualifierIndex = formulaAsString.IndexOf('=');
            var ingredients = formulaAsString.Substring(0, resultQualifierIndex);
            var result = formulaAsString.Substring(resultQualifierIndex);
            var separateIngredients = ingredients.Split(',');
            Result = new ChemInfo(result);
            Ingredients = separateIngredients.Select(s => new ChemInfo(s)).ToList();

        }

        public Formula Scale(long factor)
        {
            var ingredients = this.Ingredients.Select(i => i.Scale(factor));
            var result = this.Result.Scale(factor);
            return new Formula() {Ingredients = ingredients.ToList(), Result = result};
        }

        public void Reduce()
        {
            var newIngs = this.Ingredients.GroupBy(i => i.Chemical).Select(g => new ChemInfo() {Chemical = g.Key, Quantity = g.Sum(x => x.Quantity)});
            Ingredients = newIngs.ToList();
        }
        public ChemInfo Result { get; set; }

        public List<ChemInfo> Ingredients { get; set; }
    }
}
