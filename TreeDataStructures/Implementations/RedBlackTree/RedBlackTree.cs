using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new RbNode<TKey, TValue>(key, value);
    }

    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        var ch = newNode;
        // 1) is root
        if (ch == Root)
        {
            ch.Color = RbColor.Black;
            return;
        };
        
        while (ch != Root && IsRed(ch.Parent))
        {
            var par = ch.Parent;
            
            var grand = par.Parent;
            
            
            RbNode<TKey, TValue>? uncle;
            if (par.IsLeftChild)
            {
                uncle = grand.Right;
            }
            else
            {
                uncle = grand.Left;
            }

            // 3) parent is RED & uncle is RED
            if (IsRed(par) && IsRed(uncle))
            {
                par.Color = RbColor.Black;
                uncle?.Color = RbColor.Black;
                ch = grand;
                ch.Color = RbColor.Red;
                continue;
            }

            if (IsRed(par) && IsBlack(uncle))
            {
                if (par.IsLeftChild && ch.IsRightChild)
                {
                    RotateLeft(par);
                    ch = par;
                    par = ch.Parent;
                }
                else if (par.IsRightChild && ch.IsLeftChild)
                {
                    RotateRight(par);
                    ch = par;
                    par = ch.Parent;
                }

                par.Color = RbColor.Black;
                grand.Color = RbColor.Red;
                if (par.IsLeftChild) RotateRight(grand);
                else RotateLeft(grand);
                break;
            }

        }
        Root.Color = RbColor.Black;

    }

    private RbColor colorDel;
    protected override void RemoveNode(RbNode<TKey, TValue> node)
    {
        colorDel = node.Color;
        base.RemoveNode(node);
    }
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {

        if (colorDel == RbColor.Red) return;
        if (colorDel == RbColor.Black && IsRed(child))
        {
            child?.Color = RbColor.Black;
            return;
        }

        if (colorDel == RbColor.Black && (child == null || IsBlack(child)))
        {
            FixAfterDel(parent, child);
        }
        return;
    }

    private void FixAfterDel(RbNode<TKey, TValue> par, RbNode<TKey, TValue>? ch)
    {

        while (ch != Root && IsBlack(ch))
        {
            if (par == null) break;
            RbNode<TKey, TValue>? sib;
            if (ch == par.Left) sib = par.Right;
            else sib = par.Left;

            if (sib == null) break;
            if (IsRed(sib))
            {
                par.Color = RbColor.Red;
                sib.Color = RbColor.Black;
                if (ch == par.Left) RotateLeft(par);
                else RotateRight(par);

                sib = ch == par.Left ? par.Right : par.Left;
                if (sib == null) break;
            }

            if (IsBlack(sib) && IsBlack(sib.Right) && IsBlack(sib.Left))
            {
                sib.Color = RbColor.Red;
                if (IsBlack(par))
                {
                    ch = par;
                    par = ch.Parent;
                    if (par == null) break;
                    continue;
                }
                else
                {
                    par.Color = RbColor.Black;
                    sib.Color = RbColor.Red;
                    break;
                }
            }

            if (ch == par.Left && IsBlack(sib.Right) && IsRed(sib.Left))
            {
                sib.Left?.Color = RbColor.Black;
                sib.Color = RbColor.Red;
                RotateRight(sib);
                sib = par.Right;
            }
            else if (ch == par.Right && IsBlack(sib.Left) && IsRed(sib.Right))
            {
                sib.Right?.Color = RbColor.Black;
                sib.Color = RbColor.Red;
                RotateLeft(sib);
                sib = par.Left;
            }

            if (sib == null) break;
            sib.Color = par.Color;
            par.Color = RbColor.Black;
            if (ch == par.Left)
            {
                sib.Right.Color = RbColor.Black;
                RotateLeft(par);
            }
            else
            {
                sib.Left.Color = RbColor.Black;
                RotateRight(par);
            }

            break;
        }
        if (ch != null) ch.Color = RbColor.Black;
    }

    private bool IsRed(RbNode<TKey, TValue>? node)
    {
        return node != null && node.Color == RbColor.Red;
    }
    private bool IsBlack(RbNode<TKey, TValue>? node)
    {
        return node == null || node.Color == RbColor.Black;
    }

}