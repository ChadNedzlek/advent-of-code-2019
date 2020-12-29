using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day2
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            var program = data[0].Split(',').Select(long.Parse).ToArray();
            var result = RunWithInputs(program, 12, 2);
            Console.WriteLine($"Modified position 0: {result}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            var program = data[0].Split(',').Select(long.Parse).ToArray();
            for (int noun = 0; noun < 100; noun++)
            {
                for (int verb = 0; verb < 100; verb++)
                {
                    if (RunWithInputs(program, noun, verb) == 19690720)
                    {
                        Console.WriteLine($"matching pair 100 * {noun} + {verb} = {100 * noun + verb}");
                        return;
                    }
                }
            }
        }

        private static long RunWithInputs(long[] program, int noun, int verb)
        {
            program[1] = noun;
            program[2] = verb;
            IntCodeComputer computer = new IntCodeComputer(program);
            computer.RunProgram(out long[] memory);
            return memory[0];
        }
    }
}