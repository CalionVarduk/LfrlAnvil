using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only collection of (key, value-range) pairs.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IReadOnlyMultiDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>, ILookup<TKey, TValue>
    where TKey : notnull
{
    /// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
    new int Count { get; }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.this" />
    new IReadOnlyList<TValue> this[TKey key] { get; }

    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> Comparer { get; }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Keys" />
    new IReadOnlyCollection<TKey> Keys { get; }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Values" />
    new IReadOnlyCollection<IReadOnlyList<TValue>> Values { get; }

    /// <summary>
    /// Returns the number of elements associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Number of elements associated with the provided <paramref name="key"/>.</returns>
    [Pure]
    int GetCount(TKey key);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()" />
    [Pure]
    new IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetEnumerator();
}
