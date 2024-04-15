using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

public interface ILifetimeCache<TKey, TValue> : IReadOnlyLifetimeCache<TKey, TValue>, ICache<TKey, TValue>
    where TKey : notnull
{
    void Move(Duration delta);
}
