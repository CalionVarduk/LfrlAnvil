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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Internal;

/// <inheritdoc cref="IMemoizedCollection{T}" />
public sealed class MemoizedCollection<T> : IMemoizedCollection<T>
{
    /// <summary>
    /// Creates a new <see cref="MemoizedCollection{T}"/> instance.
    /// </summary>
    /// <param name="source">Source collection.</param>
    public MemoizedCollection(IEnumerable<T> source)
    {
        Source = new Lazy<IReadOnlyCollection<T>>( source.Materialize );
    }

    /// <inheritdoc />
    public Lazy<IReadOnlyCollection<T>> Source { get; }

    /// <inheritdoc />
    public int Count => Source.Value.Count;

    /// <inheritdoc />
    public bool IsMaterialized => Source.IsValueCreated;

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerator<T> GetEnumerator()
    {
        return Source.Value.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
