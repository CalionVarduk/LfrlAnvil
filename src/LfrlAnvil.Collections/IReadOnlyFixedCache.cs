using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections
{
    public interface IReadOnlyFixedCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        int Capacity { get; }
        IEqualityComparer<TKey> Comparer { get; }
        KeyValuePair<TKey, TValue>? Newest { get; }
        KeyValuePair<TKey, TValue>? Oldest { get; }
        new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
    }
}
