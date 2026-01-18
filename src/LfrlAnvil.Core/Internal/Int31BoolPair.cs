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

namespace LfrlAnvil.Internal;

/// <summary>
/// Represents a pair of non-negative 31-bit int and boolean values.
/// </summary>
public readonly struct Int31BoolPair : IEquatable<Int31BoolPair>
{
    private const uint BoolValueMask = 1U << 31;

    /// <summary>
    /// Underlying raw data.
    /// </summary>
    public readonly uint Data;

    /// <summary>
    /// Creates a new <see cref="Int31BoolPair"/> instance.
    /// </summary>
    /// <param name="data">Underlying raw data.</param>
    public Int31BoolPair(uint data)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a new <see cref="Int31BoolPair"/> instance.
    /// </summary>
    /// <param name="intValue">Int value.</param>
    public Int31BoolPair(int intValue)
    {
        Data = GetData( intValue );
    }

    /// <summary>
    /// Creates a new <see cref="Int31BoolPair"/> instance.
    /// </summary>
    /// <param name="intValue">Int value.</param>
    /// <param name="boolValue">Bool value.</param>
    public Int31BoolPair(int intValue, bool boolValue)
    {
        Data = GetData( intValue, boolValue );
    }

    /// <summary>
    /// Encoded non-negative 31-bit int value.
    /// </summary>
    public int IntValue => unchecked( ( int )Data & int.MaxValue );

    /// <summary>
    /// Encoded boolean value.
    /// </summary>
    public bool BoolValue => (Data & BoolValueMask) != 0;

    /// <summary>
    /// Converts provided <paramref name="value"/> to an underlying raw data with disabled boolean value.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Underlying raw data.</returns>
    /// <remarks>
    /// Raw data cannot encode negative <paramref name="value"/>
    /// and the sign bit will instead be interpreted as if the boolean value is enabled.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static uint GetData(int value)
    {
        return unchecked( ( uint )value );
    }

    /// <summary>
    /// Converts provided <paramref name="intValue"/> and <paramref name="boolValue"/> to an underlying raw data.
    /// </summary>
    /// <param name="intValue">Int value to encode.</param>
    /// <param name="boolValue">Bool value to encode.</param>
    /// <returns>Underlying raw data.</returns>
    /// <remarks>
    /// Raw data cannot encode negative <paramref name="intValue"/>
    /// and the sign bit will either be lost or will be interpreted as if the boolean value is enabled,
    /// depending on the provided <paramref name="boolValue"/>.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static uint GetData(int intValue, bool boolValue)
    {
        return boolValue ? GetActiveData( intValue ) : GetData( intValue );
    }

    /// <summary>
    /// Converts provided <paramref name="value"/> to an underlying raw data with enabled boolean value.
    /// </summary>
    /// <param name="value">Value to encode.</param>
    /// <returns>Underlying raw data.</returns>
    /// <remarks>
    /// Raw data cannot encode negative <paramref name="value"/> and the sign bit will be lost.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static uint GetActiveData(int value)
    {
        return unchecked( ( uint )value | BoolValueMask );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Int31BoolPair"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"Int = {IntValue}, Bool = {BoolValue}";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Data.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Int31BoolPair i && Equals( i );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Int31BoolPair other)
    {
        return Data == other.Data;
    }

    /// <summary>
    /// Converts the provided <paramref name="data"/> to <see cref="Int31BoolPair"/>.
    /// </summary>
    /// <param name="data">Value to convert.</param>
    /// <returns>New <see cref="Int31BoolPair"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Int31BoolPair(uint data)
    {
        return new Int31BoolPair( data );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Int31BoolPair a, Int31BoolPair b)
    {
        return a.Data == b.Data;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Int31BoolPair a, Int31BoolPair b)
    {
        return a.Data != b.Data;
    }
}
