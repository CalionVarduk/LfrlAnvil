using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a lightweight <see cref="IDataReader"/> container with multiple result sets.
/// </summary>
public readonly struct SqlMultiDataReader : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="SqlMultiDataReader"/> instance.
    /// </summary>
    /// <param name="reader">Underlying data reader.</param>
    public SqlMultiDataReader(IDataReader reader)
    {
        Reader = reader;
    }

    /// <summary>
    /// Underlying data reader.
    /// </summary>
    public IDataReader Reader { get; }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( ! Reader.IsClosed )
            Reader.Dispose();
    }

    /// <summary>
    /// Reads all record sets.
    /// </summary>
    /// <param name="reader">Query reader.</param>
    /// <returns>Returns all record sets.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public List<SqlQueryResult> ReadAll(SqlQueryReader reader)
    {
        var result = new List<SqlQueryResult>();
        while ( ! Reader.IsClosed )
            result.Add( Read( reader ) );

        return result;
    }

    /// <summary>
    /// Reads the next record set.
    /// </summary>
    /// <param name="reader">Query reader.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns the next record set.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult Read(SqlQueryReader reader, SqlQueryReaderOptions? options = null)
    {
        var result = reader.Read( Reader, options );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }

    /// <summary>
    /// Reads the next record set.
    /// </summary>
    /// <param name="reader">Query reader.</param>
    /// <param name="options">Query reader options.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>Returns the next record set.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult<TRow> Read<TRow>(SqlQueryReader<TRow> reader, SqlQueryReaderOptions? options = null)
        where TRow : notnull
    {
        var result = reader.Read( Reader, options );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }

    /// <summary>
    /// Reads the next scalar.
    /// </summary>
    /// <param name="reader">Scalar query reader.</param>
    /// <returns>Returns the next scalar.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult Read(SqlScalarQueryReader reader)
    {
        var result = reader.Read( Reader );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }

    /// <summary>
    /// Reads the next scalar.
    /// </summary>
    /// <param name="reader">Scalar query reader.</param>
    /// <typeparam name="T">Scalar type.</typeparam>
    /// <returns>Returns the next scalar.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult<T> Read<T>(SqlScalarQueryReader<T> reader)
    {
        var result = reader.Read( Reader );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }

    /// <summary>
    /// Invokes the provided delegate on the underlying <see cref="Reader"/>.
    /// </summary>
    /// <param name="reader">Delegate to invoke.</param>
    /// <returns>Returns the result of invocation of the delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TResult Read<TResult>(Func<IDataReader, TResult> reader)
    {
        var result = reader( Reader );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }
}
