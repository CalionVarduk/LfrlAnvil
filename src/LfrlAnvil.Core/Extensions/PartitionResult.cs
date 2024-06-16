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
using System.Linq;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Represents a lightweight result of a <see cref="EnumerableExtensions.Partition{T}(IEnumerable{T},Func{T,Boolean})"/> operation.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly struct PartitionResult<T>
{
    private readonly List<T>? _items;
    private readonly int _passedItemCount;

    internal PartitionResult(List<T> items, int passedItemCount)
    {
        _items = items;
        _passedItemCount = passedItemCount;
    }

    /// <summary>
    /// All items that took part in the partitioning.
    /// </summary>
    public IReadOnlyList<T> Items => _items ?? ( IReadOnlyList<T> )Array.Empty<T>();

    /// <summary>
    /// All items that passed the given partitioning predicate.
    /// </summary>
    public IEnumerable<T> PassedItems => Items.Take( _passedItemCount );

    /// <summary>
    /// All items that failed the given partitioning predicate.
    /// </summary>
    public IEnumerable<T> FailedItems => Items.Skip( _passedItemCount );

    /// <summary>
    /// All items that passed the given partitioning predicate in a <see cref="ReadOnlySpan{T}"/> form.
    /// </summary>
    public ReadOnlySpan<T> PassedItemsSpan => CollectionsMarshal.AsSpan( _items ).Slice( 0, _passedItemCount );

    /// <summary>
    /// All items that failed the given partitioning predicate in a <see cref="ReadOnlySpan{T}"/> form.
    /// </summary>
    public ReadOnlySpan<T> FailedItemsSpan => CollectionsMarshal.AsSpan( _items ).Slice( _passedItemCount );
}
