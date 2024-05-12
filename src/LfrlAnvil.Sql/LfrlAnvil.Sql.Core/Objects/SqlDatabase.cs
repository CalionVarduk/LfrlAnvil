using System;
using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc />
public abstract class SqlDatabase : ISqlDatabase
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabase"/> instance.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="schemas">Collection of schemas defined in this database.</param>
    /// <param name="connector">Connector object that can be used to connect to this database.</param>
    /// <param name="version">Current version of this database.</param>
    /// <param name="versionRecordsQuery">
    /// Query reader's executor capable of reading metadata of all versions applied to this database.
    /// </param>
    protected SqlDatabase(
        SqlDatabaseBuilder builder,
        SqlSchemaCollection schemas,
        ISqlDatabaseConnector<DbConnection> connector,
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
        Connector = connector;
        Schemas = schemas;
        Schemas.SetDatabase( this, builder.Schemas );
        TypeDefinitions.Lock();
    }

    /// <inheritdoc />
    public SqlDialect Dialect { get; }

    /// <inheritdoc />
    public Version Version { get; }

    /// <inheritdoc />
    public string ServerVersion { get; }

    /// <inheritdoc />
    public ISqlDataTypeProvider DataTypes { get; }

    /// <inheritdoc cref="ISqlDatabase.TypeDefinitions" />
    public SqlColumnTypeDefinitionProvider TypeDefinitions { get; }

    /// <inheritdoc />
    public ISqlNodeInterpreterFactory NodeInterpreters { get; }

    /// <inheritdoc cref="ISqlDatabase.QueryReaders" />
    public SqlQueryReaderFactory QueryReaders { get; }

    /// <inheritdoc cref="ISqlDatabase.ParameterBinders" />
    public SqlParameterBinderFactory ParameterBinders { get; }

    /// <inheritdoc cref="ISqlDatabase.Schemas" />
    public SqlSchemaCollection Schemas { get; }

    /// <inheritdoc />
    public SqlQueryReaderExecutor<SqlDatabaseVersionRecord> VersionRecordsQuery { get; }

    /// <inheritdoc cref="ISqlDatabase.Connector" />
    public ISqlDatabaseConnector<DbConnection> Connector { get; }

    ISqlSchemaCollection ISqlDatabase.Schemas => Schemas;
    ISqlColumnTypeDefinitionProvider ISqlDatabase.TypeDefinitions => TypeDefinitions;
    ISqlQueryReaderFactory ISqlDatabase.QueryReaders => QueryReaders;
    ISqlParameterBinderFactory ISqlDatabase.ParameterBinders => ParameterBinders;
    ISqlDatabaseConnector ISqlDatabase.Connector => Connector;

    /// <inheritdoc />
    public virtual void Dispose() { }

    /// <inheritdoc />
    [Pure]
    public SqlDatabaseVersionRecord[] GetRegisteredVersions()
    {
        using var connection = Connector.Connect();
        using var command = connection.CreateCommand();
        var result = VersionRecordsQuery.Execute( command );
        return result.IsEmpty ? Array.Empty<SqlDatabaseVersionRecord>() : result.Rows.ToArray();
    }
}
