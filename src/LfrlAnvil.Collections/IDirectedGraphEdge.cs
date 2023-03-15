namespace LfrlAnvil.Collections;

public interface IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    TEdgeValue Value { get; }
    GraphDirection Direction { get; }
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> Source { get; }
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> Target { get; }
}
