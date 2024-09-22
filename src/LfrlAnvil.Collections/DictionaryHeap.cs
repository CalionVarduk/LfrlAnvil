// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="IDictionaryHeap{TKey,TValue}" />
public class DictionaryHeap<TKey, TValue> : IDictionaryHeap<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _keyIndexMap;
    private SparseListSlim<int> _keyIndexes;
    private ListSlim<Entry> _items;

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
        _keyIndexMap = new Dictionary<TKey, int>( keyComparer );
        _keyIndexes = SparseListSlim<int>.Create();
        _items = ListSlim<Entry>.Create();
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
        var capacity = collection.TryGetNonEnumeratedCount( out var count ) ? count : 0;
        _keyIndexMap = new Dictionary<TKey, int>( capacity, keyComparer );
        _keyIndexes = SparseListSlim<int>.Create( capacity );
        _items = ListSlim<Entry>.Create( capacity );

        foreach ( var (key, value) in collection )
        {
            var keyIndex = _keyIndexes.Add( _items.Count );
            _keyIndexMap.Add( key, keyIndex );
            _items.Add( new Entry( keyIndex, key, value ) );
        }

        for ( var i = (_items.Count - 1) >> 1; i >= 0; --i )
            FixDown( i );
    }

    /// <inheritdoc />
    public IComparer<TValue> Comparer { get; }

    /// <inheritdoc />
    public IEqualityComparer<TKey> KeyComparer => _keyIndexMap.Comparer;

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
    public int GetIndex(TKey key)
    {
        var keyIndex = _keyIndexMap[key];
        return _keyIndexes[keyIndex];
    }

    /// <inheritdoc />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _keyIndexMap.ContainsKey( key );
    }

    /// <inheritdoc />
    [Pure]
    public TValue GetValue(TKey key)
    {
        var keyIndex = _keyIndexMap[key];
        var position = _keyIndexes[keyIndex];
        return _items[position].Value;
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result)
    {
        if ( _keyIndexMap.TryGetValue( key, out var keyIndex ) )
        {
            var position = _keyIndexes[keyIndex];
            result = _items[position].Value;
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
        if ( _items.IsEmpty )
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
        if ( _items.IsEmpty )
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
        var position = _items.Count;
        var keyIndex = _keyIndexes.Add( position );

        try
        {
            _keyIndexMap.Add( key, keyIndex );
        }
        catch
        {
            _keyIndexes.Remove( keyIndex );
            throw;
        }

        _items.Add( new Entry( keyIndex, key, value ) );
        FixUp( position );
    }

    /// <inheritdoc />
    public bool TryAdd(TKey key, TValue value)
    {
        var position = _items.Count;
        var keyIndex = _keyIndexes.Add( position );

        if ( ! _keyIndexMap.TryAdd( key, keyIndex ) )
        {
            _keyIndexes.Remove( keyIndex );
            return false;
        }

        _items.Add( new Entry( keyIndex, key, value ) );
        FixUp( position );
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
        if ( ! _keyIndexMap.Remove( key, out var keyIndex ) )
        {
            removed = default;
            return false;
        }

        var position = _keyIndexes[keyIndex];
        ref var entry = ref _items[position];
        ref var last = ref _items[^1];
        removed = entry.Value;
        entry = last;
        _keyIndexes[entry.KeyIndex] = position;
        _keyIndexes.Remove( keyIndex );
        _items.RemoveLast();

        if ( position < _items.Count )
            FixRelative( position, entry.Value, removed );

        return true;
    }

    /// <inheritdoc />
    public void Pop()
    {
        ref var removed = ref _items[0];
        ref var last = ref _items[^1];
        _keyIndexes[last.KeyIndex] = _keyIndexes[removed.KeyIndex];
        _keyIndexes.Remove( removed.KeyIndex );
        _keyIndexMap.Remove( removed.Key );
        removed = last;
        _items.RemoveLast();
        FixDown( 0 );
    }

    /// <inheritdoc />
    public bool TryPop()
    {
        if ( _items.IsEmpty )
            return false;

        Pop();
        return true;
    }

    /// <inheritdoc />
    public TValue Replace(TKey key, TValue value)
    {
        var keyIndex = _keyIndexMap[key];
        return Replace( keyIndex, value );
    }

    /// <inheritdoc />
    public bool TryReplace(TKey key, TValue value, [MaybeNullWhen( false )] out TValue replaced)
    {
        if ( _keyIndexMap.TryGetValue( key, out var keyIndex ) )
        {
            replaced = Replace( keyIndex, value );
            return true;
        }

        replaced = default;
        return false;
    }

    /// <inheritdoc />
    public TValue AddOrReplace(TKey key, TValue value)
    {
        if ( _keyIndexMap.TryGetValue( key, out var keyIndex ) )
            return Replace( keyIndex, value );

        Add( key, value );
        return value;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _keyIndexMap.Clear();
        _keyIndexes.Clear();
        _items.Clear();
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this heap.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _items );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="DictionaryHeap{TKey,TValue}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<TValue>
    {
        private readonly ListSlim<Entry> _items;
        private int _index;

        internal Enumerator(ListSlim<Entry> items)
        {
            _items = items;
            _index = -1;
        }

        /// <inheritdoc />
        public TValue Current => _items[_index].Value;

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return ++_index < _items.Count;
        }

        /// <inheritdoc />
        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    internal readonly record struct Entry(int KeyIndex, TKey Key, TValue Value);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private TValue Replace(int keyIndex, TValue value)
    {
        var position = _keyIndexes[keyIndex];
        ref var entry = ref _items[position];
        var oldValue = entry.Value;
        entry = new Entry( keyIndex, entry.Key, value );
        FixRelative( position, value, oldValue );
        return oldValue;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixRelative(int position, TValue value, TValue oldValue)
    {
        if ( Comparer.Compare( oldValue, value ) < 0 )
            FixDown( position );
        else
            FixUp( position );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixUp(int i)
    {
        ref var first = ref _items.First();
        while ( i > 0 )
        {
            var p = Heap.GetParentIndex( i );
            ref var item = ref Unsafe.Add( ref first, i );
            ref var parent = ref Unsafe.Add( ref first, p );

            if ( Comparer.Compare( item.Value, parent.Value ) >= 0 )
                break;

            (item, parent) = (parent, item);
            SwapIndexes( ref item, ref parent );
            i = p;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixDown(int i)
    {
        ref var first = ref _items.First();
        var l = Heap.GetLeftChildIndex( i );

        while ( l < _items.Count )
        {
            ref var item = ref Unsafe.Add( ref first, i );
            ref var target = ref Unsafe.Add( ref first, l );

            var t = l;
            if ( Comparer.Compare( item.Value, target.Value ) < 0 )
            {
                t = i;
                target = ref item;
            }

            var r = l + 1;
            if ( r < _items.Count )
            {
                ref var right = ref Unsafe.Add( ref first, r );
                if ( Comparer.Compare( right.Value, target.Value ) < 0 )
                {
                    t = r;
                    target = ref right;
                }
            }

            if ( i == t )
                break;

            (item, target) = (target, item);
            SwapIndexes( ref item, ref target );
            i = t;
            l = Heap.GetLeftChildIndex( i );
        }
    }

    private void SwapIndexes(ref Entry a, ref Entry b)
    {
        ref var keyA = ref _keyIndexes[a.KeyIndex];
        Assume.False( Unsafe.IsNullRef( ref keyA ) );
        ref var keyB = ref _keyIndexes[b.KeyIndex];
        Assume.False( Unsafe.IsNullRef( ref keyB ) );
        (keyA, keyB) = (keyB, keyA);
    }

    [Pure]
    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
