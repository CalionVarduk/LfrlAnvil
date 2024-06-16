// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Exceptions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabasePermanentConnector : ISqliteDatabaseConnector, ISqlDatabaseConnector<DbConnection>
{
    private readonly SqlitePermanentConnection _connection;
    private readonly DbConnectionEventHandler _eventHandler;
    private readonly SqlConnectionStringEntry[] _connectionStringEntries;
    private readonly bool _isInMemoryDatabase;
    private SqliteDatabase? _database;

    internal SqliteDatabasePermanentConnector(
        SqlitePermanentConnection connection,
        SqliteConnectionStringBuilder connectionStringBuilder,
        DbConnectionEventHandler eventHandler)
    {
        _database = null;
        _connection = connection;
        _eventHandler = eventHandler;
        _connectionStringEntries = SqliteHelpers.ExtractConnectionStringEntries( connectionStringBuilder );
        _isInMemoryDatabase = connectionStringBuilder.DataSource == SqliteHelpers.MemoryDataSource;
        _eventHandler.Attach( _connection );
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
    public SqliteConnection Connect(string options)
    {
        var connection = CreateConnection( options );
        connection.Open();
        return connection;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<SqliteConnection>( _connection );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqliteConnection> ConnectAsync(string options, CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection( options );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
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
    private SqliteConnection CreateConnection(string options)
    {
        if ( _isInMemoryDatabase )
            throw new InvalidOperationException( Resources.ConnectionStringToInMemoryDatabaseCannotBeModified );

        var connectionString = SqliteHelpers.ExtendConnectionString( _connectionStringEntries, options );
        var result = new SqliteConnection( connectionString );
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
