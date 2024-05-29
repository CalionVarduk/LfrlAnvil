using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="ITreeDictionaryNode{TKey,TValue}" />
public sealed class TreeDictionaryNode<TKey, TValue> : ITreeDictionaryNode<TKey, TValue>
    where TKey : notnull
{
    private List<TreeDictionaryNode<TKey, TValue>> _children;

    /// <summary>
    /// Creates a new <see cref="TreeDictionaryNode{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    public TreeDictionaryNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        _children = new List<TreeDictionaryNode<TKey, TValue>>();
    }

    /// <inheritdoc />
    public TKey Key { get; }

    /// <inheritdoc />
    public TValue Value { get; set; }

    /// <inheritdoc cref="ITreeDictionaryNode{TKey,TValue}.Parent" />
    public TreeDictionaryNode<TKey, TValue>? Parent { get; private set; }

    /// <summary>
    /// Associated <see cref="TreeDictionary{TKey,TValue}"/> instance with this node.
    /// </summary>
    public TreeDictionary<TKey, TValue>? Tree { get; private set; }

    /// <inheritdoc cref="ITreeDictionaryNode{TKey,TValue}.Children" />
    public IReadOnlyList<TreeDictionaryNode<TKey, TValue>> Children => _children;

    ITreeDictionaryNode<TKey, TValue>? ITreeDictionaryNode<TKey, TValue>.Parent => Parent;
    IReadOnlyList<ITreeDictionaryNode<TKey, TValue>> ITreeDictionaryNode<TKey, TValue>.Children => _children;
    ITreeNode<TValue>? ITreeNode<TValue>.Parent => Parent;
    IReadOnlyList<ITreeNode<TValue>> ITreeNode<TValue>.Children => _children;

    /// <summary>
    /// Returns a string representation of this <see cref="TreeDictionaryNode{TKey,TValue}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Key} => {Value}";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetTree(TreeDictionary<TKey, TValue> tree)
    {
        Assume.IsNull( Tree );
        Assume.IsNull( Parent );
        Assume.IsEmpty( _children );

        Tree = tree;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetParent(TreeDictionaryNode<TKey, TValue> parent)
    {
        Assume.IsNotNull( Tree );

        Parent = parent;
        Parent._children.Add( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ClearParent()
    {
        Assume.IsNotNull( Tree );
        Assume.IsNotNull( Parent );

        Parent = null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveFromParent()
    {
        Assume.IsNotNull( Tree );
        Assume.IsNotNull( Parent );

        Parent._children.Remove( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Clear()
    {
        Assume.IsNotNull( Tree );

        Tree = null;
        Parent = null;
        _children.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SwapParentWith(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( Tree );
        Assume.IsNotNull( node.Tree );

        (Parent, node.Parent) = (node.Parent, Parent);
    }

    internal void SwapChildrenWith(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( Tree );
        Assume.IsNotNull( node.Tree );

        (_children, node._children) = (node._children, _children);

        foreach ( var child in _children )
            child.Parent = this;

        foreach ( var child in node._children )
            child.Parent = node;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void ReplaceChildAt(int index, TreeDictionaryNode<TKey, TValue> newChild)
    {
        Assume.IsNotNull( Tree );
        Assume.IsInRange( index, 0, _children.Count - 1 );

        _children[index] = newChild;
    }

    internal void InheritChildrenFrom(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( Tree );
        Assume.IsNotNull( node.Tree );

        _children.AddRange( node._children );

        foreach ( var child in node._children )
            child.Parent = this;

        node._children.Clear();
    }
}
