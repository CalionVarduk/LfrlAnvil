namespace LfrlAnvil.Collections
{
    public interface IFixedCache<TKey, TValue> : IReadOnlyFixedCache<TKey, TValue>
        where TKey : notnull
    {
        new TValue this[TKey key] { get; set; }
        bool TryAdd(TKey key, TValue value);
        void Add(TKey key, TValue value);
    }
}
