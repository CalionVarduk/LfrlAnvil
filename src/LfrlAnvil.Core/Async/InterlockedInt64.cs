// Copyright 2026 Łukasz Furlepa
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
using System.Threading;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight representation of an <see cref="Interlocked"/> (atomic) <see cref="Int64"/>.
/// </summary>
public struct InterlockedInt64 : IEquatable<InterlockedInt64>, IComparable<InterlockedInt64>, IComparable
{
    private long _value;

    /// <summary>
    /// Creates a new <see cref="InterlockedInt64"/> instance.
    /// </summary>
    /// <param name="value">Initial value.</param>
    public InterlockedInt64(long value)
    {
        _value = value;
    }

    /// <summary>
    /// Current value.
    /// </summary>
    public long Value => Volatile.Read( ref _value );

    /// <summary>
    /// Returns a string representation of this <see cref="InterlockedInt64"/> instance.
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
        return Value.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is InterlockedInt64 b && Equals( b );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedInt64 b ? CompareTo( b ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(InterlockedInt64 other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(InterlockedInt64 other)
    {
        return Value.CompareTo( other.Value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/> and returns the old value.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long Exchange(long value)
    {
        return Interlocked.Exchange( ref _value, value );
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
    public long CompareExchange(long value, long comparand)
    {
        return Interlocked.CompareExchange( ref _value, value, comparand );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(long value)
    {
        return Exchange( value ) != value;
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>
    /// only if the current <see cref="Value"/> is equal to the provided <paramref name="expected"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <param name="expected">Value used for <see cref="Value"/> comparison.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(long value, long expected)
    {
        var oldValue = CompareExchange( value, expected );
        return oldValue != value && oldValue == expected;
    }

    /// <summary>
    /// Increments the current <see cref="Value"/> by <b>1</b>.
    /// </summary>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long Increment()
    {
        return Interlocked.Increment( ref _value );
    }

    /// <summary>
    /// Decrements the current <see cref="Value"/> by <b>1</b>.
    /// </summary>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long Decrement()
    {
        return Interlocked.Decrement( ref _value );
    }

    /// <summary>
    /// Adds provided <paramref name="value"/> to the current <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to add.</param>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long Add(long value)
    {
        return Interlocked.Add( ref _value, value );
    }

    /// <summary>
    /// Subtracts provided <paramref name="value"/> from the current <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to subtract.</param>
    /// <returns><see cref="Value"/> after the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long Subtract(long value)
    {
        return Add( unchecked( -value ) );
    }

    /// <summary>
    /// Performs a bitwise and operation on the current <see cref="Value"/> and the provided <paramref name="value"/>
    /// and stores the result in <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise and.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long And(long value)
    {
        return Interlocked.And( ref _value, value );
    }

    /// <summary>
    /// Performs a bitwise or operation on the current <see cref="Value"/> and the provided <paramref name="value"/>
    /// and stores the result in <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Value to bitwise or.</param>
    /// <returns><see cref="Value"/> before the change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public long Or(long value)
    {
        return Interlocked.Or( ref _value, value );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedInt64 a, InterlockedInt64 b)
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
    public static bool operator !=(InterlockedInt64 a, InterlockedInt64 b)
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
    public static bool operator >=(InterlockedInt64 a, InterlockedInt64 b)
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
    public static bool operator <=(InterlockedInt64 a, InterlockedInt64 b)
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
    public static bool operator >(InterlockedInt64 a, InterlockedInt64 b)
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
    public static bool operator <(InterlockedInt64 a, InterlockedInt64 b)
    {
        return a.CompareTo( b ) < 0;
    }
}
