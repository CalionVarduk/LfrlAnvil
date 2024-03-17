using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseConnectorMock : ISqlDatabaseConnector<DbConnectionMock>, ISqlDatabaseConnector<DbConnection>
{
    private readonly DbConnectionEventHandler _eventHandler;
    private SqlDatabaseMock? _database;

    internal SqlDatabaseConnectorMock(DbConnectionStringBuilder connectionStringBuilder, DbConnectionEventHandler eventHandler)
    {
        _database = null;
        ConnectionString = connectionStringBuilder;
        _eventHandler = eventHandler;
    }

    public DbConnectionStringBuilder? ConnectionString { get; }

    public SqlDatabaseMock Database
    {
        get
        {
            Assume.IsNotNull( _database );
            return _database;
        }
    }

    SqlDatabase ISqlDatabaseConnector<DbConnectionMock>.Database => Database;
    SqlDatabase ISqlDatabaseConnector<DbConnection>.Database => Database;
    ISqlDatabase ISqlDatabaseConnector.Database => Database;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DbConnectionMock Connect()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<DbConnectionMock> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    internal void SetDatabase(SqlDatabaseMock database)
    {
        Assume.IsNull( _database );
        _database = database;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DbConnectionMock CreateConnection()
    {
        var result = new DbConnectionMock( Database.ServerVersion ) { ConnectionString = ConnectionString?.ToString() ?? string.Empty };
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
