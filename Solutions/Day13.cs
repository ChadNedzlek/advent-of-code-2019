using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day13
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            var output = computer.RunProgram();
            int blocks = 0;
            while (output.Count != 0)
            {
                var x = output.Dequeue();
                var y = output.Dequeue();
                var id = output.Dequeue();
                if (id == 2)
                    blocks++;
            }

            Console.WriteLine($"{blocks} blocks drawn");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            var program = data[0].Split(',').Select(long.Parse).ToArray();
            program[0] = 2;
            IntCodeComputer computer = new IntCodeComputer(program);
            
            Channel<long> i = Channel.CreateBounded<long>(1);
            Channel<long> o = Channel.CreateBounded<long>(3);
            var r = computer.RunProgramAsync(i.Reader, o.Writer);
            long ball = 0;
            long paddle = 0;
            long score = 0;
            while (true)
            {
                var t = await Task.WhenAny(r, o.Reader.ReadAsync().AsTask());
                if (t == r)
                    break;
                var x = await (Task<long>) t;
                var y = await o.Reader.ReadAsync();
                var id = await o.Reader.ReadAsync();
                if (x == -1)
                {
                    score = id;
                    {
                        D.SetCursorPosition(0, 0);
                        D.Write($"Score: {id}");
                    }
                }
                else
                {
                    {
                        D.SetCursorPosition((int) x, (int) y + 2);
                        switch (id)
                        {
                            case 0:
                                D.Write(' ');
                                break;
                            case 1:
                                D.Write('\u2588');
                                break;
                            case 2:
                                D.Write('#');
                                break;
                            case 3:
                                D.Write('=');
                                paddle = x;
                                break;
                            case 4:
                                D.Write('O');
                                ball = x;
                                i.Writer.TryWrite(Math.Sign(ball - paddle));
                                break;
                        }
                    }
                }
            }

            Console.WriteLine($"Final score: {score}");
        }
    }
}