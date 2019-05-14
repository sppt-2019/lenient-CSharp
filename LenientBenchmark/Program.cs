#define DELAY_DEPENDS_ON_LR

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
            await RunSummation();
            await RunNarySummation();
        }

        static async Task<long> RunSummation()
        {
#if !DELAY_DEPENDS_ON_LR
            var fileName = "work-bias-wait-before-join.csv";
#else
            var fileName = "work-bias-wait-after-join.csv";
#endif
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Work Bias (iterations),Sequential,Fork Join,Lenient");
            
            var t = new Stopwatch();
            var r = new Random();
            
            var tree = BinaryTreeGenerator.CreateTree(TreeSize, () => r.Next(int.MinValue, int.MaxValue));
            long tSeq, tL, tFJ;

            var dummy = 0L;
            var _workBias = (int) Math.Pow(2, 25);

            do
            {
                t.Restart();
                var sum = BinaryTreeSummer.SumLeaves(tree, _workBias);
                t.Stop();
                tSeq = t.ElapsedTicks;
                dummy += sum;
                
                t.Restart();
                sum = await BinaryTreeSummer.SumLeavesForkJoin(tree, _workBias);
                t.Stop();
                tFJ = t.ElapsedTicks;
                dummy += sum;

                t.Restart();
                sum = await BinaryTreeSummer.SumLeavesLenient(Task.FromResult(tree), _workBias);
                t.Stop();
                tL = t.ElapsedTicks;
                dummy -= sum;

                _workBias = _workBias / 2;
                Console.WriteLine($"New work bias: {_workBias} iterations");
                f.WriteLine($"{_workBias},{tSeq},{tFJ},{tL}");
            } while (tSeq > tL);

            f.Flush();
            f.Close();
            Console.WriteLine($"Results for Summation of Tree of size {TreeSize} with work load bias");
            Console.WriteLine($"Sequential was faster with:\t\t{_workBias} iterations workload bias");
            return dummy;
        }
        
        static async Task<long> RunNarySummation()
        {
#if !DELAY_DEPENDS_ON_LR
            var fileName = "nary-work-bias-wait-before-join.csv";
#else
            var fileName = "nary-work-bias-wait-after-join.csv";
#endif
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Work Bias (iterations),Sequential,Fork Join,Lenient");
            
            var t = new Stopwatch();
            var r = new Random();
            
            var tree = NaryTreeGenerator.CreateTree<int>(TreeSize, () => r.Next(-TreeSize, +TreeSize));
            long tSeq, tL, tFJ;

            var dummy = 0L;
            var _workBias = (int) Math.Pow(2, 28);

            do
            {
                t.Restart();
                var sum = NaryTreeSummer.SumLeaves(tree, _workBias);
                t.Stop();
                tSeq = t.ElapsedTicks;
                dummy += sum;
                
                t.Restart();
                sum = await NaryTreeSummer.SumLeavesForkJoin(tree, _workBias);
                t.Stop();
                tFJ = t.ElapsedTicks;
                dummy += sum;

                t.Restart();
                sum = await NaryTreeSummer.SumLeavesLenient(Task.FromResult(tree), _workBias);
                t.Stop();
                tL = t.ElapsedTicks;
                dummy -= sum;

                _workBias = _workBias / 2;
                Console.WriteLine($"New work bias: {_workBias} iterations");
                f.WriteLine($"{_workBias},{tSeq},{tFJ},{tL}");
            } while (tSeq > tL);

            f.Flush();
            f.Close();
            Console.WriteLine($"Results for Summation of Tree of size {TreeSize} with work load bias");
            Console.WriteLine($"Sequential was faster with:\t\t{_workBias} iterations workload bias");
            return dummy;
        }
    }
}