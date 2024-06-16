// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Extensions;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents an information about two <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> instances connected together
/// through the same <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}"/> instance.
/// </summary>
/// <typeparam name="TKey">Graph entry's key type.</typeparam>
/// <typeparam name="TNodeValue">Graph node's value type.</typeparam>
/// <typeparam name="TEdgeValue">Graph edge's value type.</typeparam>
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

    /// <summary>
    /// Graph edge that connects <see cref="From"/> and <see cref="To"/>.
    /// </summary>
    public IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> Edge { get; }

    /// <summary>
    /// Source graph node.
    /// </summary>
    public IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> From { get; }

    /// <summary>
    /// Destination graph node.
    /// </summary>
    public IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> To { get; }

    /// <summary>
    /// Direction of the connection between <see cref="From"/> and <see cref="To"/>.
    /// </summary>
    public GraphDirection Direction { get; }

    /// <summary>
    /// Specifies whether or not the traversal from the <see cref="From"/> node to the <see cref="To"/> node is valid.
    /// </summary>
    public bool CanReach => (Direction & GraphDirection.Out) != GraphDirection.None;

    /// <summary>
    /// Specifies whether or not the traversal from the <see cref="To"/> node to the <see cref="From"/> node is valid.
    /// </summary>
    public bool CanBeReached => (Direction & GraphDirection.In) != GraphDirection.None;

    /// <summary>
    /// Returns a string representation of this <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
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

    /// <summary>
    /// Creates a new <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance
    /// with <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}.Source"/> node being the <see cref="From"/> node.
    /// </summary>
    /// <param name="edge">Source graph edge.</param>
    /// <returns>New <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> ForSource(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return new DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>( edge, edge.Source, edge.Target, edge.Direction );
    }

    /// <summary>
    /// Creates a new <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance
    /// with <see cref="IDirectedGraphEdge{TKey,TNodeValue,TEdgeValue}.Target"/> node being the <see cref="From"/> node.
    /// </summary>
    /// <param name="edge">Source graph edge.</param>
    /// <returns>New <see cref="DirectedGraphEdgeInfo{TKey,TNodeValue,TEdgeValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue> ForTarget(IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue> edge)
    {
        return new DirectedGraphEdgeInfo<TKey, TNodeValue, TEdgeValue>( edge, edge.Target, edge.Source, edge.Direction.Invert() );
    }
}
