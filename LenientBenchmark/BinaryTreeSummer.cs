using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public static class BinaryTreeSummer
    {
        public static int SumLeaves(Tree<int> tree, TimeSpan workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as BinaryNode<int>;
            var l = SumLeaves(node.Left, workBias);
            var r = SumLeaves(node.Right, workBias);
            
            var d = Task.Delay(workBias);
            d.Wait();
            
            return l + r;
        }

        public static async Task<int> SumLeavesForkJoin(Tree<int> tree, TimeSpan workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as BinaryNode<int>;
            
            var left = SumLeavesForkJoin(node.Left, workBias);
            var right = SumLeavesForkJoin(node.Right, workBias);
            
#if !DELAY_DEPENDS_ON_LR
            var wb = Task.Delay(workBias);
            await Task.WhenAll(left, right, wb);
#else
            await Task.WhenAll(left, right);

            await Task.Delay(workBias);
#endif
            
            return left.Result + right.Result;
        }

        public static async Task<int> SumLeavesLenient(Task<Tree<int>> tree, TimeSpan workBias)
        {
            var t = await tree;
            if (t is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var n = t as BinaryNode<int>;

            var left = SumLeavesLenient(Task.FromResult(n.Left), workBias);
            var right = SumLeavesLenient(Task.FromResult(n.Right), workBias);
#if !DELAY_DEPENDS_ON_LR
            var wb = Task.Delay(workBias);
            await Task.WhenAll(left, right, wb);
#else
            await Task.WhenAll(left, right);

            await Task.Delay(workBias);
#endif


            return left.Result + right.Result;
        }
    }
}