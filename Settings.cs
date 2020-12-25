namespace AdventOfCode
{
    public static class Settings
    {
        public static bool RunSample
#if SAMPLE
            => true;
#else
            => false;
#endif

        public static bool RunAll
#if RUNALL
            => true;
#else
            => false;
#endif
    }
}