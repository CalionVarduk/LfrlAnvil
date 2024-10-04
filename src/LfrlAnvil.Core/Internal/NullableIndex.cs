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

namespace LfrlAnvil.Internal;

/// <summary>
/// Represents a lightweight nullable 32-bit signed index.
/// </summary>
public readonly struct NullableIndex : IEquatable<NullableIndex>, IComparable<NullableIndex>, IComparable
{
    /// <summary>
    /// Represents <b>null</b> value. Equal to <see cref="int.MaxValue"/>.
    /// </summary>
    public const int NullValue = int.MaxValue;

    /// <summary>
    /// Represents a nullable index interpreted as <b>null</b>.
    /// </summary>
    public static NullableIndex Null => new NullableIndex( NullValue );

    /// <summary>
    /// Underlying index. Equal to <see cref="NullValue"/> for <see cref="Null"/> index.
    /// </summary>
    public readonly int Value;

    private NullableIndex(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Returns <b>true</b>, when this index is not <see cref="Null"/>, otherwise returns <b>false</b>.
    /// </summary>
    public bool HasValue => Value != NullValue;

    /// <summary>
    /// Creates a new non-null <see cref="NullableIndex"/> instance.
    /// </summary>
    /// <param name="value">Underlying index.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    /// <remarks>
    /// <see cref="NullValue"/> validation is performed only in <b>DEBUG</b> mode. See <see cref="Assume"/> for more information.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex Create(int value)
    {
        Assume.NotEquals( value, NullValue );
        return new NullableIndex( value );
    }

    /// <summary>
    /// Creates a new <see cref="NullableIndex"/> instance.
    /// </summary>
    /// <param name="value">Underlying nullable index.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex Create(int? value)
    {
        return value.HasValue ? Create( value.Value ) : Null;
    }

    /// <summary>
    /// Creates a new <see cref="NullableIndex"/> instance. Accepts <see cref="NullValue"/>.
    /// </summary>
    /// <param name="value">Underlying index.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex CreateUnsafe(int value)
    {
        return new NullableIndex( value );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="NullableIndex"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return HasValue ? Value.ToString() : "NULL";
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
        return obj is NullableIndex i && Equals( i );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(NullableIndex other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is NullableIndex i ? CompareTo( i ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(NullableIndex other)
    {
        return Value.CompareTo( other.Value );
    }

    /// <summary>
    /// Converts the provided nullable index to nullable <see cref="int"/>.
    /// </summary>
    /// <param name="i">Value to convert.</param>
    /// <returns>New nullable <see cref="int"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator int?(NullableIndex i)
    {
        return i.HasValue ? i.Value : null;
    }

    /// <summary>
    /// Creates a new <see cref="NullableIndex"/> instance by incrementing <paramref name="i"/> by <b>1</b>.
    /// </summary>
    /// <param name="i">Operand.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex operator ++(NullableIndex i)
    {
        return i.HasValue ? Create( i.Value + 1 ) : i;
    }

    /// <summary>
    /// Creates a new <see cref="NullableIndex"/> instance by decrementing <paramref name="i"/> by <b>1</b>.
    /// </summary>
    /// <param name="i">Operand.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex operator --(NullableIndex i)
    {
        return i.HasValue ? Create( i.Value - 1 ) : i;
    }

    /// <summary>
    /// Creates a new <see cref="NullableIndex"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex operator +(NullableIndex a, NullableIndex b)
    {
        if ( ! a.HasValue )
            return a;

        return b.HasValue ? Create( a.Value + b.Value ) : b;
    }

    /// <summary>
    /// Creates a new <see cref="NullableIndex"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="NullableIndex"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static NullableIndex operator -(NullableIndex a, NullableIndex b)
    {
        if ( ! a.HasValue )
            return a;

        return b.HasValue ? Create( a.Value - b.Value ) : b;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(NullableIndex a, NullableIndex b)
    {
        return a.Value == b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(NullableIndex a, NullableIndex b)
    {
        return a.Value != b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(NullableIndex a, NullableIndex b)
    {
        return a.Value > b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(NullableIndex a, NullableIndex b)
    {
        return a.Value <= b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(NullableIndex a, NullableIndex b)
    {
        return a.Value < b.Value;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(NullableIndex a, NullableIndex b)
    {
        return a.Value >= b.Value;
    }
}
