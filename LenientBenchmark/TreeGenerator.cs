using System;

namespace LenientBenchmark
{
    public class TreeGenerator
    {
        private static readonly Random rnd = new Random();

        public static Tree<T> CreateTree<T>(int maxChildren, Func<T> valueGenerator)
        {
            if(maxChildren == 1)
                return new Leaf<T>(valueGenerator());
            
            // Distribute remaining children randomly over the two subtrees
            var childrenLeft = rnd.Next(1, maxChildren);
            var childrenRight = maxChildren - childrenLeft;
            
            var l = CreateTree<T>(childrenLeft, valueGenerator);
            var r = CreateTree<T>(childrenRight, valueGenerator);
            
            return new Node<T>(l, r, maxChildren);
        }

        public static Tree<T> CreateBinaryTree<T>(int height, Func<T> valueGenerator)
        {
            return null;
        }

        public static Tree<int> SpawnIntTree()
        {
            var treeLeft = new Node<int>(
                new Node<int>(
                    new Node<int>(new Leaf<int>(10), new Node<int>(new Leaf<int>(9), new Leaf<int>(8))), 
                    new Node<int>(
                        new Node<int>(new Leaf<int>(7), new Leaf<int>(6)),
                        new Node<int>(
                            new Node<int>(
                                new Leaf<int>(5), new Leaf<int>(4)), 
                            new Leaf<int>(3)))),
                new Node<int>(
                    new Node<int>(new Leaf<int>(2), new Leaf<int>(1)),
                    new Leaf<int>(0)
                )
            );
            var treeRight = new Node<int>(
                new Node<int>(
                    new Node<int>(new Leaf<int>(10), new Node<int>(new Leaf<int>(9), new Leaf<int>(8))), 
                    new Node<int>(
                        new Node<int>(new Leaf<int>(7), new Leaf<int>(6)),
                        new Node<int>(
                            new Node<int>(
                                new Leaf<int>(5), new Leaf<int>(4)), 
                            new Leaf<int>(3)))),
                new Node<int>(
                    new Node<int>(new Leaf<int>(2), new Leaf<int>(1)),
                    new Leaf<int>(0)
                )
            );
            return new Node<int>(treeLeft, treeRight);
        }
    }
}