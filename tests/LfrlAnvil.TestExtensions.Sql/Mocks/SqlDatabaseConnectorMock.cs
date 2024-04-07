using System.Collections.Generic;
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
    private readonly KeyValuePair<string, object>[] _connectionStringEntries;
    private readonly string _connectionString;
    private SqlDatabaseMock? _database;

    internal SqlDatabaseConnectorMock(DbConnectionStringBuilder connectionStringBuilder, DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _eventHandler = eventHandler;
        _connectionString = connectionStringBuilder.ToString();
        _connectionStringEntries = new KeyValuePair<string, object>[connectionStringBuilder.Count];

        var i = 0;
        foreach ( var e in connectionStringBuilder )
        {
            var (key, value) = ( KeyValuePair<string, object> )e;
            _connectionStringEntries[i++] = KeyValuePair.Create( key, value );
        }
    }

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
        var connection = CreateConnection( _connectionString );
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public DbConnectionMock Connect(string options)
    {
        var connectionString = CreateConnectionString( options );
        var connection = CreateConnection( connectionString );
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<DbConnectionMock> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection( _connectionString );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<DbConnectionMock> ConnectAsync(string options, CancellationToken cancellationToken = default)
    {
        var connectionString = CreateConnectionString( options );
        var connection = CreateConnection( connectionString );
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
    private string CreateConnectionString(string options)
    {
        var result = new DbConnectionStringBuilder { ConnectionString = options };
        foreach ( var (key, value) in _connectionStringEntries )
        {
            if ( ! result.ContainsKey( key ) )
                result.Add( key, value );
        }

        return result.ToString();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DbConnectionMock CreateConnection(string connectionString)
    {
        var result = new DbConnectionMock( Database.ServerVersion ) { ConnectionString = connectionString };
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
