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
using LfrlAnvil.Collections.Exceptions;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="ITwoWayDictionary{T1,T2}" />
public class TwoWayDictionary<T1, T2> : ITwoWayDictionary<T1, T2>
    where T1 : notnull
    where T2 : notnull
{
    private readonly Dictionary<T1, T2> _forward;
    private readonly Dictionary<T2, T1> _reverse;

    /// <summary>
    /// Creates a new empty <see cref="TwoWayDictionary{T1,T2}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> forward and reverse comparer.
    /// </summary>
    public TwoWayDictionary()
    {
        _forward = new Dictionary<T1, T2>();
        _reverse = new Dictionary<T2, T1>();
    }

    /// <summary>
    /// Creates a new empty <see cref="TwoWayDictionary{T1,T2}"/> instance.
    /// </summary>
    /// <param name="forwardComparer">Forward key equality comparer.</param>
    /// <param name="reverseComparer">Reverse key equality comparer.</param>
    public TwoWayDictionary(IEqualityComparer<T1> forwardComparer, IEqualityComparer<T2> reverseComparer)
    {
        _forward = new Dictionary<T1, T2>( forwardComparer );
        _reverse = new Dictionary<T2, T1>( reverseComparer );
    }

    /// <inheritdoc cref="ITwoWayDictionary{T1,T2}.Count" />
    public int Count => _forward.Count;

    /// <summary>
    /// Represents the <typeparamref name="T1"/> => <typeparamref name="T2"/> read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<T1, T2> Forward => _forward;

    /// <summary>
    /// Represents the <typeparamref name="T2"/> => <typeparamref name="T1"/> read-only dictionary.
    /// </summary>
    public IReadOnlyDictionary<T2, T1> Reverse => _reverse;

    /// <inheritdoc />
    public IEqualityComparer<T1> ForwardComparer => _forward.Comparer;

    /// <inheritdoc />
    public IEqualityComparer<T2> ReverseComparer => _reverse.Comparer;

    bool ICollection<Pair<T1, T2>>.IsReadOnly => (( ICollection<KeyValuePair<T1, T2>> )_forward).IsReadOnly;

    /// <inheritdoc />
    public bool TryAdd(T1 first, T2 second)
    {
        if ( _forward.ContainsKey( first ) || _reverse.ContainsKey( second ) )
            return false;

        _forward.Add( first, second );
        _reverse.Add( second, first );
        return true;
    }

    /// <inheritdoc />
    public void Add(T1 first, T2 second)
    {
        Ensure.False( _forward.ContainsKey( first ), Resources.KeyExistenceInForwardDictionary );
        Ensure.False( _reverse.ContainsKey( second ), Resources.KeyExistenceInReverseDictionary );
        _forward.Add( first, second );
        _reverse.Add( second, first );
    }

    /// <inheritdoc />
    public bool TryUpdateForward(T1 first, T2 second)
    {
        if ( _reverse.ContainsKey( second ) )
            return false;

        if ( ! _forward.TryGetValue( first, out var other ) )
            return false;

        _forward[first] = second;
        _reverse.Remove( other );
        _reverse.Add( second, first );
        return true;
    }

    /// <inheritdoc />
    public void UpdateForward(T1 first, T2 second)
    {
        Ensure.False( _reverse.ContainsKey( second ), Resources.KeyExistenceInReverseDictionary );
        var other = _forward[first];
        _forward[first] = second;
        _reverse.Remove( other );
        _reverse.Add( second, first );
    }

    /// <inheritdoc />
    public bool TryUpdateReverse(T2 second, T1 first)
    {
        if ( _forward.ContainsKey( first ) )
            return false;

        if ( ! _reverse.TryGetValue( second, out var other ) )
            return false;

        _reverse[second] = first;
        _forward.Remove( other );
        _forward.Add( first, second );
        return true;
    }

    /// <inheritdoc />
    public void UpdateReverse(T2 second, T1 first)
    {
        Ensure.False( _forward.ContainsKey( first ), Resources.KeyExistenceInForwardDictionary );
        var other = _reverse[second];
        _reverse[second] = first;
        _forward.Remove( other );
        _forward.Add( first, second );
    }

    /// <inheritdoc />
    public bool RemoveForward(T1 value)
    {
        return RemoveForward( value, out _ );
    }

    /// <inheritdoc />
    public bool RemoveReverse(T2 value)
    {
        return RemoveReverse( value, out _ );
    }

    /// <inheritdoc />
    public bool RemoveForward(T1 value, [MaybeNullWhen( false )] out T2 second)
    {
        if ( ! _forward.Remove( value, out second ) )
            return false;

        _reverse.Remove( second );
        return true;
    }

    /// <inheritdoc />
    public bool RemoveReverse(T2 value, [MaybeNullWhen( false )] out T1 first)
    {
        if ( ! _reverse.Remove( value, out first ) )
            return false;

        _forward.Remove( first );
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _forward.Clear();
        _reverse.Clear();
    }

    /// <inheritdoc />
    [Pure]
    public bool Contains(T1 first, T2 second)
    {
        return _forward.TryGetValue( first, out var existingSecond ) && _reverse.Comparer.Equals( existingSecond, second );
    }

    /// <inheritdoc />
    [Pure]
    public bool Contains(Pair<T1, T2> item)
    {
        return Contains( item.First, item.Second );
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<Pair<T1, T2>> GetEnumerator()
    {
        return _forward.Select( static kv => Pair.Create( kv.Key, kv.Value ) ).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void ICollection<Pair<T1, T2>>.Add(Pair<T1, T2> item)
    {
        Add( item.First, item.Second );
    }

    bool ICollection<Pair<T1, T2>>.Remove(Pair<T1, T2> item)
    {
        if ( ! _forward.TryGetValue( item.First, out var second ) || ! _reverse.Comparer.Equals( second, item.Second ) )
            return false;

        _forward.Remove( item.First );
        _reverse.Remove( item.Second );
        return true;
    }

    void ICollection<Pair<T1, T2>>.CopyTo(Pair<T1, T2>[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }
}
