using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Caching;

public interface ICache<TKey, TValue> : IReadOnlyCache<TKey, TValue>
    where TKey : notnull
{
    new TValue this[TKey key] { get; set; }
    bool TryAdd(TKey key, TValue value);
    AddOrUpdateResult AddOrUpdate(TKey key, TValue value);
    bool Remove(TKey key);
    bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed);
    bool Restart(TKey key);
    void Clear();
}
