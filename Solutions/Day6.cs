using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace AdventOfCode.Solutions
{
    public class Day6
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            Dictionary<string, string> orbits = new Dictionary<string, string>();
            foreach (var line in data)
            {
                Rx.M(line, @"^(.*)\)(.*)$", out string center, out string satellite);
                orbits.Add(satellite, center);
            }

            int Depth(string s)
            {
                if (!orbits.TryGetValue(s, out string c))
                    return 0;
                return 1 + Depth(c);
            }

            Console.WriteLine($"Total orbits : {orbits.Keys.Sum(Depth)}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            Dictionary<string, string> orbits = new Dictionary<string, string>();
            foreach (var line in data)
            {
                Rx.M(line, @"^(.*)\)(.*)$", out string center, out string satellite);
                orbits.Add(satellite, center);
            }

            int? Depth(string s, string target)
            {
                if (s == target)
                    return 0;

                if (!orbits.TryGetValue(s, out string c))
                    return null;

                int? depth = Depth(c, target);
                return 1 + depth;
            }

            int toInterSection = 0;
            string current = orbits["YOU"];
            while (true)
            {
                var santaIntersection = Depth(orbits["SAN"], current);
                if (santaIntersection.HasValue)
                {
                    Console.WriteLine($"Transfers: {santaIntersection + toInterSection}");
                    break;
                }

                toInterSection++;
                current = orbits[current];
            }
        }
    }
}