using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public interface IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    TKey Key { get; }
    TNodeValue Value { get; }
    IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>? Graph { get; }
    IReadOnlyCollection<IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> Edges { get; }

    [Pure]
    IEnumerable<IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>> GetReachableNodes(GraphDirection direction = GraphDirection.Out);

    [Pure]
    bool ContainsEdgeTo(TKey key);

    [Pure]
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> GetEdgeTo(TKey key);

    bool TryGetEdgeTo(TKey key, [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result);
}
