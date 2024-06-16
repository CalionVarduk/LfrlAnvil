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

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a number of elapsed ticks since <see cref="DateTime.UnixEpoch"/>.
/// </summary>
public readonly struct Timestamp : IEquatable<Timestamp>, IComparable<Timestamp>, IComparable
{
    /// <summary>
    /// Represents the <see cref="DateTime.UnixEpoch"/>.
    /// </summary>
    public static readonly Timestamp Zero = new Timestamp( 0 );

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> instance.
    /// </summary>
    /// <param name="unixEpochTicks">Number of ticks since <see cref="DateTime.UnixEpoch"/>.</param>
    public Timestamp(long unixEpochTicks)
    {
        UnixEpochTicks = unixEpochTicks;
    }

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> instance.
    /// </summary>
    /// <param name="utcValue">Source date time.</param>
    public Timestamp(DateTime utcValue)
        : this( DateTime.SpecifyKind( utcValue, DateTimeKind.Utc ).Ticks - DateTime.UnixEpoch.Ticks ) { }

    /// <summary>
    /// Number of ticks elapsed since <see cref="DateTime.UnixEpoch"/>.
    /// </summary>
    public long UnixEpochTicks { get; }

    /// <summary>
    /// Gets the <see cref="DateTime"/> instance with <see cref="DateTimeKind.Utc"/> kind equivalent to this <see cref="Timestamp"/>.
    /// </summary>
    public DateTime UtcValue => DateTime.UnixEpoch.AddTicks( UnixEpochTicks );

    /// <summary>
    /// Returns a string representation of this <see cref="Timestamp"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{UnixEpochTicks} ticks";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return UnixEpochTicks.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Timestamp t && Equals( t );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Timestamp other)
    {
        return UnixEpochTicks.Equals( other.UnixEpochTicks );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Timestamp t ? CompareTo( t ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Timestamp other)
    {
        return UnixEpochTicks.CompareTo( other.UnixEpochTicks );
    }

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Duration"/> to add.</param>
    /// <returns>New <see cref="Timestamp"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Timestamp Add(Duration value)
    {
        return new Timestamp( UnixEpochTicks + value.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Duration"/> to subtract.</param>
    /// <returns>New <see cref="Timestamp"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Timestamp Subtract(Duration value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Calculates a difference between this instance and the <paramref name="other"/> instance,
    /// where this instance is treated as the end of the range.
    /// </summary>
    /// <param name="other">Instance to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Subtract(Timestamp other)
    {
        return Duration.FromTicks( UnixEpochTicks - other.UnixEpochTicks );
    }

    /// <summary>
    /// Coverts the provided duration to <see cref="DateTime"/>.
    /// </summary>
    /// <param name="source">Value to convert.</param>
    /// <returns><see cref="UtcValue"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator DateTime(Timestamp source)
    {
        return source.UtcValue;
    }

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Timestamp"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Timestamp operator +(Timestamp a, Duration b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="Timestamp"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Timestamp"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Timestamp operator -(Timestamp a, Duration b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator -(Timestamp a, Timestamp b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Timestamp a, Timestamp b)
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
    public static bool operator !=(Timestamp a, Timestamp b)
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
    public static bool operator >(Timestamp a, Timestamp b)
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
    public static bool operator <=(Timestamp a, Timestamp b)
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
    public static bool operator <(Timestamp a, Timestamp b)
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
    public static bool operator >=(Timestamp a, Timestamp b)
    {
        return a.CompareTo( b ) >= 0;
    }
}
