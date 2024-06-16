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
using System.Threading;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight representation of an <see cref="Interlocked"/> (atomic) <see cref="Enum"/>.
/// </summary>
/// /// <typeparam name="T">Enum type.</typeparam>
public struct InterlockedEnum<T> : IEquatable<InterlockedEnum<T>>, IComparable<InterlockedEnum<T>>, IComparable
    where T : struct, Enum
{
    private int _value;

    /// <summary>
    /// Creates a new <see cref="InterlockedEnum{T}"/> instance.
    /// </summary>
    /// <param name="value">Initial value.</param>
    public InterlockedEnum(T value)
    {
        _value = ( int )( object )value;
    }

    /// <summary>
    /// Current value.
    /// </summary>
    public T Value => ( T )( object )Interlocked.Add( ref _value, 0 );

    /// <summary>
    /// Returns a string representation of this <see cref="InterlockedEnum{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode( Value );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is InterlockedEnum<T> b && Equals( b );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedEnum<T> b ? CompareTo( b ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(InterlockedEnum<T> other)
    {
        return EqualityComparer<T>.Default.Equals( Value, other.Value );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(InterlockedEnum<T> other)
    {
        return Comparer<T>.Default.Compare( Value, other.Value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/> and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Exchange(T value)
    {
        return ( T )( object )Interlocked.Exchange( ref _value, ( int )( object )value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="comparand"/>
    /// and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="comparand">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T CompareExchange(T value, T comparand)
    {
        return ( T )( object )Interlocked.CompareExchange( ref _value, ( int )( object )value, ( int )( object )comparand );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(T value)
    {
        return ! EqualityComparer<T>.Default.Equals( Exchange( value ), value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="expected"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="expected">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(T value, T expected)
    {
        return EqualityComparer<T>.Default.Equals( CompareExchange( value, expected ), expected );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) < 0;
    }
}
