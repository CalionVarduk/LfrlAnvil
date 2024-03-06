using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public abstract class SqliteDatabase : SqlDatabase
{
    protected readonly SqliteConnectionStringBuilder ConnectionStringBuilder;

    internal SqliteDatabase(
        SqliteConnectionStringBuilder connectionStringBuilder,
        SqliteDatabaseBuilder builder,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base( builder, new SqliteSchemaCollection( builder.Schemas ), version, versionRecordsQuery )
    {
        ConnectionStringBuilder = connectionStringBuilder;
    }

    public new SqliteSchemaCollection Schemas => ReinterpretCast.To<SqliteSchemaCollection>( base.Schemas );
    public new SqliteDataTypeProvider DataTypes => ReinterpretCast.To<SqliteDataTypeProvider>( base.DataTypes );

    public new SqliteColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<SqliteColumnTypeDefinitionProvider>( base.TypeDefinitions );

    public new SqliteNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<SqliteNodeInterpreterFactory>( base.NodeInterpreters );
    public new SqliteQueryReaderFactory QueryReaders => ReinterpretCast.To<SqliteQueryReaderFactory>( base.QueryReaders );
    public new SqliteParameterBinderFactory ParameterBinders => ReinterpretCast.To<SqliteParameterBinderFactory>( base.ParameterBinders );

    [Pure]
    public sealed override SqliteConnection Connect()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }

    [Pure]
    public async ValueTask<SqliteConnection> ConnectSqliteAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync( cancellationToken ).ConfigureAwait( false );
        return connection;
    }

    [Pure]
    public sealed override async ValueTask<DbConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return await ConnectSqliteAsync( cancellationToken ).ConfigureAwait( false );
    }

    public virtual void Dispose() { }

    [Pure]
    protected abstract SqliteConnection CreateConnection();
}
