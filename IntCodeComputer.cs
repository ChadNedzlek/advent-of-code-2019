using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AdventOfCode
{
    public class IntCodeComputer
    {
        private readonly string _debugPrefix;

        public IntCodeComputer(IEnumerable<long> source)
            : this(source.ToImmutableArray())
        {
        }

        public IntCodeComputer(ImmutableArray<long> source)
        {
            Source = source;
        }

        public IntCodeComputer(ImmutableArray<long> source, string debugPrefix)
        {
            _debugPrefix = debugPrefix;
            Source = source;
            Debug = true;
        }

        public ImmutableArray<long> Source { get; }
        public bool Debug { get; set; }

        public IntCodeComputer CreateDebugger(string prefix = "")
        {
            return new IntCodeComputer(Source, prefix);
        }

        public Queue<long> RunProgram(params long[] input)
        {
            var q = new Queue<long>();
            foreach (long i in input)
            {
                q.Enqueue(i);
            }

            return RunProgram(q);
        }

        public Queue<long> RunProgram(Queue<long> input)
        {
            return RunProgram(input, out _);
        }

        public Queue<long> RunProgram(out long[] memory)
        {
            return RunProgram(new Queue<long>(), out memory);
        }

        public Task<long[]> RunProgramAsync(out ChannelWriter<long> input, out ChannelReader<long> output)
        {
            var i = Channel.CreateBounded<long>(1);
            var o = Channel.CreateBounded<long>(2);
            var t = RunProgramAsync(i.Reader, o.Writer);
            input = i.Writer;
            output = o.Reader;
            return t;
        }

        public async Task<long[]> RunProgramAsync(ChannelReader<long> input, ChannelWriter<long> output)
        {
            var mem = new long[Source.Length];

            // Store "high memory", memory that is outside the original program, which is just a sparse map
            Dictionary<long, int> highMemMap = new Dictionary<long, int>();
            long[] highMem = new long[0];

            Source.CopyTo(mem);
            int ip = 0;
            int relativeBase = 0;

            while (true)
            {
                long ins = mem[ip];
                long opCode = ins % 100;
                int[] modes = {(int) (ins / 100 % 10), (int) (ins / 1000 % 10), (int) (ins / 10000 % 10)};

                ref long Mem(long address)
                {
                    if (address < 0)
                        throw new AccessViolationException($"Memory at {address} invalid");

                    if (address < mem.Length)
                        return ref mem[address];

                    if (highMemMap.Count + 1 >= highMem.Length)
                    {
                        // We need more "high memory", 10 or double it, whichever is more
                        Array.Resize(ref highMem, Math.Max(10, highMem.Length * 2));
                    }

                    if (!highMemMap.TryGetValue(address, out var mappedAddress))
                    {
                        highMemMap.Add(address, mappedAddress = highMemMap.Count);
                    }

                    return ref highMem[mappedAddress];
                }

                ref long ParameterValue(int param)
                {
                    ref long paramValue = ref Mem(ip + param);
                    switch (modes[param - 1])
                    {
                        case 0:
                        {
                            DebugOutput($"(mem[mem[{ip + param}] = {paramValue}] = ");
                            ref long ret = ref Mem(paramValue);
                            DebugOutput($"{ret}) ");
                            return ref ret;
                        }
                        case 1:
                        {
                            DebugOutput($"(mem[{ip + param}] = {paramValue})");
                            return ref paramValue;
                        }
                        case 2:
                        {
                            DebugOutput($"(mem[rel {relativeBase} + {paramValue}] = ");
                            ref long ret = ref Mem(relativeBase + paramValue);
                            DebugOutput($"{ret}) ");
                            return ref ret;
                        }
                        default:
                            throw new NotSupportedException($"Unsupported parameter mode: {modes[param]}");
                    }
                }

                DebugOutput(
                    $"\n{_debugPrefix} op mem[{ip}] = {opCode} in mode {string.Join(",", modes)}\n{_debugPrefix}  "
                );

                switch (opCode)
                {
                    case 1:
                    {
                        long a = ParameterValue(1);
                        DebugOutput($"+ ");
                        long b = ParameterValue(2);
                        long res = a + b;
                        DebugOutput($"== {res} => ");
                        ParameterValue(3) = res;
                        ip += 4;
                        break;
                    }
                    case 2:
                    {
                        long a = ParameterValue(1);
                        DebugOutput($"* ");
                        long b = ParameterValue(2);
                        DebugOutput($"=> ");
                        long res = a * b;
                        DebugOutput($"== {res} => ");
                        ParameterValue(3) = res;
                        ip += 4;
                        break;
                    }
                    case 3:
                    {
                        if (input.TryRead(out long i))
                        {
                            DebugOutput($"input == {i} => ");
                        }
                        else
                        {
                            DebugOutput($"input pending...\r");
                            i = await input.ReadAsync();
                            DebugOutput($"{_debugPrefix}    ... input == {i} => ");
                        }

                        ParameterValue(1) = i;
                        ip += 2;
                        break;
                    }
                    case 4:
                    {
                        long o = ParameterValue(1);
                        DebugOutput($" == {o} => output");
                        if (output.TryWrite(o))
                        {
                        }
                        else
                        {
                            DebugOutput($" pending...");
                            await output.WriteAsync(o);
                            DebugOutput($"{_debugPrefix}    ... output");
                        }

                        ip += 2;
                        break;
                    }
                    case 5:
                    {
                        DebugOutput($"if ");
                        long p = ParameterValue(1);
                        DebugOutput($" jmp ");
                        long a = ParameterValue(2);
                        if (p != 0)
                        {
                            ip = (int) a;
                            DebugOutput($" ==> jumped");
                        }
                        else
                        {
                            ip += 3;
                            DebugOutput($" ==> skipped");
                        }

                        break;
                    }
                    case 6:
                    {
                        DebugOutput($"if not ");
                        long p = ParameterValue(1);
                        DebugOutput($" jmp ");
                        long a = ParameterValue(2);
                        if (p == 0)
                        {
                            ip = (int) a;
                            DebugOutput($" ==> jumped");
                        }
                        else
                        {
                            ip += 3;
                            DebugOutput($" ==> skipped");
                        }

                        break;
                    }
                    case 7:
                    {
                        long a = ParameterValue(1);
                        DebugOutput($"< ");
                        long b = ParameterValue(2);
                        int res = a < b ? 1 : 0;
                        DebugOutput($" == {res} => ");
                        ParameterValue(3) = res;
                        ip += 4;
                        break;
                    }
                    case 8:
                    {
                        long a = ParameterValue(1);
                        DebugOutput($"== ");
                        long b = ParameterValue(2);
                        int res = a == b ? 1 : 0;
                        DebugOutput($" == {res} => ");
                        ParameterValue(3) = res;
                        ip += 4;
                        break;
                    }
                    case 9:
                    {
                        long a = ParameterValue(1);
                        relativeBase += (int)a;
                        DebugOutput($" ==> rel base {relativeBase}");
                        ip += 2;
                        break;
                    }
                    case 99:
                        DebugOutput($"HALT\n");
                        output.Complete();
                        return mem;
                    default:
                        throw new NotSupportedException($"Unexpected op code: {ins}");
                }
            }
        }

        public Queue<long> RunProgram(Queue<long> input, out long[] memory)
        {
            Channel<long> i = Channel.CreateBounded<long>(Math.Max(input.Count, 1));
            while (input.TryDequeue(out long v))
            {
                i.Writer.WriteAsync(v).GetAwaiter().GetResult();
            }
            i.Writer.Complete();

            var o = Channel.CreateUnbounded<long>();
            memory = RunProgramAsync(i.Reader, o.Writer).GetAwaiter().GetResult();
            var result = new Queue<long>();
            while (o.Reader.TryRead(out long v))
            {
                result.Enqueue(v);
            }

            return result;
        }

        private void DebugOutput(FormattableString msg)
        {
            if (!Debug)
            {
                return;
            }

            Console.Write(msg);
        }
    }
}