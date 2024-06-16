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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IGenerator"/> extension methods.
/// </summary>
public static class GeneratorExtensions
{
    /// <summary>
    /// Creates a new <see cref="IEnumerable"/> instance (potentially infinite) from the provided generator.
    /// </summary>
    /// <param name="source">Source generator.</param>
    /// <returns>New <see cref="IEnumerable"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable ToEnumerable(this IGenerator source)
    {
        while ( source.TryGenerate( out var result ) )
            yield return result;
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance (potentially infinite) from the provided generator.
    /// </summary>
    /// <param name="source">Source generator.</param>
    /// <typeparam name="T">Generator value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> ToEnumerable<T>(this IGenerator<T> source)
    {
        while ( source.TryGenerate( out var result ) )
            yield return result;
    }
}
