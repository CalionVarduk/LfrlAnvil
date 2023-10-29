using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Collections.Exceptions;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections;

public class TreeDictionary<TKey, TValue> : ITreeDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TreeDictionaryNode<TKey, TValue>> _map;
    private TreeDictionaryNode<TKey, TValue>? _root;

    public TreeDictionary()
        : this( EqualityComparer<TKey>.Default ) { }

    public TreeDictionary(IEqualityComparer<TKey> comparer)
    {
        _map = new Dictionary<TKey, TreeDictionaryNode<TKey, TValue>>( comparer );
        _root = null;
    }

    public TValue this[TKey key]
    {
        get => _map[key].Value;
        set
        {
            if ( _map.TryGetValue( key, out var node ) )
            {
                node.Value = value;
                return;
            }

            Add( key, value );
        }
    }

    public TreeDictionaryNode<TKey, TValue>? Root => _root;
    public int Count => _map.Count;
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public IEnumerable<TKey> Keys => _map.Keys;
    public IEnumerable<TValue> Values => _map.Select( static kv => kv.Value.Value );
    public IEnumerable<TreeDictionaryNode<TKey, TValue>> Nodes => (Root?.VisitManyWithSelf( static n => n.Children )).EmptyIfNull();

    ITreeDictionaryNode<TKey, TValue>? IReadOnlyTreeDictionary<TKey, TValue>.Root => Root;
    IEnumerable<ITreeDictionaryNode<TKey, TValue>> IReadOnlyTreeDictionary<TKey, TValue>.Nodes => Nodes;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => _map.Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToList();

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
        ((ICollection<KeyValuePair<TKey, TreeDictionaryNode<TKey, TValue>>>)_map).IsReadOnly;

    public TreeDictionaryNode<TKey, TValue> SetRoot(TKey key, TValue value)
    {
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );
        SetRootImpl( node );
        return node;
    }

    public void SetRoot(TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNewNode( node );
        SetRootImpl( node );
    }

    public TreeDictionaryNode<TKey, TValue> Add(TKey key, TValue value)
    {
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );
        AddImpl( node );
        return node;
    }

    public void Add(TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNewNode( node );
        AddImpl( node );
    }

    public TreeDictionaryNode<TKey, TValue> AddTo(TKey parentKey, TKey key, TValue value)
    {
        var parentNode = _map[parentKey];
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );
        AddToImpl( parentNode, node );
        return node;
    }

    public void AddTo(TKey parentKey, TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNewNode( node );
        var parentNode = _map[parentKey];
        AddToImpl( parentNode, node );
    }

    public TreeDictionaryNode<TKey, TValue> AddTo(TreeDictionaryNode<TKey, TValue> parent, TKey key, TValue value)
    {
        AssertNode( parent );
        var node = new TreeDictionaryNode<TKey, TValue>( key, value );
        AddToImpl( parent, node );
        return node;
    }

    public void AddTo(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( parent );
        AssertNewNode( node );
        AddToImpl( parent, node );
    }

    public TreeDictionaryNode<TKey, TValue> AddSubtree(ITreeDictionaryNode<TKey, TValue> node)
    {
        return AddSubtreeToImpl( null, node );
    }

    public TreeDictionaryNode<TKey, TValue> AddSubtreeTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node)
    {
        var parentNode = _map[parentKey];
        return AddSubtreeToImpl( parentNode, node );
    }

    public TreeDictionaryNode<TKey, TValue> AddSubtreeTo(
        TreeDictionaryNode<TKey, TValue> parent,
        ITreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( parent );
        return AddSubtreeToImpl( parent, node );
    }

    public bool Remove(TKey key)
    {
        return RemoveImpl( key ) is not null;
    }

    public void Remove(TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( node );
        Remove( node.Key );
    }

    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        var node = RemoveImpl( key );
        if ( node is null )
        {
            removed = default;
            return false;
        }

        removed = node.Value;
        return true;
    }

    public int RemoveSubtree(TKey key)
    {
        return RemoveSubtreeImpl( key );
    }

    public int RemoveSubtree(TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( node );
        return RemoveSubtreeImpl( node.Key );
    }

    public void Swap(TKey firstKey, TKey secondKey)
    {
        var firstNode = _map[firstKey];
        var secondNode = _map[secondKey];
        SwapImpl( firstNode, secondNode );
    }

    public void Swap(TreeDictionaryNode<TKey, TValue> firstNode, TreeDictionaryNode<TKey, TValue> secondNode)
    {
        AssertNode( firstNode );
        AssertNode( secondNode );
        SwapImpl( firstNode, secondNode );
    }

    public TreeDictionaryNode<TKey, TValue> MoveTo(TKey parentKey, TKey key)
    {
        var parentNode = _map[parentKey];
        var node = _map[key];
        MoveToImpl( parentNode, node );
        return node;
    }

    public void MoveTo(TKey parentKey, TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( node );
        var parentNode = _map[parentKey];
        MoveToImpl( parentNode, node );
    }

    public TreeDictionaryNode<TKey, TValue> MoveTo(TreeDictionaryNode<TKey, TValue> parent, TKey key)
    {
        AssertNode( parent );
        var node = _map[key];
        MoveToImpl( parent, node );
        return node;
    }

    public void MoveTo(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( parent );
        AssertNode( node );
        MoveToImpl( parent, node );
    }

    public TreeDictionaryNode<TKey, TValue> MoveSubtreeTo(TKey parentKey, TKey key)
    {
        var parentNode = _map[parentKey];
        var node = _map[key];
        MoveSubtreeToImpl( parentNode, node );
        return node;
    }

    public void MoveSubtreeTo(TKey parentKey, TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( node );
        var parentNode = _map[parentKey];
        MoveSubtreeToImpl( parentNode, node );
    }

    public TreeDictionaryNode<TKey, TValue> MoveSubtreeTo(TreeDictionaryNode<TKey, TValue> parent, TKey key)
    {
        AssertNode( parent );
        var node = _map[key];
        MoveSubtreeToImpl( parent, node );
        return node;
    }

    public void MoveSubtreeTo(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        AssertNode( parent );
        AssertNode( node );
        MoveSubtreeToImpl( parent, node );
    }

    public void Clear()
    {
        foreach ( var (_, node) in _map )
            node.Clear();

        _map.Clear();
        _root = null;
    }

    [Pure]
    public TreeDictionary<TKey, TValue> CreateSubtree(TKey key)
    {
        return _map.TryGetValue( key, out var root )
            ? root.CreateTree()
            : new TreeDictionary<TKey, TValue>( Comparer );
    }

    [Pure]
    public TreeDictionaryNode<TKey, TValue>? GetNode(TKey key)
    {
        return _map.GetValueOrDefault( key );
    }

    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result)
    {
        if ( _map.TryGetValue( key, out var node ) )
        {
            result = node.Value;
            return true;
        }

        result = default;
        return false;
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _map
            .Select( static kv => KeyValuePair.Create( kv.Key, kv.Value.Value ) )
            .GetEnumerator();
    }

    private void SetRootImpl(TreeDictionaryNode<TKey, TValue> newRoot)
    {
        _map.Add( newRoot.Key, newRoot );
        newRoot.SetTree( this );
        _root?.SetParent( newRoot );
        _root = newRoot;
    }

    private void AddToImpl(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        _map.Add( node.Key, node );
        node.SetTree( this );
        node.SetParent( parent );
    }

    private void AddImpl(TreeDictionaryNode<TKey, TValue> node)
    {
        if ( _root is null )
            SetRootImpl( node );
        else
            AddToImpl( _root, node );
    }

    private TreeDictionaryNode<TKey, TValue> AddSubtreeToImpl(
        TreeDictionaryNode<TKey, TValue>? parent,
        ITreeDictionaryNode<TKey, TValue> node)
    {
        AssertSubtreeAddition( node );

        var subtreeRoot = new TreeDictionaryNode<TKey, TValue>( node.Key, node.Value );
        if ( parent is null )
            AddImpl( subtreeRoot );
        else
            AddToImpl( parent, subtreeRoot );

        foreach ( var descendant in node.VisitDescendants() )
        {
            if ( ContainsKey( descendant.Key ) )
                continue;

            Assume.IsNotNull( descendant.Parent );
            AddTo( descendant.Parent.Key, descendant.Key, descendant.Value );
        }

        return subtreeRoot;
    }

    private TreeDictionaryNode<TKey, TValue>? RemoveImpl(TKey key)
    {
        if ( ! _map.Remove( key, out var node ) )
            return null;

        if ( ReferenceEquals( node, _root ) )
            FixRootRemoval();
        else
            FixSubNodeRemoval( node );

        node.Clear();
        return node;
    }

    private void FixRootRemoval()
    {
        if ( Count == 0 )
        {
            _root = null;
            return;
        }

        Assume.IsNotNull( _root );
        var oldRoot = _root;
        _root = oldRoot.Children[0];
        _root.ClearParent();

        for ( var i = 1; i < oldRoot.Children.Count; ++i )
            oldRoot.Children[i].SetParent( _root );
    }

    private static void FixSubNodeRemoval(TreeDictionaryNode<TKey, TValue> node)
    {
        node.RemoveFromParent();

        for ( var i = 0; i < node.Children.Count; ++i )
        {
            Assume.IsNotNull( node.Parent );
            node.Children[i].SetParent( node.Parent! );
        }
    }

    private int RemoveSubtreeImpl(TKey key)
    {
        if ( _root is null )
            return 0;

        var oldCount = Count;

        if ( Comparer.Equals( _root.Key, key ) )
        {
            Clear();
            return oldCount;
        }

        if ( ! _map.Remove( key, out var parent ) )
            return 0;

        parent.RemoveFromParent();

        foreach ( var descendant in parent.VisitDescendants() )
        {
            _map.Remove( descendant.Key );
            descendant.Clear();
        }

        parent.Clear();
        return oldCount - Count;
    }

    private void SwapImpl(TreeDictionaryNode<TKey, TValue> first, TreeDictionaryNode<TKey, TValue> second)
    {
        if ( ReferenceEquals( first, second ) )
            return;

        if ( ReferenceEquals( first, _root ) )
        {
            SwapWithRoot( second );
            return;
        }

        if ( ReferenceEquals( second, _root ) )
        {
            SwapWithRoot( first );
            return;
        }

        if ( ReferenceEquals( first.Parent, second.Parent ) )
        {
            SwapWithSameParent( first, second );
            return;
        }

        SwapWithDifferentParents( first, second );
    }

    private void SwapWithRoot(TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( _root );
        Assume.IsNotNull( node.Parent );
        var oldRoot = _root;
        var nodeIndex = node.Parent.GetChildIndex( node );

        node.Parent.ReplaceChildAt( nodeIndex, oldRoot );
        node.SwapParentWith( oldRoot );
        node.SwapChildrenWith( oldRoot );

        _root = node;
    }

    private static void SwapWithSameParent(TreeDictionaryNode<TKey, TValue> first, TreeDictionaryNode<TKey, TValue> second)
    {
        Assume.IsNotNull( first.Parent );
        var parent = first.Parent;
        var firstNodeIndex = parent.GetChildIndex( first );
        var secondNodeIndex = parent.GetChildIndex( second );

        parent.ReplaceChildAt( firstNodeIndex, second );
        parent.ReplaceChildAt( secondNodeIndex, first );
        first.SwapChildrenWith( second );
    }

    private static void SwapWithDifferentParents(TreeDictionaryNode<TKey, TValue> first, TreeDictionaryNode<TKey, TValue> second)
    {
        Assume.IsNotNull( first.Parent );
        Assume.IsNotNull( second.Parent );
        var firstNodeIndex = first.Parent.GetChildIndex( first );
        var secondNodeIndex = second.Parent.GetChildIndex( second );

        first.Parent.ReplaceChildAt( firstNodeIndex, second );
        second.Parent.ReplaceChildAt( secondNodeIndex, first );
        first.SwapParentWith( second );
        first.SwapChildrenWith( second );
    }

    private void MoveToImpl(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        if ( ReferenceEquals( parent, node.Parent ) )
            return;

        if ( ReferenceEquals( parent, node ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.NodeCannotBeMovedToItself ) );

        if ( ReferenceEquals( node, _root ) )
        {
            MoveRootTo( parent );
            return;
        }

        MoveNodeWithParentTo( parent, node );
    }

    private void MoveRootTo(TreeDictionaryNode<TKey, TValue> parent)
    {
        Assume.IsNotNull( _root );
        var oldRoot = _root;
        oldRoot.SetParent( parent );

        _root = ReferenceEquals( parent.Parent, _root ) ? parent : oldRoot.Children[0];

        _root.RemoveFromParent();
        _root.ClearParent();
        _root.InheritChildrenFrom( oldRoot );
    }

    private static void MoveNodeWithParentTo(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        Assume.IsNotNull( node.Parent );
        var oldParent = node.Parent;
        node.RemoveFromParent();
        node.SetParent( parent );
        oldParent.InheritChildrenFrom( node );
    }

    private static void MoveSubtreeToImpl(TreeDictionaryNode<TKey, TValue> parent, TreeDictionaryNode<TKey, TValue> node)
    {
        if ( ReferenceEquals( parent, node.Parent ) )
            return;

        if ( ReferenceEquals( parent, node ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.SubtreeCannotBeMovedToItself ) );

        if ( parent.IsDescendantOf( node ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.SubtreeCannotBeMovedToOneOfItsNodes ) );

        node.RemoveFromParent();
        node.SetParent( parent );
    }

    [Pure]
    private static TreeDictionaryNode<TKey, TValue> NodeCast(ITreeDictionaryNode<TKey, TValue> node)
    {
        if ( node is TreeDictionaryNode<TKey, TValue> result )
            return result;

        ExceptionThrower.Throw( new ArgumentException( Resources.NodeIsOfIncorrectType, nameof( node ) ) );
        return default!;
    }

    private void AssertNode(TreeDictionaryNode<TKey, TValue> node)
    {
        if ( ! ReferenceEquals( node.Tree, this ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.NodeDoesNotBelongToTree ) );
    }

    private static void AssertNewNode(TreeDictionaryNode<TKey, TValue> node)
    {
        if ( ! ReferenceEquals( node.Tree, null ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.NodeAlreadyBelongsToTree ) );
    }

    private void AssertSubtreeAddition(ITreeDictionaryNode<TKey, TValue> node)
    {
        if ( node is TreeDictionaryNode<TKey, TValue> typedNode && ReferenceEquals( typedNode.Tree, this ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.SubtreeAlreadyBelongsToTree ) );

        if ( node.VisitDescendants().Any( n => ContainsKey( n.Key ) ) )
            ExceptionThrower.Throw( new ArgumentException( Resources.SomeSubtreeNodeKeysAlreadyExistInTree, nameof( node ) ) );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue result)
    {
        return TryGetValue( key, out result! );
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        Add( key, value );
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue result)
    {
        return TryGetValue( key, out result! );
    }

    [Pure]
    ITreeDictionaryNode<TKey, TValue>? IReadOnlyTreeDictionary<TKey, TValue>.GetNode(TKey key)
    {
        return GetNode( key );
    }

    [Pure]
    ITreeDictionary<TKey, TValue> IReadOnlyTreeDictionary<TKey, TValue>.CreateSubtree(TKey key)
    {
        return CreateSubtree( key );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.SetRoot(TKey key, TValue value)
    {
        return SetRoot( key, value );
    }

    void ITreeDictionary<TKey, TValue>.SetRoot(ITreeDictionaryNode<TKey, TValue> node)
    {
        SetRoot( NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        return Add( key, value );
    }

    void ITreeDictionary<TKey, TValue>.Add(ITreeDictionaryNode<TKey, TValue> node)
    {
        Add( NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.AddTo(TKey parentKey, TKey key, TValue value)
    {
        return AddTo( parentKey, key, value );
    }

    void ITreeDictionary<TKey, TValue>.AddTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node)
    {
        AddTo( parentKey, NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.AddTo(
        ITreeDictionaryNode<TKey, TValue> parent,
        TKey key,
        TValue value)
    {
        return AddTo( NodeCast( parent ), key, value );
    }

    void ITreeDictionary<TKey, TValue>.AddTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node)
    {
        AddTo( NodeCast( parent ), NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.AddSubtree(ITreeDictionaryNode<TKey, TValue> node)
    {
        return AddSubtree( node );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.AddSubtreeTo(
        TKey parentKey,
        ITreeDictionaryNode<TKey, TValue> node)
    {
        return AddSubtreeTo( parentKey, node );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.AddSubtreeTo(
        ITreeDictionaryNode<TKey, TValue> parent,
        ITreeDictionaryNode<TKey, TValue> node)
    {
        return AddSubtreeTo( NodeCast( parent ), node );
    }

    void ITreeDictionary<TKey, TValue>.Remove(ITreeDictionaryNode<TKey, TValue> node)
    {
        Remove( NodeCast( node ) );
    }

    int ITreeDictionary<TKey, TValue>.RemoveSubtree(ITreeDictionaryNode<TKey, TValue> node)
    {
        return RemoveSubtree( NodeCast( node ) );
    }

    void ITreeDictionary<TKey, TValue>.Swap(ITreeDictionaryNode<TKey, TValue> firstNode, ITreeDictionaryNode<TKey, TValue> secondNode)
    {
        Swap( NodeCast( firstNode ), NodeCast( secondNode ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.MoveTo(TKey parentKey, TKey key)
    {
        return MoveTo( parentKey, key );
    }

    void ITreeDictionary<TKey, TValue>.MoveTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node)
    {
        MoveTo( parentKey, NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.MoveTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key)
    {
        return MoveTo( NodeCast( parent ), key );
    }

    void ITreeDictionary<TKey, TValue>.MoveTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node)
    {
        MoveTo( NodeCast( parent ), NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.MoveSubtreeTo(TKey parentKey, TKey key)
    {
        return MoveSubtreeTo( parentKey, key );
    }

    void ITreeDictionary<TKey, TValue>.MoveSubtreeTo(TKey parentKey, ITreeDictionaryNode<TKey, TValue> node)
    {
        MoveSubtreeTo( parentKey, NodeCast( node ) );
    }

    ITreeDictionaryNode<TKey, TValue> ITreeDictionary<TKey, TValue>.MoveSubtreeTo(ITreeDictionaryNode<TKey, TValue> parent, TKey key)
    {
        return MoveSubtreeTo( NodeCast( parent ), key );
    }

    void ITreeDictionary<TKey, TValue>.MoveSubtreeTo(ITreeDictionaryNode<TKey, TValue> parent, ITreeDictionaryNode<TKey, TValue> node)
    {
        MoveSubtreeTo( NodeCast( parent ), NodeCast( node ) );
    }

    [Pure]
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return _map.TryGetValue( item.Key, out var node ) && EqualityComparer<TValue>.Default.Equals( node.Value, item.Value );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add( item.Key, item.Value );
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if ( ! _map.TryGetValue( item.Key, out var node ) ||
            ! EqualityComparer<TValue>.Default.Equals( node.Value, item.Value ) )
            return false;

        return Remove( item.Key );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }
}
