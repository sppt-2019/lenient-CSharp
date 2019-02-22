using System;
using System.Collections.Generic;
using System.Linq;

namespace LenientBenchmark
{
    public class NaryTreeGenerator
    {
        private static readonly Random Rnd = new Random();

        public static Tree<T> CreateTree<T>(int maxChildren, Func<T> valueGenerator, int targetNaryness = 7)
        {
            // Base case: The number of nodes in the subtree is lower than naryness, return a node containing only leaves
            if (maxChildren < targetNaryness)
            {
                var cs = new List<Tree<T>>();
                for (var i = 0; i < maxChildren; i++)
                {
                    cs.Add(new Leaf<T>(valueGenerator()));
                }
                return new NaryNode<T>(cs.ToArray());
            }

            var numbers = GenerateChildCounts(maxChildren, targetNaryness);
            return new NaryNode<T>(numbers.Select(n => CreateTree(n, valueGenerator, targetNaryness)).ToArray());
        }

        private static IEnumerable<int> GenerateChildCounts(int maxChildren, int targetNaryness)
        {
            // Generate a list of integers, each representing the number of nodes in the child
            var numbers = new List<int>();
            while (numbers.Sum() <= maxChildren)
            {
                numbers.Add(Rnd.Next(1, maxChildren / targetNaryness));
            }
            var exceedingNodes = numbers.Sum() - maxChildren;
            while (numbers[numbers.Count - 1] <= exceedingNodes)
            {
                exceedingNodes -= numbers[numbers.Count - 1];
                numbers.RemoveAt(numbers.Count - 1);
            }

            numbers[numbers.Count - 1] -= exceedingNodes;
            return numbers;
        }
    }
}