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
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="Equality{T}"/> type.
/// </summary>
public static class Equality
{
    /// <summary>
    /// Creates a new <see cref="Equality{T}"/> instance.
    /// </summary>
    /// <param name="first">First value to compare.</param>
    /// <param name="second">Second value to compare.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Equality{T}"/> instance.</returns>
    [Pure]
    public static Equality<T> Create<T>(T? first, T? second)
    {
        return new Equality<T>( first, second );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Equality{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Equality{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Equality{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Equality<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
