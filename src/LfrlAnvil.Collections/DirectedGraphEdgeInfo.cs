using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Extensions;

namespace LfrlAnvil.Collections;

public readonly struct DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    private DirectedGraphEdgeInfo(
        IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge,
        IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> from,
        IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> to,
        GraphDirection direction)
    {
        Edge = edge;
        From = from;
        To = to;
        Direction = direction;
    }

    public IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> Edge { get; }
    public IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> From { get; }
    public IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> To { get; }
    public GraphDirection Direction { get; }
    public bool CanReach => (Direction & GraphDirection.Out) != GraphDirection.None;
    public bool CanBeReached => (Direction & GraphDirection.In) != GraphDirection.None;

    [Pure]
    public override string ToString()
    {
        var directionText = Direction switch
        {
            GraphDirection.In => "<=",
            GraphDirection.Out => "=>",
            GraphDirection.Both => "<=>",
            _ => "=/="
        };

        return $"{From.Key} {directionText} {To.Key}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> ForSource(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return new DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>( edge, edge.Source, edge.Target, edge.Direction );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> ForTarget(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return new DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>( edge, edge.Target, edge.Source, edge.Direction.Invert() );
    }
}
