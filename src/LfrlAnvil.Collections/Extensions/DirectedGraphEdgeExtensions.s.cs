using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> extension methods.
/// </summary>
public static class DirectedGraphEdgeExtensions
{
    /// <summary>
    /// Attempts to create a new <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance
    /// with the provided <paramref name="node"/> being the <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}.From"/> node.
    /// </summary>
    /// <param name="edge">Source graph edge.</param>
    /// <param name="node">Expected <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}.From"/> node.</param>
    /// <typeparam name="TKey">Graph's key type.</typeparam>
    /// <typeparam name="TNodeValue">Graph node value's type.</typeparam>
    /// <typeparam name="TEdgeValue">Graph edge value's type.</typeparam>
    /// <returns>
    /// New <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance
    /// or null if the provided <paramref name="node"/> does not belong to the specified <paramref name="edge"/>.
    /// </returns>
    [Pure]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>? GetInfo<TKey, TNodeValue, TEdgeValue>(
        this IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge,
        IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
        where TKey : notnull
    {
        if ( ReferenceEquals( edge.Source, node ) )
            return GetSourceInfo( edge );

        if ( ReferenceEquals( edge.Target, node ) )
            return GetTargetInfo( edge );

        return null;
    }

    /// <summary>
    /// Creates a new <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance
    /// with <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}.Source"/> node being
    /// the <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}.From"/> node.
    /// </summary>
    /// <param name="edge">Source graph edge.</param>
    /// <typeparam name="TKey">Graph's key type.</typeparam>
    /// <typeparam name="TNodeValue">Graph node's value type.</typeparam>
    /// <typeparam name="TEdgeValue">Graph edge's value type.</typeparam>
    /// <returns>New <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    [Pure]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> GetSourceInfo<TKey, TNodeValue, TEdgeValue>(
        this IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
        where TKey : notnull
    {
        return DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>.ForSource( edge );
    }

    /// <summary>
    /// Creates a new <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance
    /// with <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}.Target"/> node being
    /// the <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}.From"/> node.
    /// </summary>
    /// <param name="edge">Source graph edge.</param>
    /// <typeparam name="TKey">Graph's key type.</typeparam>
    /// <typeparam name="TNodeValue">Graph node's value type.</typeparam>
    /// <typeparam name="TEdgeValue">Graph edge's value type.</typeparam>
    /// <returns>New <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    [Pure]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> GetTargetInfo<TKey, TNodeValue, TEdgeValue>(
        this IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
        where TKey : notnull
    {
        return DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>.ForTarget( edge );
    }
}
