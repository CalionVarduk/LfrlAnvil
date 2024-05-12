using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlQueryReader"/> bound to a specific <see cref="Sql"/> statement.
/// </summary>
/// <param name="Reader">Underlying query reader.</param>
/// <param name="Sql">Bound SQL statement.</param>
public readonly record struct SqlQueryReaderExecutor(SqlQueryReader Reader, string Sql)
{
    /// <summary>
    /// Creates an <see cref="IDataReader"/> instance and reads a collection of rows, using the specified <see cref="Sql"/> statement.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult Execute(IDbCommand command, SqlQueryReaderOptions? options = null)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader, options );
    }
}

/// <summary>
/// Represents an <see cref="SqlQueryReader{TRow}"/> bound to a specific <see cref="Sql"/> statement.
/// </summary>
/// <param name="Reader">Underlying query reader.</param>
/// <param name="Sql">Bound SQL statement.</param>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly record struct SqlQueryReaderExecutor<TRow>(SqlQueryReader<TRow> Reader, string Sql)
    where TRow : notnull
{
    /// <summary>
    /// Creates an <see cref="IDataReader"/> instance and reads a collection of rows, using the specified <see cref="Sql"/> statement.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult<TRow> Execute(IDbCommand command, SqlQueryReaderOptions? options = null)
    {
        command.CommandText = Sql;
        using var reader = command.ExecuteReader();
        return Reader.Read( reader, options );
    }
}
