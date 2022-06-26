using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

public interface IFiniteCache<TKey, TValue> : IReadOnlyFiniteCache<TKey, TValue>, IDictionary<TKey, TValue>
    where TKey : notnull
{
    new int Count { get; }
    new IEnumerable<TKey> Keys { get; }
    new IEnumerable<TValue> Values { get; }
    new TValue this[TKey key] { get; set; }
    new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
    new bool ContainsKey(TKey key);
}
