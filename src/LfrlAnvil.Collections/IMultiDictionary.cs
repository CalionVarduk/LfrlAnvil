using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IMultiDictionary<TKey, TValue> : IDictionary<TKey, IReadOnlyList<TValue>>, IReadOnlyMultiDictionary<TKey, TValue>
    where TKey : notnull
{
    new int Count { get; }
    new IEnumerable<TKey> Keys { get; }
    new IEnumerable<IReadOnlyList<TValue>> Values { get; }
    new IReadOnlyList<TValue> this[TKey key] { get; set; }

    void Add(TKey key, TValue value);
    void AddRange(TKey key, IEnumerable<TValue> values);
    void SetRange(TKey key, IEnumerable<TValue> values);
    new IReadOnlyList<TValue> Remove(TKey key);
    bool Remove(TKey key, TValue value);
    bool RemoveAt(TKey key, int index);
    bool RemoveRange(TKey key, int index, int count);
    int RemoveAll(TKey key, Predicate<TValue> predicate);

    [Pure]
    new bool ContainsKey(TKey key);

    new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out IReadOnlyList<TValue> result);
}
