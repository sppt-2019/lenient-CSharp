using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LenientBenchmark
{
    public class BenchmarkResult
    {
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }

        public BenchmarkResult(double mean, double standardDeviation)
        {
            Mean = mean;
            StandardDeviation = standardDeviation;
        }
    }

    public abstract class TestRunner<T,TU>
    {
        protected Func<T, TU> TestFunc;
        protected Stopwatch Clock;

        public Dictionary<int, BenchmarkResult> Results;

        public TestRunner(Func<T, TU> testFunc)
        {
            Clock = new Stopwatch();
            Results = new Dictionary<int, BenchmarkResult>();
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

    public class McCollinRunner<T,TU> : TestRunner<T,TU>
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
            }
        }
    }

    public class MorellRunner<T, TU> : TestRunner<T,TU>
    {
        private T[] Problems;
        public int[] ProblemSizes { get; }


        public MorellRunner(Func<T, TU> testFunc, T[] problems, int[] problemSizes) : base(testFunc)
        {
            Problems = problems;
            ProblemSizes = problemSizes;
        }

        public void Run(int iterations)
        {
            var result = new List<long>();

            for (var j = 0; j < Problems.Length; j++)
            {
                for (var i = 0; i < iterations; i++)
                {
                    var problem = Problems[j];
                    Clock.Restart();
                    var res = TestFunc(problem);
                    Clock.Stop();
                    var time = Clock.ElapsedTicks;
                    result.Add(time);
                }

                var avg = result.Average();
                var sd = GetStandardDeviation(result, avg);
                Results[ProblemSizes[j]] = new BenchmarkResult(avg, sd);
            }
        }
    }
}
