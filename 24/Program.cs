using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _24
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var input = System.IO.File.ReadAllLines("input.txt");

            //var board = new Board(input);
            List<Board> allBoards = new List<Board>();
            var range = Enumerable.Range(0,200);
            Board prevBoard = null;
            foreach(var x in range)
            {
                var board = new Board(5);
                if(prevBoard != null)
                {
                    board._outer = prevBoard;
                    prevBoard._inner = board;
                }
                prevBoard = board;
                allBoards.Add(board);
            }
            var theBoard = new Board(input);
            prevBoard._inner = theBoard;
            theBoard._outer = prevBoard;
            prevBoard = theBoard;
            allBoards.Add(theBoard);
            foreach(var x in range)
            {
                var board = new Board(5);
                if(prevBoard != null)
                {
                    board._outer = prevBoard;
                    prevBoard._inner = board;
                }
                prevBoard = board;
                allBoards.Add(board);
            }

            prevBoard = null;

            foreach(var x in range)
            {
                for(int i = 0; i < allBoards.Count; i++)
                {
                    var boardCopy = allBoards[i].Step();
                    if(allBoards[i] == theBoard)
                    {
                        boardCopy.Print();
                        theBoard = boardCopy;
                    }

                    if(prevBoard != null)
                    {
                        boardCopy._outer = prevBoard;
                        prevBoard._inner = boardCopy;
                    }     
                    allBoards[i] = boardCopy;
                    prevBoard = boardCopy;               
                }
            }

            Console.WriteLine("-----------------------------");
            foreach(var board in allBoards)
                board.Print();

            Console.WriteLine(allBoards.Sum(b => b.GetBugsCount()));
            



            // HashSet<uint> prevBoards = new HashSet<uint>();
            // uint bioDiversity = 0;
            // while (!prevBoards.Contains(board.CalculateBioDiversity()))
            // {
            //     bioDiversity = board.CalculateBioDiversity();
            //     if (bioDiversity == 18862958)
            //     {
            //         board.Print();
            //     }
            //     prevBoards.Add(bioDiversity);
            //     board = board.Step();
            // }

            // board.Print();
            // Console.WriteLine(board.CalculateBioDiversity());
            // var asInt = board.GetAsInt();
        }
    }

    public class Board
    {

        public int GetBugsCount()
        {
             var length = _rows.GetLength(0);
            var width = _rows.GetLength(1);
            int total = 0;
            for (int i = 0; i < length; i++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (_rows[i, y])
                    {
                        total++;   
                    }
                }
            } 
            return total;
        }
        public int GetAsInt()
        {
            int count = 0;
            var list = _rows.Cast<bool>()
                                .GroupBy(x => count++ / _rows.GetLength(1))
                                .SelectMany(g => g.ToArray())
                                .ToList();
            var bitArray = new BitArray(list.ToArray());
            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];
        }

        public bool[,] _rows;
        public Board _inner;
        public Board _outer;

        private bool midLeft => _rows[2,1];
        private bool midRight => _rows[2,3];
        private bool midTop => _rows[1,2];
        private bool midBottom => _rows[3,2];

        private IEnumerable<bool> topRow => Enumerable.Range(0,_rows.GetLength(0)).Select(i => _rows[0,i]);
        private IEnumerable<bool> bottomRow => Enumerable.Range(0,_rows.GetLength(0)).Select(i => _rows[4,i]);
        private IEnumerable<bool> leftRow => Enumerable.Range(0,_rows.GetLength(1)).Select(i => _rows[i,0]);
        private IEnumerable<bool> rightRow => Enumerable.Range(0,_rows.GetLength(1)).Select(i => _rows[i,4]);


        public Board(int size)
        {
            _rows = new bool[size,size];
        }

        public Board(string[] rows)
        {
            _rows = new bool[rows.Length, rows[0].Length];
            for (int i = 0; i < rows.Length; i++)
            {
                string row = (string)rows[i];
                var chars = row.ToCharArray();
                for (int y = 0; y < chars.Length; y++)
                {
                    _rows[i, y] = chars[y] == '#';
                }
            }
        }

        public uint CalculateBioDiversity()
        {
            uint totalBiodiversity = 0;
            var length = _rows.GetLength(0);
            var width = _rows.GetLength(1);
            int power = 0;
            for (int i = 0; i < length; i++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (_rows[i, y])
                    {
                        checked
                        {
                            totalBiodiversity += (uint)Math.Pow(2, power);
                        }
                    }
                    power++;
                }
            }
            return totalBiodiversity;
        }

        public void Print()
        {
            var length = _rows.GetLength(0);
            var width = _rows.GetLength(1);
            for (int i = 0; i < length; i++)
            {
                string line = "";
                for (int y = 0; y < width; y++)
                {
                    line += _rows[i, y] ? "#" : ".";
                }
                Console.WriteLine(line);
            }
            Console.WriteLine("--------------");
        }

        internal IEnumerable<bool> GetAdjacent(int i, int y)
        {
            if(i == 2 && y == 2)
                yield break;

            var length = _rows.GetLength(0);
            var width = _rows.GetLength(1);
            if (y > 0)
                yield return _rows[i, y - 1];
            if (y < length - 1)
                yield return _rows[i, y + 1];
            if (i > 0)
                yield return _rows[i - 1, y];
            if (i < width - 1)
                yield return _rows[i + 1, y];

            if (_inner != null && i == 2 && y == 1)
            {
                foreach(var x in _inner.leftRow)
                    yield return x;
            }
            if (_inner != null && i == 2 && y == 3)
            {
                foreach(var x in _inner.rightRow)
                    yield return x;
            }
            if (_inner != null && i == 1 && y == 2)
            {
                foreach(var x in _inner.topRow)
                    yield return x;
            }
            if (_inner != null && i == 3 && y == 2)
            {
                foreach(var x in _inner.bottomRow)
                    yield return x;
            }

            if (_outer != null && i == 0)
            {
                yield return _outer.midTop;
            }   
            if (_outer != null && i == 4)
            {
                yield return _outer.midBottom;
            }               
            if (_outer != null && y == 0)
            {
                yield return _outer.midLeft;
            }
            if (_outer != null && y == 4)
            {
                yield return _outer.midRight;
            }                                       
        }
        internal Board Step()
        {
            var newBoard = new Board(5);
            var length = _rows.GetLength(0);
            var width = _rows.GetLength(1);
            newBoard._rows = new bool[length, width];
            for (int i = 0; i < length; i++)
            {
                for (int y = 0; y < width; y++)
                {
                    newBoard._rows[i, y] = _rows[i, y];
                }
            }

            for (int i = 0; i < length; i++)
            {
                for (int y = 0; y < width; y++)
                {
                    int surrounding = GetAdjacent(i, y).Count(b => b == true);

                    if (newBoard._rows[i, y])
                        newBoard._rows[i, y] = surrounding == 1;
                    else if (newBoard._rows[i, y] == false)
                        newBoard._rows[i, y] = surrounding == 1 || surrounding == 2;

                }
            }
            return newBoard;
        }
    }
}
