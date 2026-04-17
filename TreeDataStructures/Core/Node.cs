namespace TreeDataStructures.Core;

public class Node<TKey, TValue, TNode>(TKey key, TValue value) where TNode : Node<TKey, TValue, TNode>
{
    public TKey Key { get; set; } = key;
    public TValue Value { get; set; } = value;

    public TNode? Left { get; set; }

    //get == public TKey GetKey() {return _key}
    //set == public void SetKey(Tkey value) {_key = value}
    public TNode? Right { get; set; }
    public TNode? Parent { get; set; }

    public bool IsLeftChild => Parent != null && Parent.Left == this;
    public bool IsRightChild => Parent != null && Parent.Right == this;
}