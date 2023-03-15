using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Exceptions;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Collections;

public sealed class DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> : IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    internal DirectedGraphEdge(
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> source,
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> target,
        TEdgeValue value,
        GraphDirection direction)
    {
        Assume.IsNotNull( source.Graph, nameof( source ) + '.' + nameof( source.Graph ) );
        Assume.Equals( source.Graph, target.Graph, nameof( source ) + '.' + nameof( source.Graph ) );
        Assume.IsInRange( direction, GraphDirection.In, GraphDirection.Both, nameof( direction ) );
        Source = source;
        Target = target;
        Value = value;
        Direction = direction;
    }

    public TEdgeValue Value { get; set; }
    public GraphDirection Direction { get; private set; }
    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> Source { get; }
    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> Target { get; }

    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>.Source => Source;
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>.Target => Target;

    [Pure]
    public override string ToString()
    {
        return DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>.ForSource( this ).ToString();
    }

    public void ChangeDirection(GraphDirection direction)
    {
        AssertNotRemoved();

        direction = direction.Sanitize();
        if ( direction == GraphDirection.None )
            throw new ArgumentException( Resources.NoneIsNotValidDirection, nameof( direction ) );

        if ( ! ReferenceEquals( Source, Target ) )
            Direction = direction;
    }

    public void Remove()
    {
        AssertNotRemoved();
        Source.Remove( this );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void UnlinkFromGraph()
    {
        Direction = GraphDirection.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AssertNotRemoved()
    {
        if ( Direction == GraphDirection.None )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.EdgeHasBeenRemovedFromGraph ) );
    }
}
