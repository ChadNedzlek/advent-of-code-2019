using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day16
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            int[] values = data[0].Select(c => c - '0').ToArray();
            int[] pattern = {0, 1, 0, -1};
            for (int iteration = 0; iteration < 100; iteration++)
            {
                D.WriteLine($"After phase {iteration} : {string.Join("", values)}");
                int[] n = new int[values.Length];
                n[^1] = values[^1];
                for (int iNum = values.Length - 2; iNum >= 0; iNum--)
                {
                    n[iNum] = (n[iNum + 1] + values[iNum]) % 10;
                }

                values = n;
            }

            Console.WriteLine($"First 8 digits after 100 iterations: {string.Join("", values[..8])}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            int[] values = data[0].Select(c => c - '0').ToArray();
            int messageOffset = values[..7].Aggregate(0, (accumulator, v) => accumulator * 10 + v);
            
            int[] n = new int[values.Length * 10_000];
            for (int i = 0; i < n.Length; i++)
            {
                n[i] = values[i % values.Length];
            }
            values = n;
            n = new int[values.Length];
            
            for (int iteration = 0; iteration < 100; iteration++)
            {
                D.WriteLine($"After phase {iteration:000} : {string.Join("", values[^50..])}");
                n[^1] = values[^1];
                for (int iNum = values.Length - 2; iNum >= 0; iNum--)
                {
                    n[iNum] = (n[iNum + 1] + values[iNum]) % 10;
                }

                Array.Copy(n, values, n.Length);
            }

            var chunk = values[messageOffset..(messageOffset + 8)];

            Console.WriteLine($"Chunk?: {string.Join("", chunk[..8])}");
        }
    }
}