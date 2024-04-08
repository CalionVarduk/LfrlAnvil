using System.Collections.Generic;

namespace LfrlAnvil.Caching;

public interface IReadOnlyCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    int Capacity { get; }
    IEqualityComparer<TKey> Comparer { get; }
    KeyValuePair<TKey, TValue>? Oldest { get; }
    KeyValuePair<TKey, TValue>? Newest { get; }
}
