using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <inheritdoc cref="ISqlDatabaseConnector" />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlDatabaseConnector : ISqlDatabaseConnector<MySqlConnection>, ISqlDatabaseConnector<DbConnection>
{
    private readonly string _connectionString;
    private readonly SqlConnectionStringEntry[] _connectionStringEntries;
    private readonly DbConnectionEventHandler _eventHandler;
    private MySqlDatabase? _database;

    internal MySqlDatabaseConnector(MySqlConnectionStringBuilder connectionStringBuilder, DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _connectionString = connectionStringBuilder.ToString();
        _connectionStringEntries = MySqlHelpers.ExtractConnectionStringEntries( connectionStringBuilder );
        _eventHandler = eventHandler;
    }

    /// <inheritdoc cref="ISqlDatabaseConnector{TConnection}.Database" />
    public MySqlDatabase Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    SqlDatabase ISqlDatabaseConnector<MySqlConnection>.Database => Database;
    SqlDatabase ISqlDatabaseConnector<DbConnection>.Database => Database;
    ISqlDatabase ISqlDatabaseConnector.Database => Database;

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlConnection Connect()
    {
        var connection = CreateConnection( _connectionString );
        connection.Open();
        return connection;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MySqlConnection Connect(string options)
    {
        var connectionString = MySqlHelpers.ExtendConnectionString( _connectionStringEntries, options );
        var connection = CreateConnection( connectionString );
        connection.Open();
        return connection;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<MySqlConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection( _connectionString );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<MySqlConnection> ConnectAsync(string options, CancellationToken cancellationToken = default)
    {
        var connectionString = MySqlHelpers.ExtendConnectionString( _connectionStringEntries, options );
        var connection = CreateConnection( connectionString );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDatabase(MySqlDatabase database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Connector, this );
        _database = database;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private MySqlConnection CreateConnection(string connectionString)
    {
        var result = new MySqlConnection( connectionString );
        _eventHandler.Attach( result );
        return result;
    }

    [Pure]
    DbConnection ISqlDatabaseConnector<DbConnection>.Connect()
    {
        return Connect();
    }

    [Pure]
    DbConnection ISqlDatabaseConnector<DbConnection>.Connect(string options)
    {
        return Connect( options );
    }

    [Pure]
    async ValueTask<DbConnection> ISqlDatabaseConnector<DbConnection>.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    async ValueTask<DbConnection> ISqlDatabaseConnector<DbConnection>.ConnectAsync(string options, CancellationToken cancellationToken)
    {
        return await ConnectAsync( options, cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    IDbConnection ISqlDatabaseConnector.Connect()
    {
        return Connect();
    }

    [Pure]
    IDbConnection ISqlDatabaseConnector.Connect(string options)
    {
        return Connect( options );
    }

    [Pure]
    async ValueTask<IDbConnection> ISqlDatabaseConnector.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    async ValueTask<IDbConnection> ISqlDatabaseConnector.ConnectAsync(string options, CancellationToken cancellationToken)
    {
        return await ConnectAsync( options, cancellationToken ).ConfigureAwait( false );
    }
}
