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
        private static Random Rnd = new Random();

        private static void Main(string[] args)
        {
            Console.WriteLine(Stopwatch.Frequency);

            Console.WriteLine("Running Accumulation");
            RunAccumulation();
            Console.WriteLine("Running Accumulation - Random");
            RunAccumulationRandom();
            Console.WriteLine("Running Summation");
            RunSummation();
            Console.WriteLine("Running Summation - Random");
            RunSummationRandom();
            Console.WriteLine("Running Linpack");
            RunLinpack();
            Console.WriteLine("Running Linpack - Random");
            RunLinpackRandom();
        }

        private static void RunLinpack()
        {
            var problems = Enumerable.Range(1, 12).Select(n => MatrixSummer.Setup((int)Math.Pow(2, n))).ToArray();
            var linpackSeq = new McCollinRunner<long[,], long>(MatrixSummer.SumSeq, problems);
            var linpackMR = new McCollinRunner<long[,], long>(MatrixSummer.SumMapReduce, problems);
            var linpackP = new McCollinRunner<long[,], long>(MatrixSummer.SumParallel, problems);
            var linpackT = new McCollinRunner<long[,], Task<long>>(MatrixSummer.SumTask, problems);

            linpackSeq.Run(100, m => m.GetLength(0));
            linpackMR.Run(100, m => m.GetLength(0));
            linpackP.Run(100, m => m.GetLength(0));
            linpackT.Run(100, m => m.GetLength(0));

            const string fileName = "linpack.csv";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Map Reduce,Map Reduce Error,Parallel Foreach,Parallel Foreach Error, Tasks, Tasks Error");

            foreach (var res in linpackSeq.Results)
            {
                var mr = linpackMR.Results[res.Key];
                var p = linpackP.Results[res.Key];
                var t = linpackT.Results[res.Key];
                f.WriteLine($"{res.Key},{res.Value.Mean},{res.Value.StandardDeviation},{mr.Mean},{mr.StandardDeviation},{p.Mean},{p.StandardDeviation},{t.Mean},{t.StandardDeviation}");
            }
            
            f.Flush();
            f.Close();
        }

        private static void RunLinpackRandom()
        {
            var problems = Enumerable.Range(1, 12).Select(n => (int) Math.Pow(2, n)).ToArray();
            var linpackSeq = new MorellRunner<long[,], long>(MatrixSummer.SumSeq, problems, s => MatrixSummer.Setup(s));
            var linpackMR = new MorellRunner<long[,], long>(MatrixSummer.SumMapReduce, problems, s => MatrixSummer.Setup(s));
            var linpackP = new MorellRunner<long[,], long>(MatrixSummer.SumParallel, problems, s => MatrixSummer.Setup(s));
            var linpackT = new MorellRunner<long[,], Task<long>>(MatrixSummer.SumTask, problems, s => MatrixSummer.Setup(s));

            linpackSeq.Run(100);
            linpackMR.Run(100);
            linpackP.Run(100);
            linpackT.Run(100);

            const string fileName = "linpack-rand.csv";
            if (File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Map Reduce,Map Reduce Error,Parallel Foreach,Parallel Foreach Error, Tasks, Tasks Error");

            foreach (var res in linpackSeq.Results)
            {
                var mr = linpackMR.Results[res.Key];
                var p = linpackP.Results[res.Key];
                var t = linpackT.Results[res.Key];
                f.WriteLine($"{res.Key},{res.Value.Mean},{res.Value.StandardDeviation},{mr.Mean},{mr.StandardDeviation},{p.Mean},{p.StandardDeviation},{t.Mean},{t.StandardDeviation}");
            }

            f.Flush();
            f.Close();
        }

        private static void RunAccumulation()
        {
            var problems = Enumerable.Range(1, 5).Select(n => TreeGenerator.CreateBinaryTree(n, () => Rnd.Next(int.MinValue, int.MaxValue))).ToArray();
            var accumSeq = new McCollinRunner<Tree<int>, List<int>>(TreeAccumulator.AccumulateLeaves, problems);
            var accumFJ = new McCollinRunner<Tree<int>, List<int>>(TreeAccumulator.AccumulateLeavesForkJoin, problems);
            var accumL = new McCollinRunner<Tree<int>, List<int>>(TreeAccumulator.AccumulateLeavesLenient, problems);

            accumSeq.Run(100, m => m.NumberOfLeaves);
            accumFJ.Run(100, m => m.NumberOfLeaves);
            accumL.Run(100, m => m.NumberOfLeaves);

            const string fileName = "accumulation.csv";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Fork Join,Fork Join Error,Lenient,Lenient Error");

            foreach (var res in accumSeq.Results)
            {
                var fj = accumFJ.Results[res.Key];
                var l = accumL.Results[res.Key];
                f.WriteLine($"{res.Key},{res.Value.Mean},{res.Value.StandardDeviation},{fj.Mean},{fj.StandardDeviation},{l.Mean},{l.StandardDeviation}");
            }

            f.Flush();
            f.Close();
        }

        private static void RunAccumulationRandom()
        {
            var problems = Enumerable.Range(1, 5).Select(n => (int)Math.Pow(10, n)).ToArray();
            Func<int, Tree<int>> treeGenerator = s => TreeGenerator.CreateTree(s, () => Rnd.Next(int.MinValue, int.MaxValue));
            var accumSeq = new MorellRunner<Tree<int>, List<int>>(TreeAccumulator.AccumulateLeaves, problems, treeGenerator);
            var accumFJ = new MorellRunner<Tree<int>, List<int>>(TreeAccumulator.AccumulateLeavesForkJoin, problems, treeGenerator);
            var accumL = new MorellRunner<Tree<int>, List<int>>(TreeAccumulator.AccumulateLeavesLenient, problems, treeGenerator);

            accumSeq.Run(100);
            accumFJ.Run(100);
            accumL.Run(100);

            const string fileName = "accumulation-rand.csv";
            if (File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Fork Join,Fork Join Error,Lenient,Lenient Error");

            foreach (var res in accumSeq.Results)
            {
                var fj = accumFJ.Results[res.Key];
                var l = accumL.Results[res.Key];
                f.WriteLine($"{res.Key},{res.Value.Mean},{res.Value.StandardDeviation},{fj.Mean},{fj.StandardDeviation},{l.Mean},{l.StandardDeviation}");
            }

            f.Flush();
            f.Close();
        }

        private static void RunSummation()
        {
            var problems = Enumerable.Range(1, 5).Select(n => TreeGenerator.CreateBinaryTree(n, () => Rnd.Next(int.MinValue, int.MaxValue))).ToArray();
            var sumSeq = new McCollinRunner<Tree<int>, int>(TreeSummer.SumLeaves, problems);
            var sumFJ = new McCollinRunner<Tree<int>, Task<int>>(TreeSummer.SumLeavesForkJoin, problems);
            var sumL = new McCollinRunner<Task<Tree<int>>, Task<int>>(TreeSummer.SumLeavesLenient, problems.Select(t => Task.FromResult(t)).ToArray());

            sumSeq.Run(100, m => m.NumberOfLeaves);
            sumFJ.Run(100, m => m.NumberOfLeaves);
            sumL.Run(100, m => m.Result.NumberOfLeaves);

            const string fileName = "summation.csv";
            if(File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Fork Join,Fork Join Error,Lenient,Lenient Error");

            foreach (var res in sumSeq.Results)
            {
                var fj = sumFJ.Results[res.Key];
                var l = sumL.Results[res.Key];
                f.WriteLine($"{res.Key},{res.Value.Mean},{res.Value.StandardDeviation},{fj.Mean},{fj.StandardDeviation},{l.Mean},{l.StandardDeviation}");
            }

            f.Flush();
            f.Close();
        }

        private static void RunSummationRandom()
        {
            var problems = Enumerable.Range(1, 5).Select(n => (int)Math.Pow(10, n)).ToArray();
            Func<int, Tree<int>> treeGenerator = s => TreeGenerator.CreateTree(s, () => Rnd.Next(int.MinValue, int.MaxValue));
            var sumSeq = new MorellRunner<Tree<int>, int>(TreeSummer.SumLeaves, problems, treeGenerator);
            var sumFJ = new MorellRunner<Tree<int>, Task<int>>(TreeSummer.SumLeavesForkJoin, problems, treeGenerator);
            var sumL = new MorellRunner<Task<Tree<int>>, Task<int>>(TreeSummer.SumLeavesLenient, problems, 
                s => Task.FromResult(TreeGenerator.CreateTree(s, () => Rnd.Next(int.MinValue, int.MaxValue))));

            sumSeq.Run(100);
            sumFJ.Run(100);
            sumL.Run(100);

            const string fileName = "summation.csv";
            if (File.Exists(fileName))
                File.Delete(fileName);
            var f = new StreamWriter(fileName);
            f.WriteLine("Problem Size,Sequential,Sequential Error,Fork Join,Fork Join Error,Lenient,Lenient Error");

            foreach (var res in sumSeq.Results)
            {
                var fj = sumFJ.Results[res.Key];
                var l = sumL.Results[res.Key];
                f.WriteLine($"{res.Key},{res.Value.Mean},{res.Value.StandardDeviation},{fj.Mean},{fj.StandardDeviation},{l.Mean},{l.StandardDeviation}");
            }

            f.Flush();
            f.Close();
        }
    }
}
