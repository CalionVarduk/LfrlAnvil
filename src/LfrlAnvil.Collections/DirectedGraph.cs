using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections;

public class DirectedGraph<TKey, TNodeValue, TEdgeValue> : IDirectedGraph<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DirectedGraphNode<TKey, TNodeValue, TEdgeValue>> _nodes;

    public DirectedGraph()
        : this( EqualityComparer<TKey>.Default ) { }

    public DirectedGraph(IEqualityComparer<TKey> keyComparer)
    {
        _nodes = new Dictionary<TKey, DirectedGraphNode<TKey, TNodeValue, TEdgeValue>>( keyComparer );
    }

    public IEqualityComparer<TKey> KeyComparer => _nodes.Comparer;
    public IReadOnlyCollection<DirectedGraphNode<TKey, TNodeValue, TEdgeValue>> Nodes => _nodes.Values;

    public IEnumerable<DirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> Edges
    {
        get
        {
            foreach ( var node in _nodes.Values )
            {
                foreach ( var edge in node.Edges )
                {
                    if ( ReferenceEquals( node, edge.Source ) )
                        yield return edge;
                }
            }
        }
    }

    IReadOnlyCollection<IDirectedGraphNode<TKey, TNodeValue, TEdgeValue>> IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>.Nodes =>
        Nodes;

    IEnumerable<IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>> IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>.Edges => Edges;

    [Pure]
    public bool ContainsNode(TKey key)
    {
        return _nodes.ContainsKey( key );
    }

    [Pure]
    public bool Contains(DirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
    {
        return ReferenceEquals( this, node.Graph );
    }

    [Pure]
    public bool Contains(IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
    {
        return node is DirectedGraphNode<TKey, TNodeValue, TEdgeValue> n && Contains( n );
    }

    [Pure]
    public bool ContainsEdge(TKey firstKey, TKey secondKey)
    {
        return TryGetNode( firstKey, out var node ) && node.ContainsEdgeTo( secondKey );
    }

    [Pure]
    public bool Contains(DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return edge.Direction != GraphDirection.None && ReferenceEquals( this, edge.Source.Graph );
    }

    [Pure]
    public bool Contains(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return edge is DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> e && Contains( e );
    }

    [Pure]
    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> GetNode(TKey key)
    {
        return _nodes[key];
    }

    public bool TryGetNode(TKey key, [MaybeNullWhen( false )] out DirectedGraphNode<TKey, TNodeValue, TEdgeValue> result)
    {
        return _nodes.TryGetValue( key, out result );
    }

    [Pure]
    public DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> GetEdge(TKey firstKey, TKey secondKey)
    {
        return GetNode( firstKey ).GetEdgeTo( secondKey );
    }

    public bool TryGetEdge(
        TKey firstKey,
        TKey secondKey,
        [MaybeNullWhen( false )] out DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        if ( TryGetNode( firstKey, out var node ) )
            return node.TryGetEdgeTo( secondKey, out result );

        result = null;
        return false;
    }

    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> AddNode(TKey key, TNodeValue value)
    {
        var node = new DirectedGraphNode<TKey, TNodeValue, TEdgeValue>( this, key, value );
        _nodes.Add( key, node );
        return node;
    }

    public bool TryAddNode(TKey key, TNodeValue value, [MaybeNullWhen( false )] out DirectedGraphNode<TKey, TNodeValue, TEdgeValue> added)
    {
        ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _nodes, key, out var exists );
        if ( exists )
        {
            added = null;
            return false;
        }

        node = new DirectedGraphNode<TKey, TNodeValue, TEdgeValue>( this, key, value );
        added = node;
        return true;
    }

    public DirectedGraphNode<TKey, TNodeValue, TEdgeValue> GetOrAddNode(TKey key, TNodeValue value)
    {
        ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _nodes, key, out var exists )!;
        if ( ! exists )
            node = new DirectedGraphNode<TKey, TNodeValue, TEdgeValue>( this, key, value );

        return node;
    }

    public DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> AddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction = GraphDirection.Out)
    {
        return GetNode( firstKey ).AddEdgeTo( secondKey, value, direction );
    }

    public bool TryAddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> added)
    {
        if ( TryGetNode( firstKey, out var node ) )
            return node.TryAddEdgeTo( secondKey, value, direction, out added );

        added = null;
        return false;
    }

    public bool RemoveNode(TKey key)
    {
        if ( ! _nodes.Remove( key, out var node ) )
            return false;

        node.UnlinkFromGraph();
        return true;
    }

    public bool RemoveNode(TKey key, [MaybeNullWhen( false )] out TNodeValue removed)
    {
        if ( ! _nodes.Remove( key, out var node ) )
        {
            removed = default;
            return false;
        }

        removed = node.Value;
        node.UnlinkFromGraph();
        return true;
    }

    public bool RemoveEdge(TKey firstKey, TKey secondKey)
    {
        return _nodes.TryGetValue( firstKey, out var node ) && node.RemoveEdgeTo( secondKey );
    }

    public bool RemoveEdge(TKey firstKey, TKey secondKey, [MaybeNullWhen( false )] out TEdgeValue removed)
    {
        if ( _nodes.TryGetValue( firstKey, out var node ) )
            return node.RemoveEdgeTo( secondKey, out removed );

        removed = default;
        return false;
    }

    public bool Remove(DirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
    {
        if ( ! ReferenceEquals( this, node.Graph ) )
            return false;

        _nodes.Remove( node.Key );
        node.UnlinkFromGraph();
        return true;
    }

    public bool Remove(IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
    {
        return node is DirectedGraphNode<TKey, TNodeValue, TEdgeValue> n && Remove( n );
    }

    public bool Remove(DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return ReferenceEquals( this, edge.Source.Graph ) && edge.Source.Remove( edge );
    }

    public bool Remove(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return edge is DirectedGraphEdge<TKey, TNodeValue, TEdgeValue> e && Remove( e );
    }

    public void Clear()
    {
        foreach ( var node in _nodes.Values )
            node.ClearFromGraph();

        _nodes.Clear();
    }

    [Pure]
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>.GetNode(TKey key)
    {
        return GetNode( key );
    }

    bool IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>.TryGetNode(
        TKey key,
        [MaybeNullWhen( false )] out IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> result)
    {
        var exists = TryGetNode( key, out var node );
        return OptionalValues.TryGet( exists, node, out result );
    }

    [Pure]
    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>.GetEdge(
        TKey firstKey,
        TKey secondKey)
    {
        return GetEdge( firstKey, secondKey );
    }

    bool IReadOnlyDirectedGraph<TKey, TNodeValue, TEdgeValue>.TryGetEdge(
        TKey firstKey,
        TKey secondKey,
        [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> result)
    {
        var exists = TryGetEdge( firstKey, secondKey, out var edge );
        return OptionalValues.TryGet( exists, edge, out result );
    }

    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IDirectedGraph<TKey, TNodeValue, TEdgeValue>.AddNode(TKey key, TNodeValue value)
    {
        return AddNode( key, value );
    }

    bool IDirectedGraph<TKey, TNodeValue, TEdgeValue>.TryAddNode(
        TKey key,
        TNodeValue value,
        [MaybeNullWhen( false )] out IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> added)
    {
        var exists = TryAddNode( key, value, out var node );
        return OptionalValues.TryGet( exists, node, out added );
    }

    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> IDirectedGraph<TKey, TNodeValue, TEdgeValue>.GetOrAddNode(TKey key, TNodeValue value)
    {
        return GetOrAddNode( key, value );
    }

    IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> IDirectedGraph<TKey, TNodeValue, TEdgeValue>.AddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction)
    {
        return AddEdge( firstKey, secondKey, value, direction );
    }

    bool IDirectedGraph<TKey, TNodeValue, TEdgeValue>.TryAddEdge(
        TKey firstKey,
        TKey secondKey,
        TEdgeValue value,
        GraphDirection direction,
        [MaybeNullWhen( false )] out IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> added)
    {
        var exists = TryAddEdge( firstKey, secondKey, value, direction, out var edge );
        return OptionalValues.TryGet( exists, edge, out added );
    }
}
