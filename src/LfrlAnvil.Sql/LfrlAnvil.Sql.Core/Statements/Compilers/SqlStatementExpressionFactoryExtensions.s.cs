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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Statements.Compilers;

/// <summary>
/// Contains various statement expression factory extension methods.
/// </summary>
public static class SqlStatementExpressionFactoryExtensions
{
    /// <summary>
    /// Creates a new <see cref="SqlQueryReaderExpression{TRow}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="SqlQueryReaderExpression{TRow}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="TRow"/> is not a valid row type
    /// or does not contain a valid constructor or does not contain any valid members.
    /// </exception>
    [Pure]
    public static SqlQueryReaderExpression<TRow> CreateExpression<TRow>(
        this ISqlQueryReaderFactory factory,
        SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        var expression = factory.CreateExpression( typeof( TRow ), options );
        return new SqlQueryReaderExpression<TRow>( expression );
    }

    /// <summary>
    /// Creates a new compiled <see cref="SqlQueryReader{TRow}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="SqlQueryReader{TRow}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="TRow"/> is not a valid row type
    /// or does not contain a valid constructor or does not contain any valid members.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReader<TRow> Create<TRow>(this ISqlQueryReaderFactory factory, SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        return factory.CreateExpression<TRow>( options ).Compile();
    }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncQueryReaderExpression{TRow}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="SqlAsyncQueryReaderExpression{TRow}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="TRow"/> is not a valid row type
    /// or does not contain a valid constructor or does not contain any valid members
    /// or this factory does not support asynchronous expressions.
    /// </exception>
    [Pure]
    public static SqlAsyncQueryReaderExpression<TRow> CreateAsyncExpression<TRow>(
        this ISqlQueryReaderFactory factory,
        SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        var expression = factory.CreateAsyncExpression( typeof( TRow ), options );
        return new SqlAsyncQueryReaderExpression<TRow>( expression );
    }

    /// <summary>
    /// Creates a new compiled <see cref="SqlAsyncQueryReader{TRow}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="options">Optional <see cref="SqlQueryReaderCreationOptions"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="SqlAsyncQueryReader{TRow}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="TRow"/> is not a valid row type
    /// or does not contain a valid constructor or does not contain any valid members
    /// or this factory does not support asynchronous expressions.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryReader<TRow> CreateAsync<TRow>(
        this ISqlQueryReaderFactory factory,
        SqlQueryReaderCreationOptions? options = null)
        where TRow : notnull
    {
        return factory.CreateAsyncExpression<TRow>( options ).Compile();
    }

    /// <summary>
    /// Creates a new <see cref="SqlScalarQueryReaderExpression{T}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="isNullable">Specifies whether or not the result is nullable. Equal to <b>false</b> by default.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlScalarQueryReaderExpression{T}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">When <typeparamref name="T"/> is not a valid result type.</exception>
    [Pure]
    public static SqlScalarQueryReaderExpression<T> CreateScalarExpression<T>(this ISqlQueryReaderFactory factory, bool isNullable = false)
    {
        var expression = factory.CreateScalarExpression( typeof( T ), isNullable );
        return new SqlScalarQueryReaderExpression<T>( expression );
    }

    /// <summary>
    /// Creates a new compiled <see cref="SqlScalarQueryReader{T}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="isNullable">Specifies whether or not the result is nullable. Equal to <b>false</b> by default.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlScalarQueryReader{T}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">When <typeparamref name="T"/> is not a valid result type.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryReader<T> CreateScalar<T>(this ISqlQueryReaderFactory factory, bool isNullable = false)
    {
        return factory.CreateScalarExpression<T>( isNullable ).Compile();
    }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncScalarQueryReaderExpression{T}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="isNullable">Specifies whether or not the result is nullable. Equal to <b>false</b> by default.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlAsyncScalarQueryReaderExpression{T}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="T"/> is not a valid result type
    /// or this factory does not support asynchronous expressions.
    /// </exception>
    [Pure]
    public static SqlAsyncScalarQueryReaderExpression<T> CreateAsyncScalarExpression<T>(
        this ISqlQueryReaderFactory factory,
        bool isNullable = false)
    {
        var expression = factory.CreateAsyncScalarExpression( typeof( T ), isNullable );
        return new SqlAsyncScalarQueryReaderExpression<T>( expression );
    }

    /// <summary>
    /// Creates a new compiled <see cref="SqlAsyncScalarQueryReader{T}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="isNullable">Specifies whether or not the result is nullable. Equal to <b>false</b> by default.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlAsyncScalarQueryReader{T}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="T"/> is not a valid result type
    /// or this factory does not support asynchronous expressions.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarQueryReader<T> CreateAsyncScalar<T>(this ISqlQueryReaderFactory factory, bool isNullable = false)
    {
        return factory.CreateAsyncScalarExpression<T>( isNullable ).Compile();
    }

    /// <summary>
    /// Creates a new <see cref="SqlParameterBinderExpression{TSource}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <typeparam name="TSource">Parameter source type.</typeparam>
    /// <returns>New <see cref="SqlParameterBinderExpression{TSource}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="TSource"/> is not a valid parameter source type or does not contain any valid members.
    /// </exception>
    [Pure]
    public static SqlParameterBinderExpression<TSource> CreateExpression<TSource>(
        this ISqlParameterBinderFactory factory,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        var expression = factory.CreateExpression( typeof( TSource ), options );
        return new SqlParameterBinderExpression<TSource>( expression );
    }

    /// <summary>
    /// Creates a new compiled <see cref="SqlParameterBinder{TSource}"/> instance.
    /// </summary>
    /// <param name="factory">Source factory.</param>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <typeparam name="TSource">Parameter source type.</typeparam>
    /// <returns>New <see cref="SqlParameterBinder{TSource}"/> instance.</returns>
    /// <exception cref="SqlCompilerException">
    /// When <typeparamref name="TSource"/> is not a valid parameter source type or does not contain any valid members.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterBinder<TSource> Create<TSource>(
        this ISqlParameterBinderFactory factory,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull
    {
        return factory.CreateExpression<TSource>( options ).Compile();
    }
}
