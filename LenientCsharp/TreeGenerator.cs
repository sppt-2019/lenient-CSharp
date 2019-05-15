using System;

namespace LenientBenchmark
{
    internal class TreeGenerator
    {
        private static readonly Random rnd = new Random();

        public static Tree<T> CreateTree<T>(int maxChildren, Func<T> valueGenerator)
        {
            if(maxChildren == 1)
                return new Leaf<T>(valueGenerator());
            
            // Distribute remaining children randomly over the two subtrees
            var childrenLeft = rnd.Next(1, maxChildren);
            var childrenRight = maxChildren - childrenLeft;
            
            var l = CreateTree(childrenLeft, valueGenerator);
            var r = CreateTree(childrenRight, valueGenerator);
            
            return new Node<T>(l, r, maxChildren);
        }

        public static Tree<T> CreateBinaryTree<T>(int depth, Func<T> valueGenerator)
        {
            if (depth == 1)
                return new Leaf<T>(valueGenerator());
            return new Node<T>(
                CreateBinaryTree(depth - 1, valueGenerator), CreateBinaryTree(depth - 1, valueGenerator), (int) Math.Pow(2, depth));
        }
    }
}