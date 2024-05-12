using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a DB connector.
/// </summary>
public interface ISqlDatabaseConnector
{
    /// <summary>
    /// <see cref="ISqlDatabase"/> instance that this connector belongs to.
    /// </summary>
    ISqlDatabase Database { get; }

    /// <summary>
    /// Connects to the database.
    /// </summary>
    /// <returns><see cref="IDbConnection"/> instance that represents an established connection.</returns>
    [Pure]
    IDbConnection Connect();

    /// <summary>
    /// Connects to the database.
    /// </summary>
    /// <param name="options">Partial connection string that can be used to modify how the connection gets established.</param>
    /// <returns><see cref="IDbConnection"/> instance that represents an established connection.</returns>
    /// <remarks>Immutable connection string entries provided in <paramref name="options"/> will be ignored.</remarks>
    [Pure]
    IDbConnection Connect(string options);

    /// <summary>
    /// Connects to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> instance.</param>
    /// <returns>A task that returns an <see cref="IDbConnection"/> instance that represents an established connection.</returns>
    [Pure]
    ValueTask<IDbConnection> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to the database asynchronously.
    /// </summary>
    /// <param name="options">Partial connection string that can be used to modify how the connection gets established.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> instance.</param>
    /// <returns>A task that returns an <see cref="IDbConnection"/> instance that represents an established connection.</returns>
    /// <remarks>Immutable connection string entries provided in <paramref name="options"/> will be ignored.</remarks>
    [Pure]
    ValueTask<IDbConnection> ConnectAsync(string options, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a DB connector.
/// </summary>
/// <typeparam name="TConnection">DB connection type.</typeparam>
public interface ISqlDatabaseConnector<TConnection> : ISqlDatabaseConnector
    where TConnection : DbConnection
{
    /// <summary>
    /// <see cref="SqlDatabase"/> instance that this connector belongs to.
    /// </summary>
    new SqlDatabase Database { get; }

    /// <summary>
    /// Connects to the database.
    /// </summary>
    /// <returns><see cref="DbConnection"/> instance that represents an established connection.</returns>
    [Pure]
    new TConnection Connect();

    /// <summary>
    /// Connects to the database.
    /// </summary>
    /// <param name="options">Partial connection string that can be used to modify how the connection gets established.</param>
    /// <returns><see cref="DbConnection"/> instance that represents an established connection.</returns>
    /// <remarks>Immutable connection string entries provided in <paramref name="options"/> will be ignored.</remarks>
    [Pure]
    new TConnection Connect(string options);

    /// <summary>
    /// Connects to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> instance.</param>
    /// <returns>A task that returns an <see cref="DbConnection"/> instance that represents an established connection.</returns>
    [Pure]
    new ValueTask<TConnection> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to the database asynchronously.
    /// </summary>
    /// <param name="options">Partial connection string that can be used to modify how the connection gets established.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> instance.</param>
    /// <returns>A task that returns an <see cref="DbConnection"/> instance that represents an established connection.</returns>
    /// <remarks>Immutable connection string entries provided in <paramref name="options"/> will be ignored.</remarks>
    [Pure]
    new ValueTask<TConnection> ConnectAsync(string options, CancellationToken cancellationToken = default);
}
