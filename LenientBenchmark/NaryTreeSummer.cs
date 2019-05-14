using System;
using System.Linq;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public class NaryTreeSummer
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
            
            var node = tree as NaryNode<int>;
            var s = node.Children.Sum(c => SumLeaves(c, workBias));
            var sum = Delay(workBias);
            
            return s + sum;
        }

        public static async Task<int> SumLeavesForkJoin(Tree<int> tree, int workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as NaryNode<int>;

            var sums = node.Children.Select(c => SumLeavesForkJoin(c, workBias)).ToList();
            
#if !DELAY_DEPENDS_ON_LR
            //Start the 'work bias' before blocking wait on child computation, i.e. we're waiting while the children
            //are computing
            var wb = Task.Run(() => Delay(workBias));

            await Task.WhenAll(sums);
            await wb;
            return wb.Result + sums.Sum(t => t.Result);
#else
            await Task.WhenAll(sums);
            var res = Delay(workBias);
            return res + sums.Sum(t => t.Result);
#endif
        }

        public static async Task<int> SumLeavesLenient(Task<Tree<int>> tree, int workBias)
        {
            var t = await tree;
            if (t is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var n = t as NaryNode<int>;

            var sums = n.Children.Select(c => SumLeavesLenient(Task.FromResult(c), workBias)).ToList();
#if !DELAY_DEPENDS_ON_LR
            //Start the 'work bias' before blocking wait on child computation, i.e. we're waiting while the children
            //are computing
            var wb = Task.Run(() => Delay(workBias));

            await Task.WhenAll(sums);
            await wb;
            return wb.Result + sums.Sum(ta => ta.Result);
#else
            await Task.WhenAll(sums);
            var res = Delay(workBias);
            return res + sums.Sum(t => t.Result);
#endif
        }
    }
}