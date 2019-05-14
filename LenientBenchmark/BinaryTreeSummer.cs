using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public static class BinaryTreeSummer
    {
        private static int Delay(int workBias)
        {
            var sum = 0;
            for (var i = 0; i < workBias / 2; i++)
            {
                sum += i;
            }

            for (var i = 0; i < workBias / 2; i++)
            {
                sum -= i;
            }

            return sum;
        }
        
        public static int SumLeaves(Tree<int> tree, int workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as BinaryNode<int>;
            var l = SumLeaves(node.Left, workBias);
            var r = SumLeaves(node.Right, workBias);
            var sum = Delay(workBias);
            
            return sum + l + r;
        }

        public static async Task<int> SumLeavesForkJoin(Tree<int> tree, int workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as BinaryNode<int>;
            
            var left = SumLeavesForkJoin(node.Left, workBias);
            var right = SumLeavesForkJoin(node.Right, workBias);
            
#if !DELAY_DEPENDS_ON_LR
            var wb = Task.Run(() => Delay(workBias));
            await Task.WhenAll(left, right, wb);
            return wb.Result + left.Result + right.Result;
#else
            await Task.WhenAll(left, right);
            var res = Delay(workBias);
            return res + left.Result + right.Result;
#endif
        }

        public static async Task<int> SumLeavesLenient(Task<Tree<int>> tree, int workBias)
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
            var wb = Task.Run(() => Delay(workBias));
            await Task.WhenAll(left, right, wb);
            return wb.Result + left.Result + right.Result;
#else
            await Task.WhenAll(left, right);
            var res = Delay(workBias);
            return res + left.Result + right.Result;
#endif
        }
    }
}