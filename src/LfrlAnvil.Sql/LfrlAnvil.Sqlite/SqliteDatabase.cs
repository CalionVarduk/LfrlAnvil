using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public abstract class SqliteDatabase : ISqlDatabase
{
    internal SqliteDatabase(
        SqliteDatabaseBuilder builder,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        Version version)
    {
        Version = version;
        DataTypes = builder.DataTypes;
        TypeDefinitions = builder.TypeDefinitions;
        NodeInterpreterFactory = builder.NodeInterpreterFactory;
        ServerVersion = builder.ServerVersion;
        VersionRecordsQuery = versionRecordsQuery;
        Schemas = new SqliteSchemaCollection( this, builder.Schemas );
    }

    public Version Version { get; }
    public SqliteSchemaCollection Schemas { get; }
    public SqliteDataTypeProvider DataTypes { get; }
    public SqliteColumnTypeDefinitionProvider TypeDefinitions { get; }
    public SqliteNodeInterpreterFactory NodeInterpreterFactory { get; }
    public string ServerVersion { get; }
    public SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    ISqlSchemaCollection ISqlDatabase.Schemas => Schemas;
    ISqlDataTypeProvider ISqlDatabase.DataTypes => DataTypes;
    ISqlColumnTypeDefinitionProvider ISqlDatabase.TypeDefinitions => TypeDefinitions;
    ISqlNodeInterpreterFactory ISqlDatabase.NodeInterpreterFactory => NodeInterpreterFactory;

    [Pure]
    public abstract SqliteConnection Connect();

    [Pure]
    public abstract ValueTask<SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default);

    [Pure]
    public SqlDatabaseVersionRecord[] GetRegisteredVersions()
    {
        using var connection = Connect();
        using var command = connection.CreateCommand();
        var result = VersionRecordsQuery.Execute( command );
        return result.IsEmpty ? Array.Empty<SqlDatabaseVersionRecord>() : result.Rows.ToArray();
    }

    public virtual void Dispose() { }

    [Pure]
    IDbConnection ISqlDatabase.Connect()
    {
        return Connect();
    }

    [Pure]
    async ValueTask<IDbConnection> ISqlDatabase.ConnectAsync(CancellationToken cancellationToken)
    {
        return await ConnectAsync( cancellationToken );
    }
}
