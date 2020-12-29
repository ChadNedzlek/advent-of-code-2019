using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class SkipDay5
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            var result = computer.RunProgram(1);
            foreach (long output in result)
            {
                Console.WriteLine("Day 5 diagnostic output : " + output);
            }
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            var result = computer.RunProgram(5);
            foreach (long output in result)
            {
                Console.WriteLine("Day 5 output : " + output);
            }
        }
    }
}