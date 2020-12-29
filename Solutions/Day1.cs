using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day1
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            Console.WriteLine($"Fuel required: {data.Select(long.Parse).Select(m => (m / 3) - 2).Sum()}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            IEnumerable<long> moduleWeights = data.Select(long.Parse);
            long sum = 0;
            foreach (long m in moduleWeights)
            {
                long mass = m;
                while(mass > 0){
                    mass = Math.Max(0, mass / 3 - 2);
                    sum += mass;
                }
            }

            Console.WriteLine($"Fuel required: {sum}");
        }
    }
}