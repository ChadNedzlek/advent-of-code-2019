using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day4
    {
        public static async Task Problem1()
        {
            int low = 254032, high = 789860;

            bool Match(int i)
            {
                if (!Regex.IsMatch(i.ToString(), @"(.)\1"))
                    return false;

                var orderedDigits = string.Join("", i.ToString().Select(x => x).OrderBy(x => x));

                if (i.ToString() != orderedDigits)
                    return false;
                return true;
            }

            var matched = Enumerable.Range(low, high - low + 1).Count(Match);
            Console.WriteLine($"{matched} numbers matched weird rules");
        }

        public static async Task Problem2()
        {
            int low = 254032, high = 789860;

            bool Match(int i)
            {
                if (!Regex.IsMatch(i.ToString(), @"(.)\1"))
                    return false;

                var matches = Regex.Matches(i.ToString(), @"(.)\1*");
                if (matches.All(m => m.Length != 2))
                    return false;

                var orderedDigits = string.Join("", i.ToString().Select(x => x).OrderBy(x => x));

                if (i.ToString() != orderedDigits)
                    return false;
                return true;
            }

            var matched = Enumerable.Range(low, high - low + 1).Count(Match);
            Console.WriteLine($"{matched} numbers matched weird rules");
        }
    }
}