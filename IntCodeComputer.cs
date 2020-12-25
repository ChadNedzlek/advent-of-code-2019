using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Advent2019
{
    public class IntCodeComputer
    {
        private string _debugPrefix;
        public ImmutableArray<int> Source { get; }
        public bool Debug { get; set; }

        public IntCodeComputer(IEnumerable<int> source)
            : this(source.ToImmutableArray())
        {
        }

        public IntCodeComputer(ImmutableArray<int> source)
        {
            Source = source;
        }

        public IntCodeComputer(ImmutableArray<int> source, string debugPrefix)
        {
            _debugPrefix = debugPrefix;
            Source = source;
            Debug = true;
        }

        public IntCodeComputer CreateDebugger(string prefix)
        {
            return new IntCodeComputer(Source, prefix);
        }

        public Queue<int> RunProgram(params int[] input)
        {
            Queue<int> q = new Queue<int>();
            foreach (int i in input)
            {
                q.Enqueue(i);
            }
            return RunProgram(q);
        }

        public Queue<int> RunProgram(Queue<int> input)
        {
            return RunProgram(input, out _);
        }

        public async Task<int[]> RunProgramAsync(ChannelReader<int> input, ChannelWriter<int> output)
        {
            var mem = new int[Source.Length];
            Source.CopyTo(mem);
            int ip = 0;

            while (true)
            {
                int ins = mem[ip];
                int opCode = ins % 100;
                int[] modes = { (ins / 100 % 10), (ins / 1000 % 10), (ins / 10000 % 10) };

                ref int Access(int param)
                {
                    switch (modes[param - 1])
                    {
                        case 0:
                            {
                                int value = mem[ip + param];
                                ref int ret = ref mem[value];
                                DebugOutput($"(mem[{ip + param}] = {value}] = {ret}) ");
                                return ref ret;
                            }
                        case 1:
                            {
                                ref int ret = ref mem[ip + param];
                                DebugOutput($"(mem[{ip + param}] = {ret}) ");
                                return ref mem[ip + param];
                            }
                        default:
                            throw new NotSupportedException($"Unsupported parameter mode: {modes[param]}");
                    }
                }

                DebugOutput($"\n{_debugPrefix} op[mem[{ip}] = {opCode} in mode {string.Join(",", modes)}\n{_debugPrefix}  ");

                switch (opCode)
                {
                    case 1:
                        {
                            int a = Access(1);
                            DebugOutput("+ ");
                            int b = Access(2);
                            int res = a + b;
                            DebugOutput($"== {res} => ");
                            Access(3) = res;
                            ip += 4;
                            break;
                        }
                    case 2:
                        {
                            int a = Access(1);
                            DebugOutput("* ");
                            int b = Access(2);
                            DebugOutput("=> ");
                            int res = a * b;
                            DebugOutput($"== {res} => ");
                            Access(3) = res;
                            ip += 4;
                            break;
                        }
                    case 3:
                        {
                            if (input.TryRead(out var i))
                            {
                                DebugOutput($"input == {i} => ");

                            }
                            else
                            {
                                DebugOutput($"input pending...\r");
                                i = await input.ReadAsync();
                                DebugOutput($"{_debugPrefix}    ... input == {i} => ");
                            }

                            Access(1) = i;
                            ip += 2;
                            break;
                        }
                    case 4:
                        {
                            int o = Access(1);
                            DebugOutput($" == {o} => output");
                            if (output.TryWrite(o))
                            {
                            }
                            else
                            {
                                DebugOutput(" pending...");
                                await output.WriteAsync(o);
                                DebugOutput($"{_debugPrefix}    ... output");
                            }

                            ip += 2;
                            break;
                        }
                    case 5:
                        {
                            DebugOutput("if ");
                            int p = Access(1);
                            DebugOutput(" jmp ");
                            int a = Access(2);
                            if (p == 1)
                            {
                                ip = a;
                                DebugOutput(" jumped");
                            }
                            else
                            {
                                ip += 3;
                                DebugOutput(" skipped");
                            }

                            break;
                        }
                    case 6:
                        {
                            DebugOutput("if not ");
                            int p = Access(1);
                            DebugOutput(" jmp ");
                            int a = Access(2);
                            if (p == 0)
                            {
                                ip = a;
                                DebugOutput(" jumped");
                            }
                            else
                            {
                                ip += 3;
                                DebugOutput(" skipped");
                            }

                            break;
                        }
                    case 7:
                        {
                            int a = Access(1);
                            DebugOutput("< ");
                            int b = Access(2);
                            int res = a < b ? 1 : 0;
                            DebugOutput($" == {res} => ");
                            Access(3) = res;
                            ip += 4;
                            break;
                        }
                    case 8:
                        {
                            int a = Access(1);
                            DebugOutput("== ");
                            int b = Access(2);
                            int res = a == b ? 1 : 0;
                            DebugOutput($" == {res} => ");
                            Access(3) = res;
                            ip += 4;
                            break;
                        }
                    case 99:
                        DebugOutput("HALT\n");
                        output.Complete();
                        return mem;
                    default:
                        throw new NotSupportedException($"Unexpected op code: {ins}");
                }
            }
        }

        public Queue<int> RunProgram(Queue<int> input, out int[] memory)
        {
            Channel<int> i = Channel.CreateBounded<int>(input.Count);
            while (input.TryDequeue(out int v))
                i.Writer.WriteAsync(v).GetAwaiter().GetResult();
            Channel<int> o = Channel.CreateUnbounded<int>();
            memory = RunProgramAsync(i.Reader, o.Writer).GetAwaiter().GetResult();
            Queue<int> result = new Queue<int>();
            while (o.Reader.TryRead(out int v))
                result.Enqueue(v);
            return result;
        }

        private void DebugOutput(FormattableString msg)
        {
            if (!Debug)
                return;

            Console.Write(msg);
        }

        private void DebugOutput(string msg)
        {
            if (!Debug)
                return;

            Console.Write(msg);
        }
    }
}