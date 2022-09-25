using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Collections;

public interface IReadOnlyMultiDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>, ILookup<TKey, TValue>
    where TKey : notnull
{
    new int Count { get; }
    new IReadOnlyList<TValue> this[TKey key] { get; }
    IEqualityComparer<TKey> Comparer { get; }
    new IReadOnlyCollection<TKey> Keys { get; }
    new IReadOnlyCollection<IReadOnlyList<TValue>> Values { get; }

    [Pure]
    int GetCount(TKey key);

    [Pure]
    new IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetEnumerator();
}
