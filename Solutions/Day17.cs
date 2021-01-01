using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day17
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            var scaffold = computer.RunProgram();
            List<List<char>> display = new List<List<char>>();
            List<char> currentLine = new List<char>();
            while (scaffold.TryDequeue(out long cell))
            {
                D.Write((char) cell);
                if (cell == '\n')
                {
                    if (currentLine.Count > 0)
                        display.Add(currentLine);
                    currentLine = new List<char>();
                }
                else
                {
                    currentLine.Add((char) cell);
                }
            }

            char Get(int x, int y)
            {
                if (x < 0 || y < 0 || y >= display.Count)
                    return '\0';

                var line = display[y];
                if (x >= line.Count)
                    return '\0';

                return display[y][x];
            }

            int ScaffoldOne(int x, int y)
            {
                return Get(x, y) == '#' ? 1 : 0;
            }

            long alignment = 0;
            for (int y = 0; y < display.Count; y++)
            {
                for (int x = 0; x < display[y].Count; x++)
                {
                    int adj = ScaffoldOne(x - 1, y) +
                        ScaffoldOne(x + 1, y) +
                        ScaffoldOne(x, y - 1) +
                        ScaffoldOne(x, y + 1) +
                        ScaffoldOne(x, y);

                    if (adj == 5)
                    {
                        D.WriteLine($"Found intersection at [{y}][{x}] = {y * x}");
                        alignment += y * x;
                    }
                }
            }

            D.WriteLine();
            Console.WriteLine($"Sum of alignment parameters is {alignment}");
        }

        public static async Task XProblem2()
        {
            var data = await Data.GetDataLines();
            var program = data[0].Split(',').Select(long.Parse).ToArray();
            program[0] = 2;
            IntCodeComputer computer = new IntCodeComputer(program);
            
            var running = computer.RunProgramAsync(out var input, out var output);
            Task<long> botOutput;
            Task<string> user;

            void PullBot() => botOutput = output.ReadAsync().AsTask();
            void PullUser() => user = Task.Run(() => Console.In.ReadLineAsync());

            PullBot();
            PullUser();

            long last = 0;

            while (true)
            {
                var completed = await Task.WhenAny(running, botOutput, user);
                if (completed == running)
                {
                    while (output.TryRead(out long o))
                    {
                        last = o;
                        Console.Write((char) o);
                    }
                    Console.WriteLine();
                    Console.WriteLine("PROGRAM COMPLETE!");
                    Console.WriteLine();
                    break;
                }

                if (completed == botOutput)
                {
                    var o = await botOutput;
                    last = o;
                    Console.Write((char) o);
                    PullBot();
                    continue;
                }

                string read = await user;
                foreach (var c in read)
                {
                    await input.WriteAsync(c);
                }

                await input.WriteAsync('\n');
                PullUser();
            }

            Console.WriteLine($"Accumulated value: {last}");
        }
    }
}