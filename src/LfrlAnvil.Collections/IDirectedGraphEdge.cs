namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic <see cref="IDirectedGraph{TKey,TNodeValue,TEdgeValue}"/> edge.
/// </summary>
/// <typeparam name="TKey">Graph's key type.</typeparam>
/// <typeparam name="TNodeValue">Graph node's value type.</typeparam>
/// <typeparam name="TEdgeValue">Graph edge's value type.</typeparam>
public interface IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    TEdgeValue Value { get; }

    /// <summary>
    /// Direction of this edge, from <see cref="Source"/> to <see cref="Target"/>.
    /// </summary>
    GraphDirection Direction { get; }

    /// <summary>
    /// Source node of this edge.
    /// </summary>
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> Source { get; }

    /// <summary>
    /// Target node of this edge.
    /// </summary>
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> Target { get; }
}
