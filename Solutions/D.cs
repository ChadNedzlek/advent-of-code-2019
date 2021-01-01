using System;
using System.Diagnostics;

namespace AdventOfCode.Solutions
{
    public static class D
    {
        [Conditional("DEBUG")]
        public static void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
        
        [Conditional("DEBUG")]
        public static void WriteLine()
        {
            Console.WriteLine();
        }
        
        [Conditional("DEBUG")]
        public static void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }
        
        [Conditional("DEBUG")]
        public static void Write(string msg)
        {
            Console.Write(msg);
        }
        
        [Conditional("DEBUG")]
        public static void Write(char msg)
        {
            Console.Write(msg);
        }
    }
}