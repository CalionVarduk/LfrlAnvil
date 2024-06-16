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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// Contains methods for unsafe reinterpret casting.
/// </summary>
public static class ReinterpretCast
{
    /// <summary>
    /// Reinterprets the provided <paramref name="value"/> as an object of the desired reference type.
    /// </summary>
    /// <param name="value">Value to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns></returns>
    /// <remarks>This method is unsafe, use with caution. See <see cref="Unsafe.As{T}(Object)"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    [return: NotNullIfNotNull( "value" )]
    public static T? To<T>(object? value)
        where T : class
    {
        Debug.Assert( value is null or T, ExceptionResources.AssumedInstanceOfType( typeof( T ), value?.GetType(), nameof( value ) ) );
        return Unsafe.As<T>( value );
    }
}
