using System;
using System.Collections.Generic;
using System.Linq;

namespace _08
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = System.IO.File.ReadAllText("input.txt").ToCharArray();
            var pixels = input.Select(c => Int32.Parse(c.ToString())).ToArray();
            int width = 25;
            int height =6;
            var layerSize = width * height;

            var layers = new List<Layer>();
            while (pixels.Take(layerSize).Count() > 0)
            {
                var newLayer = new Layer(pixels.Take(layerSize).ToArray(), width, height);
                layers.Add(newLayer);
                pixels = pixels.Skip(layerSize).ToArray();
            }
            // var minZeros = layers.Select(l => l.AllPixels.Where(p => p == 0).Count()).Min();
            // var layweWithMinZeros = layers.Where(l => l.AllPixels.Where(p => p ==0).Count() == minZeros).Single();
            // var oneDigits = layweWithMinZeros.AllPixels.Count(p => p == 1);
            // var twoDigits = layweWithMinZeros.AllPixels.Count(p => p == 2);
            layers.Reverse();;
            Layer final = layers.First();
            foreach (var layer in layers)
            {
                final.ApplyLayer(layer);
            }


            final.Print();
        }

        public class Layer
        {
            int[,] _rows;

            public Layer(int[] pixels, int width, int height)
            {
                if (pixels.Length != width * height)
                    throw new ArgumentException("Bad length for size of layer");

                AllPixels = pixels;
                int[,] rows = new int[height, width];

                int currentRow = 0;
                int currentPosition = 0;

                for (int i = 0; i < pixels.Length; i++)
                {
                    Console.WriteLine(currentPosition + "," + currentRow);
                    rows[currentRow, currentPosition] = pixels[i];
                    if (currentPosition == width - 1)
                    {
                        currentRow++;
                        currentPosition = 0;
                    }
                    else
                    {
                        currentPosition++;
                    }
                }
                _rows = rows;
            }

            public int[] AllPixels;

            public int[] GetRow(int rowNumber)
            {
                return Enumerable.Range(0, _rows.GetLength(1))
                        .Select(x => _rows[rowNumber, x])
                        .ToArray();
            }

            public void ApplyLayer(Layer secondLayer)
            {
                for (int k = 0; k < _rows.GetLength(0); k++)
                    for (int l = 0; l < _rows.GetLength(1); l++)
                    {
                        var belowPixel = _rows[k, l];
                        var abovePixel = secondLayer._rows[k, l];
                        _rows[k, l] = ApplyPixel(belowPixel, abovePixel);
                    }
            }

            private int ApplyPixel(int pixelBelow, int pixelAbove)
            {
                if (pixelAbove == 2)
                    return pixelBelow;
                return pixelAbove;
            }

            public void Print()
            {
                var rowCount = _rows.GetLength(0);
                var colCount = _rows.GetLength(1);
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        var val = _rows[row, col];
                        var output = val == 0? "█": " ";
                        Console.Write(String.Format("{0}", output));
                    }
                    Console.WriteLine();
                }
            }

        }

    }
}
