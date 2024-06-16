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
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased scalar query reader result.
/// </summary>
public readonly struct SqlScalarQueryResult : IEquatable<SqlScalarQueryResult>
{
    /// <summary>
    /// Represents a result without a <see cref="Value"/>.
    /// </summary>
    public static readonly SqlScalarQueryResult Empty = new SqlScalarQueryResult();

    /// <summary>
    /// Creates a new <see cref="SqlScalarQueryResult"/> instance with a <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public SqlScalarQueryResult(object? value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Specifies whether or not any value was read.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlScalarQueryResult"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return HasValue ? $"{nameof( Value )}({Value})" : $"{nameof( Empty )}()";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( HasValue, Value );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlScalarQueryResult r && Equals( r );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlScalarQueryResult other)
    {
        if ( ! HasValue )
            return ! other.HasValue;

        return other.HasValue && Equals( Value, other.Value );
    }

    /// <summary>
    /// Returns the underlying value.
    /// </summary>
    /// <returns><see cref="Value"/>.</returns>
    /// <exception cref="InvalidOperationException">When <see cref="HasValue"/> is equal to <b>false</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValue()
    {
        return HasValue ? Value : throw new InvalidOperationException( ExceptionResources.ScalarResultDoesNotHaveValue );
    }

    /// <summary>
    ///Returns the underlying value if it exists, otherwise returns the provided <paramref name="default"/> value.
    /// </summary>
    /// <param name="default">Default value.</param>
    /// <returns><see cref="Value"/> if it exists, otherwise provided <paramref name="default"/> value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public object? GetValueOrDefault(object? @default)
    {
        return HasValue ? Value : @default;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlScalarQueryResult a, SqlScalarQueryResult b)
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
    public static bool operator !=(SqlScalarQueryResult a, SqlScalarQueryResult b)
    {
        return ! a.Equals( b );
    }
}

/// <summary>
/// Represents a generic scalar query reader result.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct SqlScalarQueryResult<T> : IEquatable<SqlScalarQueryResult<T>>
{
    /// <summary>
    /// Represents a result without a <see cref="Value"/>.
    /// </summary>
    public static readonly SqlScalarQueryResult<T> Empty = new SqlScalarQueryResult<T>();

    /// <summary>
    /// Creates a new <see cref="SqlScalarQueryResult{T}"/> instance with a <see cref="Value"/>.
    /// </summary>
    /// <param name="value">Underlying value.</param>
    public SqlScalarQueryResult(T? value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Specifies whether or not any value was read.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlScalarQueryResult{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var typeText = typeof( T ).GetDebugString();
        return HasValue ? $"{nameof( Value )}<{typeText}>({Value})" : $"{nameof( Empty )}<{typeText}>()";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( HasValue, Value );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is SqlScalarQueryResult<T> r && Equals( r );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(SqlScalarQueryResult<T> other)
    {
        if ( ! HasValue )
            return ! other.HasValue;

        return other.HasValue && Generic<T>.AreEqual( Value, other.Value );
    }

    /// <summary>
    /// Returns the underlying value.
    /// </summary>
    /// <returns><see cref="Value"/>.</returns>
    /// <exception cref="InvalidOperationException">When <see cref="HasValue"/> is equal to <b>false</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? GetValue()
    {
        return HasValue ? Value : throw new InvalidOperationException( ExceptionResources.ScalarResultDoesNotHaveValue );
    }

    /// <summary>
    ///Returns the underlying value if it exists, otherwise returns the provided <paramref name="default"/> value.
    /// </summary>
    /// <param name="default">Default value.</param>
    /// <returns><see cref="Value"/> if it exists, otherwise provided <paramref name="default"/> value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? GetValueOrDefault(T? @default)
    {
        return HasValue ? Value : @default;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(SqlScalarQueryResult<T> a, SqlScalarQueryResult<T> b)
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
    public static bool operator !=(SqlScalarQueryResult<T> a, SqlScalarQueryResult<T> b)
    {
        return ! a.Equals( b );
    }
}
