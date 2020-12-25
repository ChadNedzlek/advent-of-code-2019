using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdventOfCode.Solutions;

namespace AdventOfCode
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            IEnumerable<Type> dayTypes =
                Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name.StartsWith("Day"));

            bool all = args.Length > 0 || Settings.RunAll;

            IOrderedEnumerable<Type> orderedDays = dayTypes.OrderBy(d => int.Parse(d.Name.Substring(3)));
            if (all)
            {
                foreach (Type day in orderedDays)
                {
                    Console.WriteLine($" ************* {day.Name}");
                    IEnumerable<MethodInfo> methods = day.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        .Where(m => m.Name.StartsWith("Problem"));
                    foreach (MethodInfo method in methods.OrderBy(m => m.Name))
                    {
                        Console.WriteLine($" ** {method.Name}");
                        await (Task) method.Invoke(null, null);
                    }
                }
            }
            else
            {
                Type day = orderedDays.Last();
                IEnumerable<MethodInfo> methods = day.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Where(m => m.Name.StartsWith("Problem"));
                MethodInfo method = methods.OrderBy(m => m.Name).Last();
                await (Task) method.Invoke(null, null);
            }
        }
    }
}