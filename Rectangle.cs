namespace AdventOfCode
{
    public readonly struct Rectangle
    {
        public Rectangle(int top, int bottom, int left, int right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }

        public int Top { get; }
        public int Bottom { get; }
        public int Left { get; }
        public int Right { get; }
    }
}