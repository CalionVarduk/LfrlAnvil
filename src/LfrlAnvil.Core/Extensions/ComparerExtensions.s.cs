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
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IComparer{T}"/> extension methods.
/// </summary>
public static class ComparerExtensions
{
    /// <summary>
    /// Creates a new <see cref="IComparer{T}"/> instance that inverts comparison result returned by the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source comparer.</param>
    /// <typeparam name="T">Comparer value type.</typeparam>
    /// <returns>New <see cref="IComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> Invert<T>(this IComparer<T> source)
    {
        return new InvertedComparer<T>( source );
    }
}
