using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Exceptions;
using LfrlAnvil.Collections.Extensions;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Collections;

public sealed class DirectedGraphNode<TKey, TNodeValue, TEdgeValue> : IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> _edges;

    internal DirectedGraphNode(DirectedGraph<TKey, TNodeValue, TEdgeValue> graph, TKey key, TNodeValue value)
    {
        Key = key;
        Value = value;
        Graph = graph;
        _edges = new Dictionary<TKey, DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>>( graph.KeyComparer );
    }

    public TKey Key { get; }
    public TNodeValue Value { get; set; }
    public DirectedGraph<TKey, TNodeValue, TEdgeValue>? Graph { get; private set; }
    public IReadOnlyCollection<DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> Edges => _edges.Values;

    IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>? IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>.Graph => Graph;
    IReadOnlyCollection<IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>.Edges => Edges;

    [Pure]
    public override string ToString()
    {
        return $"{Key} => {Value}";
    }

    [Pure]
    public IEnumerable<DirectedGraphNode<TKey, TNodeValue, TEdgeValue>> GetReachableNodes(GraphDirection direction = GraphDirection.Out)
    {
        direction = direction.Sanitize();
        if ( Graph is null || direction == GraphDirection.None || _edges.Count == 0 )
            yield break;

        var invertedDirection = direction.Invert();
        var reachedKeys = new HashSet<TKey>( Graph.KeyComparer );
        var nodes = new Queue<DirectedGraphNode<TKey, TNodeValue, TEdgeValue>>();
        AddReachableNodesToQueue( this, nodes, reachedKeys, direction, invertedDirection );

        while ( nodes.TryDequeue( out var node ) )
        {
            yield return node;

            AddReachableNodesToQueue( node, nodes, reachedKeys, direction, invertedDirection );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static void AddReachableNodesToQueue(
            DirectedGraphNode<TKey, TNodeValue, TEdgeValue> node,
            Queue<DirectedGraphNode<TKey, TNodeValue, TEdgeValue>> queue,
            HashSet<TKey> reached,
            GraphDirection direction,
            GraphDirection invertedDirection)
        {
            foreach ( var edge in node._edges.Values )
            {
                if ( ReferenceEquals( node, edge.Source ) )
                {
                    if ( (edge.Direction & direction) != GraphDirection.None && reached.Add( edge.Target.Key ) )
                        queue.Enqueue( edge.Target );
                }
                else
                {
                    if ( (edge.Direction & invertedDirection) != GraphDirection.None && reached.Add( edge.Source.Key ) )
                        queue.Enqueue( edge.Source );
                }
            }
        }
    }

    [Pure]
    public bool ContainsEdgeTo(TKey key)
    {
        return _edges.ContainsKey( key );
    }

    [Pure]
    public DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> GetEdgeTo(TKey key)
    {
        return _edges[key];
    }

    public bool TryGetEdgeTo(TKey key, [MaybeNullWhen( false )] out DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        return _edges.TryGetValue( key, out result );
    }

    public DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> AddEdgeTo(
        TKey key,
        TEdgeValue value,
        GraphDirection direction = GraphDirection.Out)
    {
        AssertNotRemoved();
        Assume.IsNotNull( Graph );
        return AddEdgeInternal( Graph.GetNode( key ), value, direction );
    }

    public bool TryAddEdgeTo(
        TKey key,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        AssertNotRemoved();
        Assume.IsNotNull( Graph );

        if ( Graph.TryGetNode( key, out var target ) )
            return TryAddEdgeInternal( target, value, direction, out result );

        result = null;
        return false;
    }

    public DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> AddEdgeTo(
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> node,
        TEdgeValue value,
        GraphDirection direction = GraphDirection.Out)
    {
        AssertNotRemoved();

        if ( ! ReferenceEquals( Graph, node.Graph ) )
            throw new ArgumentException( Resources.NodesMustBelongToTheSameGraph, nameof( node ) );

        return AddEdgeInternal( node, value, direction );
    }

    public bool TryAddEdgeTo(
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> node,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        AssertNotRemoved();

        if ( ReferenceEquals( Graph, node.Graph ) )
            return TryAddEdgeInternal( node, value, direction, out result );

        result = null;
        return false;
    }

    public bool RemoveEdgeTo(TKey key)
    {
        AssertNotRemoved();

        if ( ! _edges.Remove( key, out var edge ) )
            return false;

        UnlinkEdge( edge );
        return true;
    }

    public bool RemoveEdgeTo(TKey key, [MaybeNullWhen( false )] out TEdgeValue removed)
    {
        AssertNotRemoved();

        if ( ! _edges.Remove( key, out var edge ) )
        {
            removed = default;
            return false;
        }

        UnlinkEdge( edge );
        removed = edge.Value;
        return true;
    }

    public bool Remove(DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        AssertNotRemoved();

        if ( edge.Direction == GraphDirection.None )
            return false;

        if ( ReferenceEquals( this, edge.Source ) )
            _edges.Remove( edge.Target.Key );
        else if ( ReferenceEquals( this, edge.Target ) )
            _edges.Remove( edge.Source.Key );
        else
            return false;

        UnlinkEdge( edge );
        return true;
    }

    public int RemoveEdges(GraphDirection direction)
    {
        AssertNotRemoved();

        direction = direction.Sanitize();
        if ( direction == GraphDirection.None )
            return 0;

        if ( direction == GraphDirection.Both )
        {
            var count = _edges.Count;
            UnlinkAllEdges();
            return count;
        }

        var invertedDirection = direction.Invert();
        var keysToRemove = new List<TKey>();

        foreach ( var (key, edge) in _edges )
        {
            var newDirection = edge.Direction & ~(ReferenceEquals( this, edge.Source ) ? direction : invertedDirection);
            if ( newDirection == GraphDirection.None )
                keysToRemove.Add( key );
            else
                edge.ChangeDirection( newDirection );
        }

        foreach ( var key in keysToRemove )
        {
            _edges.Remove( key, out var edge );
            Assume.IsNotNull( edge );
            UnlinkEdge( edge );
        }

        return keysToRemove.Count;
    }

    public void Remove()
    {
        AssertNotRemoved();
        Assume.IsNotNull( Graph );
        Graph.Remove( this );
    }

    internal void UnlinkFromGraph()
    {
        Assume.IsNotNull( Graph );
        Graph = null;
        UnlinkAllEdges();
    }

    internal void ClearFromGraph()
    {
        Assume.IsNotNull( Graph );
        Graph = null;

        foreach ( var edge in _edges.Values )
            edge.UnlinkFromGraph();

        _edges.Clear();
    }

    internal void UnlinkAllEdges()
    {
        foreach ( var edge in _edges.Values )
            UnlinkEdge( edge );

        _edges.Clear();
    }

    private DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> AddEdgeInternal(
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> target,
        TEdgeValue value,
        GraphDirection direction)
    {
        Assume.IsNotNull( Graph );

        direction = direction.Sanitize();
        if ( direction == GraphDirection.None )
            ExceptionThrower.Throw( new ArgumentException( Resources.NoneIsNotValidDirection, nameof( direction ) ) );

        DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge;
        if ( ReferenceEquals( this, target ) )
        {
            edge = new DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>( this, this, value, GraphDirection.Both );
            _edges.Add( Key, edge );
        }
        else
        {
            edge = new DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>( this, target, value, direction );
            _edges.Add( target.Key, edge );
            target._edges.Add( Key, edge );
        }

        return edge;
    }

    private bool TryAddEdgeInternal(
        DirectedGraphNode<TKey, TNodeValue, TEdgeValue> target,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        Assume.IsNotNull( Graph );

        direction = direction.Sanitize();
        if ( direction == GraphDirection.None )
            ExceptionThrower.Throw( new ArgumentException( Resources.NoneIsNotValidDirection, nameof( direction ) ) );

        ref var edge = ref CollectionsMarshal.GetValueRefOrAddDefault( _edges, target.Key, out var exists );
        if ( exists )
        {
            result = null;
            return false;
        }

        if ( ReferenceEquals( this, target ) )
            edge = new DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>( this, this, value, GraphDirection.Both );
        else
        {
            edge = new DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>( this, target, value, direction );
            target._edges.Add( Key, edge );
        }

        result = edge;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UnlinkEdge(DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        edge.UnlinkFromGraph();
        if ( ReferenceEquals( edge.Source, edge.Target ) )
            return;

        var other = ReferenceEquals( this, edge.Source ) ? edge.Target : edge.Source;
        other._edges.Remove( Key );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AssertNotRemoved()
    {
        if ( Graph is null )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.NodeHasBeenRemovedFromGraph ) );
    }

    [Pure]
    IEnumerable<IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>> IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>.GetReachableNodes(
        GraphDirection direction)
    {
        return GetReachableNodes( direction );
    }

    [Pure]
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>.GetEdgeTo(TKey key)
    {
        return GetEdgeTo( key );
    }

    bool IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>.TryGetEdgeTo(
        TKey key,
        [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        var exists = TryGetEdgeTo( key, out var edge );
        return OptionalValues.TryGet( exists, edge, out result );
    }
}
