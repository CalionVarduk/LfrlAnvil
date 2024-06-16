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

using System.Collections;
using System.Collections.Generic;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a collection of <see cref="IVariableNode"/> instances.
/// </summary>
public interface IVariableNodeCollection : IEnumerable
{
    /// <summary>
    /// Number of elements in this collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Underlying collection of node keys.
    /// </summary>
    IEnumerable Keys { get; }

    /// <summary>
    /// Underlying collection of <see cref="IVariableNode"/> instances.
    /// </summary>
    IReadOnlyCollection<IVariableNode> Values { get; }

    /// <summary>
    /// Collection of keys of changed nodes.
    /// </summary>
    IEnumerable ChangedNodeKeys { get; }

    /// <summary>
    /// Collection of keys of invalid nodes.
    /// </summary>
    IEnumerable InvalidNodeKeys { get; }

    /// <summary>
    /// Collection of keys of nodes with warnings.
    /// </summary>
    IEnumerable WarningNodeKeys { get; }

    /// <summary>
    /// Collection of keys of nodes set as read-only.
    /// </summary>
    IEnumerable ReadOnlyNodeKeys { get; }

    /// <summary>
    /// Collection of keys of nodes that have been modified since their creation.
    /// </summary>
    IEnumerable DirtyNodeKeys { get; }
}

/// <summary>
/// Represents a collection of <see cref="IVariableNode"/> instances.
/// </summary>
/// <typeparam name="TKey">Node's key type.</typeparam>
public interface IVariableNodeCollection<TKey> : IReadOnlyDictionary<TKey, IVariableNode>, IVariableNodeCollection
    where TKey : notnull
{
    /// <summary>
    /// Number of elements in this collection.
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// Underlying collection of node keys.
    /// </summary>
    new IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// Underlying collection of <see cref="IVariableNode"/> instances.
    /// </summary>
    new IReadOnlyCollection<IVariableNode> Values { get; }

    /// <summary>
    /// Node key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> Comparer { get; }

    /// <summary>
    /// Collection of keys of changed nodes.
    /// </summary>
    new IReadOnlySet<TKey> ChangedNodeKeys { get; }

    /// <summary>
    /// Collection of keys of invalid nodes.
    /// </summary>
    new IReadOnlySet<TKey> InvalidNodeKeys { get; }

    /// <summary>
    /// Collection of keys of nodes with warnings.
    /// </summary>
    new IReadOnlySet<TKey> WarningNodeKeys { get; }

    /// <summary>
    /// Collection of keys of nodes set as read-only.
    /// </summary>
    new IReadOnlySet<TKey> ReadOnlyNodeKeys { get; }

    /// <summary>
    /// Collection of keys of nodes that have been modified since their creation.
    /// </summary>
    new IReadOnlySet<TKey> DirtyNodeKeys { get; }
}
