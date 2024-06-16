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

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic <see cref="IDirectedGraph{TKey,TNodeValue,TEdgeValue}"/> edge.
/// </summary>
/// <typeparam name="TKey">Graph's key type.</typeparam>
/// <typeparam name="TNodeValue">Graph node's value type.</typeparam>
/// <typeparam name="TEdgeValue">Graph edge's value type.</typeparam>
public interface IDirectedGraphEdge<TKey, TNodeValue, TEdgeValue>
    where TKey : notnull
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    TEdgeValue Value { get; }

    /// <summary>
    /// Direction of this edge, from <see cref="Source"/> to <see cref="Target"/>.
    /// </summary>
    GraphDirection Direction { get; }

    /// <summary>
    /// Source node of this edge.
    /// </summary>
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> Source { get; }

    /// <summary>
    /// Target node of this edge.
    /// </summary>
    IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> Target { get; }
}
