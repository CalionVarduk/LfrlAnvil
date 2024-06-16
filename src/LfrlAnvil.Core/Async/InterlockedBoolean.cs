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
using System.Threading;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight representation of an <see cref="Interlocked"/> (atomic) <see cref="Boolean"/>.
/// </summary>
public struct InterlockedBoolean : IEquatable<InterlockedBoolean>, IComparable<InterlockedBoolean>, IComparable
{
    private int _value;

    /// <summary>
    /// Creates a new <see cref="InterlockedBoolean"/> instance.
    /// </summary>
    /// <param name="value">Initial value.</param>
    public InterlockedBoolean(bool value)
    {
        _value = value ? 1 : 0;
    }

    /// <summary>
    /// Current value.
    /// </summary>
    public bool Value => Interlocked.Add( ref _value, 0 ).IsOdd();

    /// <summary>
    /// Returns a string representation of this <see cref="InterlockedBoolean"/> instance.
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
        return obj is InterlockedBoolean b && Equals( b );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedBoolean b ? CompareTo( b ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(InterlockedBoolean other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(InterlockedBoolean other)
    {
        return Value.CompareTo( other.Value );
    }

    /// <summary>
    /// Sets <see cref="Value"/> to <b>true</b>.
    /// </summary>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool WriteTrue()
    {
        return Interlocked.Exchange( ref _value, 1 ).IsEven();
    }

    /// <summary>
    /// Sets <see cref="Value"/> to <b>false</b>.
    /// </summary>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool WriteFalse()
    {
        return Interlocked.Exchange( ref _value, 0 ).IsOdd();
    }

    /// <summary>
    /// Sets <see cref="Value"/> to the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>true</b> when value has changed, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(bool value)
    {
        return value ? WriteTrue() : WriteFalse();
    }

    /// <summary>
    /// Toggles (negates) the current <see cref="Value"/>.
    /// </summary>
    /// <returns><see cref="Value"/> after change.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Toggle()
    {
        return Interlocked.Increment( ref _value ).IsOdd();
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedBoolean a, InterlockedBoolean b)
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
    public static bool operator !=(InterlockedBoolean a, InterlockedBoolean b)
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
    public static bool operator >=(InterlockedBoolean a, InterlockedBoolean b)
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
    public static bool operator <=(InterlockedBoolean a, InterlockedBoolean b)
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
    public static bool operator >(InterlockedBoolean a, InterlockedBoolean b)
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
    public static bool operator <(InterlockedBoolean a, InterlockedBoolean b)
    {
        return a.CompareTo( b ) < 0;
    }
}
