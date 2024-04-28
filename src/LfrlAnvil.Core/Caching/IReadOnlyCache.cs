using System.Collections.Generic;

namespace LfrlAnvil.Caching;

/// <summary>
/// Represents a generic read-only cache of keyed entries.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
public interface IReadOnlyCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Maximum capacity of this cache.
    /// Adding entries to caches whose capacity have been reached will cause their <see cref="Oldest"/> entry to be removed.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// <see cref="IEqualityComparer{T}"/> instance used for key equality comparison.
    /// </summary>
    IEqualityComparer<TKey> Comparer { get; }

    /// <summary>
    /// Currently oldest cache entry.
    /// </summary>
    /// <remarks>
    /// This entry will be removed when new entry addition
    /// causes <see cref="IReadOnlyCollection{T}.Count"/> to exceed <see cref="Capacity"/>.
    /// </remarks>
    KeyValuePair<TKey, TValue>? Oldest { get; }
}
