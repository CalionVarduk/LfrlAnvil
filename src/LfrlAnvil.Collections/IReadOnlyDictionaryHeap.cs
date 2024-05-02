using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only heap data structure with the ability to identify entries by keys.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IReadOnlyDictionaryHeap<TKey, TValue> : IReadOnlyHeap<TValue>
{
    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Returns the key associated with an entry located at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">0-based position.</param>
    /// <returns>Key associated with an entry located at the specified <paramref name="index"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// When <paramref name="index"/> is not in [<b>0</b>, <see cref="IReadOnlyCollection{T}.Count"/>) range.
    /// </exception>
    [Pure]
    TKey GetKey(int index);

    /// <summary>
    /// Checks whether or not an entry with the specified <paramref name="key"/> exists in this heap.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns><b>true</b> when entry with the specified <paramref name="key"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool ContainsKey(TKey key);

    /// <summary>
    /// Returns an entry associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to get an entry for.</param>
    /// <returns>Entry associated with the specified <paramref name="key"/>.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="key"/> does not exist in this heap.</exception>
    [Pure]
    TValue GetValue(TKey key);

    /// <summary>
    /// Attempts to return an entry associated with the specified <paramref name="key"/> if it exists.
    /// </summary>
    /// <param name="key">Key to get an entry for.</param>
    /// <param name="result"><b>out</b> parameter that returns an entry associated with the specified <paramref name="key"/>.</param>
    /// <returns><b>true</b> when key exists in this heap, otherwise <b>false</b>.</returns>
    bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
}
