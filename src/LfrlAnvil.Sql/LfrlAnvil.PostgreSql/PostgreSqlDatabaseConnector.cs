using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

public sealed class PostgreSqlDatabaseConnector : ISqlDatabaseConnector<NpgsqlConnection>, ISqlDatabaseConnector<DbConnection>
{
    private readonly string _connectionString;
    private readonly SqlConnectionStringEntry[] _connectionStringEntries;
    private readonly DbConnectionEventHandler _eventHandler;
    private PostgreSqlDatabase? _database;

    internal PostgreSqlDatabaseConnector(NpgsqlConnectionStringBuilder connectionStringBuilder, DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _connectionString = connectionStringBuilder.ToString();
        _connectionStringEntries = PostgreSqlHelpers.ExtractConnectionStringEntries( connectionStringBuilder );
        _eventHandler = eventHandler;
    }

    public PostgreSqlDatabase Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    SqlDatabase ISqlDatabaseConnector<NpgsqlConnection>.Database => Database;
    SqlDatabase ISqlDatabaseConnector<DbConnection>.Database => Database;
    ISqlDatabase ISqlDatabaseConnector.Database => Database;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public NpgsqlConnection Connect()
    {
        var connection = CreateConnection( _connectionString );
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public NpgsqlConnection Connect(string options)
    {
        var connectionString = PostgreSqlHelpers.ExtendConnectionString( _connectionStringEntries, options );
        var connection = CreateConnection( connectionString );
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<NpgsqlConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection( _connectionString );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<NpgsqlConnection> ConnectAsync(string options, CancellationToken cancellationToken = default)
    {
        var connectionString = PostgreSqlHelpers.ExtendConnectionString( _connectionStringEntries, options );
        var connection = CreateConnection( connectionString );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDatabase(PostgreSqlDatabase database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Connector, this );
        _database = database;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private NpgsqlConnection CreateConnection(string connectionString)
    {
        var result = new NpgsqlConnection( connectionString );
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
