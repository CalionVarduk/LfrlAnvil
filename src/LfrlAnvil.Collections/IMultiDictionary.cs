using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic collection of (key, value-range) pairs.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IMultiDictionary<TKey, TValue> : IDictionary<TKey, IReadOnlyList<TValue>>, IReadOnlyMultiDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <inheritdoc cref="ICollection{T}.Count" />
    new int Count { get; }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.Keys" />
    new IReadOnlyCollection<TKey> Keys { get; }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.Values" />
    new IReadOnlyCollection<IReadOnlyList<TValue>> Values { get; }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
    new IReadOnlyList<TValue> this[TKey key] { get; set; }

    /// <summary>
    /// Adds a new entry to this dictionary.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    void Add(TKey key, TValue value);

    /// <summary>
    /// Adds a range of entries associated with the provided <paramref name="key"/> to this dictionary.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="values">Range of values.</param>
    void AddRange(TKey key, IEnumerable<TValue> values);

    /// <summary>
    /// Sets a range of entries associated with the provided <paramref name="key"/> in this dictionary.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="values">Range of values.</param>
    void SetRange(TKey key, IEnumerable<TValue> values);

    /// <summary>
    /// Removes all elements associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns>All removed elements associated with the specified <paramref name="key"/>.</returns>
    new IReadOnlyList<TValue> Remove(TKey key);

    /// <summary>
    /// Attempts to remove a specific element associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Value to remove.</param>
    /// <returns><b>true</b> when key exists and value has been removed, otherwise <b>false</b>.</returns>
    bool Remove(TKey key, TValue value);

    /// <summary>
    /// Attempts to remove an element associated with the specified <paramref name="key"/> at the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="index">0-based position of an element to remove.</param>
    /// <returns><b>true</b> when an element has been removed, otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> is less than <b>0</b>
    /// or greater than or equal to the number of elements associated with the specified <paramref name="key"/>.
    /// </exception>
    bool RemoveAt(TKey key, int index);

    /// <summary>
    /// Attempts to remove a range of elements associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="index">0-based position of a first element to remove.</param>
    /// <param name="count">Total number of elements to remove, starting from the provided <paramref name="key"/>.</param>
    /// <returns><b>true</b> when a range of elements has been removed, otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> or <paramref name="count"/> is less than <b>0</b>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// When <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements.
    /// </exception>
    bool RemoveRange(TKey key, int index, int count);

    /// <summary>
    /// Attempts to remove all elements associated with the specified <paramref name="key"/>
    /// that pass the provided <paramref name="predicate"/>.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="predicate">Delegate that defines which elements to remove.</param>
    /// <returns>Number of removed elements.</returns>
    int RemoveAll(TKey key, Predicate<TValue> predicate);

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey(TKey)" />
    [Pure]
    new bool ContainsKey(TKey key);

    /// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)" />
    new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out IReadOnlyList<TValue> result);
}
