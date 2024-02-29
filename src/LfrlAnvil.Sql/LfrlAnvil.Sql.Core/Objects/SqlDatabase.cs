using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlDatabase : ISqlDatabase
{
    protected SqlDatabase(
        SqlDatabaseBuilder builder,
        SqlSchemaCollection schemas,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
    {
        Version = version;
        VersionRecordsQuery = versionRecordsQuery;
        Dialect = builder.Dialect;
        ServerVersion = builder.ServerVersion;
        DataTypes = builder.DataTypes;
        TypeDefinitions = builder.TypeDefinitions;
        NodeInterpreters = builder.NodeInterpreters;
        QueryReaders = builder.QueryReaders;
        ParameterBinders = builder.ParameterBinders;
        Schemas = schemas;
        Schemas.SetDatabase( this, builder.Schemas );
        TypeDefinitions.Lock();
    }

    public SqlDialect Dialect { get; }
    public Version Version { get; }
    public string ServerVersion { get; }
    public ISqlDataTypeProvider DataTypes { get; }
    public SqlColumnTypeDefinitionProvider TypeDefinitions { get; }
    public ISqlNodeInterpreterFactory NodeInterpreters { get; }
    public SqlQueryReaderFactory QueryReaders { get; }
    public SqlParameterBinderFactory ParameterBinders { get; }
    public SqlSchemaCollection Schemas { get; }
    public SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    ISqlSchemaCollection ISqlDatabase.Schemas => Schemas;
    ISqlColumnTypeDefinitionProvider ISqlDatabase.TypeDefinitions => TypeDefinitions;
    ISqlQueryReaderFactory ISqlDatabase.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabase.ParameterBinders => ParameterBinders;

    public virtual void Dispose() { }

    [Pure]
    public abstract DbConnection Connect();

    [Pure]
    public abstract ValueTask<DbConnection> ConnectAsync(CancellationToken cancellationToken = default);

    [Pure]
    public SqlDatabaseVersionRecord[] GetRegisteredVersions()
    {
        using var connection = Connect();
        using var command = connection.CreateCommand();
        var result = VersionRecordsQuery.Execute( command );
        return result.IsEmpty ? Array.Empty<SqlDatabaseVersionRecord>() : result.Rows.ToArray();
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
