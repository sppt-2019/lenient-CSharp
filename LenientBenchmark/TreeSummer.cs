using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public class TreeSummer
    {
        public static int SumLeaves(Tree<int> tree, TimeSpan workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }

            var d = Task.Delay(workBias);
            d.Wait();
            
            var node = tree as Node<int>;
            return SumLeaves(node.Left, workBias) + SumLeaves(node.Right, workBias);
        }

        public static async Task<int> SumLeavesForkJoin(Tree<int> tree, TimeSpan workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as Node<int>;
            
            var left = SumLeavesForkJoin(node.Left, workBias);
            var right = SumLeavesForkJoin(node.Right, workBias);
            await Task.WhenAll(left, right);

            await Task.Delay(workBias);
            
            return left.Result + right.Result;
        }

        public static async Task<int> SumLeavesLenient(Task<Tree<int>> tree, TimeSpan workBias)
        {
            var t = await tree;
            if (t is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var n = t as Node<int>;

            var left = SumLeavesLenient(Task.FromResult(n.Left), workBias);
            var right = SumLeavesLenient(Task.FromResult(n.Right), workBias);
            await Task.WhenAll(left, right);

            await Task.Delay(workBias);
            
            return left.Result + right.Result;
        }
    }
}