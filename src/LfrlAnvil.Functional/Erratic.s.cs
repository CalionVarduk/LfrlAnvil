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
/// Creates instances of <see cref="Erratic{T}"/> type.
/// </summary>
public static class Erratic
{
    /// <summary>
    /// Attempts to extract the underlying type from the provided <see cref="Erratic{T}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Erratic{T}"/> type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Erratic{T}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Erratic<> ) );
        return result.Length == 0 ? null : result[0];
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> from the provided <paramref name="func"/> delegate invocation result
    /// wrapped in a try-catch block.
    /// </summary>
    /// <param name="func">Delegate to invoke.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<T> Try<T>(Func<T> func)
    {
        try
        {
            return new Erratic<T>( func() );
        }
        catch ( Exception exc )
        {
            return new Erratic<T>( exc );
        }
    }

    /// <summary>
    /// Creates a new <see cref="Erratic{T}"/> from the provided <paramref name="action"/> delegate invocation result
    /// wrapped in a try-catch block.
    /// </summary>
    /// <param name="action">Delegate to invoke.</param>
    /// <returns>New <see cref="Erratic{T}"/> instance with <see cref="Nil"/> value type.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<Nil> Try(Action action)
    {
        try
        {
            action();
            return new Erratic<Nil>( Nil.Instance );
        }
        catch ( Exception exc )
        {
            return new Erratic<Nil>( exc );
        }
    }
}
