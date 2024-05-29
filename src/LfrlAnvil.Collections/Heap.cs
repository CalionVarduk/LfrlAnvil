using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="IHeap{T}" />
public class Heap<T> : IHeap<T>
{
    private readonly List<T> _items;

    /// <summary>
    /// Creates a new empty <see cref="Heap{T}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    public Heap()
        : this( Comparer<T>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="Heap{T}"/> instance.
    /// </summary>
    /// <param name="comparer">Comparer to use.</param>
    public Heap(IComparer<T> comparer)
    {
        Comparer = comparer;
        _items = new List<T>();
    }

    /// <summary>
    /// Creates a new <see cref="Heap{T}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="collection">Initial collection of entries.</param>
    public Heap(IEnumerable<T> collection)
        : this( collection, Comparer<T>.Default ) { }

    /// <summary>
    /// Creates a new <see cref="Heap{T}"/> instance.
    /// </summary>
    /// <param name="collection">Initial collection of entries.</param>
    /// <param name="comparer">Comparer to use.</param>
    public Heap(IEnumerable<T> collection, IComparer<T> comparer)
    {
        Comparer = comparer;
        _items = collection.ToList();

        for ( var i = (_items.Count - 1) >> 1; i >= 0; --i )
            FixDown( i );
    }

    /// <inheritdoc />
    public IComparer<T> Comparer { get; }

    /// <inheritdoc />
    public T this[int index] => _items[index];

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public void Add(T item)
    {
        _items.Add( item );
        FixUp( _items.Count - 1 );
    }

    /// <inheritdoc />
    public T Extract()
    {
        var result = Peek();
        Pop();
        return result;
    }

    /// <inheritdoc />
    public bool TryExtract([MaybeNullWhen( false )] out T result)
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
    [Pure]
    public T Peek()
    {
        return _items[0];
    }

    /// <inheritdoc />
    public bool TryPeek([MaybeNullWhen( false )] out T result)
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
    public void Pop()
    {
        _items[0] = _items[^1];
        _items.RemoveLast();
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
    public T Replace(T item)
    {
        var result = Peek();
        _items[0] = item;
        FixDown( 0 );
        return result;
    }

    /// <inheritdoc />
    public bool TryReplace(T item, [MaybeNullWhen( false )] out T replaced)
    {
        if ( _items.Count == 0 )
        {
            replaced = default;
            return false;
        }

        replaced = Replace( item );
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _items.Clear();
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<T> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixUp(int i)
    {
        while ( i > 0 )
        {
            var p = Heap.GetParentIndex( i );
            if ( Comparer.Compare( _items[i], _items[p] ) >= 0 )
                break;

            _items.SwapItems( i, p );
            i = p;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixDown(int i)
    {
        var l = Heap.GetLeftChildIndex( i );

        while ( l < _items.Count )
        {
            var r = l + 1;
            var m = Comparer.Compare( _items[l], _items[i] ) < 0 ? l : i;

            if ( r < _items.Count && Comparer.Compare( _items[r], _items[m] ) < 0 )
                m = r;

            if ( m == i )
                break;

            _items.SwapItems( i, m );
            i = m;
            l = Heap.GetLeftChildIndex( i );
        }
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
