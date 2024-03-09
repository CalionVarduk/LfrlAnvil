using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.MySql.Objects;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlDatabase : SqlDatabase
{
    private readonly MySqlConnectionStringBuilder _connectionStringBuilder;
    private readonly ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> _connectionChangeCallbacks;
    private readonly string _connectionString;

    internal MySqlDatabase(
        MySqlConnectionStringBuilder connectionStringBuilder,
        MySqlDatabaseBuilder builder,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> connectionChangeCallbacks)
        : base( builder, new MySqlSchemaCollection( builder.Schemas ), version, versionRecordsQuery )
    {
        _connectionString = connectionStringBuilder.ToString();
        _connectionStringBuilder = connectionStringBuilder;
        _connectionChangeCallbacks = connectionChangeCallbacks;
    }

    public new MySqlSchemaCollection Schemas => ReinterpretCast.To<MySqlSchemaCollection>( base.Schemas );
    public new MySqlDataTypeProvider DataTypes => ReinterpretCast.To<MySqlDataTypeProvider>( base.DataTypes );

    public new MySqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<MySqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new MySqlNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<MySqlNodeInterpreterFactory>( base.NodeInterpreters );
    public new MySqlQueryReaderFactory QueryReaders => ReinterpretCast.To<MySqlQueryReaderFactory>( base.QueryReaders );
    public new MySqlParameterBinderFactory ParameterBinders => ReinterpretCast.To<MySqlParameterBinderFactory>( base.ParameterBinders );

    [Pure]
    public override MySqlConnection Connect()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    [Pure]
    public async ValueTask<MySqlConnection> ConnectMySqlAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [Pure]
    public override async ValueTask<DbConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return await ConnectMySqlAsync( cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    private MySqlConnection CreateConnection()
    {
        var result = new MySqlConnection( _connectionString );
        InitializeConnectionEventHandlers( result, _connectionChangeCallbacks );
        return result;
    }
}
