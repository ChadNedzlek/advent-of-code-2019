using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class SkipDay25
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));

            Channel<long> input = Channel.CreateBounded<long>(1);
            Channel<long> output = Channel.CreateBounded<long>(1);
            var running = computer.RunProgramAsync(input.Reader, output.Writer);
            Task<long> botOutput;
            Task<string> user;

            void PullBot() => botOutput = output.Reader.ReadAsync().AsTask();
            void PullUser() => user = Task.Run(() => Console.In.ReadLineAsync());

            PullBot();
            PullUser();

            while (true)
            {
                var completed = await Task.WhenAny(running, botOutput, user);
                if (completed == running)
                {
                    Console.WriteLine();
                    Console.WriteLine("PROGRAM COMPLETE!");
                    Console.WriteLine();
                    break;
                }

                if (completed == botOutput)
                {
                    var o = await botOutput;
                    Console.Write((char) o);
                    PullBot();
                    continue;
                }

                string read = await user;
                foreach (var c in read)
                {
                    await input.Writer.WriteAsync(c);
                }

                await input.Writer.WriteAsync('\n');
                PullUser();
            }
        }
    }
}