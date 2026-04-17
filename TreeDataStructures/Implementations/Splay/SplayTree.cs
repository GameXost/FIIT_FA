using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new BstNode<TKey, TValue>(key, value);
    }

    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        var x = newNode;
        while (x.Parent != null)
        {
            SplayStep(x);
        }
    }

    public override bool Remove(TKey key)
    {
        var nodeToDelete = FindNGetNode(key);
        if (nodeToDelete == null )return false;
        while (nodeToDelete.Parent != null)
        {
            SplayStep(nodeToDelete);
        }
        
        if (Comparer.Compare(key, nodeToDelete.Key) != 0)
        {
            return false;
        }
        RemoveNode(nodeToDelete);
        Count--;
        return true;
    }

    protected override void RemoveNode(BstNode<TKey, TValue> node)
    {
        var L = node.Left;
        var R = node.Right;
        
        node.Left = null;
        node.Right = null;
        
        L?.Parent = null;
        R?.Parent = null;
        if (L == null && R == null)
        {
            Root = null;
        }
        else if (L == null)
        {
            Root = R;
        }
        else if (R == null)
        {
            Root = L;
        }
        else
        {
            var minRight = R;
            while (minRight.Left != null)
            {
                minRight = minRight.Left;
            }

            while (minRight.Parent != null)
            {
                SplayStep(minRight);
            }

            minRight.Left = L;
            L.Parent = minRight;
            Root = minRight;
        }
        
    }


    public override bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNGetNode(key);
        if (node == null)
        {
            value = default;
            return false;
        }
        while (node.Parent != null)
        {
            SplayStep(node);
        }

        if (Comparer.Compare(key, node.Key) == 0)
        {
            value = node.Value;
            return true;
        }
        
        value = default;
        return false;
    }

    private void SplayStep(BstNode<TKey, TValue> node)
    {
        var x = node;
        var y = x.Parent;
        var z = y?.Parent;
        
        //case 0 x is the root
        if (y == null) return;
        
        // case 1: x's parent is the root -> then one rotation
        if (z == null)
        {
            if (x.IsLeftChild) RotateRight(y);
            else RotateLeft(y);
            return;
        }
        
        //case 2: x and y are ont the straight line both right or left child
        if (x.IsLeftChild && y.IsLeftChild) RotateDoubleRight(z);
        else if (x.IsRightChild && y.IsRightChild) RotateDoubleLeft(z);
        
        // case 3: zig-zag line one node is right child another is left, or vice versa
        else if (x.IsLeftChild && y.IsRightChild) RotateBigLeft(z);
        else if (x.IsRightChild && y.IsLeftChild) RotateBigRight(z);
        return;
    }

    private BstNode<TKey, TValue>? FindNGetNode(TKey key)
    {
        var current = Root;
        var lastNode = current;
        while (current != null)
        {
            lastNode = current;
            var cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) return current;
            current = cmp < 0 ? current.Left : current.Right;
        }

        return lastNode;
    }
}


