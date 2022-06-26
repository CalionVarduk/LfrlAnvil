using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IReadOnlyDictionaryHeap<TKey, TValue> : IReadOnlyHeap<TValue>
{
    IEqualityComparer<TKey> KeyComparer { get; }

    [Pure]
    TKey GetKey(int index);

    [Pure]
    bool ContainsKey(TKey key);

    [Pure]
    TValue GetValue(TKey key);

    bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
}
