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
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Creates instances of <see cref="Mutation{T}"/> type.
/// </summary>
public static class Mutation
{
    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="oldValue">Old value.</param>
    /// <param name="value">New value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Mutation{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Create<T>(T oldValue, T value)
    {
        return new Mutation<T>( oldValue, value );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Mutation{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Mutation{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Mutation{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Mutation<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
