using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdventOfCode
{
    public static class Data
    {
        public static Task<string[]> GetDataLines(
            [CallerFilePath] string file = null,
            [CallerMemberName] string problem = null)
        {
            string suffix = Settings.RunSample ? "_sample" : "";

            var dayPart = Path.GetFileNameWithoutExtension(file);
            var problemPart = problem;
            var path =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "data",
                    $"{dayPart}_{problemPart}{suffix}.txt"
                );

            if (File.Exists(path)) return File.ReadAllLinesAsync(path);

            path =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "data",
                    $"{dayPart}_problem1{suffix}.txt"
                );

            if (File.Exists(path)) return File.ReadAllLinesAsync(path);

            path =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "data",
                    $"{dayPart}{suffix}.txt"
                );

            if (File.Exists(path)) return File.ReadAllLinesAsync(path);

            path =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "data",
                    $"{dayPart}_{problemPart}.txt"
                );

            if (File.Exists(path)) return File.ReadAllLinesAsync(path);

            path =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "data",
                    $"{dayPart}_problem1.txt"
                );

            if (File.Exists(path)) return File.ReadAllLinesAsync(path);

            path =
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "data",
                    $"{dayPart}.txt"
                );

            return File.ReadAllLinesAsync(path);
        }
    }
}