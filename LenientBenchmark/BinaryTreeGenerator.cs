using System;

namespace LenientBenchmark
{
    public class BinaryTreeGenerator
    {
        private static readonly Random Rnd = new Random();

        public static Tree<T> CreateTree<T>(int maxChildren, Func<T> valueGenerator)
        {
            if(maxChildren == 1)
                return new Leaf<T>(valueGenerator());
            
            // Distribute remaining children randomly over the two subtrees
            var childrenLeft = Rnd.Next(1, maxChildren);
            var childrenRight = maxChildren - childrenLeft;
            
            var l = CreateTree<T>(childrenLeft, valueGenerator);
            var r = CreateTree<T>(childrenRight, valueGenerator);
            
            return new BinaryNode<T>(l, r);
        }

        public static Tree<int> SpawnIntTree()
        {
            var treeLeft = new BinaryNode<int>(
                new BinaryNode<int>(
                    new BinaryNode<int>(new Leaf<int>(10), new BinaryNode<int>(new Leaf<int>(9), new Leaf<int>(8))), 
                    new BinaryNode<int>(
                        new BinaryNode<int>(new Leaf<int>(7), new Leaf<int>(6)),
                        new BinaryNode<int>(
                            new BinaryNode<int>(
                                new Leaf<int>(5), new Leaf<int>(4)), 
                            new Leaf<int>(3)))),
                new BinaryNode<int>(
                    new BinaryNode<int>(new Leaf<int>(2), new Leaf<int>(1)),
                    new Leaf<int>(0)
                )
            );
            var treeRight = new BinaryNode<int>(
                new BinaryNode<int>(
                    new BinaryNode<int>(new Leaf<int>(10), new BinaryNode<int>(new Leaf<int>(9), new Leaf<int>(8))), 
                    new BinaryNode<int>(
                        new BinaryNode<int>(new Leaf<int>(7), new Leaf<int>(6)),
                        new BinaryNode<int>(
                            new BinaryNode<int>(
                                new Leaf<int>(5), new Leaf<int>(4)), 
                            new Leaf<int>(3)))),
                new BinaryNode<int>(
                    new BinaryNode<int>(new Leaf<int>(2), new Leaf<int>(1)),
                    new Leaf<int>(0)
                )
            );
            return new BinaryNode<int>(treeLeft, treeRight);
        }
    }
}