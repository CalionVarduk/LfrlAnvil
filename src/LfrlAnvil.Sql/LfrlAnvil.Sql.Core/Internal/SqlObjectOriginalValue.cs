﻿// Copyright 2024 Łukasz Furlepa
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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an original value of an <see cref="SqlObjectBuilder"/> property.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct SqlObjectOriginalValue<T>
{
    private SqlObjectOriginalValue(T? value, bool exists)
    {
        Value = value;
        Exists = exists;
    }

    /// <summary>
    /// Original value.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Specifies whether or not the value has changed.
    /// </summary>
    public bool Exists { get; }

    /// <summary>
    /// Creates a new <see cref="SqlObjectOriginalValue{T}"/> instance with a changed value.
    /// </summary>
    /// <param name="value">Original value.</param>
    /// <returns>New <see cref="SqlObjectOriginalValue{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectOriginalValue<T> Create(T value)
    {
        return new SqlObjectOriginalValue<T>( value, exists: true );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectOriginalValue{T}"/> instance with a unchanged value.
    /// </summary>
    /// <returns>New <see cref="SqlObjectOriginalValue{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectOriginalValue<T> CreateEmpty()
    {
        return new SqlObjectOriginalValue<T>( value: default, exists: false );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlObjectOriginalValue{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Exists ? $"Value<{typeof( T ).GetDebugString()}>({Value})" : $"Empty<{typeof( T ).GetDebugString()}>()";
    }

    /// <summary>
    /// Returns <see cref="Value"/> or the provided <paramref name="default"/> value if <see cref="Exists"/> is equal to <b>false</b>.
    /// </summary>
    /// <param name="default">Default value.</param>
    /// <returns>
    /// <see cref="Value"/> or the provided <paramref name="default"/> value if <see cref="Exists"/> is equal to <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T GetValueOrDefault(T @default)
    {
        return Exists ? Value! : @default;
    }
}
