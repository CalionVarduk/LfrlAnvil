using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlAsyncQueryReader"/> bound to a specific <see cref="Sql"/> statement.
/// </summary>
/// <param name="Reader">Underlying query reader.</param>
/// <param name="Sql">Bound SQL statement.</param>
public readonly record struct SqlAsyncQueryReaderExecutor(SqlAsyncQueryReader Reader, string Sql)
{
    /// <summary>
    /// Asynchronously creates an <see cref="IDataReader"/> instance and reads a collection of rows,
    /// using the specified <see cref="Sql"/> statement.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryResult> ExecuteAsync(
        IDbCommand command,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, options, cancellationToken ).ConfigureAwait( false );
    }
}

/// <summary>
/// Represents an <see cref="SqlAsyncQueryReader{TRow}"/> bound to a specific <see cref="Sql"/> statement.
/// </summary>
/// <param name="Reader">Underlying query reader.</param>
/// <param name="Sql">Bound SQL statement.</param>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly record struct SqlAsyncQueryReaderExecutor<TRow>(SqlAsyncQueryReader<TRow> Reader, string Sql)
    where TRow : notnull
{
    /// <summary>
    /// Asynchronously creates an <see cref="IDataReader"/> instance and reads a collection of rows,
    /// using the specified <see cref="Sql"/> statement.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryResult<TRow>> ExecuteAsync(
        IDbCommand command,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, options, cancellationToken ).ConfigureAwait( false );
    }
}
