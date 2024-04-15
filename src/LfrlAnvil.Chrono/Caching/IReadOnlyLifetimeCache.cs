using System.Diagnostics.Contracts;
using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

public interface IReadOnlyLifetimeCache<TKey, TValue> : IReadOnlyCache<TKey, TValue>
    where TKey : notnull
{
    Duration Lifetime { get; }
    Timestamp StartTimestamp { get; }
    Timestamp CurrentTimestamp { get; }

    [Pure]
    Duration GetRemainingLifetime(TKey key);
}
