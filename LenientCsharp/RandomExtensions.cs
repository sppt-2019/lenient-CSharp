using System;

namespace LenientBenchmark
{
    public static class RandomExtensions
    {
        public static long NextLong(this Random rnd)
        {
            var buffer = new byte[8];
            rnd.NextBytes (buffer);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static long NextLong(this Random rnd, long min, long max)
        {
            EnsureMinLEQMax(ref min, ref max);
            var numbersInRange = unchecked(max - min + 1);
            if (numbersInRange < 0)
                throw new ArgumentException("Size of range between min and max must be less than or equal to Int64.MaxValue");

            var randomOffset = rnd.NextLong();
            if (IsModuloBiased(randomOffset, numbersInRange))
                return rnd.NextLong(min, max); // Try again
            else
                return min + PositiveModuloOrZero(randomOffset, numbersInRange);
        }

        static bool IsModuloBiased(long randomOffset, long numbersInRange)
        {
            var greatestCompleteRange = numbersInRange * (long.MaxValue / numbersInRange);
            return randomOffset > greatestCompleteRange;
        }

        static long PositiveModuloOrZero(long dividend, long divisor)
        {
            long mod;
            Math.DivRem(dividend, divisor, out mod);
            if(mod < 0)
                mod += divisor;
            return mod;
        }

        static void EnsureMinLEQMax(ref long min, ref long max)
        {
            if(min <= max)
                return;
            var temp = min;
            min = max;
            max = temp;
        }
    }
}