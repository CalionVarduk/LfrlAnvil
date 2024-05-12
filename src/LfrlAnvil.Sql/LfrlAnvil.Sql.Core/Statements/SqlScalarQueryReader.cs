using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents a type-erased scalar query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
public readonly record struct SqlScalarQueryReader(SqlDialect Dialect, Func<IDataReader, SqlScalarQueryResult> Delegate)
{
    /// <summary>
    /// Reads a scalar value.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult Read(IDataReader reader)
    {
        return Delegate( reader );
    }
}

/// <summary>
/// Represents a generic scalar query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
/// <typeparam name="T">Value type.</typeparam>
public readonly record struct SqlScalarQueryReader<T>(SqlDialect Dialect, Func<IDataReader, SqlScalarQueryResult<T>> Delegate)
{
    /// <summary>
    /// Reads a scalar value.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult<T> Read(IDataReader reader)
    {
        return Delegate( reader );
    }
}
