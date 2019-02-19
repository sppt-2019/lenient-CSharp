using System.Collections.Generic;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public static class TreeAccumulator
    {
        public static void AccumulateLeaves<T>(Tree<T> tree, List<T> leaves)
        {
            if (tree is Leaf<T> leaf)
            {
                leaves.Add(leaf.Value);
            }
            else
            {
                var node = tree as Node<T>;
                
                AccumulateLeaves<T>(node.Left, leaves);
                AccumulateLeaves<T>(node.Right, leaves);
            }
        }

        public static async Task AccumulateLeavesForkJoin<T>(Tree<T> tree, List<T> leaves)
        {
            if (tree is Leaf<T> leaf)
            {
                leaves.Add(leaf.Value);
            }
            else
            {
                var node = tree as Node<T>;

                var left = Task.Run(async () =>
                {
                    var leftLeaves = new List<T>();
                    await AccumulateLeavesForkJoin<T>(node.Left, leftLeaves);
                    return leftLeaves;
                });
                var right = Task.Run(async () =>
                {
                    var rightLeaves = new List<T>();
                    await AccumulateLeavesForkJoin(node.Right, rightLeaves);
                    return rightLeaves;
                });
                var l = await left;
                leaves.AddRange(l);
                var r = await right;
                leaves.AddRange(r);
            }
        }

        public static async Task AccumulateLeavesLenient<T>(Task<Tree<T>> tree, Task<List<T>> leaves)
        {
            var t = await tree;
            if (t is Leaf<T> leaf)
            {
                var l = await leaves;
                l.Add(leaf.Value);
            }
            else
            {
                var n = t as Node<T>;

                var left = Task.Run(async () =>
                {
                    await AccumulateLeavesLenient<T>(Task.FromResult(n.Left), leaves);
                    return leaves.Result;
                });
                var right = Task.Run(async () =>
                {
                    await AccumulateLeavesLenient<T>(Task.FromResult(n.Right), left);
                });

                await Task.WhenAll(left, right);
            }
        }
    }
}