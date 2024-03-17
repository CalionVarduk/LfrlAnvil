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

internal sealed class SqliteDatabasePermanentConnector : ISqlDatabaseConnector<SqliteConnection>, ISqlDatabaseConnector<DbConnection>
{
    private readonly SqlitePermanentConnection _connection;
    private readonly SqliteConnectionStringBuilder _connectionStringBuilder;
    private SqliteDatabase? _database;

    internal SqliteDatabasePermanentConnector(
        SqlitePermanentConnection connection,
        SqliteConnectionStringBuilder connectionStringBuilder,
        DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _connection = connection;
        _connectionStringBuilder = connectionStringBuilder;
        eventHandler.Attach( _connection );
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
        return _connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<SqliteConnection>( _connection );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void CloseConnection()
    {
        _connection.Close();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetDatabase(SqliteDatabase database)
    {
        Assume.IsNull( _database );
        Assume.Equals( database.Connector, this );
        _database = database;
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
