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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Bounds{T}"/> extension methods.
/// </summary>
public static class BoundsExtensions
{
    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance created from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Conversion source.</param>
    /// <typeparam name="T">Bounds value type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance with two elements: <see cref="Bounds{T}.Min"/> followed by <see cref="Bounds{T}.Max"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Bounds<T> source)
        where T : IComparable<T>
    {
        yield return source.Min;
        yield return source.Max;
    }
}
