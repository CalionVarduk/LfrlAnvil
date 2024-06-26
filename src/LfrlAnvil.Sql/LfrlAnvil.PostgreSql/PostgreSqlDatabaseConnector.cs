﻿// Copyright 2024 Łukasz Furlepa
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

/// <inheritdoc cref="ISqlDatabaseConnector" />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
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

    /// <inheritdoc cref="ISqlDatabaseConnector{TConnection}.Database" />
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

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public NpgsqlConnection Connect()
    {
        var connection = CreateConnection( _connectionString );
        connection.Open();
        return connection;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public NpgsqlConnection Connect(string options)
    {
        var connectionString = PostgreSqlHelpers.ExtendConnectionString( _connectionStringEntries, options );
        var connection = CreateConnection( connectionString );
        connection.Open();
        return connection;
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<NpgsqlConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection( _connectionString );
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    /// <inheritdoc />
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
