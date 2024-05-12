using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlAsyncScalarQueryReader"/> bound to a specific <see cref="Sql"/> statement.
/// </summary>
/// <param name="Reader">Underlying query reader.</param>
/// <param name="Sql">Bound SQL statement.</param>
public readonly record struct SqlAsyncScalarQueryReaderExecutor(SqlAsyncScalarQueryReader Reader, string Sql)
{
    /// <summary>
    /// Asynchronously creates an <see cref="IDataReader"/> instance and reads a scalar value,
    /// using the specified <see cref="Sql"/> statement.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlScalarQueryResult> ExecuteAsync(IDbCommand command, CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, cancellationToken ).ConfigureAwait( false );
    }
}

/// <summary>
/// Represents an <see cref="SqlAsyncScalarQueryReader{TRow}"/> bound to a specific <see cref="Sql"/> statement.
/// </summary>
/// <param name="Reader">Underlying query reader.</param>
/// <param name="Sql">Bound SQL statement.</param>
/// <typeparam name="T">Value type.</typeparam>
public readonly record struct SqlAsyncScalarQueryReaderExecutor<T>(SqlAsyncScalarQueryReader<T> Reader, string Sql)
{
    /// <summary>
    /// Asynchronously creates an <see cref="IDataReader"/> instance and reads a scalar value,
    /// using the specified <see cref="Sql"/> statement.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> to read from.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlScalarQueryResult<T>> ExecuteAsync(IDbCommand command, CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, cancellationToken ).ConfigureAwait( false );
    }
}
