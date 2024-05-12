using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
public readonly record struct SqlQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult> Delegate
)
{
    /// <summary>
    /// Reads a collection of rows.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}

/// <summary>
/// Represents a generic query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly record struct SqlQueryReader<TRow>(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>> Delegate
)
    where TRow : notnull
{
    /// <summary>
    /// Reads a collection of rows.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult<TRow> Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}
