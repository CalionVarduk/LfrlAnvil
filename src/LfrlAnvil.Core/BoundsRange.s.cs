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
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="BoundsRange{T}"/> type.
/// </summary>
public static class BoundsRange
{
    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from a single <see cref="Bounds{T}"/> instance.
    /// </summary>
    /// <param name="value">Single range.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<T> Create<T>(Bounds<T> value)
        where T : IComparable<T>
    {
        return new BoundsRange<T>( value );
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from a collection of <see cref="Bounds{T}"/> instances.
    /// </summary>
    /// <param name="range">Collection of ranges.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    /// <exception cref="ArgumentException">When <paramref name="range"/> is not ordered.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BoundsRange<T> Create<T>(IEnumerable<Bounds<T>> range)
        where T : IComparable<T>
    {
        return new BoundsRange<T>( range );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="BoundsRange{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="BoundsRange{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="BoundsRange{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( BoundsRange<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
