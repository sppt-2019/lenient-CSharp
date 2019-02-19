using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LenientBenchmark
{
    class Program
    {
        private const int NumberOfRuns = 100;
        private const int MaxTreeSize = 100000;
        
        static async Task Main(string[] args)
        {
            await RunAccumulation();
            await RunSummation();
        }

        static async Task RunAccumulation()
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
                
                    //Line 0 is reserved for dll information
                    Write(2, $"{((float) i / NumberOfRuns) * 100}%");
                }

                double avgSeq = seq.Average(), avgFJ = fd.Average(), avgL = l.Average();
                double sdSeq = GetStandardDeviation(seq.Sum(c => c * c), avgSeq),
                    sdFJ = GetStandardDeviation(fd.Sum(c => c * c), avgFJ),
                    sdL = GetStandardDeviation(l.Sum(c => c * c), avgL);
                
                f.WriteLine($"{ps},{avgSeq},{sdSeq},{avgFJ},{sdFJ},{avgL},{sdL}");
            
                Console.Clear();
            }
            
            f.Flush();
            f.Close();
        }

        static double GetStandardDeviation(long deltaTimeSquared, double mean)
        {
            return Math.Sqrt((deltaTimeSquared - mean * mean * NumberOfRuns) / (NumberOfRuns - 1));
        }

        static void Write(int lineNo, string msg)
        {
            Console.SetCursorPosition(0,lineNo);
            Console.WriteLine();
            Console.SetCursorPosition(0,lineNo);
            Console.WriteLine(msg);
        }

        static async Task RunSummation()
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
                double sdSeq = GetStandardDeviation(seq.Sum(c => c * c), avgSeq),
                    sdFJ = GetStandardDeviation(fd.Sum(c => c * c), avgFJ),
                    sdL = GetStandardDeviation(l.Sum(c => c * c), avgL);
                
                f.WriteLine($"{ps},{avgSeq},{sdSeq},{avgFJ},{sdFJ},{avgL},{sdL}");
            
                Console.Clear();
            }
            
            f.Flush();
            f.Close();
        }
    }
}