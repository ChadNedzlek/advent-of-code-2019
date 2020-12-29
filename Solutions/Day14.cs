using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day14
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            Dictionary<string, Reaction> allReactions = ParseReactions(data);

            Dictionary<string, long> need = new Dictionary<string, long>{{"FUEL", 1}};
            Dictionary<string, long> excess = new Dictionary<string, long>();
            do
            {
                foreach (var (neededElement, targetCount) in need)
                {
                    if (!allReactions.TryGetValue(neededElement, out var r))
                    {
                        continue;
                    }

                    D.WriteLine($"Need {targetCount} of {neededElement}...");

                    long have = excess.GetValueOrDefault(neededElement);
                    if (have > targetCount)
                    {
                        D.WriteLine($"... had {have} in storage, consumed {targetCount}, leaving {have - targetCount}");
                        excess[neededElement] = have - targetCount;
                    }
                    else
                    {
                        excess.Remove(neededElement);
                        long more = targetCount - have;
                        if (have > 0)
                        {
                            D.WriteLine($"... already have {have} in excess, need {more} more");
                        }
                        long reactionCount = (long) Math.Ceiling(more / (double) r.Amount);
                        long produced = reactionCount * r.Amount;
                        D.WriteLine($"... to run {reactionCount} reactions to produce {produced}");
                        foreach (var (inputElement, inputCount) in r.Inputs)
                        {
                            long inputNeed = inputCount * reactionCount;
                            D.WriteLine($"... need {inputNeed} {inputElement}");
                            if (excess.TryGetValue(inputElement, out var ex) && ex > 0)
                            {
                                if (ex > inputNeed)
                                {
                                    excess[inputElement] -= inputNeed;
                                    D.WriteLine($"  ... pulled {inputNeed} from storage, leaving {excess[inputElement]}");
                                    continue;
                                }

                                inputNeed -= ex;
                                excess.Remove(inputElement);
                                D.WriteLine($"  ... pulled {ex} from storage, require {inputNeed} more");
                            }
                            
                            long currentNeed = need.GetValueOrDefault(inputElement);
                            need[inputElement] = currentNeed + inputNeed;
                            D.WriteLine($"  ... with existing need of {currentNeed}, for a total of {need[inputElement]}");
                        }

                        var surplus = produced - targetCount;
                        if (surplus > 0)
                        {
                            excess[neededElement] = excess.GetValueOrDefault(neededElement) + surplus;
                            D.WriteLine($"... resulting in {surplus} additional {neededElement} (currently {excess[neededElement]} excess)");
                        }
                    }

                    need.Remove(neededElement);
                    break;
                }
            } while (need.Count > 1);

            Console.WriteLine($"Needed ORE : {need["ORE"]}");
        }

        private static Dictionary<string, Reaction> ParseReactions(string[] data)
        {
            Dictionary<string, Reaction> allReactions = new Dictionary<string, Reaction>();
            foreach (var line in data)
            {
                var m = Regex.Match(line, @"^(?:(\d+) ([A-Z]+)(?:, | => ))*(\d+) ([A-Z]+)$");
                var b = ImmutableDictionary.CreateBuilder<string, int>();
                for (int i = 0; i < m.Groups[1].Captures.Count; i++)
                {
                    int count = int.Parse(m.Groups[1].Captures[i].Value);
                    string element = m.Groups[2].Captures[i].Value;
                    b.Add(element, count);
                }

                string outputElement = m.Groups[4].Value;
                allReactions.Add(outputElement, new Reaction(b.ToImmutable(), outputElement, int.Parse(m.Groups[3].Value)));
            }

            return allReactions;
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            Dictionary<string, Reaction> allReactions = ParseReactions(data);

            bool CanProduceFuel(long amount)
            {
                Dictionary<string, long> excess = new Dictionary<string, long>{{"ORE", 1_000_000_000_000}};
                Dictionary<string, long> need = new Dictionary<string, long> {{"FUEL", amount}};
                do
                {
                    foreach (var (neededElement, targetCount) in need)
                    {
                        if (!allReactions.TryGetValue(neededElement, out var r))
                        {
                            continue;
                        }

                        long have = excess.GetValueOrDefault(neededElement);
                        if (have > targetCount)
                        {
                            excess[neededElement] = have - targetCount;
                        }
                        else
                        {
                            excess.Remove(neededElement);
                            long more = targetCount - have;

                            long reactionCount = (long) Math.Ceiling(more / (double) r.Amount);
                            long produced = reactionCount * r.Amount;
                            foreach (var (inputElement, inputCount) in r.Inputs)
                            {
                                long inputNeed = inputCount * reactionCount;
                                if (excess.TryGetValue(inputElement, out var ex) && ex > 0)
                                {
                                    if (ex > inputNeed)
                                    {
                                        excess[inputElement] -= inputNeed;
                                        continue;
                                    }

                                    inputNeed -= ex;
                                    excess.Remove(inputElement);
                                }

                                long currentNeed = need.GetValueOrDefault(inputElement);
                                need[inputElement] = currentNeed + inputNeed;

                                if (inputElement == "ORE")
                                {
                                    return false;
                                }
                            }

                            var surplus = produced - targetCount;
                            if (surplus > 0)
                            {
                                excess[neededElement] = excess.GetValueOrDefault(neededElement) + surplus;
                            }
                        }

                        need.Remove(neededElement);
                        break;
                    }
                } while (need.Count > 0);

                return true;
            }

            long low = 1, high = 1_000_000_000_000;
            while (low != high)
            {
                long mid = (low + high + 1) / 2;
                if (CanProduceFuel(mid))
                {
                    low = mid;
                    D.WriteLine($"Able to produce {mid} fuel, trying range {low} - {high}");
                }
                else
                {
                    high = mid - 1;
                    D.WriteLine($"Failed to produce {mid} fuel, trying range {low} - {high}");
                }
            }

            Console.WriteLine($"Fer per trillion {low}");
        }

        public class Reaction
        {
            public ImmutableDictionary<string, int> Inputs { get; }
            public string Output { get; }
            public int Amount { get; }

            public Reaction(ImmutableDictionary<string, int> inputs, string output, int amount)
            {
                Inputs = inputs;
                Output = output;
                Amount = amount;
            }
        }
    }


}