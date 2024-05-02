using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections;

/// <inheritdoc />
public class DictionaryHeap<TKey, TValue> : IDictionaryHeap<TKey, TValue>
    where TKey : notnull
{
    private readonly List<DictionaryHeapNode<TKey, TValue>> _items;
    private readonly Dictionary<TKey, DictionaryHeapNode<TKey, TValue>> _map;

    /// <summary>
    /// Creates a new empty <see cref="DictionaryHeap{TKey,TValue}"/> instance with <see cref="EqualityComparer{T}.Default"/> key comparer
    /// and <see cref="Comparer{T}.Default"/> entry comparer.
    /// </summary>
    public DictionaryHeap()
        : this( EqualityComparer<TKey>.Default, Comparer<TValue>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="DictionaryHeap{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="keyComparer">Key equality comparer.</param>
    /// <param name="comparer">Entry comparer.</param>
    public DictionaryHeap(IEqualityComparer<TKey> keyComparer, IComparer<TValue> comparer)
    {
        Comparer = comparer;
        _items = new List<DictionaryHeapNode<TKey, TValue>>();
        _map = new Dictionary<TKey, DictionaryHeapNode<TKey, TValue>>( keyComparer );
    }

    /// <summary>
    /// Creates a new <see cref="DictionaryHeap{TKey,TValue}"/> instance with <see cref="EqualityComparer{T}.Default"/> key comparer
    /// and <see cref="Comparer{T}.Default"/> entry comparer.
    /// </summary>
    /// <param name="collection">Initial collection of entries.</param>
    public DictionaryHeap(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        : this( collection, EqualityComparer<TKey>.Default, Comparer<TValue>.Default ) { }

    /// <summary>
    /// Creates a new <see cref="DictionaryHeap{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="collection">Initial collection of entries.</param>
    /// <param name="keyComparer">Key equality comparer.</param>
    /// <param name="comparer">Entry comparer.</param>
    public DictionaryHeap(
        IEnumerable<KeyValuePair<TKey, TValue>> collection,
        IEqualityComparer<TKey> keyComparer,
        IComparer<TValue> comparer)
    {
        Comparer = comparer;
        _items = new List<DictionaryHeapNode<TKey, TValue>>();
        _map = new Dictionary<TKey, DictionaryHeapNode<TKey, TValue>>( keyComparer );

        foreach ( var (key, value) in collection )
        {
            var node = CreateNode( key, value );
            _map.Add( key, node );
            _items.Add( node );
        }

        for ( var i = (_items.Count - 1) >> 1; i >= 0; --i )
            FixDown( i );
    }

    /// <inheritdoc />
    public IComparer<TValue> Comparer { get; }

    /// <inheritdoc />
    public IEqualityComparer<TKey> KeyComparer => _map.Comparer;

    /// <inheritdoc />
    public TValue this[int index] => _items[index].Value;

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    [Pure]
    public TKey GetKey(int index)
    {
        return _items[index].Key;
    }

    /// <inheritdoc />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    /// <inheritdoc />
    [Pure]
    public TValue GetValue(TKey key)
    {
        return _map[key].Value;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Pure]
    public TValue Peek()
    {
        return _items[0].Value;
    }

    /// <inheritdoc />
    public bool TryPeek([MaybeNullWhen( false )] out TValue result)
    {
        if ( _items.Count == 0 )
        {
            result = default;
            return false;
        }

        result = Peek();
        return true;
    }

    /// <inheritdoc />
    public TValue Extract()
    {
        var result = Peek();
        Pop();
        return result;
    }

    /// <inheritdoc />
    public bool TryExtract([MaybeNullWhen( false )] out TValue result)
    {
        if ( _items.Count == 0 )
        {
            result = default;
            return false;
        }

        result = Extract();
        return true;
    }

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        var node = CreateNode( key, value );
        _map.Add( key, node );
        _items.Add( node );
        FixUp( _items.Count - 1 );
    }

    /// <inheritdoc />
    public bool TryAdd(TKey key, TValue value)
    {
        var node = CreateNode( key, value );
        if ( ! _map.TryAdd( key, node ) )
            return false;

        _items.Add( node );
        FixUp( _items.Count - 1 );
        return true;
    }

    /// <inheritdoc />
    public TValue Remove(TKey key)
    {
        if ( ! TryRemove( key, out var removed ) )
            throw new KeyNotFoundException( $"The given key '{key}' was not present in the dictionary." );

        return removed;
    }

    /// <inheritdoc />
    public bool TryRemove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        if ( ! _map.Remove( key, out var node ) )
        {
            removed = default;
            return false;
        }

        var lastNode = _items[^1];
        _items[node.Index] = lastNode;
        lastNode.AssignIndexFrom( node );
        _items.RemoveLast();

        if ( node.Index < _items.Count )
            FixRelative( lastNode, node.Value );

        removed = node.Value;
        return true;
    }

    /// <inheritdoc />
    public void Pop()
    {
        var nodeToPop = _items[0];
        var lastNode = _items[^1];
        _items[0] = lastNode;
        lastNode.AssignIndexFrom( nodeToPop );
        _items.RemoveLast();
        _map.Remove( nodeToPop.Key );
        FixDown( 0 );
    }

    /// <inheritdoc />
    public bool TryPop()
    {
        if ( _items.Count == 0 )
            return false;

        Pop();
        return true;
    }

    /// <inheritdoc />
    public TValue Replace(TKey key, TValue value)
    {
        var node = _map[key];
        return Replace( node, value );
    }

    /// <inheritdoc />
    public bool TryReplace(TKey key, TValue value, [MaybeNullWhen( false )] out TValue replaced)
    {
        if ( _map.TryGetValue( key, out var node ) )
        {
            replaced = Replace( node, value );
            return true;
        }

        replaced = default;
        return false;
    }

    /// <inheritdoc />
    public TValue AddOrReplace(TKey key, TValue value)
    {
        if ( _map.TryGetValue( key, out var node ) )
            return Replace( node, value );

        Add( key, value );
        return value;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _items.Clear();
        _map.Clear();
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<TValue> GetEnumerator()
    {
        return _items.Select( static n => n.Value ).GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DictionaryHeapNode<TKey, TValue> CreateNode(TKey key, TValue value)
    {
        return new DictionaryHeapNode<TKey, TValue>( key, value, _items.Count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private TValue Replace(DictionaryHeapNode<TKey, TValue> node, TValue value)
    {
        var oldValue = node.Value;
        node.Value = value;
        FixRelative( node, oldValue );
        return oldValue;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixRelative(DictionaryHeapNode<TKey, TValue> node, TValue oldValue)
    {
        if ( Comparer.Compare( oldValue, node.Value ) < 0 )
            FixDown( node.Index );
        else
            FixUp( node.Index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixUp(int i)
    {
        while ( i > 0 )
        {
            var p = Heap.GetParentIndex( i );
            var node = _items[i];
            var parentNode = _items[p];

            if ( Comparer.Compare( node.Value, parentNode.Value ) >= 0 )
                break;

            _items.SwapItems( i, p );
            node.SwapIndexWith( parentNode );
            i = p;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixDown(int i)
    {
        var l = Heap.GetLeftChildIndex( i );

        while ( l < _items.Count )
        {
            var node = _items[i];
            var leftChildNode = _items[l];

            var r = l + 1;
            var nodeToSwap = Comparer.Compare( leftChildNode.Value, node.Value ) < 0 ? leftChildNode : node;

            if ( r < _items.Count )
            {
                var rightChildNode = _items[r];
                if ( Comparer.Compare( rightChildNode.Value, nodeToSwap.Value ) < 0 )
                    nodeToSwap = rightChildNode;
            }

            if ( ReferenceEquals( node, nodeToSwap ) )
                break;

            _items.SwapItems( i, nodeToSwap.Index );
            node.SwapIndexWith( nodeToSwap );
            i = node.Index;
            l = Heap.GetLeftChildIndex( i );
        }
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
