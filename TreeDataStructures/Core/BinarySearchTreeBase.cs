using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Interfaces;

namespace TreeDataStructures.Core;

public abstract class BinarySearchTreeBase<TKey, TValue, TNode>(IComparer<TKey>? comparer = null)
    : ITree<TKey, TValue>
    where TNode : Node<TKey, TValue, TNode>
{
    protected TNode? Root;

    public IComparer<TKey> Comparer { get; protected set; } =
        comparer ?? Comparer<TKey>.Default; // use it to compare Keys

    public int Count { get; protected set; }
    public bool IsReadOnly => false;


    public ICollection<TKey> Keys => InOrder().Select(e => e.Key).ToArray();

    public ICollection<TValue> Values => InOrder().Select(e => e.Value).ToArray();


    public virtual void Add(TKey key, TValue value)
    {
        var current = Root;
        TNode? parent = null;

        while (current != null)
        {
            parent = current;
            var cmp = Comparer.Compare(key, current.Key);
            switch (cmp)
            {
                case < 0:
                    current = current.Left;
                    break;
                case > 0:
                    current = current.Right;
                    break;
                default:
                    current.Value = value;
                    return;
            }
        }

        var node = CreateNode(key, value);
        if (Root == null)
        {
            Root = node;
        }
        else
        {
            node.Parent = parent;

            if (Comparer.Compare(node.Key, parent.Key) < 0)
                parent.Left = node;
            else
                parent.Right = node;
        }

        OnNodeAdded(node);
        Count++;
    }


    public virtual bool Remove(TKey key)
    {
        var node = FindNode(key);
        if (node == null) return false;

        RemoveNode(node);
        Count--;
        return true;
    }

    public virtual bool ContainsKey(TKey key)
    {
        return FindNode(key) != null;
    }

    public virtual bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            value = node.Value;
            return true;
        }

        value = default;
        return false;
    }

    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();
        set => Add(key, value);
    }


    public IEnumerable<TreeEntry<TKey, TValue>> InOrder()
    {
        return new TreeIterator(this, TraversalStrategy.InOrder);
    }

    public IEnumerable<TreeEntry<TKey, TValue>> InOrderReverse()
    {
        return new TreeIterator(this, TraversalStrategy.InOrderReverse);
    }

    public IEnumerable<TreeEntry<TKey, TValue>> PreOrder()
    {
        return new TreeIterator(this, TraversalStrategy.PreOrder);
    }

    public IEnumerable<TreeEntry<TKey, TValue>> PostOrder()
    {
        return new TreeIterator(this, TraversalStrategy.PostOrder);
    }

    public IEnumerable<TreeEntry<TKey, TValue>> PreOrderReverse()
    {
        return new TreeIterator(this, TraversalStrategy.PreOrderReverse);
    }

    public IEnumerable<TreeEntry<TKey, TValue>> PostOrderReverse()
    {
        return new TreeIterator(this, TraversalStrategy.PostOrderReverse);
    }


    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return InOrder().Select(e => new KeyValuePair<TKey, TValue>(e.Key, e.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Root = null;
        Count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        this.ToList().CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    protected virtual int GetNodeDepth(TNode node)
    {
        int depth = 0;
        while (node.Parent != null)
        {
            node = node.Parent;
            depth++;
        }
        return depth;
    }

    protected virtual void RemoveNode(TNode node)
    {
        if (node.Left == null)
        {
            Transplant(node, node.Right);
            OnNodeRemoved(node.Parent, node.Right);
        }
        else if (node.Right == null)
        {
            Transplant(node, node.Left);
            OnNodeRemoved(node.Parent, node.Left);
        }
        else
        {
            var current = node.Right;
            while (current.Left != null) current = current.Left;

            var parent = current.Parent;
            var child = current.Right;

            // cur.Left = NULL
            if (current != node
                    .Right) // проверка на случай, если цикл выполнился 0 раз, т.е у нас самый левый потомок правого поддерева - это самый первый правый потомок
            {
                Transplant(current, current.Right);
                current.Right = node.Right;
                current.Right.Parent = current;
            }
            else
            {
                parent = current;
            }

            Transplant(node, current);
            current.Left = node.Left;
            current.Left.Parent = current;
            OnNodeRemoved(parent, child);
            node.Parent = null;
            node.Right = null;
            node.Left = null;
        }
    }

    private struct TreeIterator : IEnumerable<TreeEntry<TKey, TValue>>, IEnumerator<TreeEntry<TKey, TValue>>
    {
        private readonly BinarySearchTreeBase<TKey, TValue, TNode> _tree;
        private readonly TraversalStrategy _strategy;
        private TNode? _current;
        private bool _isStarted;

        public TreeIterator(BinarySearchTreeBase<TKey, TValue, TNode> tree, TraversalStrategy strategy)
        {
            _tree = tree;
            _strategy = strategy;
            _current = null;
            _isStarted = false;
        }

        public IEnumerator<TreeEntry<TKey, TValue>> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public TreeEntry<TKey, TValue> Current
        {
            get
            {
                if (_current == null) throw new InvalidOperationException();
                var depth = _tree.GetNodeDepth(_current);
                return new TreeEntry<TKey, TValue>(_current.Key, _current.Value, depth);
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_tree.Root == null) return false;
            if (!_isStarted)
            {
                _isStarted = true;
                switch (_strategy)
                {
                    case TraversalStrategy.InOrder: // ЛКП
                        _current = MoveLeft(_tree.Root);
                        return _current != null;
                    case TraversalStrategy.InOrderReverse: // ПКЛ
                        _current = MoveRight(_tree.Root);
                        return _current != null;
                    case TraversalStrategy.PostOrder: // ЛПК
                        _current = FindFirst(_tree.Root);
                        return _current != null;
                    case TraversalStrategy.PostOrderReverse: // ПЛК
                        _current = FindFirstReverse(_tree.Root);
                        return _current != null;
 
                    case TraversalStrategy.PreOrder: // КЛП
                        _current = _tree.Root;
                        return _current != null;
                    case TraversalStrategy.PreOrderReverse: // КПЛ
                        _current = _tree.Root;
                        return _current != null;
                }
            }

            _current = Next(_current, _strategy);
            return _current != null;
        }

        public void Reset()
        {
            _current = null;
            _isStarted = false;
        }

        public void Dispose()
        {
        }


        private static TNode MoveLeft(TNode node)
        {
            while (node.Left != null) node = node.Left;
            return node;
        }

        private static TNode MoveRight(TNode node)
        {
            while (node.Right != null) node = node.Right;
            return node;
        }

        private static TNode? MoveUpLeft(TNode node)
        {
            while (node.Parent != null && !node.IsLeftChild) node = node.Parent;
            return node.Parent;
        }

        private static TNode? MoveUpRight(TNode node)
        {
            while (node.Parent != null && !node.IsRightChild) node = node.Parent;
            return node.Parent;
        }

        private static TNode? Next(TNode? node, TraversalStrategy strategy)
        {
            if (node is null) return null;

            switch (strategy)
            {
                case TraversalStrategy.InOrder:
                    if (node.Right != null) return MoveLeft(node.Right);
                    return MoveUpLeft(node);

                case TraversalStrategy.InOrderReverse:
                    if (node.Left != null) return MoveRight(node.Left);
                    return MoveUpRight(node);
                case TraversalStrategy.PostOrder:
                    if (node.IsRightChild) return node.Parent;
                    if (node.IsLeftChild)
                    {
                        if (node.Parent != null && node.Parent.Right != null)
                            return FindFirst(node.Parent.Right);
                        if (node.Parent != null && node.Parent.Right == null)
                            return node.Parent;
                    }

                    break;
                case TraversalStrategy.PostOrderReverse:
                    if (node.IsLeftChild) return node.Parent;
                    if (node.IsRightChild)
                    {
                        if (node.Parent?.Left != null) return FindFirst(node.Parent.Left);
                        return node.Parent;
                    }
                    break;
                    
                    
                    
                case TraversalStrategy.PreOrder:
                    if (node.Left != null) return node.Left;
                    if (node.Left == null && node.Right != null) return node.Right;

                    node = MoveUpLeft(node);
                    if (node == null) return null;
                    while (node != null && node.Right == null) node = MoveUpLeft(node);
                    return node?.Right;

                case TraversalStrategy.PreOrderReverse:
                    if (node.Right != null) return node.Right;
                    if (node.Left != null) return node.Left;

                    node = MoveUpRight(node);
                    if (node == null) return null;
                    while (node != null && node.Left == null) node = MoveUpRight(node);
                    return node?.Left;

                    break;

            }

            return null;
        }

        private static TNode FindFirst(TNode node)
        {
            node = MoveLeft(node);
            while (node.Right != null) node = MoveLeft(node.Right);
            return node;
        }

        private static TNode FindFirstReverse(TNode node)
        {
            node = MoveRight(node);
            while (node.Left != null) node = MoveRight(node.Left);
            return node;
        }
    }


    #region Hooks

    /// <summary>
    ///     Вызывается после успешной вставки
    /// </summary>
    /// <param name="newNode">Узел, который встал на место</param>
    protected virtual void OnNodeAdded(TNode newNode)
    {
    }

    /// <summary>
    ///     Вызывается после удаления.
    /// </summary>
    /// <param name="parent">Узел, чей ребенок изменился</param>
    /// <param name="child">Узел, который встал на место удаленного</param>
    protected virtual void OnNodeRemoved(TNode? parent, TNode? child)
    {
    }

    #endregion


    #region Helpers

    protected abstract TNode CreateNode(TKey key, TValue value);


    protected TNode? FindNode(TKey key)
    {
        var current = Root;
        while (current != null)
        {
            var cmp = Comparer.Compare(key, current.Key);
            if (cmp == 0) return current;

            current = cmp < 0 ? current.Left : current.Right;
        }

        return null;
    }

    protected void RotateLeft(TNode x)
    {
        var y = x.Right;
        if (y == null) return;
        x.Right = y.Left;
        y.Left?.Parent = x;
        Transplant(x, y);
        y.Left = x;
        x.Parent = y;
    }

    protected void RotateRight(TNode y)
    {
        var x = y.Left;
        if (x == null) return;

        y.Left = x.Right;
        if (x.Right != null) x.Right.Parent = y;
        Transplant(y, x);
        x.Right = y;
        y.Parent = x;
    }

    protected void RotateBigLeft(TNode x)
    {
        if (x.Right == null) return;

        RotateRight(x.Right);
        RotateLeft(x);
    }

    protected void RotateBigRight(TNode y)
    {
        if (y.Left == null) return;
        RotateLeft(y.Left);
        RotateRight(y);
    }

    protected void RotateDoubleLeft(TNode x)
    {
        var rgt = x.Right;
        if (rgt == null) return;
        RotateLeft(x);
        RotateLeft(rgt);
    }

    protected void RotateDoubleRight(TNode y)
    {
        var lft = y.Left;
        if (lft == null) return;
        RotateRight(y);
        RotateRight(lft);
    }

    protected void Transplant(TNode u, TNode? v)
    {
        if (u.Parent == null)
            Root = v;
        else if (u.IsLeftChild)
            u.Parent.Left = v;
        else
            u.Parent.Right = v;

        v?.Parent = u.Parent;
    }

    #endregion
    private enum TraversalStrategy
    {
        InOrder,
        InOrderReverse,
        PreOrder,
        PreOrderReverse,
        PostOrder,
        PostOrderReverse
    }
}