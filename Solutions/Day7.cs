using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day7
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            long max = 0;
            int[] maxPermutation = null;
            foreach (var p in Permutations(new[] {0, 1, 2, 3, 4}))
            {
                long signal = 0;
                for (int iEngine = 0; iEngine < p.Length; iEngine++)
                {
                    int phaseSetting = p[iEngine];
                    signal = computer.RunProgram(phaseSetting, signal).Dequeue();
                }

                if (maxPermutation == null || signal > max)
                {
                    max = signal;
                    maxPermutation = p;
                }
            }

            Console.WriteLine($"Best sequence {string.Join(", ", maxPermutation)} is {max}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            bool debug = false;
            long max = 0;
            int[] maxPermutation = null;
            foreach (var p in Permutations(new[] {5, 6, 7, 8, 9}))
            {
                var ioPipelines = Enumerable.Repeat(0, 5).Select(_ => Channel.CreateBounded<long>(1)).ToArray();
                Task[] engines = p.Select(
                        async (phase, index) =>
                        {
                            // Engine 1 pulls input from pipeline 1
                            Channel<long> engineChannel = ioPipelines[index];
                            // Write the initial phase to the input pipeline
                            await engineChannel.Writer.WriteAsync(phase);
                            // Engine 1 pushes its output into engine 2 (and loops around)
                            int iNext = (index + 1) % p.Length;
                            Channel<long> nextEngineChannel = ioPipelines[iNext];

                            if (debug)
                                computer = computer.CreateDebugger(((char)(index + 'A')).ToString());

                            // Start up the engine!
                            await computer.RunProgramAsync(engineChannel.Reader, nextEngineChannel.Writer);
                        }
                    )
                    .ToArray();

                // This is the initial "zero" seed for the engine A
                await ioPipelines[0].Writer.WriteAsync(0);

                // Wait for all engines to halt
                await Task.WhenAll(engines);

                // When all the engines have halted, we need to find the output from the last engine
                // (which is the first engines input)
                long signal = await ioPipelines[0].Reader.ReadAsync();

                if (maxPermutation == null || signal > max)
                {
                    max = signal;
                    maxPermutation = p;
                }
            }

            Console.WriteLine($"Best sequence {string.Join(", ", maxPermutation)} is {max}");
        }

        public static IEnumerable<int[]> Permutations(int[] input, int[] seed = null, int index = 0)
        {
            if (index >= input.Length)
            {
                yield return (int[]) seed.Clone();
                yield break;
            }

            seed ??= new int[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                var ind = Array.IndexOf(seed, input[i], 0, index);
                if (ind != -1)
                {
                    continue;
                }

                seed[index] = input[i];
                foreach (var permutation in Permutations(input, seed, index+1 ))
                {
                    yield return permutation;
                }
            }
        }
    }
}