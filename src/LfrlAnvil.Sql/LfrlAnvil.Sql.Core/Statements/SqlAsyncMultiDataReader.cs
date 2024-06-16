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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an asynchronous lightweight <see cref="IDataReader"/> container with multiple result sets.
/// </summary>
public readonly struct SqlAsyncMultiDataReader : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Creates a new <see cref="SqlAsyncMultiDataReader"/> instance.
    /// </summary>
    /// <param name="reader">Underlying data reader.</param>
    public SqlAsyncMultiDataReader(IDataReader reader)
    {
        Reader = ( DbDataReader )reader;
    }

    /// <summary>
    /// Underlying data reader.
    /// </summary>
    public DbDataReader Reader { get; }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( ! Reader.IsClosed )
            Reader.Dispose();
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask DisposeAsync()
    {
        if ( ! Reader.IsClosed )
            await Reader.DisposeAsync().ConfigureAwait( false );
    }

    /// <summary>
    /// Reads all record sets asynchronously.
    /// </summary>
    /// <param name="reader">Asynchronous query reader.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns all record sets.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<List<SqlQueryResult>> ReadAllAsync(SqlAsyncQueryReader reader, CancellationToken cancellationToken = default)
    {
        var result = new List<SqlQueryResult>();
        while ( ! Reader.IsClosed )
            result.Add( await ReadAsync( reader, null, cancellationToken ).ConfigureAwait( false ) );

        return result;
    }

    /// <summary>
    /// Reads the next record set asynchronously.
    /// </summary>
    /// <param name="reader">Asynchronous query reader.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the next record set.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryResult> ReadAsync(
        SqlAsyncQueryReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await reader.ReadAsync( Reader, options, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    /// <summary>
    /// Reads the next record set asynchronously.
    /// </summary>
    /// <param name="reader">Asynchronous query reader.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the next record set.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryResult<TRow>> ReadAsync<TRow>(
        SqlAsyncQueryReader<TRow> reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRow : notnull
    {
        var result = await reader.ReadAsync( Reader, options, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    /// <summary>
    /// Reads the next scalar asynchronously.
    /// </summary>
    /// <param name="reader">Asynchronous scalar query reader.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the next scalar.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlScalarQueryResult> ReadAsync(SqlAsyncScalarQueryReader reader, CancellationToken cancellationToken = default)
    {
        var result = await reader.ReadAsync( Reader, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    /// <summary>
    /// Reads the next scalar asynchronously.
    /// </summary>
    /// <param name="reader">Asynchronous scalar query reader.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="T">Scalar type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the next scalar.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlScalarQueryResult<T>> ReadAsync<T>(
        SqlAsyncScalarQueryReader<T> reader,
        CancellationToken cancellationToken = default)
    {
        var result = await reader.ReadAsync( Reader, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    /// <summary>
    /// Invokes the provided asynchronous delegate on the underlying <see cref="Reader"/>.
    /// </summary>
    /// <param name="reader">Asynchronous delegate to invoke.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the result of invocation of the delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<TResult> ReadAsync<TResult>(
        Func<DbDataReader, CancellationToken, ValueTask<TResult>> reader,
        CancellationToken cancellationToken = default)
    {
        var result = await reader( Reader, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    /// <summary>
    /// Invokes the provided asynchronous delegate on the underlying <see cref="Reader"/>.
    /// </summary>
    /// <param name="reader">Asynchronous delegate to invoke.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the result of invocation of the delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<TResult> ReadAsync<TResult>(
        Func<DbDataReader, CancellationToken, Task<TResult>> reader,
        CancellationToken cancellationToken = default)
    {
        var result = await reader( Reader, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }
}
