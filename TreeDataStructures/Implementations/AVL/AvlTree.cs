using System;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
            return new AvlNode<TKey, TValue>(key, value);
            
    }

    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        BalanceUp(newNode.Parent);
    }

    private void BalanceUp(AvlNode<TKey, TValue>? node)
    {
        var son = node; 
        while (son != null)
        { 
            UpdateHeight(son);
            var bal = Balance(son);
            if (bal < -1)
            {
                LeftHeavy(son);
            }
            else if (bal > 1)
            {
                RightHeavy(son);
            }

            son = son.Parent;
        }
    }

    protected override void OnNodeRemoved(AvlNode<TKey, TValue>? parent, AvlNode<TKey, TValue>? child)
    {
        BalanceUp(parent);
    }

    private int Balance(AvlNode<TKey, TValue> node)
    {
        int rH = 0;
        int lH = 0;
        if (node.Left != null) lH = node.Left.Height;
        if (node.Right != null) rH = node.Right.Height;
        return rH - lH;
    }
    
    private void LeftHeavy(AvlNode<TKey, TValue> node)
    {
        if (node.Left == null) return;
        int bal = Balance(node.Left);
        
        if (bal <= 0)
        { 
            RotateRight(node);
            var newRoot = node.Parent;
            if (newRoot == null) return; 
            UpdateHeight(node);
            UpdateHeight(newRoot);
        }
        else 
        {
            RotateBigRight(node);
            var newRoot = node.Parent;
            if (newRoot == null) return;
            if (newRoot.Left != null) UpdateHeight(newRoot.Left);
            if (newRoot.Right != null)UpdateHeight(newRoot.Right);
            UpdateHeight(newRoot);
        }
    }

    private void RightHeavy(AvlNode<TKey, TValue> node)
    {
        if (node.Right == null) return;

        int bal = Balance(node.Right);

        if (bal >= 0)
        {
            RotateLeft(node);
            var newRoot = node.Parent;
            if (newRoot == null) return;
            UpdateHeight(node);
            UpdateHeight(newRoot);
        }
        else
        {
            RotateBigLeft(node);
            var newRoot = node.Parent;
            if (newRoot == null) return;
            if (newRoot.Left != null) UpdateHeight(newRoot.Left);
            if (newRoot.Right != null)UpdateHeight(newRoot.Right);
            UpdateHeight(newRoot);
        }
    }

    private void UpdateHeight(AvlNode<TKey, TValue> node)
    {
        int lH = node.Left?.Height ?? 0;
        int rH = node.Right?.Height ?? 0;
        node.Height = 1 + Math.Max(lH, rH);
        
    }

}