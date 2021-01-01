using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day22
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            int size = 10_007;
            LinkedList<IOp> ops = new LinkedList<IOp>();
            foreach (var line in data)
            {
                if (line == "deal into new stack")
                {
                    ops.AddLast(new Flip(size));
                    continue;
                }

                if (Rx.IsM(line, @"^cut (-?\d+)$", out int cut))
                {
                    ops.AddLast(new Cut(size, cut));
                    continue;
                }

                if (Rx.IsM(line, @"^deal with increment (-?\d+)$", out int deal))
                {
                    ops.AddLast(new Deal(size, deal));
                    continue;
                }
            }

            ReduceOperations(ops);

            if (size < 30)
            {
                long[] cards = new long[size];
                long[] invCards = new long[size];
                for (int i = 0; i < size; i++)
                {
                    cards[Find(i, ops)] = i;
                    invCards[i] = At(i, ops);
                }

                Console.WriteLine($"Located cards: {string.Join(' ', cards)}");
                Console.WriteLine($"Inverse cards: {string.Join(' ', invCards)}");
            }

            long find = Find(2019, ops);
            Console.WriteLine($"Find index of card 2019: {find}");
            Console.WriteLine($"Which is at index: {At(find, ops)}");
        }

        private static long At(long index, LinkedList<IOp> ops)
        {
            for (var n = ops.Last; n != null; n = n.Previous)
            {
                index = n.Value.Invert(index);
            }

            return index;
        }

        private static long Find(long value, LinkedList<IOp> ops)
        {
            return ops.Aggregate(value, (agg, o) => o.Transform(agg));
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            long size = 119315717514047;
            long iterations = 101741582076661;
            LinkedList<IOp> ops = new LinkedList<IOp>();
            foreach (var line in data)
            {
                if (line == "deal into new stack")
                {
                    ops.AddLast(new Flip(size));
                    continue;
                }

                if (Rx.IsM(line, @"^cut (-?\d+)$", out int cut))
                {
                    ops.AddLast(new Cut(size, cut));
                    continue;
                }

                if (Rx.IsM(line, @"^deal with increment (-?\d+)$", out int deal))
                {
                    ops.AddLast(new Deal(size, deal));
                    continue;
                }
            }

            Dictionary<long, IEnumerable<IOp>> partial = new Dictionary<long, IEnumerable<IOp>>();
            ReduceOperations(ops);
            partial.Add(1, ops);
            for (long i = 2; i < iterations; i *= 2)
            {
                // Glue them together
                ops = new LinkedList<IOp>(ops.Concat(ops));
                // Reduce it again to keep it tight
                ReduceOperations(ops);
                partial.Add(i, ops);
            }

            LinkedList<IOp> finalOp = new LinkedList<IOp>();
            finalOp.AddLast(new Noop(size));
            for (int i = 0; i < 64; i++)
            {
                long partialChunk = 1L << i;
                if ((iterations & partialChunk) != 0)
                {
                    finalOp = new LinkedList<IOp>(finalOp.Concat(partial[partialChunk]));
                    ReduceOperations(finalOp);
                }
            }

            Console.WriteLine($"At index 2020: {At(2020, finalOp)}");
        }

        private static void ReduceOperations(LinkedList<IOp> ops)
        {
            bool reduced;
            do
            {
                D.WriteLine();
                D.WriteLine("Current pattern: ");
                DumpOpSet(ops);
                reduced = false;

                // Try to merge any adjacent nodes
                for (var n = ops.First; n.Next != null; n = n.Next)
                {
                    if (n.Value.IsNothing)
                    {
                        ops.Remove(n);
                        reduced = true;
                        break;
                    }

                    IOp mergeWithNext = n.Value.Merge(n.Next.Value);
                    if (mergeWithNext != null)
                    {
                        ops.AddBefore(n, mergeWithNext);
                        ops.Remove(n.Next);
                        ops.Remove(n);
                        reduced = true;
                        break;
                    }
                }
                
                if (!reduced)
                {
                    // If we didn't merge any, let's see if some reordering might let us
                    for (var n = ops.First; n.Next?.Next != null; n = n.Next)
                    {
                        // If we have A -> B -> C and we can merge A and C...
                        var aNode = n;
                        var bNode = n.Next;
                        var cNode = n.Next.Next;
                        IOp aAndC = aNode.Value.Merge(cNode.Value);
                        if (aAndC != null)
                        {
                            {
                                // Lets try doing it B -> A+C
                                (IOp b, IOp a, IOp d) = aNode.Value.Swap(bNode.Value);
                                // If a/b returned that's good, but we can't add a D, or it messes everything up
                                if (a != null && d == null)
                                {
                                    ops.AddBefore(aNode, b);
                                    ops.AddBefore(aNode, a.Merge(cNode.Value));
                                    ops.Remove(cNode);
                                    ops.Remove(bNode);
                                    ops.Remove(aNode);
                                    reduced = true;
                                    break;
                                }
                            }
                            {
                                // Lets try doing it A -> C -> B (-> D) 
                                // ... A+C -> B (-> D)
                                (IOp c, IOp b, IOp d) = bNode.Value.Swap(cNode.Value);
                                if (c != null)
                                {
                                    IOp ac = aNode.Value.Merge(c);
                                    ops.AddBefore(aNode, ac);
                                    ops.AddBefore(aNode, b);
                                    if (d != null)
                                        ops.AddBefore(aNode, d);
                                    ops.Remove(cNode);
                                    ops.Remove(bNode);
                                    ops.Remove(aNode);
                                    reduced = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (!reduced)
                {
                    // Ok, we are in scary land, we might have A -> B -> C -> D
                    // where A and D could be merged
                    for (var n = ops.First; n.Next?.Next?.Next != null; n = n.Next)
                    {
                        var aNode = n;
                        var bNode = aNode.Next;
                        var cNode = bNode.Next;
                        var dNode = cNode.Next;
                        IOp aAndD = aNode.Value.Merge(dNode.Value);
                        if (aAndD != null)
                        {
                            // We need to flip A/B and then A/C, with no new nodes appearing
                            (IOp b, IOp aPrime, IOp abForbidden) = aNode.Value.Swap(bNode.Value);
                            if (b == null || abForbidden != null)
                            {
                                // Either we don't know how to swap A and B, or it turned into
                                // B -> A -> X -> C -> D
                                // And we can't continue the move
                                continue;
                            }

                            // Now we have B -> A` -> C -> D
                            (IOp c, IOp a, IOp acForbidden) = aPrime.Swap(cNode.Value);
                            if (c == null || acForbidden != null)
                            {
                                // Either we couldn't swap A` and C, or it turned into
                                // B -> C -> A` -> X -> D
                                // And we can't continue the move (since the point was to merge A and D, which are no longer adjacent
                                continue;
                            }

                            // Alrighty, we can do B -> C -> A+D
                            ops.AddBefore(aNode, b);
                            ops.AddBefore(aNode, c);
                            ops.AddBefore(aNode, a.Merge(dNode.Value));
                            ops.Remove(dNode);
                            ops.Remove(cNode);
                            ops.Remove(bNode);
                            ops.Remove(aNode);
                            reduced = true;
                            break;
                        }
                    }
                }
            } while (reduced);
        }

        [Conditional("DEBUG")]
        public static void DumpOpSet(IEnumerable<IOp> ops)
        {
            foreach (var op in ops)
            {
                D.WriteLine(op.ToString());
            }
        }

        public interface IOp
        {
            long Transform(long input);
            long Invert(long input);
            bool IsNothing { get; }
            IOp Merge(IOp other);
            (IOp a, IOp b, IOp add) Swap(IOp after);
        }

        public abstract class BasicOp : IOp
        {
            public readonly long Size;

            protected BasicOp(long size)
            {
                Size = size;
            }

            public abstract long Invert(long input);
            public abstract bool IsNothing { get; }
            public abstract long Transform(long input);
            public abstract IOp Merge(IOp other);
            public abstract (IOp a, IOp b, IOp add) Swap(IOp after);
        }

        public class Noop : BasicOp
        {
            public override long Transform(long input)
            {
                return input;
            }
            
            public override long Invert(long input) => Transform(input);

            public override bool IsNothing => true;

            public override IOp Merge(IOp other)
            {
                return other;
            }

            public override (IOp a, IOp b, IOp add) Swap(IOp after)
            {
                return (after, this, null);
            }

            public Noop(long size)
                : base(size)
            {
            }
            public override string ToString() => "nothing";
        }

        public class Flip : BasicOp
        {
            public Flip(long size) : base(size)
            {
            }

            public override long Transform(long input)
            {
                return Size - input - 1;
            }

            public override long Invert(long input) => Transform(input);

            public override bool IsNothing => false;

            public override IOp Merge(IOp other)
            {
                if (other is Flip)
                    return new Noop(Size);

                return null;
            }

            public override (IOp a, IOp b, IOp add) Swap(IOp after)
            {
                if (after is Cut cut)
                {
                    return (new Cut(Size, -cut.Amount), this, null);
                }

                if (after is Deal deal)
                {
                    return (new Deal(Size, deal.Increment), this, new Cut(Size, deal.Increment - 1));
                }

                return (null, null, null);
            }
            public override string ToString() => "deal into new stack";
        }

        public class Deal : BasicOp
        {
            public readonly long Increment;

            public Deal(long size, long increment)
                : base(size)
            {
                if (increment == 0)
                    throw new ArgumentOutOfRangeException();

                Increment = increment;
            }

            public override long Transform(long input)
            {
                return FancyMath.MulMod(Increment, input, Size);
            }
            private long _modInv = 0;
            public override long Invert(long input)
            {
                if (_modInv == 0)
                {
                    _modInv = FancyMath.ModInverse(Increment, Size);
                }
                
                return FancyMath.MulMod(_modInv, input, Size);
            }

            public override IOp Merge(IOp other)
            {
                if (other is Deal d)
                {
                    return new Deal(Size, FancyMath.MulMod(Increment, d.Increment, Size));
                }

                return null;
            }

            public override (IOp a, IOp b, IOp add) Swap(IOp after)
            {
                if (after is Flip flip)
                {
                    return (flip, this, new Cut(Size, Size - Increment + 1));
                }

                return (null, null, null);
            }
            public override bool IsNothing => Increment == 1;

            public override string ToString() => $"deal with increment {Increment}";
        }

        public class Cut : BasicOp
        {
            public readonly long Amount;

            public Cut(long size, long amount) : base(size)
            {
                Amount = amount;
            }

            public override long Transform(long input)
            {
                return (input + Size - Amount) % Size;
            }

            public override long Invert(long input)
            {
                return (input + Amount + Size) % Size;
            }

            public override IOp Merge(IOp other)
            {
                if (other is Cut cut)
                    return new Cut(Size, (Amount + cut.Amount) % Size);

                return null;
            }

            public override (IOp a, IOp b, IOp add) Swap(IOp after)
            {
                if (after is Deal d)
                {
                    return (d, new Cut(Size, FancyMath.MulMod(d.Increment, Amount, Size)), null);
                }

                if (after is Flip f)
                {
                    return (f, new Cut(Size, -Amount), null);
                }

                return (null, null, null);
            }
            public override bool IsNothing => Amount == 0;

            public override string ToString() => $"cut {Amount}";
        }
    }
}