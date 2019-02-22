using System;
using System.Linq;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public class NaryTreeSummer
    {
        public static int SumLeaves(Tree<int> tree, TimeSpan workBias)
        {
            if (tree is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var node = tree as NaryNode<int>;
            var s = node.Children.Sum(c => SumLeaves(c, workBias));
            
            var d = Task.Delay(workBias);
            d.Wait();
            
            return s;
        }

        public static async Task<int> SumLeavesForkJoin(Tree<int> tree, TimeSpan workBias)
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
            var wb = Task.Delay(workBias);

            await Task.WhenAll(sums);
            await wb;
#else
            await Task.WhenAll(sums);
            await Task.Delay(workBias);
#endif
            
            return sums.Sum(t => t.Result);
        }

        public static async Task<int> SumLeavesLenient(Task<Tree<int>> tree, TimeSpan workBias)
        {
            var t = await tree;
            if (t is Leaf<int> leaf)
            {
                return leaf.Value;
            }
            
            var n = t as NaryNode<int>;

            var sums = n.Children.Select(c => SumLeavesLenient(Task.FromResult(c), workBias)).ToList();
#if !DELAY_DEPENDS_ON_LR
            var wb = Task.Delay(workBias);
            await Task.WhenAll(sums);
            await wb;
#else
            await Task.WhenAll(sums);
            await Task.Delay(workBias);
#endif


            return sums.Sum(st => st.Result);
        }
    }
}