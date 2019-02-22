using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace LenientBenchmark
{
    public abstract class Tree<T>
    {
        
    }

    public class Leaf<T> : Tree<T>
    {
        public T Value { get; set; }

        public Leaf(T value)
        {
            Value = value;
        }
    }

    public class BinaryNode<T> : Tree<T>
    {
        public Tree<T> Left { get; set; }
        public Tree<T> Right { get; set; }

        public BinaryNode(Tree<T> left, Tree<T> right)
        {
            Left = left;
            Right = right;
        }
    }

    public class NaryNode<T> : Tree<T>
    {
        public IEnumerable<Tree<T>> Children { get; set; }

        public NaryNode(params Tree<T>[] children)
        {
            Children = children;
        }
    }
}