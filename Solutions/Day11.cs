using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day11
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            Dictionary<(int x, int y), int> colors = new Dictionary<(int x, int y), int>();
            int x = 0, y = 0;
            int dx = 0, dy = -1;
            Channel<long> input = Channel.CreateBounded<long>(1);
            Channel<long> output = Channel.CreateBounded<long>(2);
            Task executing = computer.RunProgramAsync(input.Reader, output.Writer);
            while (true)
            {
                await input.Writer.WriteAsync(colors.GetValueOrDefault((x, y), 0));
                Task<long> paintTask = output.Reader.ReadAsync().AsTask();
                var stepped = await Task.WhenAny(executing, paintTask);
                if (stepped == executing)
                {
                    // Program halted
                    break;
                }

                var color = await paintTask;
                var dir = await output.Reader.ReadAsync();
                colors[(x, y)] = (int) color;
                if (dir == 0)
                {
                    var o = dx;
                    dx = dy;
                    dy = -o;
                }
                else
                {
                    var o = dx;
                    dx = -dy;
                    dy = o;
                }

                x += dx;
                y += dy;
            }
            Console.WriteLine($"Painted {colors.Count} tiles");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            Dictionary<(int x, int y), int> colors = new Dictionary<(int x, int y), int>();
            {
                colors[(0, 0)] = 1;
                int x = 0, y = 0;
                int dx = 0, dy = -1;
                Channel<long> input = Channel.CreateBounded<long>(1);
                Channel<long> output = Channel.CreateBounded<long>(2);
                Task executing = computer.RunProgramAsync(input.Reader, output.Writer);
                while (true)
                {
                    await input.Writer.WriteAsync(colors.GetValueOrDefault((x, y), 0));
                    Task<long> paintTask = output.Reader.ReadAsync().AsTask();
                    var stepped = await Task.WhenAny(executing, paintTask);
                    if (stepped == executing)
                    {
                        // Program halted
                        break;
                    }

                    var color = await paintTask;
                    var dir = await output.Reader.ReadAsync();
                    colors[(x, y)] = (int) color;
                    if (dir == 0)
                    {
                        var o = dx;
                        dx = dy;
                        dy = -o;
                    }
                    else
                    {
                        var o = dx;
                        dx = -dy;
                        dy = o;
                    }

                    x += dx;
                    y += dy;
                }
            }

            int lx = colors.Keys.Min(k => k.x);
            int hx = colors.Keys.Max(k => k.x);
            int ly = colors.Keys.Min(k => k.y);
            int hy = colors.Keys.Max(k => k.y);

            for (int y = ly; y <= hy; y++)
            {
                for (int x = lx; x <= hx; x++)
                {
                    if (colors.GetValueOrDefault((x, y), 0) == 1)
                    {
                        Console.Write('\u2588');
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.WriteLine();
            }
        }
    }
}