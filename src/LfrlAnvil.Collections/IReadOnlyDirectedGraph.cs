using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only directed graph data structure.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TNodeValue">Node's value type.</typeparam>
/// <typeparam name="TEdgeValue">Node's edge type.</typeparam>
public interface IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Specifies the current collection of <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instances
    /// that belong to this graph.
    /// </summary>
    IReadOnlyCollection<IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>> Nodes { get; }

    /// <summary>
    /// Specifies the current collection of <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instances
    /// that belong to this graph.
    /// </summary>
    IEnumerable<IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> Edges { get; }

    /// <summary>
    /// Checks whether or not a node with the specified <paramref name="key"/> exists in this graph.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns><b>true</b> when node exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool ContainsNode(TKey key);

    /// <summary>
    /// Checks whether or not the provided <paramref name="node"/> exists in this graph.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <returns><b>true</b> when node exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node);

    /// <summary>
    /// Checks whether or not an edge that connects nodes associated with the <paramref name="firstKey"/> and <paramref name="secondKey"/>
    /// exists in this graph.
    /// </summary>
    /// <param name="firstKey">Key of the first node to check.</param>
    /// <param name="secondKey">Key of the second node to check.</param>
    /// <returns><b>true</b> when edge exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool ContainsEdge(TKey firstKey, TKey secondKey);

    /// <summary>
    /// Checks whether or not the provided <paramref name="edge"/> exists in this graph.
    /// </summary>
    /// <param name="edge">Edge to check.</param>
    /// <returns><b>true</b> when edge exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge);

    /// <summary>
    /// Returns the node associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <returns>
    /// <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instance associated with the provided <paramref name="key"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">When key does not exist in this graph.</exception>
    [Pure]
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> GetNode(TKey key);

    /// <summary>
    /// Attempts to return the node associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns an <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instance
    /// associated with the provided <paramref name="key"/>.
    /// </param>
    /// <returns><b>true</b> when the node exists, otherwise <b>false</b>.</returns>
    bool TryGetNode(TKey key, [MaybeNullWhen( false )] out IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> result);

    /// <summary>
    /// Returns the edge that connects nodes associated with the specified <paramref name="firstKey"/> and <paramref name="secondKey"/>.
    /// </summary>
    /// <param name="firstKey">First node's key.</param>
    /// <param name="secondKey">Second node's key.</param>
    /// <returns>
    /// <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance that connects nodes associated with
    /// <paramref name="firstKey"/> and <paramref name="secondKey"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// When <paramref name="firstKey"/> does not exist in this graph
    /// or when an edge from the first node to the node associated with the specified <paramref name="secondKey"/> does not exist.
    /// </exception>
    [Pure]
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> GetEdge(TKey firstKey, TKey secondKey);

    /// <summary>
    /// Returns the edge that connects nodes associated with the specified <paramref name="firstKey"/> and <paramref name="secondKey"/>.
    /// </summary>
    /// <param name="firstKey">First node's key.</param>
    /// <param name="secondKey">Second node's key.</param>
    /// <param name="result"><b>out</b> parameter that returns an <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance
    /// that connects nodes associated with <paramref name="firstKey"/> and <paramref name="secondKey"/>.</param>
    /// <returns><b>true</b> when the edge exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool TryGetEdge(TKey firstKey, TKey secondKey, [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result);
}
