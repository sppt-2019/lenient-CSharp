using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LenientBenchmark
{
    internal class Program
    {
        private const int NumberOfRuns = 100;
        private const int MaxTreeSize = 100000;

        private static async Task Main(string[] args)
        {
            Console.WriteLine(Stopwatch.Frequency);
            
            //await RunAccumulation();
            //await RunSummation();
            RunLinpack();
        }

        private static long Time<U, T>(Func<T, U> code, T arg)
        {
            U result;
            var t = new Stopwatch();
            t.Start();
            result = code(arg);
            t.Stop();
            return t.ElapsedTicks;
        }

        private static void RunLinpack()
        {
            const string fileName = "linpack.csv";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Map Reduce,Map Reduce Error,Parallel Foreach,Parallel Foreach Error, Tasks, Tasks Error");
            
            var seq = new List<long>();
            var mr = new List<long>();
            var pfe = new List<long>();
            var tasks = new List<long>();

            for (var ps = 2; ps <= 4096; ps *= 2)
            {
                Write(1, "Problem size: " + ps);
                for (var i = 0; i <= NumberOfRuns; i++)
                {
                    var m = Linpack.Setup(ps, ps);
            
                    seq.Add(Time(Linpack.SumSeq, m));
                    mr.Add(Time(Linpack.SumMapReduce, m));
                    pfe.Add(Time(Linpack.SumParallel, m));
                    tasks.Add(Time(Linpack.SumTask, m));
                
                    //Line 0 is reserved for dll information
                    Write(2, $"{((float) i / NumberOfRuns) * 100}%");
                }

                double avgSeq = seq.Skip(1).Average(), avgMR = mr.Skip(1).Average(), avgPFE = pfe.Skip(1).Average(), avgT = tasks.Skip(1).Average();
                double sdSeq = GetStandardDeviation(seq.Skip(1), avgSeq),
                    sdMR = GetStandardDeviation(mr.Skip(1), avgMR),
                    sdPFE = GetStandardDeviation(pfe.Skip(1), avgPFE),
                    sdT = GetStandardDeviation(tasks.Skip(1), avgT);
                
                f.WriteLine($"{ps},{avgSeq},{sdSeq},{avgMR},{sdMR},{avgPFE},{sdPFE},{avgT},{sdT}");
            
                Console.Clear();
            }
            
            f.Flush();
            f.Close();
        }

        private static async Task RunAccumulation()
        {
            const string fileName = "accumulation.csv";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Fork Join,Fork Join Error,Lenient,Lenient Error");
            
            var seq = new List<long>();
            var fd = new List<long>();
            var l = new List<long>();
            
            var t = new Timer();
            var r = new Random();

            for (var ps = 10; ps <= MaxTreeSize; ps *= 10)
            {
                //Line 0 is reserved for dll information
                Write(1, "Problem size: " + ps);
                for (var i = 0; i <= NumberOfRuns; i++)
                {
                    var tree = TreeGenerator.CreateTree(ps, () => r.Next(-MaxTreeSize, MaxTreeSize));
            
                    var acc = new List<int>();
                    t.Play();
                    TreeAccumulator.AccumulateLeaves(tree, acc);
                    var tSeq = t.Check();
                    seq.Add(tSeq);
            
                    var accForkJoin = new List<int>();
                    t.Play();
                    await TreeAccumulator.AccumulateLeavesForkJoin(tree, accForkJoin);
                    var tFJ = t.Check();
                    fd.Add(tFJ);
            
                    var accLenient = new List<int>();
                    t.Play();
                    await TreeAccumulator.AccumulateLeavesLenient(Task.FromResult(tree), Task.FromResult(accLenient));
                    var tL = t.Check();
                    l.Add(tL);
                
                    Write(2, $"{((float) i / NumberOfRuns) * 100}%");
                }

                double avgSeq = seq.Skip(1).Average(), avgFJ = fd.Skip(1).Average(), avgL = l.Skip(1).Average();
                double sdSeq = GetStandardDeviation(seq.Skip(1), avgSeq),
                    sdFJ = GetStandardDeviation(fd.Skip(1), avgFJ),
                    sdL = GetStandardDeviation(l.Skip(1), avgL);
                
                f.WriteLine($"{ps},{avgSeq},{sdSeq},{avgFJ},{sdFJ},{avgL},{sdL}");
            
                Console.Clear();
            }
            
            f.Flush();
            f.Close();
        }

        private static double GetStandardDeviation(IEnumerable<long> runningTimesEnum, double mean)
        {
            var runningTimes = runningTimesEnum.ToList();
            var squaredDeviation = runningTimes.Sum(n => Math.Pow(n - mean, 2));
            return Math.Sqrt(squaredDeviation / runningTimes.Count - 1);
        }

        private static void Write(int lineNo, string msg)
        {
            Console.SetCursorPosition(0,lineNo);
            Console.WriteLine();
            Console.SetCursorPosition(0,lineNo);
            Console.WriteLine(msg);
        }

        private static async Task RunSummation()
        {
            const string fileName = "summation.csv";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Fork Join,Fork Join Error,Lenient,Lenient Error");
            
            var seq = new List<long>();
            var fd = new List<long>();
            var l = new List<long>();
            
            var t = new Timer();
            var r = new Random();

            for (var ps = 10; ps <= MaxTreeSize; ps *= 10)
            {
                Write(1, "Problem size: " + ps);
                
                for (var i = 0; i < NumberOfRuns; i++)
                {
                    var tree = TreeGenerator.CreateTree(ps, () => r.Next(int.MinValue, int.MaxValue));

                    t.Play();
                    var sum = TreeSummer.SumLeaves(tree);
                    var tSeq = t.Check();
                    seq.Add(tSeq);

                    t.Play();
                    sum = await TreeSummer.SumLeavesForkJoin(tree);
                    var tFJ = t.Check();
                    fd.Add(tFJ);

                    t.Play();
                    sum = await TreeSummer.SumLeavesLenient(Task.FromResult(tree));
                    var tL = t.Check();
                    l.Add(tL);

                    Write(2, $"{((float) i / NumberOfRuns) * 100}%");
                }
                
                double avgSeq = seq.Average(), avgFJ = fd.Average(), avgL = l.Average();
                double sdSeq = GetStandardDeviation(seq, avgSeq),
                    sdFJ = GetStandardDeviation(fd, avgFJ),
                    sdL = GetStandardDeviation(l, avgL);
                
                f.WriteLine($"{ps},{avgSeq},{sdSeq},{avgFJ},{sdFJ},{avgL},{sdL}");
            
                Console.Clear();
            }
            
            f.Flush();
            f.Close();
        }
    }
}
