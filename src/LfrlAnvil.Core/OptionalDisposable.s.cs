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

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="OptionalDisposable{T}"/> type.
/// </summary>
public static class OptionalDisposable
{
    /// <summary>
    /// Creates a new <see cref="OptionalDisposable{T}"/> instance.
    /// </summary>
    /// <param name="value">Disposable underlying object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="OptionalDisposable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OptionalDisposable<T> Create<T>(T value)
        where T : IDisposable
    {
        return new OptionalDisposable<T>( value );
    }

    /// <summary>
    /// Attempts to create a new <see cref="OptionalDisposable{T}"/> instance.
    /// </summary>
    /// <param name="value">Disposable underlying object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="OptionalDisposable{T}"/> instance or <see cref="OptionalDisposable{T}.Empty"/> when <paramref name="value"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OptionalDisposable<T> TryCreate<T>(T? value)
        where T : IDisposable
    {
        return Generic<T>.IsNotNull( value ) ? Create( value ) : OptionalDisposable<T>.Empty;
    }

    /// <summary>
    /// Attempts to create a new <see cref="OptionalDisposable{T}"/> instance.
    /// </summary>
    /// <param name="value">Disposable underlying object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="OptionalDisposable{T}"/> instance or <see cref="OptionalDisposable{T}.Empty"/> when <paramref name="value"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static OptionalDisposable<T> TryCreate<T>(T? value)
        where T : struct, IDisposable
    {
        return value.HasValue ? Create( value.Value ) : OptionalDisposable<T>.Empty;
    }

    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="OptionalDisposable{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="OptionalDisposable{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="OptionalDisposable{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( OptionalDisposable<> ) );
        return result.Length == 0 ? null : result[0];
    }
}
