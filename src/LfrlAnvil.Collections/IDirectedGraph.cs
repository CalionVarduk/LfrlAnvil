using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

public interface IDirectedGraph<TKey, TNodeValue, TEdgeValue> : IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> AddNode(TKey key, TNodeValue value);
    bool TryAddNode(TKey key, TNodeValue value, [MaybeNullWhen( false )] out IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> added);
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> GetOrAddNode(TKey key, TNodeValue value);

    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> AddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction = GraphDirection.Out);

    bool TryAddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> added);

    bool RemoveNode(TKey key);
    bool RemoveNode(TKey key, [MaybeNullWhen( false )] out TNodeValue removed);
    bool RemoveEdge(TKey firstKey, TKey secondKey);
    bool RemoveEdge(TKey firstKey, TKey secondKey, [MaybeNullWhen( false )] out TEdgeValue removed);
    bool Remove(IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node);
    bool Remove(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge);
    void Clear();
}
