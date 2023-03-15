using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    IEqualityComparer<TKey> KeyComparer { get; }
    IReadOnlyCollection<IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>> Nodes { get; }
    IEnumerable<IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> Edges { get; }

    [Pure]
    bool ContainsNode(TKey key);

    [Pure]
    bool Contains(IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node);

    [Pure]
    bool ContainsEdge(TKey firstKey, TKey secondKey);

    [Pure]
    bool Contains(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge);

    [Pure]
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> GetNode(TKey key);

    bool TryGetNode(TKey key, [MaybeNullWhen( false )] out IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> result);

    [Pure]
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> GetEdge(TKey firstKey, TKey secondKey);

    [Pure]
    bool TryGetEdge(TKey firstKey, TKey secondKey, [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result);
}
