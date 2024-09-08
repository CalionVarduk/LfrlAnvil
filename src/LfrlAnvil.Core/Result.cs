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

namespace LfrlAnvil;

/// <summary>
/// Represents a result of an operation.
/// </summary>
public readonly struct Result
{
    /// <summary>
    /// Returns a valid <see cref="Result"/> instance.
    /// </summary>
    public static Result Valid => new Result( null );

    internal Result(Exception? exception)
    {
        Exception = exception;
    }

    /// <summary>
    /// Optional <see cref="System.Exception"/> associated with this result.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Creates a <see cref="Result"/> with exception.
    /// </summary>
    /// <param name="exception"><see cref="System.Exception"/> associated with the result.</param>
    /// <returns>New <see cref="Result"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Result Error(Exception exception)
    {
        return new Result( exception );
    }

    /// <summary>
    /// Creates a <see cref="Result{T}"/> with exception and optional value.
    /// </summary>
    /// <param name="exception"><see cref="System.Exception"/> associated with the result.</param>
    /// <param name="value">Optional value associated with the result.</param>
    /// <typeparam name="T">Type of an optional value.</typeparam>
    /// <returns>New <see cref="Result{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Result<T> Error<T>(Exception exception, T? value = default)
    {
        return new Result<T>( value, exception );
    }

    /// <summary>
    /// Creates a <see cref="Result{T}"/> with optional value.
    /// </summary>
    /// <param name="value">Optional value associated with the result.</param>
    /// <typeparam name="T">Type of an optional value.</typeparam>
    /// <returns>New <see cref="Result{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Result<T> Create<T>(T? value = default)
    {
        return new Result<T>( value, null );
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        return Exception?.ToString() ?? "<VALID>";
    }

    /// <summary>
    /// Rethrows the <see cref="Exception"/>, if it isn't null, otherwise does nothing.
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfError()
    {
        Exception?.Rethrow();
    }
}

/// <summary>
/// Represents a result of an operation.
/// </summary>
/// <typeparam name="T">Type of an optional value.</typeparam>
public readonly struct Result<T>
{
    internal Result(T? value, Exception? exception)
    {
        Value = value;
        Exception = exception;
    }

    /// <summary>
    /// Optional value associated with this result.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Optional <see cref="System.Exception"/> associated with this result.
    /// </summary>
    public Exception? Exception { get; }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Value )}: {Value?.ToString() ?? "<NULL>"}{Environment.NewLine}{Exception?.ToString() ?? "<VALID>"}";
    }

    /// <summary>
    /// Rethrows the <see cref="Exception"/>, if it isn't null, otherwise does nothing.
    /// </summary>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ThrowIfError()
    {
        Exception?.Rethrow();
    }

    /// <summary>
    /// Converts provided <paramref name="value"/> to <see cref="Result"/>.
    /// </summary>
    /// <param name="value">Object to convert.</param>
    /// <returns>New <see cref="Result"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Result(Result<T> value)
    {
        return new Result( value.Exception );
    }
}
