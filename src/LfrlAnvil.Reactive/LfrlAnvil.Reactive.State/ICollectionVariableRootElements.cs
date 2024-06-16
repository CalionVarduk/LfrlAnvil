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
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a type-erased collection of <see cref="IVariableNode"/> elements
/// that belong to an <see cref="IReadOnlyCollectionVariableRoot"/>.
/// </summary>
public interface ICollectionVariableRootElements : IEnumerable
{
    /// <summary>
    /// Number of elements in this collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Underlying collection of element keys.
    /// </summary>
    IEnumerable Keys { get; }

    /// <summary>
    /// Underlying collection of <see cref="IVariableNode"/> elements.
    /// </summary>
    IReadOnlyCollection<IVariableNode> Values { get; }

    /// <summary>
    /// Collection of keys of invalid nodes.
    /// </summary>
    IEnumerable InvalidElementKeys { get; }

    /// <summary>
    /// Collection of keys of nodes with warnings.
    /// </summary>
    IEnumerable WarningElementKeys { get; }

    /// <summary>
    /// Collection of keys of added elements.
    /// </summary>
    IEnumerable AddedElementKeys { get; }

    /// <summary>
    /// Collection of keys of removed elements.
    /// </summary>
    IEnumerable RemovedElementKeys { get; }

    /// <summary>
    /// Collection of keys of changed elements.
    /// </summary>
    IEnumerable ChangedElementKeys { get; }
}

/// <summary>
/// Represents a generic collection of elements that belong to an <see cref="IReadOnlyCollectionVariableRoot{TKey,TElement}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public interface ICollectionVariableRootElements<TKey, TElement> : ICollectionVariableRootElements, IReadOnlyDictionary<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Number of elements in this collection.
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// Underlying collection of element keys.
    /// </summary>
    new IReadOnlyCollection<TKey> Keys { get; }

    /// <summary>
    /// Underlying collection of <see cref="VariableNode"/> elements.
    /// </summary>
    new IReadOnlyCollection<TElement> Values { get; }

    /// <summary>
    /// Collection of keys of invalid nodes.
    /// </summary>
    new IReadOnlySet<TKey> InvalidElementKeys { get; }

    /// <summary>
    /// Collection of keys of nodes with warnings.
    /// </summary>
    new IReadOnlySet<TKey> WarningElementKeys { get; }

    /// <summary>
    /// Collection of keys of added elements.
    /// </summary>
    new IReadOnlySet<TKey> AddedElementKeys { get; }

    /// <summary>
    /// Collection of keys of removed elements.
    /// </summary>
    new IReadOnlySet<TKey> RemovedElementKeys { get; }

    /// <summary>
    /// Collection of keys of changed elements.
    /// </summary>
    new IReadOnlySet<TKey> ChangedElementKeys { get; }

    /// <summary>
    /// Element key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> KeyComparer { get; }
}
