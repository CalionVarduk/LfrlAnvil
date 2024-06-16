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
using LfrlAnvil.Chrono;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Identifiers;

/// <summary>
/// A lightweight object that can be used as a unique ID.
/// </summary>
public readonly struct Identifier : IEquatable<Identifier>, IComparable<Identifier>, IComparable
{
    /// <summary>
    /// Specifies maximum possible <see cref="High"/> value.
    /// </summary>
    public const ulong MaxHighValue = (1UL << 48) - 1;

    /// <summary>
    /// Underlying value.
    /// </summary>
    public readonly ulong Value;

    /// <summary>
    /// Creates a new <see cref="Identifier"/> instance.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public Identifier(ulong value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Identifier"/> instance.
    /// </summary>
    /// <param name="high">Desired <see cref="High"/> value. Only the first 48 bits will be used.</param>
    /// <param name="low">Desired <see cref="Low"/> value.</param>
    public Identifier(ulong high, ushort low)
        : this( (high << 16) | low ) { }

    /// <summary>
    /// Specifies the 48-bit high value of this instance. If this <see cref="Identifier"/> has been created by
    /// an <see cref="IdentifierGenerator"/> then this value represents a <see cref="Timestamp"/> at which it has been created.
    /// That <see cref="Timestamp"/> can be extracted by invoking the <see cref="IdentifierGenerator.GetTimestamp(Identifier)"/> method.
    /// </summary>
    public ulong High => Value >> 16;

    /// <summary>
    /// Specifies the low value of this instance. If this <see cref="Identifier"/> has been created by an <see cref="IdentifierGenerator"/>
    /// then this value represents a unique sequential number with which it has been created at a given time slice represented by its
    /// <see cref="High"/> value.
    /// </summary>
    public ushort Low => unchecked( ( ushort )Value );

    /// <summary>
    /// Returns a string representation of this <see cref="Identifier"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Identifier )}({Value})";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Identifier id && Equals( id );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Identifier other)
    {
        return Value.Equals( other.Value );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Identifier id ? CompareTo( id ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Identifier other)
    {
        return Value.CompareTo( other.Value );
    }

    /// <summary>
    /// Converts the provided <paramref name="id"/> to <see cref="UInt64"/>. Returns <see cref="Identifier.Value"/>.
    /// </summary>
    /// <param name="id">Object to convert.</param>
    /// <returns>Returns <see cref="Identifier.Value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator ulong(Identifier id)
    {
        return id.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Identifier a, Identifier b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Identifier a, Identifier b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) >= 0;
    }
}
