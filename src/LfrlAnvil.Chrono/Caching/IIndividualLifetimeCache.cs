namespace LfrlAnvil.Chrono.Caching;

public interface IIndividualLifetimeCache<TKey, TValue> : ILifetimeCache<TKey, TValue>
    where TKey : notnull
{
    bool TryAdd(TKey key, TValue value, Duration lifetime);
    AddOrUpdateResult AddOrUpdate(TKey key, TValue value, Duration lifetime);
}
