using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LenientBenchmark
{
    class BenchmarkResult
    {
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }

        public BenchmarkResult(double mean, double standardDeviation)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
        }
    }

    abstract class TestRunner<T,TU>
    {
        protected Func<T, TU> TestFunc;
        protected Stopwatch Clock;

        public Dictionary<int, BenchmarkResult> Results;

        public TestRunner(Func<T, TU> testFunc)
        {
            TestFunc = testFunc;
        }

        protected static void Write(int lineNo, string msg)
        {
            Console.SetCursorPosition(0, lineNo);
            Console.WriteLine();
            Console.SetCursorPosition(0, lineNo);
            Console.WriteLine(msg);
        }

        protected static double GetStandardDeviation(IEnumerable<long> runningTimesEnum, double mean)
        {
            var runningTimes = runningTimesEnum.ToList();
            var squaredDeviation = runningTimes.Sum(n => Math.Pow(n - mean, 2));
            return Math.Sqrt(squaredDeviation / runningTimes.Count - 1);
        }
    }

    class McCollinRunner<T,TU> : TestRunner<T,TU>
    {
        private T[] Args;
        public McCollinRunner(Func<T, TU> testFunc, T[] args) : base(testFunc)
        {
            Args = args;
        }

        public void Run(int iterations, Func<T, int> getProblemSize)
        {
            var result = new List<long>();

            foreach (var p in Args)
            {
                for (int i = 0; i < iterations; i++)
                {
                    Clock.Restart();
                    var res = TestFunc(p);
                    Clock.Stop();
                    var time = Clock.ElapsedTicks;
                    result.Add(time);
                }

                var avg = result.Average();
                var sd = GetStandardDeviation(result, avg);
                Results[getProblemSize(p)] = new BenchmarkResult(avg, sd);

                Console.Clear();
            }
        }
    }

    class MorellRunner<T, TU> : TestRunner<T,TU>
    {
        private int[] ProblemSizes;
        Func<int, T> ProblemGenerator;

        public MorellRunner(Func<T, TU> testFunc, int[] problemSizes, Func<int, T> problemGenerator) : base(testFunc)
        {
            ProblemSizes = problemSizes;
            ProblemGenerator = problemGenerator;
        }

        public void Run(int iterations)
        {
            var result = new List<long>();

            foreach (var p in ProblemSizes)
            {
                for (int i = 0; i < iterations; i++)
                {
                    var problem = ProblemGenerator(p);

                    Clock.Restart();
                    var res = TestFunc(problem);
                    Clock.Stop();
                    var time = Clock.ElapsedTicks;
                    result.Add(time);
                }

                var avg = result.Average();
                var sd = GetStandardDeviation(result, avg);
                Results[p] = new BenchmarkResult(avg, sd);

                Console.Clear();
            }
        }
    }
}
