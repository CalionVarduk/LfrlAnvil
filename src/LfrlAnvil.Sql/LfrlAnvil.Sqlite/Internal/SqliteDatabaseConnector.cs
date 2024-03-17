using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseConnector : ISqlDatabaseConnector<SqliteConnection>, ISqlDatabaseConnector<DbConnection>
{
    private readonly string _connectionString;
    private readonly SqliteConnectionStringBuilder _connectionStringBuilder;
    private readonly DbConnectionEventHandler _eventHandler;
    private SqliteDatabase? _database;

    internal SqliteDatabaseConnector(
        string connectionString,
        SqliteConnectionStringBuilder connectionStringBuilder,
        DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _connectionString = connectionString;
        _connectionStringBuilder = connectionStringBuilder;
        _eventHandler = eventHandler;
    }

    public SqliteDatabase Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    SqlDatabase ISqlDatabaseConnector<SqliteConnection>.Database => Database;
    SqlDatabase ISqlDatabaseConnector<DbConnection>.Database => Database;
    ISqlDatabase ISqlDatabaseConnector.Database => Database;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteConnection Connect()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDatabase(SqliteDatabase database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Connector, this );
        _database = database;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private SqliteConnection CreateConnection()
    {
        var result = new SqliteConnection( _connectionString );
        _eventHandler.Attach( result );
        return result;
    }

    [Pure]
    DbConnection ISqlDatabaseConnector<DbConnection>.Connect()
    {
        return Connect();
    }

    [Pure]
    async ValueTask<DbConnection> ISqlDatabaseConnector<DbConnection>.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    IDbConnection ISqlDatabaseConnector.Connect()
    {
        return Connect();
    }

    [Pure]
    async ValueTask<IDbConnection> ISqlDatabaseConnector.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }
}
