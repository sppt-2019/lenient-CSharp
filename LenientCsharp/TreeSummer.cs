using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    internal class TreeSummer
    {
        public static int SumLeaves(Tree<int> tree)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as Node<int>;
            return SumLeaves(node.Left) + SumLeaves(node.Right);
        }

        public static async Task<int> SumLeavesForkJoin(Tree<int> tree)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as Node<int>;
            
            var left = SumLeavesForkJoin(node.Left);
            var right = SumLeavesForkJoin(node.Right);
            await Task.WhenAll(left, right);
            
            return left.Result + right.Result;
        }

        public static async Task<int> SumLeavesLenient(Task<Tree<int>> tree)
        {
            var t = await tree;
            if (t is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var n = t as Node<int>;

            var left = SumLeavesLenient(Task.FromResult(n.Left));
            var right = SumLeavesLenient(Task.FromResult(n.Right));
            await Task.WhenAll(left, right);
            
            return left.Result + right.Result;
        }
    }
}