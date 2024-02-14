using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.MySql.Objects;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlDatabase : ISqlDatabase
{
    [DebuggerBrowsable( DebuggerBrowsableState.Never )]
    private readonly string _connectionString;

    private readonly Action<SqlDatabaseConnectionChangeEvent>[] _connectionChangeCallbacks;

    internal MySqlDatabase(
        string connectionString,
        MySqlDatabaseBuilder builder,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        Version version)
    {
        _connectionString = connectionString;
        VersionRecordsQuery = versionRecordsQuery;
        Version = version;
        Dialect = builder.Dialect;
        DataTypes = builder.DataTypes;
        TypeDefinitions = builder.TypeDefinitions;
        NodeInterpreters = builder.NodeInterpreters;
        QueryReaders = builder.QueryReaders;
        ParameterBinders = builder.ParameterBinders;
        ServerVersion = builder.ServerVersion;
        Schemas = new MySqlSchemaCollection( this, builder.Schemas );
        _connectionChangeCallbacks = builder.ConnectionChanges.GetCallbacksArray();
    }

    public SqlDialect Dialect { get; }
    public Version Version { get; }
    public MySqlSchemaCollection Schemas { get; }
    public MySqlDataTypeProvider DataTypes { get; }
    public MySqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    public MySqlNodeInterpreterFactory NodeInterpreters { get; }
    public MySqlQueryReaderFactory QueryReaders { get; }
    public MySqlParameterBinderFactory ParameterBinders { get; }
    public string ServerVersion { get; }
    public SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    ISqlSchemaCollection ISqlDatabase.Schemas => Schemas;
    ISqlDataTypeProvider ISqlDatabase.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabase.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabase.NodeInterpreters => NodeInterpreters;
    ISqlQueryReaderFactory ISqlDatabase.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabase.ParameterBinders => ParameterBinders;

    [Pure]
    public MySqlConnection Connect()
    {
        var result = CreateConnection();
        result.Open();
        return result;
    }

    [Pure]
    public async ValueTask<MySqlConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var result = CreateConnection();
        await result.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return result;
    }

    [Pure]
    public SqlDatabaseVersionRecord[] GetRegisteredVersions()
    {
        using var connection = Connect();
        using var command = connection.CreateCommand();
        var result = VersionRecordsQuery.Execute( command );
        return result.IsEmpty ? Array.Empty<SqlDatabaseVersionRecord>() : result.Rows.ToArray();
    }

    public void Dispose() { }

    [Pure]
    private MySqlConnection CreateConnection()
    {
        var result = new MySqlConnection( _connectionString );
        result.StateChange += OnConnectionStateChange;
        result.Disposed += OnConnectionDisposal;
        return result;
    }

    private void OnConnectionStateChange(object sender, StateChangeEventArgs e)
    {
        var @event = new SqlDatabaseConnectionChangeEvent( (DbConnection)sender, e );
        foreach ( var callback in _connectionChangeCallbacks )
            callback( @event );
    }

    private void OnConnectionDisposal(object? sender, EventArgs e)
    {
        var connection = (DbConnection?)sender;
        if ( connection is not null )
        {
            connection.StateChange -= OnConnectionStateChange;
            connection.Disposed -= OnConnectionDisposal;
        }
    }

    [Pure]
    IDbConnection ISqlDatabase.Connect()
    {
        return Connect();
    }

    [Pure]
    async ValueTask<IDbConnection> ISqlDatabase.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken ).ConfigureAwait( false );
    }
}
