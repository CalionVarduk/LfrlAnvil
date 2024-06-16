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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Memory{T}"/> extension methods.
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// Returns an enumerator instance created from the given memory's <see cref="ReadOnlyMemory{T}.Span"/>.
    /// </summary>
    /// <param name="source">Read-only source memory.</param>
    /// <typeparam name="T">Memory element type.</typeparam>
    /// <returns>Underlying memory's span's enumerator.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlySpan<T>.Enumerator GetEnumerator<T>(this ReadOnlyMemory<T> source)
    {
        return source.Span.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator instance created from the given memory's <see cref="Memory{T}.Span"/>.
    /// </summary>
    /// <param name="source">Source memory.</param>
    /// <typeparam name="T">Memory element type.</typeparam>
    /// <returns>Underlying memory's span's enumerator.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Span<T>.Enumerator GetEnumerator<T>(this Memory<T> source)
    {
        return source.Span.GetEnumerator();
    }
}
