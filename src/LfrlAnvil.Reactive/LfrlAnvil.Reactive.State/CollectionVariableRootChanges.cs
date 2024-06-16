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

using System;
using System.Collections.Generic;
using LfrlAnvil.Reactive.State.Internal;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents changes to make to <see cref="ICollectionVariableRoot{TKey,TElement,TValidationResult}"/>.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
public readonly struct CollectionVariableRootChanges<TKey, TElement>
    where TKey : notnull
    where TElement : VariableNode
{
    /// <summary>
    /// Represents an empty collection.
    /// </summary>
    public static readonly CollectionVariableRootChanges<TKey, TElement> Empty = new CollectionVariableRootChanges<TKey, TElement>();

    private readonly IEnumerable<TElement>? _elementsToAdd;
    private readonly IEnumerable<TKey>? _keysToRestore;

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRootChanges{TKey,TElement}"/> instance.
    /// </summary>
    /// <param name="elementsToAdd">Collection of elements to add.</param>
    /// <param name="keysToRestore">Collection of keys of removed elements to restore.</param>
    public CollectionVariableRootChanges(IEnumerable<TElement> elementsToAdd, IEnumerable<TKey> keysToRestore)
    {
        _elementsToAdd = elementsToAdd;
        _keysToRestore = keysToRestore;
    }

    /// <summary>
    /// Collection of elements to add.
    /// </summary>
    public IEnumerable<TElement> ElementsToAdd => _elementsToAdd ?? Array.Empty<TElement>();

    /// <summary>
    /// Collection of keys of removed elements to restore.
    /// </summary>
    public IEnumerable<TKey> KeysToRestore => _keysToRestore ?? Array.Empty<TKey>();
}
