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

namespace LfrlAnvil.Mapping.Internal;

/// <summary>
/// Represents a <see cref="SourceType"/> => <see cref="DestinationType"/> mapping definition key.
/// </summary>
public readonly struct TypeMappingKey : IEquatable<TypeMappingKey>
{
    /// <summary>
    /// Creates a new <see cref="TypeMappingKey"/> instance.
    /// </summary>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    public TypeMappingKey(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    /// <summary>
    /// Source type.
    /// </summary>
    public Type? SourceType { get; }

    /// <summary>
    /// Destination type.
    /// </summary>
    public Type? DestinationType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="TypeMappingKey"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( TypeMappingKey )}({SourceType?.FullName} => {DestinationType?.FullName})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( SourceType, DestinationType );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TypeMappingKey k && Equals( k );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(TypeMappingKey other)
    {
        return SourceType == other.SourceType && DestinationType == other.DestinationType;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(TypeMappingKey a, TypeMappingKey b)
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
    public static bool operator !=(TypeMappingKey a, TypeMappingKey b)
    {
        return ! a.Equals( b );
    }
}
