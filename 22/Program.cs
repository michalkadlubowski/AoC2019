using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace _22
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string[] input = System.IO.File.ReadAllLines("input.txt");
            var shuffles = input.Select(i => GetShuffleFromString(i));

            var deck = new Deck(10007);
            foreach(var shuffle in shuffles)
                deck.Shuffle(shuffle);

            Console.WriteLine(deck.GetPositionOfCard(2019));            

        }
        public static IShuffle GetShuffleFromString(string text)
        {
            var numRegex = new Regex(@"(-?\d+)");
            if(text == "deal into new stack")
                return new DealIntoNewStack();
            if(text.StartsWith("cut"))
            {
                var cutNo = int.Parse(numRegex.Match(text).Value);
                return new Cut(cutNo);
            }
            if(text.StartsWith("deal with"))
            {
                var cutNo = int.Parse(numRegex.Match(text).Value);
                return new DealWithIncrement(cutNo);
            } 
            throw new ArgumentException();           
        }
    }



    public class Deck
    {
        public int GetPositionOfCard(int card)
        {
            return _cards.IndexOf(card);
        }
        public void Print()
        {
            var res = string.Join(',', _cards.Select(c => c.ToString()).ToArray());
            Console.WriteLine(res);
        }
        private List<int> _cards;

        public Deck(int size)
        {
            _cards = Enumerable.Range(0, size).ToList();
        }

        public void Shuffle(IShuffle shuffle)
        {
            _cards = shuffle.Shuffle(_cards);
        }
    }

    public class DealWithIncrement : IShuffle
    {
        private int _incremenet;

        public DealWithIncrement(int incremenet)
        {
            _incremenet = incremenet;
        }

        public List<int> Shuffle(List<int> cards)
        {
            var result = new int[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                var position = i * _incremenet % cards.Count;
                result[position] = cards[i];
            }
            return result.ToList();

        }
    }

    public class DealIntoNewStack : IShuffle
    {
        public List<int> Shuffle(List<int> cards)
        {
            var copy = new List<int>(cards);
            copy.Reverse();
            return copy;
        }
    }
    public class Cut : IShuffle
    {
        int _cutValue;
        public Cut(int cutValue)
        {
            _cutValue = cutValue;
        }
        public List<int> Shuffle(List<int> cards)
        {
            var result = new List<int>();
            if (_cutValue > 0)
            {
                var firstPart = cards.Take(_cutValue);
                var secondPart = cards.Skip(_cutValue);
                result.AddRange(secondPart);
                result.AddRange(firstPart);
            }
            else
            {
                var firstPart = cards.Take(cards.Count + _cutValue);
                var secondPart = cards.Skip(cards.Count + _cutValue);
                result.AddRange(secondPart);
                result.AddRange(firstPart);
            }
            return result;
        }
    }
    public interface IShuffle
    {
        List<int> Shuffle(List<int> cards);
    }
}
