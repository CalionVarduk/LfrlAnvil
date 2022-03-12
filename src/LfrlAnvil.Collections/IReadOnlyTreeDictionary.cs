using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections
{
    public interface IReadOnlyTreeDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        ITreeDictionaryNode<TKey, TValue>? Root { get; }
        IEnumerable<ITreeDictionaryNode<TKey, TValue>> Nodes { get; }
        IEqualityComparer<TKey> Comparer { get; }
        new IEnumerable<TKey> Keys { get; }
        new IEnumerable<TValue> Values { get; }

        [Pure]
        ITreeDictionaryNode<TKey, TValue>? GetNode(TKey key);

        [Pure]
        ITreeDictionary<TKey, TValue> CreateSubtree(TKey key);

        new bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
    }
}
