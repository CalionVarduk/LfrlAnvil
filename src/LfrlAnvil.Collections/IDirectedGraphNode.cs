using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic <see cref="IDirectedGraph{TKey,TNodeValue,TEdgeValue}"/> node.
/// </summary>
/// <typeparam name="TKey">Graph's key type.</typeparam>
/// <typeparam name="TNodeValue">Graph node's value type.</typeparam>
/// <typeparam name="TEdgeValue">Graph edge's value type.</typeparam>
public interface IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    /// <summary>
    /// Underlying key.
    /// </summary>
    TKey Key { get; }

    /// <summary>
    /// Underlying value.
    /// </summary>
    TNodeValue Value { get; }

    /// <summary>
    /// Associated <see cref="IReadOnlyDirectedGraph{TKey,TNodeValue,TEdgeValue}"/> instance associated with this node.
    /// </summary>
    IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>? Graph { get; }

    /// <summary>
    /// Collection of <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instances connected to this node.
    /// </summary>
    IReadOnlyCollection<IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> Edges { get; }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all graph nodes that are reachable from this node,
    /// using the provided <paramref name="direction"/>.
    /// </summary>
    /// <param name="direction">Direction of node traversal. Equal to <see cref="GraphDirection.Out"/> by default.</param>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    IEnumerable<IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>> GetReachableNodes(GraphDirection direction = GraphDirection.Out);

    /// <summary>
    /// Checks whether or not this node is connected with a node associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Node's key to check.</param>
    /// <returns><b>true</b> when the two nodes are directly connected, otherwise <b>false</b>.</returns>
    [Pure]
    bool ContainsEdgeTo(TKey key);

    /// <summary>
    /// Returns an <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance that connects this node
    /// and the node associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Other node's key.</param>
    /// <returns><see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When an edge to the node associated with the specified <paramref name="key"/> does not exist.
    /// </exception>
    [Pure]
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> GetEdgeTo(TKey key);

    /// <summary>
    /// Attempts to return an <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance that connects this node
    /// and the node associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Other node's key.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns an <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance.
    /// </param>
    /// <returns><b>true</b> when the connection exists, otherwise <b>false</b>.</returns>
    bool TryGetEdgeTo(TKey key, [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result);
}
