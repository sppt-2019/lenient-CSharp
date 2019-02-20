using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        static async Task<long> RunSummation()
        {
#if !DELAY_DEPENDS_ON_LR
            var fileName = "work_bias_wait_before_join.csv";
#else
            var fileName = "work_bias_wait_after_join.csv";
#endif
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Work Bias (ms),Work Bias (ticks),Sequential,Fork Join,Lenient");
            
            var t = new Stopwatch();
            var r = new Random();
            var workBias = TimeSpan.FromSeconds(0.5);
            
            var tree = TreeGenerator.CreateTree(TreeSize, () => r.Next(int.MinValue, int.MaxValue));
            long tSeq, tL, tFJ;

            var dummy = 0L;

            do
            {
                t.Restart();
                var sum = TreeSummer.SumLeaves(tree, workBias);
                t.Stop();
                tSeq = t.ElapsedTicks;
                dummy += sum;
                
                t.Restart();
                sum = await TreeSummer.SumLeavesForkJoin(tree, workBias);
                t.Stop();
                tFJ = t.ElapsedTicks;
                dummy += sum;

                t.Restart();
                sum = await TreeSummer.SumLeavesLenient(Task.FromResult(tree), workBias);
                t.Stop();
                tL = t.ElapsedTicks;
                dummy -= sum;

                workBias = workBias / 2;
                Console.WriteLine($"New work bias: {workBias.Ticks} ticks ({workBias.Milliseconds} ms)");
                f.WriteLine($"{workBias.Milliseconds},{workBias.Ticks},{tSeq},{tFJ},{tL}");
            } while (tSeq > tL);

            f.Flush();
            f.Close();
            Console.WriteLine($"Results for Summation of Tree of size {TreeSize} with work load bias");
            Console.WriteLine($"Sequential was faster with:\t\t{workBias.Ticks} ticks ({workBias.Milliseconds} ms) workload bias");
            return dummy;
        }
    }
}