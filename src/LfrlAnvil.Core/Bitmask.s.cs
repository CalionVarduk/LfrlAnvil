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
/// Creates instances of <see cref="Bitmask{T}"/> type.
/// </summary>
public static class Bitmask
{
    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance.
    /// </summary>
    /// <param name="value">Bitmask value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    public static Bitmask<T> Create<T>(T value)
        where T : struct, IConvertible, IComparable
    {
        return new Bitmask<T>( value );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Bitmask{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Bitmask{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Bitmask{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Bitmask<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
