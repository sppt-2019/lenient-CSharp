using Microsoft.VisualBasic.CompilerServices;
using System;

namespace LenientBenchmark
{
    public abstract class Tree<T>
    {
        public abstract int NumberOfLeaves { get; }
    }

    public class Leaf<T> : Tree<T>
    {
        public override int NumberOfLeaves { get => 1; }
        public T Value { get; set; }

        public Leaf(T value)
        {
            Value = value;
        }
    }

    public class Node<T> : Tree<T>
    {
        public override int NumberOfLeaves => noLeaves != null ? noLeaves.Value : throw new NullReferenceException("'noLeaves' has not been set");
        int? noLeaves;

        public Tree<T> Left { get; set; }
        public Tree<T> Right { get; set; }

        public Node(Tree<T> left, Tree<T> right)
        {
            Left = left;
            Right = right;
        }

        public Node(Tree<T> left, Tree<T> right, int noLeaves)
        {
            Left = left;
            Right = right;
            this.noLeaves = noLeaves;
        }
    }
}