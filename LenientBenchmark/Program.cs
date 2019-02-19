using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    class Program
    {
        private const int NumberOfRuns = 1;
        private const int TreeSize = 60;
        
        static async Task Main(string[] args)
        {
            await RunAccumulation();
            await RunSummation();
        }

        static async Task RunAccumulation()
        {
            var seq = new List<long>();
            var fd = new List<long>();
            var l = new List<long>();
            
            var t = new Timer();
            var r = new Random();
            
            for (var i = 0; i < NumberOfRuns; i++)
            {
                var tree = TreeGenerator.CreateTree(TreeSize, () => r.Next(-TreeSize, TreeSize));
            
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
            }

            var avgSeq = seq.Average();
            var avgFJ = fd.Average();
            var avgL = l.Average();

            Console.WriteLine($"Results for Accumulation of Tree of size {TreeSize} with {NumberOfRuns} runs");
            Console.WriteLine($"Sequential:\t\t{avgSeq} ticks");
            Console.WriteLine($"Fork Join:\t\t{avgFJ} ticks");
            Console.WriteLine($"Lenient:\t\t{avgL} ticks");
        }

        static async Task RunSummation()
        {
            var seq = new List<long>();
            var fd = new List<long>();
            var l = new List<long>();
            
            var t = new Timer();
            var r = new Random();
            var workBias = TimeSpan.FromSeconds(0.5);
            
            for (var i = 0; i < NumberOfRuns; i++)
            {
                var tree = TreeGenerator.CreateTree(TreeSize, () => r.Next(int.MinValue, int.MaxValue));
            
                t.Play();
                var sum = TreeSummer.SumLeaves(tree, workBias);
                var tSeq = t.Check();
                seq.Add(tSeq);
            
                t.Play();
                sum = await TreeSummer.SumLeavesForkJoin(tree, workBias);
                var tFJ = t.Check();
                fd.Add(tFJ);
            
                t.Play();
                sum = await TreeSummer.SumLeavesLenient(Task.FromResult(tree), workBias);
                var tL = t.Check();
                l.Add(tL);
            }

            var avgSeq = seq.Average();
            var avgFJ = fd.Average();
            var avgL = l.Average();

            Console.WriteLine($"Results for Summation of Tree of size {TreeSize} with {NumberOfRuns} runs");
            Console.WriteLine($"Sequential:\t\t{avgSeq} ticks");
            Console.WriteLine($"Fork Join:\t\t{avgFJ} ticks");
            Console.WriteLine($"Lenient:\t\t{avgL} ticks");
        }
    }
}