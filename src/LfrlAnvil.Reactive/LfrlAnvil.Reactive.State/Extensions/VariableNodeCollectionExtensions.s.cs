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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Reactive.State.Extensions;

/// <summary>
/// Contains <see cref="IVariableNodeCollection{TKey}"/> extension methods.
/// </summary>
public static class VariableNodeCollectionExtensions
{
    /// <summary>
    /// Returns a collection that contains <see cref="IVariableNode"/> instances marked as invalid.
    /// </summary>
    /// <param name="nodes">Source node collection.</param>
    /// <typeparam name="TKey">Node's key type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IVariableNode> FindAllInvalid<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.InvalidNodeKeys.Select( k => nodes[k] );
    }

    /// <summary>
    /// Returns a collection that contains <see cref="IVariableNode"/> instances marked as containing warnings.
    /// </summary>
    /// <param name="nodes">Source node collection.</param>
    /// <typeparam name="TKey">Node's key type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IVariableNode> FindAllWarning<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.WarningNodeKeys.Select( k => nodes[k] );
    }

    /// <summary>
    /// Returns a collection that contains <see cref="IVariableNode"/> instances marked as changed.
    /// </summary>
    /// <param name="nodes">Source node collection.</param>
    /// <typeparam name="TKey">Node's key type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IVariableNode> FindAllChanged<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.ChangedNodeKeys.Select( k => nodes[k] );
    }

    /// <summary>
    /// Returns a collection that contains <see cref="IVariableNode"/> instances marked as read-only.
    /// </summary>
    /// <param name="nodes">Source node collection.</param>
    /// <typeparam name="TKey">Node's key type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IVariableNode> FindAllReadOnly<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.ReadOnlyNodeKeys.Select( k => nodes[k] );
    }

    /// <summary>
    /// Returns a collection that contains <see cref="IVariableNode"/> instances marked as dirty.
    /// </summary>
    /// <param name="nodes">Source node collection.</param>
    /// <typeparam name="TKey">Node's key type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<IVariableNode> FindAllDirty<TKey>(this IVariableNodeCollection<TKey> nodes)
        where TKey : notnull
    {
        return nodes.DirtyNodeKeys.Select( k => nodes[k] );
    }
}
