using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Exceptions;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}" />
public sealed class DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> : IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    internal DirectedGraphEdge(
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> source,
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> target,
        TEdgeValue value,
        GraphDirection direction)
    {
        Assume.IsNotNull( source.Graph );
        Assume.Equals( source.Graph, target.Graph );
        Assume.IsInRange( direction, GraphDirection.In, GraphDirection.Both );
        Source = source;
        Target = target;
        Value = value;
        Direction = direction;
    }

    /// <inheritdoc />
    public TEdgeValue Value { get; set; }

    /// <inheritdoc />
    public GraphDirection Direction { get; private set; }

    /// <inheritdoc cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}.Source" />
    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> Source { get; }

    /// <inheritdoc cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}.Target" />
    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> Target { get; }

    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>.Source => Source;
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>.Target => Target;

    /// <summary>
    /// Returns a string representation of this <see cref="DirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>.ForSource( this ).ToString();
    }

    /// <summary>
    /// Changes this edge's <see cref="Direction"/>.
    /// </summary>
    /// <param name="direction">New direction.</param>
    /// <exception cref="InvalidOperationException">When this edge has been removed.</exception>
    /// <exception cref="ArgumentException">When <paramref name="direction"/> is equal to <see cref="GraphDirection.None"/>.</exception>
    public void ChangeDirection(GraphDirection direction)
    {
        AssertNotRemoved();

        direction = direction.Sanitize();
        if ( direction == GraphDirection.None )
            throw new ArgumentException( Resources.NoneIsNotValidDirection, nameof( direction ) );

        if ( ! ReferenceEquals( Source, Target ) )
            Direction = direction;
    }

    /// <summary>
    /// Removes this edge from the associated <see cref="DirectedGraph{TKey,TNodeValue,TEdgeValue}"/> instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">When this edge has already been removed.</exception>
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
