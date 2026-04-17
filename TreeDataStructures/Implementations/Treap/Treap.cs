using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
{
    /// <summary>
    ///     Разрезает дерево с корнем <paramref name="root" /> на два поддерева:
    ///     Left: все ключи <= <paramref name="key" />
    ///     Right: все ключи > <paramref name="key" />
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(
        TreapNode<TKey, TValue>? root, TKey key)
    {
        if (root == null) return (null, null);
        if (Comparer.Compare(root.Key, key) <= 0)
        {
            var (leftTree, rightTree) = Split(root.Right, key);
            root.Right = leftTree;
            if (leftTree != null) leftTree.Parent = root;
            if (rightTree != null) rightTree.Parent = null;
            return (root, rightTree);
        }
        else
        {
            var (leftTree, rightTree) = Split(root.Left, key);
            root.Left = rightTree;
            
            // if (rightTree != null) 
            rightTree?.Parent = root;
            // if (leftTree != null)
            leftTree?.Parent = null;
            return (leftTree, root);
        }
    }

    /// <summary>
    ///     Сливает два дерева в одно.
    ///     Важное условие: все ключи в <paramref name="left" /> должны быть меньше ключей в <paramref name="right" />.
    ///     Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null) return right;
        if (right == null) return left;

        if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            if (left.Right != null) left.Right.Parent = left;
            return left;
        }
        else
        {
            right.Left = Merge(left, right.Left);
            if (right.Left != null) right.Left.Parent = right;
            return right;
        }
    }


    public override void Add(TKey key, TValue value)
    {
        // ArgumentNullException.ThrowIfNull(key);
        var exists = FindNode(key);
        if (exists != null)
        {
            exists.Value = value;
            return;
        }

        var node = CreateNode(key, value);
        var (leftTree, rightTree) = Split(Root, key);
        var temp = Merge(leftTree, node);
        Root = Merge(temp, rightTree);
        Root?.Parent = null;
        Count++;
    }

    public override bool Remove(TKey key)
    {
        // ArgumentNullException.ThrowIfNull(key);
        var node = FindNode(key);
        if (node == null) return false;
        
        var prev = GetPrevious(node);
        if (prev == null)
        {
            var (mid, right) = Split(Root, key);
            Root = right;
        }
        else
        {
            var (left, rest) = Split(Root, prev.Key);
            var (mid, right) = Split(rest, key);

            Root = Merge(left, right);
        }

        if (Root != null) Root.Parent = null;
        
        node.Left = null;
        node.Parent = null;
        node.Right = null;
        Count--;
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        try
        {
            return new TreapNode<TKey, TValue>(key, value);
        }
        catch (OutOfMemoryException exception)
        {
            throw new OutOfMemoryException("bruh lack of memory: ", exception);
        }
    }

    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode)
    {
    }

    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child)
    {
    }

    private TreapNode<TKey, TValue>? GetPrevious(TreapNode<TKey, TValue> node)
    {
        if (node.Left != null)
        {
            var cur = node.Left;
            while (cur.Right != null)
            {
                cur = cur.Right;
            }

            return cur;
        }

        var par = node.Parent;
        var curr = node;
        while (par != null && curr == par.Left)
        {
            curr = par;
            par = par.Parent;
        }
        return par;

    }
}