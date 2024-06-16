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

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents a dependency implementor key.
/// </summary>
public readonly struct ImplementorKey : IEquatable<ImplementorKey>
{
    private readonly int _data;

    private ImplementorKey(IDependencyKey value, int data)
    {
        Value = value;
        _data = data;
    }

    /// <summary>
    /// Underlying key.
    /// </summary>
    public IDependencyKey Value { get; }

    /// <summary>
    /// Specifies whether or not this key denotes a shared implementor.
    /// </summary>
    public bool IsShared => _data == -1;

    /// <summary>
    /// Specifies a 0-based index of an implementor denoted by this key in the range dependency resolver
    /// or null when the implementor is shared or is not included in range dependency resolver.
    /// </summary>
    public int? RangeIndex => _data >= 0 ? _data : null;

    /// <summary>
    /// Creates a new <see cref="ImplementorKey"/> instance.
    /// </summary>
    /// <param name="value">Underlying key.</param>
    /// <param name="rangeIndex">Optional 0-based index in the range dependency resolver. Equal to null by default.</param>
    /// <returns>New <see cref="ImplementorKey"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ImplementorKey Create(IDependencyKey value, int? rangeIndex = null)
    {
        Assume.Conditional( rangeIndex is not null, () => Assume.IsGreaterThanOrEqualTo( rangeIndex!.Value, 0 ) );
        return new ImplementorKey( value, rangeIndex ?? int.MinValue );
    }

    /// <summary>
    /// Creates a new <see cref="ImplementorKey"/> instance for a shared implementor.
    /// </summary>
    /// <param name="value">Underlying key.</param>
    /// <returns>New <see cref="ImplementorKey"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ImplementorKey CreateShared(IDependencyKey value)
    {
        return new ImplementorKey( value, -1 );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ImplementorKey"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return IsShared ? $"{Value} (shared)" : $"{Value}{GetRangeIndexText( RangeIndex )}";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Value, _data );
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ImplementorKey k && Equals( k );
    }

    /// <inheritdoc />
    public bool Equals(ImplementorKey other)
    {
        return Value.Equals( other.Value ) && _data == other._data;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(ImplementorKey a, ImplementorKey b)
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
    public static bool operator !=(ImplementorKey a, ImplementorKey b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    private static string GetRangeIndexText(int? index)
    {
        return index is null ? string.Empty : $" (position in range: {index.Value})";
    }
}
