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
            //await RunAccumulation();
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

        static async Task<long> RunSummation()
        {
            var t = new Timer();
            var r = new Random();
            var workBias = TimeSpan.FromSeconds(0.5);
            
            var tree = TreeGenerator.CreateTree(TreeSize, () => r.Next(int.MinValue, int.MaxValue));
            long tSeq, tL;

            var dummy = 0L;

            do
            {
                t.Play();
                var sum = TreeSummer.SumLeaves(tree, workBias);
                tSeq = t.Check();
                dummy += sum;

                t.Play();
                sum = await TreeSummer.SumLeavesLenient(Task.FromResult(tree), workBias);
                tL = t.Check();
                dummy -= sum;

                workBias = workBias / 2;
                Console.WriteLine($"Sequential: {tSeq} ticks\t\tLenient: {tL} ticks\t\tNew work bias: {workBias.Ticks} ticks ({workBias.Milliseconds} ms)");
            } while (tSeq > tL);

            Console.WriteLine($"Results for Summation of Tree of size {TreeSize} with work load bias");
            Console.WriteLine($"Sequential was faster with:\t\t{workBias.Ticks} ticks ({workBias.Milliseconds} ms) workload bias");
            return dummy;
        }
    }
}