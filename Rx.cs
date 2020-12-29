using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace AdventOfCode
{
    public static class Rx
    {
        public static void M<T1>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1
        )
        {
            Match m = Validate<T1>(input, pattern, 1);
            v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
        }
        
        public static bool IsM<T1>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1
        )
        {
            Match m = ValidateIs<T1>(input, pattern, 1);
            if (m.Success)
            {
                v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
                return true;
            }

            v1 = default;
            return false;
        }

        public static void M<T1, T2>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1,
            out T2 v2
        )
        {
            Match m = Validate<T1>(input, pattern, 2);
            v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
            v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
        }

        public static bool IsM<T1, T2>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1,
            out T2 v2
        )
        {
            Match m = ValidateIs<T1>(input, pattern, 2);
            if (m.Success)
            {
                v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
                v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
                return true;
            }

            v1 = default;
            v2 = default;
            return false;
        }

        public static void M<T1, T2, T3>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1,
            out T2 v2,
            out T3 v3
        )
        {
            Match m = Validate<T1>(input, pattern, 3);
            v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
            v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
            v3 = (T3) Convert.ChangeType(m.Groups[3].Value, typeof(T3));
        }

        public static bool IsM<T1, T2, T3>(string input, [RegexPattern]string pattern,
            out T1 v1,
            out T2 v2,
            out T3 v3)
        {
            Match m = ValidateIs<T1>(input, pattern, 4);
            if (m.Success)
            {
                v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
                v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
                v3 = (T3) Convert.ChangeType(m.Groups[3].Value, typeof(T3));
                return true;
            }

            v1 = default;
            v2 = default;
            v3 = default;
            return false;
        }

        public static void M<T1, T2, T3, T4>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1,
            out T2 v2,
            out T3 v3,
            out T4 v4
        )
        {
            Match m = Validate<T1>(input, pattern, 4);
            v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
            v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
            v3 = (T3) Convert.ChangeType(m.Groups[3].Value, typeof(T3));
            v4 = (T4) Convert.ChangeType(m.Groups[4].Value, typeof(T4));
        }

        public static bool IsM<T1, T2, T3, T4>(string input, [RegexPattern]string pattern,
            out T1 v1,
            out T2 v2,
            out T3 v3,
            out T4 v4)
        {
            Match m = ValidateIs<T1>(input, pattern, 4);
            if (m.Success)
            {
                v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
                v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
                v3 = (T3) Convert.ChangeType(m.Groups[3].Value, typeof(T3));
                v4 = (T4) Convert.ChangeType(m.Groups[4].Value, typeof(T4));
                return true;
            }

            v1 = default;
            v2 = default;
            v3 = default;
            v4 = default;
            return false;
        }

        public static void M<T1, T2, T3, T4, T5>(
            string input,
            [RegexPattern] string pattern,
            out T1 v1,
            out T2 v2,
            out T3 v3,
            out T4 v4,
            out T5 v5
        )
        {
            Match m = Validate<T1>(input, pattern, 5);
            v1 = (T1) Convert.ChangeType(m.Groups[1].Value, typeof(T1));
            v2 = (T2) Convert.ChangeType(m.Groups[2].Value, typeof(T2));
            v3 = (T3) Convert.ChangeType(m.Groups[3].Value, typeof(T3));
            v4 = (T4) Convert.ChangeType(m.Groups[4].Value, typeof(T4));
            v5 = (T5) Convert.ChangeType(m.Groups[5].Value, typeof(T5));
        }

        private static Match Validate<T1>(string input, string pattern, int count)
        {
            Match m = Regex.Match(input, pattern);
            if (!m.Success)
            {
                throw new ArgumentException("Regex is not matched");
            }

            if (m.Groups.Count != count + 1)
            {
                throw new ArgumentException($"Regex requires {count} match group");
            }

            return m;
        }

        private static Match ValidateIs<T1>(string input, string pattern, int count)
        {
            Match m = Regex.Match(input, pattern);
            if (!m.Success)
            {
                return m;
            }

            if (m.Groups.Count != count + 1)
            {
                throw new ArgumentException($"Regex requires {count} match group");
            }

            return m;
        }
    }
}