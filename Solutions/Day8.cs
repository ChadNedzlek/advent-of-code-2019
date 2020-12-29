using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day8
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            int minZeroCount = int.MaxValue;
            int oneCount = 0;
            int twoCount = 0;
            for (int i = 0; i < data[0].Length; i += 6 * 25)
            {
                var chunk = data[0][i..(i + 25 * 6)];
                var zeroCount = chunk.Count(c => c == '0');
                if (zeroCount < minZeroCount)
                {
                    minZeroCount = zeroCount;
                    oneCount = chunk.Count(c => c == '1');
                    twoCount = chunk.Count(c => c == '2');

                    Console.WriteLine();
                    for (int r = 0; r < 6; r++)
                    {
                        for (int c = 0; c < 25; c++)
                        {
                            Console.Write(chunk[c + r * 25]);
                        }
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }
            }

            Console.WriteLine($"1 digits x 2 digits = {oneCount} x {twoCount} = {oneCount * twoCount}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            char[] message = new char[6 * 25];
            Array.Fill(message, '2');
            for (int i = 0; i < data[0].Length; i += 6 * 25)
            {
                var chunk = data[0][i..(i + 25 * 6)];
                for (int ic = 0; ic < chunk.Length; ic++)
                {
                    if (message[ic] == '2')
                        message[ic] = chunk[ic];
                }
            }

            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 25; c++)
                {
                    Console.Write(message[c + r * 25] == '0' ? ' ' : '#');
                }
                Console.WriteLine();
            }
        }
    }
}