using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic directed graph data structure.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TNodeValue">Node's value type.</typeparam>
/// <typeparam name="TEdgeValue">Node's edge type.</typeparam>
public interface IDirectedGraph<TKey, TNodeValue, TEdgeValue> : IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    /// <summary>
    /// Adds a new node to this graph.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    /// <returns>Created <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When the provided <paramref name="key"/> already exists in this graph.</exception>
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> AddNode(TKey key, TNodeValue value);

    /// <summary>
    /// Attempts to add a new node to this graph.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    /// <param name="added">
    /// <b>out</b> parameter that returns the created <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instance.
    /// </param>
    /// <returns><b>true</b> when a new node was added, otherwise <b>false</b>.</returns>
    bool TryAddNode(TKey key, TNodeValue value, [MaybeNullWhen( false )] out IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> added);

    /// <summary>
    /// Gets an existing node or adds a new node to this graph if it does not exist.
    /// </summary>
    /// <param name="key">Node's key.</param>
    /// <param name="value">Node's value.</param>
    /// <returns>
    /// An existing <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> with unchanged
    /// <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}.Value"/> or a created
    /// <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instance if it did not exist.
    /// </returns>
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> GetOrAddNode(TKey key, TNodeValue value);

    /// <summary>
    /// Adds a new edge to this graph.
    /// </summary>
    /// <param name="firstKey">Source node's key.</param>
    /// <param name="secondKey">Target node's key.</param>
    /// <param name="value">Edge's value.</param>
    /// <param name="direction">Edge's direction. Equal to <see cref="GraphDirection.Out"/> by default.</param>
    /// <returns>Created <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    /// <exception cref="KeyNotFoundException">When source or target node's key does not exist in this graph.</exception>
    /// <exception cref="ArgumentException">
    /// When <paramref name="direction"/> is equal to <see cref="GraphDirection.None"/> or when the edge already exists.
    /// </exception>
    /// <remarks>
    /// When source and target nodes are the same, then the <paramref name="direction"/> will be equal to <see cref="GraphDirection.Both"/>.
    /// </remarks>
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> AddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction = GraphDirection.Out);

    /// <summary>
    /// Attempts to add a new edge to this graph.
    /// </summary>
    /// <param name="firstKey">Source node's key.</param>
    /// <param name="secondKey">Target node's key.</param>
    /// <param name="value">Edge's value.</param>
    /// <param name="direction">Edge's direction.</param>
    /// <param name="added">
    /// <b>out</b> parameter that returns the created <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance.
    /// </param>
    /// <returns><b>true</b> when a new edge was added, otherwise <b>false</b>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="direction"/> is equal to <see cref="GraphDirection.None"/>.</exception>
    /// <remarks>
    /// When source and target nodes are the same, then the <paramref name="direction"/> will be equal to <see cref="GraphDirection.Both"/>.
    /// </remarks>
    bool TryAddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> added);

    /// <summary>
    /// Attempts to remove a node associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns><b>true</b> when node was removed, otherwise <b>false</b>.</returns>
    /// <remarks>Removes all connected edges as well.</remarks>
    bool RemoveNode(TKey key);

    /// <summary>
    /// Attempts to remove a node associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <param name="removed"><b>out</b> parameter that returns the removed node's value.</param>
    /// <returns><b>true</b> when node was removed, otherwise <b>false</b>.</returns>
    /// <remarks>Removes all connected edges as well.</remarks>
    bool RemoveNode(TKey key, [MaybeNullWhen( false )] out TNodeValue removed);

    /// <summary>
    /// Attempts to remove an edge that connects nodes associated
    /// with the specified <paramref name="firstKey"/> and <paramref name="secondKey"/>.
    /// </summary>
    /// <param name="firstKey">Key of the first node.</param>
    /// <param name="secondKey">Key of the second node.</param>
    /// <returns><b>true</b> when edge was removed, otherwise <b>false</b>.</returns>
    bool RemoveEdge(TKey firstKey, TKey secondKey);

    /// <summary>
    /// Attempts to remove an edge that connects nodes associated
    /// with the specified <paramref name="firstKey"/> and <paramref name="secondKey"/>.
    /// </summary>
    /// <param name="firstKey">Key of the first node.</param>
    /// <param name="secondKey">Key of the second node.</param>
    /// <param name="removed"><b>out</b> parameter that returns the removed edge's value.</param>
    /// <returns><b>true</b> when edge was removed, otherwise <b>false</b>.</returns>
    bool RemoveEdge(TKey firstKey, TKey secondKey, [MaybeNullWhen( false )] out TEdgeValue removed);

    /// <summary>
    /// Attempts to remove the provided <paramref name="node"/>.
    /// </summary>
    /// <param name="node">Node to remove.</param>
    /// <returns><b>true</b> when node was removed, otherwise <b>false</b>.</returns>
    /// <remarks>Removes all connected edges as well.</remarks>
    bool Remove(IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node);

    /// <summary>
    /// Attempts to remove the provided <paramref name="edge"/>.
    /// </summary>
    /// <param name="edge">Edge to remove.</param>
    /// <returns><b>true</b> when edge was removed, otherwise <b>false</b>.</returns>
    bool Remove(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge);

    /// <summary>
    /// Removes all nodes and edges from this graph.
    /// </summary>
    void Clear();
}
