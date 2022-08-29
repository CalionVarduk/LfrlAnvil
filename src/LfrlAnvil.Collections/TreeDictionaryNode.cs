using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections;

public sealed class TreeDictionaryNode<TKey, TValue> : ITreeDictionaryNode<TKey, TValue>
    where TKey : notnull
{
    private List<TreeDictionaryNode<TKey, TValue>> _children;

    public TreeDictionaryNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        _children = new List<TreeDictionaryNode<TKey, TValue>>();
    }

    public TKey Key { get; }
    public TValue Value { get; set; }
    public TreeDictionaryNode<TKey, TValue>? Parent { get; private set; }
    public TreeDictionary<TKey, TValue>? Tree { get; private set; }
    public IReadOnlyList<TreeDictionaryNode<TKey, TValue>> Children => _children;

    ITreeDictionaryNode<TKey, TValue>? ITreeDictionaryNode<TKey, TValue>.Parent => Parent;
    IReadOnlyList<ITreeDictionaryNode<TKey, TValue>> ITreeDictionaryNode<TKey, TValue>.Children => _children;
    ITreeNode<TValue>? ITreeNode<TValue>.Parent => Parent;
    IReadOnlyList<ITreeNode<TValue>> ITreeNode<TValue>.Children => _children;

    [Pure]
    public override string ToString()
    {
        return $"{Key} => {Value}";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetTree(TreeDictionary<TKey, TValue> tree)
    {
        Assume.IsNull( Tree, nameof( Tree ) );
        Assume.IsNull( Parent, nameof( Parent ) );
        Assume.IsEmpty( _children, nameof( _children ) );

        Tree = tree;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetParent(TreeDictionaryNode<TKey, TValue> parent)
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );

        Parent = parent;
        Parent._children.Add( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ClearParent()
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );
        Assume.IsNotNull( Parent, nameof( Parent ) );

        Parent = null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveFromParent()
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );
        Assume.IsNotNull( Parent, nameof( Parent ) );

        Parent._children.Remove( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );

        Tree = null;
        Parent = null;
        _children.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SwapParentWith(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );
        Assume.IsNotNull( node.Tree, nameof( node ) + '.' + nameof( Tree ) );

        (Parent, node.Parent) = (node.Parent, Parent);
    }

    internal void SwapChildrenWith(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );
        Assume.IsNotNull( node.Tree, nameof( node ) + '.' + nameof( Tree ) );

        (_children, node._children) = (node._children, _children);

        foreach ( var child in _children )
            child.Parent = this;

        foreach ( var child in node._children )
            child.Parent = node;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReplaceChildAt(int index, TreeDictionaryNode<TKey, TValue> newChild)
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );
        Assume.IsInRange( index, 0, _children.Count - 1, nameof( index ) );

        _children[index] = newChild;
    }

    internal void InheritChildrenFrom(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( Tree, nameof( Tree ) );
        Assume.IsNotNull( node.Tree, nameof( node ) + '.' + nameof( Tree ) );

        _children.AddRange( node._children );

        foreach ( var child in node._children )
            child.Parent = this;

        node._children.Clear();
    }
}
