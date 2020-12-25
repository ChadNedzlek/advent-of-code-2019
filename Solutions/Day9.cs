using System;
using System.Linq;
using System.Threading.Tasks;
using Advent2019;

namespace AdventOfCode.Solutions
{
    public class Day9
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            RunProgram(data, 1);
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            RunProgram(data, 2);
        }

        private static void RunProgram(string[] data, int input)
        {
            foreach (var line in data)
            {
                IntCodeComputer computer = new IntCodeComputer(line.Split(',').Select(long.Parse));
                var result = computer.RunProgram(input);
                Console.Write("Output: ");
                while (result.TryDequeue(out var v))
                {
                    Console.Write(v);
                    if (result.Count > 0)
                        Console.Write(",");
                }

                Console.WriteLine();
            }
        }
    }
}