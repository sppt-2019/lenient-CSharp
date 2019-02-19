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

    public class Node<T> : Tree<T>
    {
        public Tree<T> Left { get; set; }
        public Tree<T> Right { get; set; }

        public Node(Tree<T> left, Tree<T> right)
        {
            Left = left;
            Right = right;
        }
    }
}