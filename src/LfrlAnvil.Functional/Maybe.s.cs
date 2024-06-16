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
using LfrlAnvil.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Creates instances of <see cref="Maybe{T}"/> type.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Represents a lack of value.
    /// </summary>
    public static readonly Nil None = Nil.Instance;

    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance with a value.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="value"/> is null.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> Some<T>(T? value)
        where T : notnull
    {
        if ( Generic<T>.IsNull( value ) )
            ExceptionThrower.Throw( new ArgumentNullException( nameof( value ) ) );

        return new Maybe<T>( value );
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Maybe{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Maybe{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Maybe{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Maybe<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
