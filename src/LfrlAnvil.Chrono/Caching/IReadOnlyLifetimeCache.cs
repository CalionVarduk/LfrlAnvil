using System.Diagnostics.Contracts;
using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

/// <summary>
/// Represents a generic read-only cache of keyed entries with a limited lifetime.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
/// <remarks>Reading entries resets their lifetime.</remarks>
public interface IReadOnlyLifetimeCache<TKey, TValue> : IReadOnlyCache<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Lifetime of added entries.
    /// </summary>
    Duration Lifetime { get; }

    /// <summary>
    /// <see cref="Timestamp"/> of the creation of this cache.
    /// </summary>
    Timestamp StartTimestamp { get; }

    /// <summary>
    /// <see cref="Timestamp"/> at which this cache currently is.
    /// </summary>
    Timestamp CurrentTimestamp { get; }

    /// <summary>
    /// Gets the remaining lifetime of an entry with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Entry's lifetime or <see cref="Duration.Zero"/> when key does not exist.</returns>
    [Pure]
    Duration GetRemainingLifetime(TKey key);
}
