using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections.Extensions;

public static class DirectedGraphEdgeExtensions
{
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

    [Pure]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> GetSourceInfo<TKey, TNodeValue, TEdgeValue>(
        this IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
        where TKey : notnull
    {
        return DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>.ForSource( edge );
    }

    [Pure]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> GetTargetInfo<TKey, TNodeValue, TEdgeValue>(
        this IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
        where TKey : notnull
    {
        return DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>.ForTarget( edge );
    }
}
