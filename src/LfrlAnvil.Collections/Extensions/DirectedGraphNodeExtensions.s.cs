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

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="IDirectedGraphNode{TKey,TNodeValue,TEdgeValue}"/> extension methods.
/// </summary>
public static class DirectedGraphNodeExtensions
{
    /// <summary>
    /// Checks whether or not the <paramref name="node"/> is a root node, that is a node that cannot be reached by any graph edge.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <typeparam name="TKey">Graph key type.</typeparam>
    /// <typeparam name="TNodeValue">Graph node value type.</typeparam>
    /// <typeparam name="TEdgeValue">Graph edge value type.</typeparam>
    /// <returns><b>true</b> when <paramref name="node"/> is considered to be a root node, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool IsRoot<TKey, TNodeValue, TEdgeValue>(this IDirectedGraphNode<TKey, TNodeValue, TEdgeValue> node)
        where TKey : notnull
    {
        foreach ( var edge in node.Edges )
        {
            var info = edge.GetInfo( node );
            if ( info is not null && info.Value.CanBeReached )
                return false;
        }

        return true;
    }
}
