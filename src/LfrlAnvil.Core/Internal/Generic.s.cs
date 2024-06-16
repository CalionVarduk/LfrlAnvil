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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

/// <summary>
/// Contains helper methods for generics.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
public static class Generic<T>
{
    /// <summary>
    /// Specifies whether or not the object type is a nullable value type.
    /// </summary>
    public static readonly bool IsNullableType =
        typeof( T ).IsValueType && Nullable.GetUnderlyingType( typeof( T ) ) is not null;

    /// <summary>
    /// Checks whether or not the provided <paramref name="obj"/> is null.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns><b>true</b> when <paramref name="obj"/> is null, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNull([NotNullWhen( false )] T? obj)
    {
        if ( typeof( T ).IsValueType )
            return IsNullableType && EqualityComparer<T>.Default.Equals( obj, default );

        return ReferenceEquals( obj, null );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="obj"/> is not null.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns><b>true</b> when <paramref name="obj"/> is not null, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNotNull([NotNullWhen( true )] T? obj)
    {
        return ! IsNull( obj );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="obj"/> is equivalent to default.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns><b>true</b> when <paramref name="obj"/> is equivalent to default, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsDefault([NotNullWhen( false )] T? obj)
    {
        return typeof( T ).IsValueType
            ? EqualityComparer<T>.Default.Equals( obj, default )
            : ReferenceEquals( obj, null );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="obj"/> is not equivalent to default.
    /// </summary>
    /// <param name="obj">Object to check.</param>
    /// <returns><b>true</b> when <paramref name="obj"/> is not equivalent to default, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNotDefault([NotNullWhen( true )] T? obj)
    {
        return ! IsDefault( obj );
    }

    /// <summary>
    /// Checks whether or not two instances are considered equal by the <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are considered to be equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool AreEqual(T? a, T? b)
    {
        return EqualityComparer<T>.Default.Equals( a, b );
    }

    /// <summary>
    /// Checks whether or not two instances are considered not equal by the <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are considered to not be equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool AreNotEqual(T? a, T? b)
    {
        return ! AreEqual( a, b );
    }

    /// <summary>
    /// Returns a string representation of the provided <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object to create string representation for.</param>
    /// <returns>
    /// String representation of <paramref name="obj"/> or <see cref="String.Empty"/> when <paramref name="obj"/> is null.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ToString(T? obj)
    {
        return IsNull( obj ) ? string.Empty : obj.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Calculates hash code for the provided <paramref name="obj"/>
    /// by using the <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="obj">Object to calculate hash code for.</param>
    /// <returns>Calculated hash code or <b>0</b> when <paramref name="obj"/> is null.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetHashCode(T? obj)
    {
        return IsNull( obj ) ? 0 : EqualityComparer<T>.Default.GetHashCode( obj );
    }
}
