using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

/// <summary>
/// Represents a generic cache of keyed entries with a limited lifetime.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
/// <remarks>Reading entries resets their lifetime.</remarks>
public interface ILifetimeCache<TKey, TValue> : IReadOnlyLifetimeCache<TKey, TValue>, ICache<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Moves this cache forward in time and removes entries with elapsed lifetimes.
    /// </summary>
    /// <param name="delta"><see cref="Duration"/> to add to the <see cref="IReadOnlyLifetimeCache{TKey,TValue}.CurrentTimestamp"/>.</param>
    void Move(Duration delta);
}
