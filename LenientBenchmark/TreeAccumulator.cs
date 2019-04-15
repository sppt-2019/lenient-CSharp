using System.Collections.Generic;
using System.Threading.Tasks;

namespace LenientBenchmark
{
    public static class TreeAccumulator
    {
        public static List<T> AccumulateLeaves<T>(Tree<T> tree)
        {
            var l = new List<T>();
            AccumulateLeaves(tree, l);
            return l;
        }

        private static void AccumulateLeaves<T>(Tree<T> tree, List<T> leaves)
        {
            if (tree is Leaf<T> leaf)
            {
                leaves.Add(leaf.Value);
            }
            else
            {
                var node = tree as Node<T>;

                AccumulateLeaves(node.Left, leaves);
                AccumulateLeaves(node.Right, leaves);
            }
        }

        public static List<T> AccumulateLeavesForkJoin<T>(Tree<T> tree)
        {
            var l = new List<T>();
            var t = AccumulateLeavesForkJoin(tree, l);
            t.Wait();
            return l;
        }

        private static async Task AccumulateLeavesForkJoin<T>(Tree<T> tree, List<T> leaves)
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
                    await AccumulateLeavesForkJoin(node.Left, leftLeaves);
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
        
        public static List<T> AccumulateLeavesLenient<T>(Tree<T> tree)
        {
            var l = Task.Run(() => new List<T>());
            var t = AccumulateLeavesLenient(Task.FromResult(tree), l);
            Task.WaitAll(l, t);
            return l.Result;
        }

        private static async Task AccumulateLeavesLenient<T>(Task<Tree<T>> tree, Task<List<T>> leaves)
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
                    await AccumulateLeavesLenient(Task.FromResult(n.Left), leaves);
                    return leaves.Result;
                });
                var right = Task.Run(async () =>
                {
                    await AccumulateLeavesLenient(Task.FromResult(n.Right), left);
                });

                await Task.WhenAll(left, right);
            }
        }
    }
}